using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util.IDX;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Write an entity to a stream in IDX format.
/// </summary>
public sealed class ToIDX : CompoundStep<StringStream>
{
    /// <inheritdoc />
    protected override async Task<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        //TODO maybe stream the result?
        var entity = await Entity.Run(stateMonad, cancellationToken);

        if (entity.IsFailure)
            return entity.ConvertFailure<StringStream>();

        var toDocumentResult = await ConvertToDocument.Run(stateMonad, cancellationToken);

        if (toDocumentResult.IsFailure)
            return toDocumentResult.ConvertFailure<StringStream>();

        Result<string, IErrorBuilder> result;

        if (toDocumentResult.Value)
            result = entity.Value.TryConvertToIDXDocument();
        else
            result = entity.Value.TryConvertToIDXData();

        if (result.IsFailure)
            return result.MapError(x => x.WithLocation(this)).ConvertFailure<StringStream>();

        return new StringStream(result.Value);
    }

    /// <summary>
    /// The entity to write
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Entity> Entity { get; set; } = null!;

    /// <summary>
    /// True to convert to a document.
    /// False to convert to data.
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation("Convert to a document")]
    public IStep<bool> ConvertToDocument { get; set; } = new BoolConstant(true);

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<ToIDX, StringStream>();
}

}
