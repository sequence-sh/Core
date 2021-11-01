using System;
using System.Collections.Generic;
using Moq;
using Reductech.EDR.ConnectorManagement.Base;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class GetSettingsTests : StepTestBase<GetSettings, Entity>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            var version = typeof(IStep).Assembly.GetName().Version!;

            var baseEntity =
                Entity.Create(
                    ("Connectors",
                     new Dictionary<string, object>()
                     {
                         {
                             "Reductech.EDR.Core",
                             new ConnectorSettings()
                             {
                                 Id      = "Reductech.EDR.Core",
                                 Version = version.ToString(3),
                                 Enable  = true
                             }
                         }
                     }
                    )
                );

            yield return new StepCase(
                "Default Settings",
                new Log<Entity> { Value = new GetSettings() },
                Unit.Default,
                baseEntity.Serialize()
            );

            var newConnectorSettings = new ConnectorSettings
            {
                Enable   = true,
                Id       = "MyConnector",
                Version  = new Version(1, 2, 3, 4).ToString(),
                Settings = new Dictionary<string, object>() { { "a", 1 }, { "b", 2 } }
            };

            var entity2 =
                Entity.Create(
                    ("Connectors",
                     new Dictionary<string, object>() { { "MyConnector", newConnectorSettings } }
                    )
                );

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var stepFactoryStore = StepFactoryStore.Create(
                mockRepository.OneOf<IExternalContext>(),
                mockRepository.OneOf<IRestClientFactory>(),
                new ConnectorData(
                    newConnectorSettings,
                    null
                )
            );

            yield return new StepCase(
                "Extra Settings",
                new Log<Entity> { Value = new GetSettings() },
                Unit.Default,
                entity2.Serialize()
            ).WithStepFactoryStore(stepFactoryStore);
        }
    }
}

}
