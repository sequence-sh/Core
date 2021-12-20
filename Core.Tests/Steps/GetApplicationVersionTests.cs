using Reductech.Sequence.ConnectorManagement.Base;

namespace Reductech.Sequence.Core.Tests.Steps;

public partial class
    GetApplicationVersionTests : StepTestBase<GetApplicationVersion, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            var entryAssembly = Assembly.GetEntryAssembly()!;

            var assemblyName = entryAssembly.GetName();

            Console.WriteLine(assemblyName);

            var expectedOutput =
                ConnectorSettings.DefaultForAssembly(entryAssembly).VersionString();

            //This will be the the ReSharper Test Runner

            yield return new StepCase(
                "Get Application Version",
                new GetApplicationVersion(),
                expectedOutput
            );
        }
    }
}
