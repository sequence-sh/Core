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
/// Write entities to a stream in Json format.
/// </summary>
public sealed class ToJsonArray : CompoundStep<StringStream>
{
    /// <inheritdoc />
    protected override async Task<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        //TODO maybe stream the result?
        var list = await Entities.Run(stateMonad, cancellationToken);

        if (list.IsFailure)
            return list.ConvertFailure<StringStream>();

        var formatResult = await FormatOutput.Run(stateMonad, cancellationToken);

        if (formatResult.IsFailure)
            return formatResult.ConvertFailure<StringStream>();

        var formatting = formatResult.Value ? Formatting.Indented : Formatting.None;

        var elements = await list.Value.GetElementsAsync(cancellationToken);

        if (elements.IsFailure)
            return elements.ConvertFailure<StringStream>();

        var jsonString = JsonConvert.SerializeObject(
            elements.Value,
            formatting,
            EntityJsonConverter.Instance
        );

        return new StringStream(jsonString);
    }

    /// <summary>
    /// The entities to write.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<Entity>> Entities { get; set; } = null!;

    /// <summary>
    /// Whether to format to the Json output
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation("true")]
    public IStep<bool> FormatOutput { get; set; } = new BoolConstant(true);

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<ToJsonArray, StringStream>();
}

}
