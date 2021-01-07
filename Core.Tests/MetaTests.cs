using System.Reflection;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests
{

/// <summary>
/// Makes sure all steps have a test class
/// </summary>
public class MetaTests : MetaTestsBase
{
    /// <inheritdoc />
    public override Assembly StepAssembly => typeof(IStep).Assembly;

    /// <inheritdoc />
    public override Assembly TestAssembly => typeof(MetaTests).Assembly;
}

}
