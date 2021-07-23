using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Checks if an entity has a particular property.
/// </summary>
[Alias("DoesEntityHave")]
public sealed class EntityHasProperty : CompoundStep<bool>
{
    /// <inheritdoc />
    protected override async Task<Result<bool, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var r = await stateMonad.RunStepsAsync(
            Entity,
            Property.WrapStringStream(),
            cancellationToken
        );

        if (r.IsFailure)
            return r.ConvertFailure<bool>();

        var (entity, property) = r.Value;

        var result = entity.TryGetValue(property).HasValue;

        return result;
    }

    /// <summary>
    /// The entity to check the property on.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Entity> Entity { get; set; } = null!;

    /// <summary>
    /// The name of the property to check.
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<StringStream> Property { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<EntityHasProperty, bool>();
}

}
