using Reductech.EDR.Core.ExternalProcesses;
using Thinktecture;
using Thinktecture.Adapters;

namespace Reductech.EDR.Core.Abstractions
{

/// <summary>
/// The external context of a sequence.
/// </summary>
public sealed record ExternalContext(
    IFileSystem FileSystemHelper,
    IExternalProcessRunner ExternalProcessRunner,
    IConsole Console) : IExternalContext
{
    /// <summary>
    /// The real external context
    /// </summary>
    public static readonly ExternalContext Default = new(
        FileSystemAdapter.Default,
        ExternalProcesses.ExternalProcessRunner.Instance,
        new ConsoleAdapter()
    );
}

}
