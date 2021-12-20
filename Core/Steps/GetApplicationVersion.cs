using System.Diagnostics;

namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Gets the current version of the application
/// </summary>
public sealed class GetApplicationVersion : CompoundStep<StringStream>
{
    /// <inheritdoc />
    protected override async Task<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var entryAssembly = Assembly.GetEntryAssembly();

        if (entryAssembly is null)
            return new StringStream("Unknown Version");

        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(entryAssembly.Location);
        string          version         = fileVersionInfo.ProductVersion!;

        var name = entryAssembly.GetName().Name;

        var text = $"{name} {version}";

        return new StringStream(text);
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<GetApplicationVersion, StringStream>();
}
