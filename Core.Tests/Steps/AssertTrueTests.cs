using System.Collections.Generic;
using JetBrains.Annotations;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class AssertTrueTests : StepTestBase<AssertTrue, Unit>
    {
        /// <inheritdoc />
        public AssertTrueTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases {
            get
            {
                yield return new DeserializeCase("Is true true",
                    "AssertTrue(Boolean: true)",
                    Unit.Default
                );
            } }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Is true true",
                    new AssertTrue
                    {
                        Boolean = Constant(true)
                    }, Unit.Default
                );
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases
        {
            get
            {
                yield return new ErrorCase("Failed Assertion",
                    new AssertTrue
                    {
                        Boolean = Constant(false)
                    },
                    new ErrorBuilder($"Assertion Failed 'False'", ErrorCode.IndexOutOfBounds)
                );

                yield return CreateDefaultErrorCase();
            }
        }
    }
}