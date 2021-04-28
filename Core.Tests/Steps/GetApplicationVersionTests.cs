using System;
using System.Collections.Generic;
using System.Reflection;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps
{

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
                ConnectorInformation.TryCreate(
                    entryAssembly,
                    ConnectorSettings.DefaultForAssembly(entryAssembly)
                )!.ToString();

            //This will be the the ReSharper Test Runner

            yield return new StepCase(
                "Get Application Version",
                new GetApplicationVersion(),
                expectedOutput
            );
        }
    }
}

}
