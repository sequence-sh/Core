﻿using System.Diagnostics;

namespace Sequence.Core.Tests.Steps;

public partial class
    GetConnectorInformationTests : StepTestBase<GetConnectorInformation, Array<Entity>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            var coreAssembly = Assembly.GetAssembly(typeof(IStep));
            var fileVersion  = FileVersionInfo.GetVersionInfo(coreAssembly!.Location);

            var version = fileVersion.ProductVersion;

            var coreEntity = Entity.Create(
                ("Id", "Sequence.Core"),
                ("Version", version)
            ); //This version number need to be bumped

            yield return new StepCase(
                "Core",
                new GetConnectorInformation(),
                new[] { coreEntity }.ToSCLArray()
            );
        }
    }
}
