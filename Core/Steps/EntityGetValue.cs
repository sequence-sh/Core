using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Gets the value of a property from an entity
/// </summary>
[Alias("From")]
public sealed class EntityGetValue<T> : CompoundStep<T>
{
    /// <inheritdoc />
    protected override async Task<Result<T, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var entity = await Entity.Run(stateMonad, cancellationToken);

        if (entity.IsFailure)
            return entity.ConvertFailure<T>();

        var propertyResult = await Property.Run(stateMonad, cancellationToken)
            .Map(x => x.GetStringAsync());

        if (propertyResult.IsFailure)
            return propertyResult.ConvertFailure<T>();

        var epk = new EntityPropertyKey(propertyResult.Value);

        var entityValue = entity.Value.TryGetValue(epk)
                .ToResult(
                    ErrorCode.SchemaViolationMissingProperty.ToErrorBuilder(propertyResult.Value) as
                        IErrorBuilder
                )
                .Bind(x => x.TryGetValue<T>())
                .MapError(x => x.WithLocation(this))
            ;

        return entityValue;
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
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override Result<ITypeReference, IError> GetMemberType(
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            throw new NotImplementedException();
        }
    }
}

///// <summary>
///// Gets the value of a property from an entity
///// </summary>
//public sealed class EntityGetValueStepFactory : SimpleStepFactory<EntityGetValue, T>
//{
//    private EntityGetValueStepFactory() { }

//    /// <summary>
//    /// The instance.
//    /// </summary>
//    public static SimpleStepFactory<EntityGetValue, StringStream> Instance { get; } =
//        new EntityGetValueStepFactory();

//    /// <inheritdoc />
//    public override IStepSerializer Serializer => EntityGetValueSerializer.Instance;
//}

}
