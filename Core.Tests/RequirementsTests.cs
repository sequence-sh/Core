using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Xunit;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests
{

[AutoTheory.UseTestOutputHelper]
public partial class RequirementsTests
{
    public static readonly List<Requirement> BaseRequirements =
        new() { new ConnectorRequirement("Reductech.EDR.Core.Tests") };

    [Fact]
    public void BasicStepShouldHaveNoRequirements()
    {
        var step = new Print<int>() { Value = new Sum() { Terms = Array(1, 2, 3) } };

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
                    new VersionRequirement("Reductech.EDR.Core.Tests", "FixedRequirement")
                )
            );
    }

    [Fact]
    public void FixedRequirementsStepShouldHaveRequirementsEvenWhenNested()
    {
        var step = new Print<int>() { Value = new FixedRequirementsStep() };

        var requirements = step.GetAllRequirements().ToList();

        requirements.Should()
            .BeEquivalentTo(
                BaseRequirements.Append(
                    new VersionRequirement("Reductech.EDR.Core.Tests", "FixedRequirement")
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
                        "Reductech.EDR.Core.Tests",
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
        var step = new Print<int>()
        {
            Value = new RuntimeRequirementsStep() { BaseStep = Constant(1) }
        };

        var requirements = step.GetAllRequirements().ToList();

        requirements.Should()
            .BeEquivalentTo(
                BaseRequirements.Append(
                    new VersionRequirement(
                        "Reductech.EDR.Core.Tests",
                        "MySoftware",
                        new Version("1.0.0"),
                        new Version("2.0.0")
                    )
                )
            );
    }

    private class RuntimeRequirementsStep : CompoundStep<int>
    {
        /// <inheritdoc />
        protected override Task<Result<int, IError>> Run(
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
        public IStep<int> BaseStep { get; set; } = null!;

        /// <inheritdoc />
        public override IStepFactory StepFactory =>
            new SimpleStepFactory<RuntimeRequirementsStep, int>();
    }

    private class FixedRequirementsStep : CompoundStep<int>
    {
        /// <inheritdoc />
        protected override async Task<Result<int, IError>> Run(
            IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return 1;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => FixedRequirementsStepFactory.Instance;

        private class
            FixedRequirementsStepFactory : SimpleStepFactory<FixedRequirementsStep, int>
        {
            private FixedRequirementsStepFactory() { }

            public static SimpleStepFactory<FixedRequirementsStep, int> Instance { get; } =
                new FixedRequirementsStepFactory();

            /// <inheritdoc />
            public override IEnumerable<Requirement> Requirements => base.Requirements.Prepend(
                new VersionRequirement("Reductech.EDR.Core.Tests", "FixedRequirement")
            );
        }
    }
}

}
