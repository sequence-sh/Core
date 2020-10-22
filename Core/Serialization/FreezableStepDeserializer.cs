using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
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
        public override Result<IFreezableStep, YamlException> TryDeserialize(IParser parser, Func<IParser, Type, object?> nestedObjectDeserializer)
        {
            if (parser.Current == null)
                return new YamlException("Reader is empty");

            var markStart = parser.Current.Start;
            var markEnd = parser.Current.End;

            var dictionary = new Dictionary<string, StepMember>();

            IStepFactory? factory = null;
            Configuration? configuration = null;

            parser.Consume<MappingStart>();
            while (!parser.TryConsume<MappingEnd>(out _))
            {
                //TODO handle promises

                var keyResult = TryDeserializeNested<string>(nestedObjectDeserializer, parser);

                if (keyResult.IsFailure)
                    return keyResult.ConvertFailure<IFreezableStep>();

                if (keyResult.Value.Equals(YamlMethods.TypeString, StringComparison.OrdinalIgnoreCase))
                {
                    if (factory != null)
                        return new YamlException(markStart, markEnd, $"Duplicate property '{keyResult.Value}'");

                    var typeNameResult = TryDeserializeNested<string>(nestedObjectDeserializer, parser)
                        .Bind(x => StepMemberParser
                            .StepFactoryStore.Dictionary.TryFindOrFail(x, $"'{x}' is not the name of a Step")
                            .MapError(e => new YamlException(markStart, markEnd, e))
                        );

                    if (typeNameResult.IsFailure)
                        return typeNameResult.ConvertFailure<IFreezableStep>();

                    factory = typeNameResult.Value;
                }
                else if (keyResult.Value.Equals(YamlMethods.ConfigString, StringComparison.OrdinalIgnoreCase))
                {
                    if (configuration != null)
                        return new YamlException(markStart, markEnd, $"Duplicate property '{keyResult.Value}'");

                    var configResult = TryDeserializeNested<Configuration>(nestedObjectDeserializer, parser);

                    if (configResult.IsFailure)
                        return configResult.ConvertFailure<IFreezableStep>();

                    configuration = configResult.Value;
                }
                else
                {
                    if (dictionary.ContainsKey(keyResult.Value))
                        return new YamlException(markStart, markEnd, $"Duplicate property '{keyResult.Value}'");


                    var memberResult = TryDeserializeNested<StepMember>(nestedObjectDeserializer, parser);

                    if (memberResult.IsFailure)
                        return memberResult.ConvertFailure<IFreezableStep>();

                    dictionary.Add(keyResult.Value, memberResult.Value);
                }
            }


            if (factory == null)
                return new YamlException(markStart, markEnd, $"The '{YamlMethods.TypeString}' property must be set.");

            var fsd = FreezableStepData.TryCreate(factory, dictionary);
            if(fsd.IsFailure)
                throw new YamlException(fsd.Error);

            return new CompoundFreezableStep(factory, fsd.Value, configuration);

        }
    }
}