using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class DoNothingTests : StepTestBase<DoNothing, Unit>
    {
        /// <inheritdoc />
        public DoNothingTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Do nothing", new DoNothing(), Unit.Default);
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Do nothing", "DoNothing", Unit.Default);
            }

        }

        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases => new List<ErrorCase>();
    }
}