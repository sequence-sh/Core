using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Gets the names of all properties of an entity
/// </summary>
[SCLExample(
    "EntityGetProperties (property1: 1, property2: 2)",
    ExpectedOutput = "[\"property1\", \"property2\"]"
)]
[Alias("GetProps")]
[Alias("GetProperties")]
public sealed class EntityGetProperties : CompoundStep<Array<StringStream>>
{
    /// <inheritdoc />
    protected override async Task<Result<Array<StringStream>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var entity = await Entity.Run(stateMonad, cancellationToken);

        if (entity.IsFailure)
            return entity.ConvertFailure<Array<StringStream>>();

        var array = entity.Value.Dictionary.OrderBy(x => x.Value.Order)
            .Select(x => new StringStream(x.Key))
            .ToSCLArray();

        return array;
    }

    /// <summary>
    /// The entity to get properties from
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Entity> Entity { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<EntityGetProperties, Array<StringStream>>();
}
