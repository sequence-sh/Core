using System;
using System.Collections.Generic;
using System.Linq;
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
                SCLSettings.EmptySettings,
                true
            );

            yield return new TestCase(
                "Requirement not met",
                new RequirementTestStep { RequirementStep = placeholder },
                SCLSettings.EmptySettings,
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

    private static SCLSettings CreateWidgetSettings(Version version, params string[] features)
    {
        string featuresString;

        if (features.Any())
            featuresString = ", \"features\": ["
                           + string.Join(", ", features.Select(f => $"\"{f}\"")) + "]";
        else
            featuresString = "";

        var connectorsString =
            $@"{{
  ""connectors"": {{
    ""widget"": {{
      ""version"": ""{version}""
      {featuresString}
    }}
  }}
}}";

        return SCLSettings.CreateFromString(connectorsString);
    }

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

        [StepProperty(4)]
        [DefaultValueExplanation("Nothing")]
        [RequiredVersion("Widget", null, null, null, "sprocket")]
        public IStep<bool>? RequiredFeatureStep { get; init; }

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
