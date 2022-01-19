using Reductech.Sequence.ConnectorManagement.Base;

namespace Reductech.Sequence.Core.Tests.Steps;

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
                             "Reductech.Sequence.Core",
                             new ConnectorSettings()
                             {
                                 Id      = "Reductech.Sequence.Core",
                                 Version = version.ToString(3),
                                 Enable  = true
                             }
                         }
                     }
                    )
                );

            yield return new StepCase(
                "Default Settings",
                new Log { Value = new GetSettings() },
                Unit.Default,
                baseEntity.Serialize(SerializeOptions.Serialize)
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

            var stepFactoryStore = StepFactoryStore.TryCreate(
                    mockRepository.OneOf<IExternalContext>(),
                    new ConnectorData(
                        newConnectorSettings,
                        null
                    )
                )
                .GetOrThrow();

            yield return new StepCase(
                "Extra Settings",
                new Log { Value = new GetSettings() },
                Unit.Default,
                entity2.Serialize(SerializeOptions.Serialize)
            ).WithStepFactoryStore(stepFactoryStore);
        }
    }
}
