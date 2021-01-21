using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoTheory;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Reductech.EDR.Core.Tests
{

public partial class PropertyRequirementTests
{
    [AutoTheory.GenerateTheory("RequirementTests")]
    private IEnumerable<TestCase> TestCases
    {
        get
        {
            var placeholder = new BoolConstant(true);

            yield return new TestCase(
                "No Requirement",
                new RequirementTestStep(),
                EmptySettings.Instance,
                true
            );

            yield return new TestCase(
                "Requirement not met",
                new RequirementTestStep { RequirementStep = placeholder },
                EmptySettings.Instance,
                false
            );

            yield return new TestCase(
                "Requirement met",
                new RequirementTestStep { RequirementStep = placeholder },
                new WidgetSettings(new Version(1, 0)),
                true
            );

            yield return new TestCase(
                "Version below min version",
                new RequirementTestStep { MinVersionStep = placeholder },
                new WidgetSettings(new Version(1, 0)),
                false
            );

            yield return new TestCase(
                "Version above min version",
                new RequirementTestStep { MinVersionStep = placeholder },
                new WidgetSettings(new Version(3, 0)),
                true
            );

            yield return new TestCase(
                "Version above max version",
                new RequirementTestStep { MaxVersionStep = placeholder },
                new WidgetSettings(new Version(6, 0)),
                false
            );

            yield return new TestCase(
                "Version below max version",
                new RequirementTestStep { MaxVersionStep = placeholder },
                new WidgetSettings(new Version(4, 0)),
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
                new WidgetSettings(new Version(5, 0)),
                true
            );
        }
    }

    private record TestCase(
        string Name,
        RequirementTestStep Step,
        ISettings Settings,
        bool ExpectSuccess) : ITestInstance
    {
        /// <inheritdoc />
        public void Run(ITestOutputHelper testOutputHelper)
        {
            var r = Step.Verify(Settings);

            if (ExpectSuccess)
                r.ShouldBeSuccessful(x => x.AsString);
            else
                r.ShouldBeFailure();
        }
    }

    private record WidgetSettings(Version Version) : ISettings
    {
        /// <inheritdoc />
        public Result<Unit, IErrorBuilder> CheckRequirement(Requirement requirement)
        {
            if (!requirement.Name.Equals("widget", StringComparison.OrdinalIgnoreCase))
                return ErrorCode.RequirementNotMet.ToErrorBuilder(requirement);

            if (requirement.MinVersion != null && Version < requirement.MinVersion)
                return ErrorCode.RequirementNotMet.ToErrorBuilder(requirement);

            if (requirement.MaxVersion != null && Version > requirement.MaxVersion)
                return ErrorCode.RequirementNotMet.ToErrorBuilder(requirement);

            return Unit.Default;
        }

        /// <inheritdoc />
        public override string ToString() => $"Widget {Version}";
    }

    private class RequirementTestStep : CompoundStep<bool>
    {
        /// <inheritdoc />
        protected override async Task<Result<bool, IError>> Run(
            IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            return true;
        }

        [StepProperty(1)]
        [DefaultValueExplanation("Nothing")]
        [RequiredVersion("Widget", null)]
        public IStep<bool>? RequirementStep { get; set; } = null;

        [StepProperty(2)]
        [DefaultValueExplanation("Nothing")]
        [RequiredVersion("Widget", "2.0")]
        public IStep<bool>? MinVersionStep { get; set; } = null;

        [StepProperty(3)]
        [DefaultValueExplanation("Nothing")]
        [RequiredVersion("Widget", null, "5.0")]
        public IStep<bool>? MaxVersionStep { get; set; } = null;

        /// <inheritdoc />
        public override IStepFactory StepFactory => RequirementTestStepFactory.Instance;
    }

    private class RequirementTestStepFactory : SimpleStepFactory<RequirementTestStep, bool>
    {
        private RequirementTestStepFactory() { }

        public static SimpleStepFactory<RequirementTestStep, bool> Instance { get; } =
            new RequirementTestStepFactory();
    }
}

}
