using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests;

/// <summary>
/// Makes sure all steps have a test class
/// </summary>
public partial class MetaTests : MetaTestsBase
{
    /// <inheritdoc />
    public override Assembly StepAssembly => typeof(IStep).Assembly;

    /// <inheritdoc />
    public override Assembly TestAssembly => typeof(MetaTests).Assembly;
}
