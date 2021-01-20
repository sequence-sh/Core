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
/// Create an entity from an IDX Stream
/// </summary>
public sealed class FromIDX : CompoundStep<Entity>
{
    /// <inheritdoc />
    protected override async Task<Result<Entity, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var text = await Stream.Run(stateMonad, cancellationToken)
            .Map(x => x.GetStringAsync());

        if (text.IsFailure)
            return text.ConvertFailure<Entity>();

        var parser = new IdxParser(IdxParserConfiguration.Default);

        var parseResult = parser.TryParseEntity(text.Value);

        if (parseResult.IsFailure)
            return parseResult.ConvertFailure<Entity>().MapError(x => x.WithLocation(this));

        return parseResult.Value;
    }

    /// <summary>
    /// Stream containing the Json data.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<StringStream> Stream { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory => FromIDXStepFactory.Instance;
}

/// <summary>
/// Create an entity from an IDX Stream
/// </summary>
public sealed class FromIDXStepFactory : SimpleStepFactory<FromIDX, Entity>
{
    private FromIDXStepFactory() { }

    /// <summary>
    /// The Instance
    /// </summary>
    public static SimpleStepFactory<FromIDX, Entity> Instance { get; } = new FromIDXStepFactory();
}

}
