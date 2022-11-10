using System.Diagnostics;

namespace Sequence.Core.Steps;

/// <summary>
/// Gets the current version of the application
/// </summary>
public sealed class GetApplicationVersion : CompoundStep<StringStream>
{
    /// <inheritdoc />
    #pragma warning disable CS1998
    protected override async ValueTask<Result<StringStream, IError>> Run(
        #pragma warning restore CS1998
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var entryAssembly = Assembly.GetEntryAssembly();

        if (entryAssembly is null)
            return new StringStream("Unknown Version");

        var fileVersionInfo = FileVersionInfo.GetVersionInfo(entryAssembly.Location);
        var version         = fileVersionInfo.ProductVersion!;

        var name = entryAssembly.GetName().Name;

        var text = $"{name} {version}";

        return new StringStream(text);
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<GetApplicationVersion, StringStream>();
}
