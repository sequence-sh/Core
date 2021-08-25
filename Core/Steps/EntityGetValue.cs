using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Gets the value of a property from an entity
/// </summary>
[Alias("From")]
[SCLExample("(foo: 123)['foo']",            expectedOutput: "123")]
[SCLExample("(foo: (bar: 123))['foo.bar']", expectedOutput: "123")]
public sealed class EntityGetValue<T> : CompoundStep<T>
{
    /// <inheritdoc />
    protected override async Task<Result<T, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var r = await stateMonad.RunStepsAsync(
            Entity,
            Property.WrapStringStream(),
            cancellationToken
        );

        if (r.IsFailure)
            return r.ConvertFailure<T>();

        var (entity, property) = r.Value;

        EntityPropertyKey epk;

        if (property.StartsWith("$"))
        {
            epk = new EntityPropertyKey(property.TrimStart('$'));
        }
        else
        {
            var keys = property.Split(".");
            epk = new EntityPropertyKey(keys);
        }

        var entityValue = entity.TryGetValue(epk);

        if (entityValue.HasNoValue)
            return EntityValue.GetDefaultValue<T>();

        var result = entityValue.Value.TryGetValue<T>()
            .MapError(x => x.WithLocation(this));

        return result;
    }

    /// <summary>
    /// The entity to get the property from.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Entity> Entity { get; set; } = null!;

    /// <summary>
    /// The name of the property to get.
    /// Returns an empty string if the property is not present.
    /// </summary>
    [StepProperty(2)]
    [Required]
    [Alias("Get")]
    public IStep<StringStream> Property { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory => EntityGetValueStepFactory.Instance;

    /// <inheritdoc />
    public override bool ShouldBracketWhenSerialized => false;

    private sealed class EntityGetValueStepFactory : GenericStepFactory
    {
        private EntityGetValueStepFactory() { }
        public static GenericStepFactory Instance { get; } = new EntityGetValueStepFactory();

        /// <inheritdoc />
        public override Type StepType { get; } = typeof(EntityGetValue<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "The required type";

        /// <inheritdoc />
        protected override Result<TypeReference, IError> GetGenericTypeParameter(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            return callerMetadata.ExpectedType;
        }

        /// <inheritdoc />
        protected override TypeReference GetOutputTypeReference(TypeReference memberTypeReference)
        {
            return memberTypeReference;
        }

        /// <inheritdoc />
        public override IStepSerializer Serializer => EntityGetValueSerializer.Instance;
    }
}

}
