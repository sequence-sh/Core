using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace Reductech.EDR.Core.Serialization
{
    internal class SchemaManifest : Manifest
    {
        private SchemaManifest() {}

        public static Manifest Instance { get; } = new SchemaManifest();

        /// <inheritdoc />
        public override string Name => nameof(Schema);

        /// <inheritdoc />
        public override string KeyPropertyName => nameof(Schema.Name);

        /// <inheritdoc />
        public override IReadOnlyDictionary<string, Type> SpecialProperties { get;} = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            {nameof(Schema.AllowExtraProperties), typeof(bool)},
            {nameof(Schema.Properties), typeof(Dictionary<string, SchemaProperty>)}
        };

        /// <inheritdoc />
        public override Result<IFreezableStep, IErrorBuilder> TryBuild(
            string keyName,
            StepMemberParser stepMemberParser, IReadOnlyDictionary<string, object> specialPropertyValues,
            IReadOnlyDictionary<string, StepMember> stepMemberPropertyValues)
        {
            var schema = new Schema
            {
                Name = keyName
            };

            if (specialPropertyValues.TryGetValue(nameof(Schema.Properties), out var propertiesObject) &&
                propertiesObject is Dictionary<string, SchemaProperty> propertiesDict)
                schema.Properties = propertiesDict;
            else
                return Result.Failure<IFreezableStep, IErrorBuilder>(
                    ErrorHelper.MissingParameterError(nameof(Schema.Properties), nameof(Schema)));

            if(specialPropertyValues.TryGetValue(nameof(Schema.AllowExtraProperties), out var ep) && ep is bool b)

                    schema.AllowExtraProperties = b;

            var c= new ConstantFreezableStep(schema);

            return c;
        }
    }

    internal class CompoundFreezableStepManifest : Manifest
    {
        private CompoundFreezableStepManifest(){}

        public static Manifest Instance { get; } = new CompoundFreezableStepManifest();

        /// <inheritdoc />
        public override string Name => nameof(CompoundFreezableStep);


        /// <inheritdoc />
        public override string KeyPropertyName => YamlMethods.TypeString;

        /// <inheritdoc />
        public override IReadOnlyDictionary<string, Type> SpecialProperties { get; } = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            {YamlMethods.ConfigString, typeof(Configuration)}
        };


        /// <inheritdoc />
        public override Result<IFreezableStep, IErrorBuilder> TryBuild(
            string keyProperty,
            StepMemberParser stepMemberParser,
            IReadOnlyDictionary<string, object> specialPropertyValues,
            IReadOnlyDictionary<string, StepMember> stepMemberPropertyValues)
        {
            if (!stepMemberParser.StepFactoryStore.Dictionary.TryGetValue(keyProperty, out var factory))
                return Result.Failure<IFreezableStep, IErrorBuilder>(new ErrorBuilder(
                    $"'{keyProperty}' is not the name of a step",
                    ErrorCode.CouldNotDeserialize));

            var requiredProperties = factory.RequiredProperties.ToHashSet(StringComparer.OrdinalIgnoreCase);
            requiredProperties.ExceptWith(stepMemberPropertyValues.Keys);

            var errors = requiredProperties
                .Select(missing => ErrorHelper.MissingParameterError(missing, factory.TypeName)).ToList();
            if(errors.Any())
                return Result.Failure<IFreezableStep, IErrorBuilder>(ErrorBuilderList.Combine(errors));


            var fsd = FreezableStepData.TryCreate(factory!, stepMemberPropertyValues);

            if (fsd.IsFailure)
                return fsd.ConvertFailure<IFreezableStep>();

            Configuration? configuration = null;

            if (specialPropertyValues.TryGetValue(YamlMethods.ConfigString, out var c) && c is Configuration c2)
                configuration = c2;

            return new CompoundFreezableStep(factory!, fsd.Value, configuration);
        }
    }
    internal abstract class Manifest
    {
        public abstract string Name { get; }

        public abstract string KeyPropertyName { get; } //these must be unique

        /// <summary>
        /// Properties of this type which should not deserialize to a step member
        /// </summary>
        public abstract IReadOnlyDictionary<string, Type> SpecialProperties { get; }

        public abstract Result<IFreezableStep, IErrorBuilder> TryBuild(
            string key,
            StepMemberParser stepMemberParser,
            IReadOnlyDictionary<string, object> specialPropertyValues,
            IReadOnlyDictionary<string, StepMember> stepMemberPropertyValues);

        /// <inheritdoc />
        public override string ToString() => Name;
    }


    internal class FreezableStepDeserializer : TypedYamlDeserializer<IFreezableStep>
    {
        /// <inheritdoc />
        public FreezableStepDeserializer(StepMemberParser stepMemberParser) => StepMemberParser = stepMemberParser;

        /// <summary>
        /// The step member parser
        /// </summary>
        public StepMemberParser StepMemberParser { get; }

        private static readonly IReadOnlyDictionary<string, Manifest> Manifests = new List<Manifest>
        {
            CompoundFreezableStepManifest.Instance,
            SchemaManifest.Instance
        }
            .ToDictionary(x=>x.KeyPropertyName, StringComparer.OrdinalIgnoreCase)!;



        /// <inheritdoc />
        public override Result<IFreezableStep, IError> TryDeserialize(IParser parser, Func<IParser, Type, object?> nestedObjectDeserializer)
        {

            var markStart = parser.Current!.Start;

            var specialObjectDictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            var stepMemberDictionary = new Dictionary<string, StepMember>(StringComparer.OrdinalIgnoreCase);

            var errors = new List<IError>();

            parser.Consume<MappingStart>();

            var manifest = Maybe<(Manifest manifest, string  key)>.None;

            while (!parser.TryConsume<MappingEnd>(out _))
            {
                //TODO handle promises
                var keyResult = TryDeserializeNested<string>(nestedObjectDeserializer, parser);

                if (keyResult.IsFailure)
                    return keyResult.ConvertFailure<IFreezableStep>();

                if (manifest.HasNoValue)
                {
                    if (!Manifests.TryGetValue(keyResult.Value, out var m))
                        return Result.Failure<IFreezableStep, IError>(
                            new ErrorBuilder(
                                    $"Could not deserialize an object whose first property is '{keyResult.Value}'",
                                    ErrorCode.CouldNotDeserialize)
                                .WithLocation(new YamlRegionErrorLocation(markStart, parser.Current.End)));

                    var keyValue = TryDeserializeNested<string>(nestedObjectDeserializer, parser);

                    if (keyValue.IsFailure)
                        return keyValue.ConvertFailure<IFreezableStep>();

                    manifest = (m, keyValue.Value);
                }
                else if(manifest.Value.manifest.SpecialProperties.TryGetValue(keyResult.Value, out var t))
                {
                    var specialObjectResult = TryDeserializeNested(nestedObjectDeserializer, t, parser);
                    if (specialObjectResult.IsSuccess)
                    {
                        if (!specialObjectDictionary.TryAdd(keyResult.Value, specialObjectResult.Value))
                            errors.Add(ErrorHelper.DuplicateParameterError(keyResult.Value).WithLocation(markStart, parser.Current.End));
                    }
                    else
                        errors.Add(specialObjectResult.Error);
                }
                else
                {
                    var memberResult = TryDeserializeNested<StepMember>(nestedObjectDeserializer, parser);

                    if (memberResult.IsFailure)
                        errors.Add(memberResult.Error);
                    else if (!stepMemberDictionary.TryAdd(keyResult.Value, memberResult.Value))
                        errors.Add(ErrorHelper.DuplicateParameterError(keyResult.Value).WithLocation(markStart, parser.Current.End));
                }
            }

            if (errors.Any())
                return ErrorList.Combine(errors);

            var buildResult = manifest.Value.manifest.TryBuild(manifest.Value.key, StepMemberParser, specialObjectDictionary, stepMemberDictionary)
                    .MapError(x=>x.WithLocation(markStart, parser.Current.End));

            return buildResult;

        }
    }
}