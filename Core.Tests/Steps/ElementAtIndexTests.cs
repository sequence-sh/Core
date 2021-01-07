using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class ElementAtIndexTests : StepTestBase<ElementAtIndex<StringStream>, StringStream>
    {
        /// <inheritdoc />
        public ElementAtIndexTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) {}

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Index 0",
                    new ElementAtIndex<StringStream>
                    {
                        Index = Constant(0),
                        Array = Array(("Hello"), ("World") )
                    }, "Hello");

                yield return new StepCase("Index 1",
                    new ElementAtIndex<StringStream>
                    {
                        Index = Constant(1),
                        Array = Array( ("Hello"),  ("World"))
                    }, "World");
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Index 0",
                    "ElementAtIndex Array: ['Hello', 'World'] Index: 0", "Hello");

                yield return new DeserializeCase("Index 1",
                    "ElementAtIndex Array: ['Hello', 'World'] Index: 1", "World");

            }
        }

        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases {
            get
            {
                yield return new ErrorCase("Index -1",
                    new ElementAtIndex<StringStream>
                    {
                        Index = Constant(-1),
                        Array = Array( ("Hello"),  ("World"))
                    },
                    new ErrorBuilder( ErrorCode.IndexOutOfBounds)
                    );

                yield return new ErrorCase("Index too big",
                    new ElementAtIndex<StringStream>
                    {
                        Index = Constant(2),
                        Array = Array( ("Hello"),  ("World"))
                    },
                    new ErrorBuilder( ErrorCode.IndexOutOfBounds)
                    );


                foreach (var errorCase in base.ErrorCases)
                    yield return errorCase;
            } }
    }
}