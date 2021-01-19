using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Newtonsoft.Json;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Create entities from a Json Stream
/// </summary>
public sealed class FromJson : CompoundStep<Array<Entity>>
{
    /// <inheritdoc />
    protected override async Task<Result<Array<Entity>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var text = await Stream.Run(stateMonad, cancellationToken)
            .Map(x => x.GetStringAsync());

        if (text.IsFailure)
            return text.ConvertFailure<Array<Entity>>();

        var entities = JsonConvert.DeserializeObject<List<Entity>>(
            text.Value,
            EntityJsonConverter.Instance
        );

        return new Array<Entity>(entities!);
    }

    /// <summary>
    /// Stream containing the Json data.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<StringStream> Stream { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory => FromJsonStepFactory.Instance;
}

/// <summary>
/// Create entities from a Json Stream
/// </summary>
public sealed class FromJsonStepFactory : SimpleStepFactory<FromJson, Array<Entity>>
{
    private FromJsonStepFactory() { }

    /// <summary>
    /// The Instance
    /// </summary>
    public static SimpleStepFactory<FromJson, Array<Entity>> Instance { get; } =
        new FromJsonStepFactory();
}

}
