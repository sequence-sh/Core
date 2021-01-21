using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Reads text from a file.
/// </summary>
[Alias("ReadFromFile")]
public sealed class FileRead : CompoundStep<StringStream>
{
    /// <inheritdoc />
    protected override async Task<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var path = await Path.Run(stateMonad, cancellationToken)
            .Map(async x => await x.GetStringAsync());

        if (path.IsFailure)
            return path.ConvertFailure<StringStream>();

        var encoding = await Encoding.Run(stateMonad, cancellationToken);

        if (encoding.IsFailure)
            return encoding.ConvertFailure<StringStream>();

        var decompress = await Decompress.Run(stateMonad, cancellationToken);

        if (decompress.IsFailure)
            return decompress.ConvertFailure<StringStream>();

        var result = stateMonad.FileSystemHelper.ReadFile(path.Value, decompress.Value)
            .MapError(x => x.WithLocation(this))
            .Map(x => new StringStream(x, encoding.Value)); //TODO fix

        return result;
    }

    /// <summary>
    /// The name of the file to read.
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Log(LogOutputLevel.Trace)]
    public IStep<StringStream> Path { get; set; } = null!;

    /// <summary>
    /// How the file is encoded.
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation("UTF8 no BOM")]
    public IStep<EncodingEnum> Encoding { get; set; } =
        new EnumConstant<EncodingEnum>(EncodingEnum.UTF8);

    /// <summary>
    /// Whether to decompress this string
    /// </summary>
    [StepProperty(3)]
    [DefaultValueExplanation("false")]
    public IStep<bool> Decompress { get; set; } = new BoolConstant(false);

    /// <inheritdoc />
    public override IStepFactory StepFactory => FileReadStepFactory.Instance;
}

/// <summary>
/// Reads text from a file.
/// </summary>
public sealed class FileReadStepFactory : SimpleStepFactory<FileRead, StringStream>
{
    private FileReadStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static SimpleStepFactory<FileRead, StringStream> Instance { get; } =
        new FileReadStepFactory();
}

}
