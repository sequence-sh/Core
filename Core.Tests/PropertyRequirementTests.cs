using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoTheory;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests
{

public partial class PropertyRequirementTests
{
    [GenerateTheory("RequirementTests")]
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
        }
    }

    private record TestCase(
        string Name,
        RequirementTestStep Step,
        SCLSettings Settings,
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

    private static SCLSettings CreateWidgetSettings(Version version)
    {
        var map = new SCLSettings(
            new SCLSettingsValue.Map(
                new Dictionary<string, SCLSettingsValue>(StringComparer.OrdinalIgnoreCase)
                {
                    {
                        "widget",
                        new SCLSettingsValue.Map(
                            new Dictionary<string, SCLSettingsValue>(
                                StringComparer.OrdinalIgnoreCase
                            )
                            {
                                {
                                    SCLSettings.VersionKey,
                                    new SCLSettingsValue.Primitive(version.ToString())
                                }
                            }
                        )
                    }
                }
            )
        );

        return map;
    }

    //private record WidgetSettings(Version Version) : ISettings
    //{
    //    /// <inheritdoc />
    //    public Result<Unit, IErrorBuilder> CheckRequirement(Requirement requirement)
    //    {
    //        if (!requirement.Name.Equals("widget", StringComparison.OrdinalIgnoreCase))
    //            return ErrorCode.RequirementNotMet.ToErrorBuilder(requirement);

    //        if (requirement.MinVersion != null && Version < requirement.MinVersion)
    //            return ErrorCode.RequirementNotMet.ToErrorBuilder(requirement);

    //        if (requirement.MaxVersion != null && Version > requirement.MaxVersion)
    //            return ErrorCode.RequirementNotMet.ToErrorBuilder(requirement);

    //        return Unit.Default;
    //    }

    //    /// <inheritdoc />
    //    public override string ToString() => $"Widget {Version}";
    //}

    private class RequirementTestStep : CompoundStep<bool>
    {
        /// <inheritdoc />
        protected override async Task<Result<bool, IError>> Run(
            IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return true;
        }

        [StepProperty(1)]
        [DefaultValueExplanation("Nothing")]
        [RequiredVersion("Widget", null)]
        public IStep<bool>? RequirementStep { get; init; }

        [StepProperty(2)]
        [DefaultValueExplanation("Nothing")]
        [RequiredVersion("Widget", "2.0")]
        public IStep<bool>? MinVersionStep { get; init; }

        [StepProperty(3)]
        [DefaultValueExplanation("Nothing")]
        [RequiredVersion("Widget", null, "5.0")]
        public IStep<bool>? MaxVersionStep { get; init; }

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
