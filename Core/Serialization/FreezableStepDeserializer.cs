using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace Reductech.EDR.Core.Serialization
{
    internal class FreezableStepDeserializer : TypedYamlDeserializer<IFreezableStep>
    {
        /// <inheritdoc />
        public FreezableStepDeserializer(StepMemberParser stepMemberParser) => StepMemberParser = stepMemberParser;

        /// <summary>
        /// The step member parser
        /// </summary>
        public StepMemberParser StepMemberParser { get; }


        /// <inheritdoc />
        public override Result<IFreezableStep, IError> TryDeserialize(IParser parser, Func<IParser, Type, object?> nestedObjectDeserializer)
        {

            var markStart = parser.Current!.Start;
            //var markEnd = parser.Current.End;

            var dictionary = new Dictionary<string, StepMember>();

            IStepFactory? factory = null;
            Configuration? configuration = null;

            var errors = new List<IError>();

            parser.Consume<MappingStart>();
            while (!parser.TryConsume<MappingEnd>(out _))
            {
                //TODO handle promises

                var keyResult = TryDeserializeNested<string>(nestedObjectDeserializer, parser);

                if (keyResult.IsFailure)
                    return keyResult.ConvertFailure<IFreezableStep>();

                if (keyResult.Value.Equals(YamlMethods.TypeString, StringComparison.OrdinalIgnoreCase))
                {
                    var typeNameResult = TryDeserializeNested<string>(nestedObjectDeserializer, parser)
                        .Bind(x => StepMemberParser
                            .StepFactoryStore.Dictionary.TryFindOrFail(x, () => ErrorHelper.MissingStepError(x).WithLocation(markStart, parser.Current.End))
                        );

                    if (typeNameResult.IsFailure)
                        errors.Add(typeNameResult.Error);
                    else if(factory == null)
                        factory = typeNameResult.Value;
                    else
                        errors.Add(ErrorHelper.DuplicateParameterError(keyResult.Value).WithLocation(markStart, parser.Current.End));
                }
                else if (keyResult.Value.Equals(YamlMethods.ConfigString, StringComparison.OrdinalIgnoreCase))
                {
                    var configResult = TryDeserializeNested<Configuration>(nestedObjectDeserializer, parser);

                    if (configResult.IsFailure)
                        errors.Add(configResult.Error);
                    else if (configuration == null)
                        configuration = configResult.Value;
                    else
                        errors.Add(ErrorHelper.DuplicateParameterError(keyResult.Value).WithLocation(markStart, parser.Current.End));
                }
                else
                {
                    var memberResult = TryDeserializeNested<StepMember>(nestedObjectDeserializer, parser);

                    if (memberResult.IsFailure)
                        errors.Add(memberResult.Error);
                    else if (!dictionary.TryAdd(keyResult.Value, memberResult.Value))
                        errors.Add(ErrorHelper.DuplicateParameterError(keyResult.Value).WithLocation(markStart, parser.Current.End));
                }
            }


            if (factory == null)
            {
                errors.Add(ErrorHelper.MissingParameterError(YamlMethods.TypeString, "Step Definition").WithLocation(new YamlRegionErrorLocation(markStart, parser.Current.End)));
            }
            else
            {
                var requiredProperties = factory.RequiredProperties.ToHashSet(StringComparer.OrdinalIgnoreCase);
                requiredProperties.ExceptWith(dictionary.Keys);

                errors.AddRange(requiredProperties
                    .Select(missing =>
                        ErrorHelper.MissingParameterError(missing, factory.TypeName)
                        .WithLocation(markStart, parser.Current.End)));
            }

            if (errors.Any())
                return ErrorList.Combine(errors);


            var fsd =
                FreezableStepData.TryCreate(factory!, dictionary)
                    .MapError(x=> x.WithLocation(new YamlRegionErrorLocation(markStart, parser.Current.End)));

            if (fsd.IsFailure)
                return fsd.ConvertFailure<IFreezableStep>();

            return new CompoundFreezableStep(factory!, fsd.Value, configuration);

        }
    }
}