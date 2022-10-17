using Reductech.Sequence.ConnectorManagement.Base;

namespace Reductech.Sequence.Core.Tests;

public partial class PropertyRequirementTests
{
    [GenerateTheory("RequirementTests")]
    private IEnumerable<TestCase> TestCases
    {
        get
        {
            var placeholder = new SCLConstant<SCLBool>(SCLBool.True);

            yield return new TestCase(
                "No Requirement",
                new RequirementTestStep(),
                null,
                true
            );

            yield return new TestCase(
                "Requirement not met",
                new RequirementTestStep { RequirementStep = placeholder },
                null,
                false
            );

            yield return new TestCase(
                "Requirement met",
                new RequirementTestStep { RequirementStep = placeholder },
                CreateWidgetSettings(new Version(1, 0)),
                true
            );

            yield return new TestCase(
                "Version below min version",
                new RequirementTestStep { MinVersionStep = placeholder },
                CreateWidgetSettings(new Version(1, 0)),
                false
            );

            yield return new TestCase(
                "Version above min version",
                new RequirementTestStep { MinVersionStep = placeholder },
                CreateWidgetSettings(new Version(3, 0)),
                true
            );

            yield return new TestCase(
                "Version above max version",
                new RequirementTestStep { MaxVersionStep = placeholder },
                CreateWidgetSettings(new Version(6, 0)),
                false
            );

            yield return new TestCase(
                "Version below max version",
                new RequirementTestStep { MaxVersionStep = placeholder },
                CreateWidgetSettings(new Version(4, 0)),
                true
            );

            yield return new TestCase(
                "All requirements met",
                new RequirementTestStep
                {
                    MaxVersionStep  = placeholder,
                    MinVersionStep  = placeholder,
                    RequirementStep = placeholder
                },
                CreateWidgetSettings(new Version(5, 0)),
                true
            );

            yield return new TestCase(
                "No Features",
                new RequirementTestStep { RequiredFeatureStep = placeholder },
                CreateWidgetSettings(new Version(1, 0)),
                false
            );

            yield return new TestCase(
                "Wrong Feature",
                new RequirementTestStep { RequiredFeatureStep = placeholder },
                CreateWidgetSettings(new Version(1, 0), "Kludge"),
                false
            );

            yield return new TestCase(
                "Right Features",
                new RequirementTestStep { RequiredFeatureStep = placeholder },
                CreateWidgetSettings(new Version(1, 0), "sprocket"),
                true
            );
        }
    }

    private record TestCase(
        string Name,
        RequirementTestStep Step,
        ConnectorSettings? ConnectorSettings,
        bool ExpectSuccess) : ITestInstance
    {
        /// <inheritdoc />
        public void Run(ITestOutputHelper testOutputHelper)
        {
            ConnectorData[] connectorData;

            if (ConnectorSettings is null)
                connectorData = System.Array.Empty<ConnectorData>();
            else
            {
                connectorData = new[] { new ConnectorData(ConnectorSettings, null) };
            }

            var mockRepository = new MockRepository(MockBehavior.Strict);

            var sfs = StepFactoryStore.TryCreate(
                    mockRepository.OneOf<IExternalContext>(),
                    connectorData
                )
                .GetOrThrow();

            var r = Step.Verify(sfs);

            if (ExpectSuccess)
                r.ShouldBeSuccessful();
            else
                r.ShouldBeFailure();
        }
    }

    private static ConnectorSettings CreateWidgetSettings(Version version, params string[] features)
    {
        var connectorSettings =
            new ConnectorSettings
            {
                Id      = "Reductech.Sequence.Core.Tests",
                Version = new Version(1, 0).ToString(),
                Enable  = true,
                Settings =
                    new Dictionary<string, object>
                    {
                        { RequirementTestStep.VersionKey, version },
                        { RequirementTestStep.FeaturesKey, features },
                    }
            };

        return connectorSettings;
    }

    private class RequirementTestStep : CompoundStep<SCLBool>
    {
        public const string VersionKey = "WidgetVersion";
        public const string FeaturesKey = "WidgetFeatures";

        /// <inheritdoc />
        protected override async ValueTask<Result<SCLBool, IError>> Run(
            IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return SCLBool.True;
        }

        [StepProperty(1)]
        [DefaultValueExplanation("Nothing")]
        [RequiredVersion(VersionKey, null)]
        public IStep<SCLBool>? RequirementStep { get; init; }

        [StepProperty(2)]
        [DefaultValueExplanation("Nothing")]
        [RequiredVersion(VersionKey, "2.0")]
        public IStep<SCLBool>? MinVersionStep { get; init; }

        [StepProperty(3)]
        [DefaultValueExplanation("Nothing")]
        [RequiredVersion(VersionKey, null, "5.0")]
        public IStep<SCLBool>? MaxVersionStep { get; init; }

        [StepProperty(4)]
        [DefaultValueExplanation("Nothing")]
        [RequiredFeature(FeaturesKey, "sprocket")]
        public IStep<SCLBool>? RequiredFeatureStep { get; init; }

        /// <inheritdoc />
        public override IStepFactory StepFactory => RequirementTestStepFactory.Instance;
    }

    private class RequirementTestStepFactory : SimpleStepFactory<RequirementTestStep, SCLBool>
    {
        private RequirementTestStepFactory() { }

        public static SimpleStepFactory<RequirementTestStep, SCLBool> Instance { get; } =
            new RequirementTestStepFactory();
    }
}
