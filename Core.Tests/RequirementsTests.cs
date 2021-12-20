namespace Reductech.Sequence.Core.Tests;

[UseTestOutputHelper]
public partial class RequirementsTests
{
    public static readonly List<Requirement> BaseRequirements =
        new() { new ConnectorRequirement("Reductech.Sequence.Core.Tests") };

    [Fact]
    public void BasicStepShouldHaveNoRequirements()
    {
        var step = new Print() { Value = new Sum() { Terms = Array(1, 2, 3) } };

        var requirements = step.GetAllRequirements().ToList();

        requirements.Should().BeEmpty();
    }

    [Fact]
    public void FixedRequirementsStepShouldHaveRequirements()
    {
        var step = new FixedRequirementsStep();

        var requirements = step.GetAllRequirements().ToList();

        requirements.Should()
            .BeEquivalentTo(
                BaseRequirements.Append(
                    new VersionRequirement("Reductech.Sequence.Core.Tests", "FixedRequirement")
                )
            );
    }

    [Fact]
    public void FixedRequirementsStepShouldHaveRequirementsEvenWhenNested()
    {
        var step = new Print() { Value = new FixedRequirementsStep() };

        var requirements = step.GetAllRequirements().ToList();

        requirements.Should()
            .BeEquivalentTo(
                BaseRequirements.Append(
                    new VersionRequirement("Reductech.Sequence.Core.Tests", "FixedRequirement")
                )
            );
    }

    [Fact]
    public void RuntimeRequirementsStepShouldHaveRequirements()
    {
        var step = new RuntimeRequirementsStep { BaseStep = Constant(1) };

        var requirements = step.GetAllRequirements().ToList();

        requirements.Should()
            .BeEquivalentTo(
                BaseRequirements.Append(
                    new VersionRequirement(
                        "Reductech.Sequence.Core.Tests",
                        "MySoftware",
                        new Version("1.0.0"),
                        new Version("2.0.0")
                    )
                )
            );
    }

    [Fact]
    public void NestedRuntimeRequirementsStepShouldHaveRequirements()
    {
        var step = new Print() { Value = new RuntimeRequirementsStep() { BaseStep = Constant(1) } };

        var requirements = step.GetAllRequirements().ToList();

        requirements.Should()
            .BeEquivalentTo(
                BaseRequirements.Append(
                    new VersionRequirement(
                        "Reductech.Sequence.Core.Tests",
                        "MySoftware",
                        new Version("1.0.0"),
                        new Version("2.0.0")
                    )
                )
            );
    }

    private class RuntimeRequirementsStep : CompoundStep<SCLInt>
    {
        /// <inheritdoc />
        protected override Task<Result<SCLInt, IError>> Run(
            IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            return BaseStep.Run(stateMonad, cancellationToken);
        }

        [Required]
        [StepProperty(1)]
        [RequiredVersion(
            "MySoftware",
            minRequiredVersion: "1.0.0",
            maxRequiredVersion: "2.0.0"
        )]
        public IStep<SCLInt> BaseStep { get; set; } = null!;

        /// <inheritdoc />
        public override IStepFactory StepFactory =>
            new SimpleStepFactory<RuntimeRequirementsStep, SCLInt>();
    }

    private class FixedRequirementsStep : CompoundStep<SCLInt>
    {
        /// <inheritdoc />
        protected override async Task<Result<SCLInt, IError>> Run(
            IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return 1.ConvertToSCLObject();
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => FixedRequirementsStepFactory.Instance;

        private class
            FixedRequirementsStepFactory : SimpleStepFactory<FixedRequirementsStep, SCLInt>
        {
            private FixedRequirementsStepFactory() { }

            public static SimpleStepFactory<FixedRequirementsStep, SCLInt> Instance { get; } =
                new FixedRequirementsStepFactory();

            /// <inheritdoc />
            public override IEnumerable<Requirement> Requirements => base.Requirements.Prepend(
                new VersionRequirement("Reductech.Sequence.Core.Tests", "FixedRequirement")
            );
        }
    }
}
