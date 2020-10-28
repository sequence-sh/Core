using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class ElementAtIndexTests : StepTestBase<ElementAtIndex<string>, string>
    {
        /// <inheritdoc />
        public ElementAtIndexTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Index 0",
                    new ElementAtIndex<string>()
                    {
                        Index = Constant(1),
                        Array = Array("Hello", "World")
                    }, "Hello");

                yield return new StepCase("Index 1",
                    new ElementAtIndex<string>()
                    {
                        Index = Constant(1),
                        Array = Array("Hello", "World")
                    }, "Hello");
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Index 0",
                    "ElementAtIndex(Array = ['Hello', 'World'], Index = 0", "Hello");

                yield return new DeserializeCase("Index 1",
                    "ElementAtIndex(Array = ['Hello', 'World'], Index = 1", "Hello");

            }
        }

        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases {
            get
            {
                yield return new ErrorCase("Index -1",
                    new ElementAtIndex<string>()
                    {
                        Index = Constant(1),
                        Array = Array("Hello", "World")
                    },
                    new ErrorBuilder("Index", ErrorCode.IndexOutOfBounds)
                    );

                yield return new ErrorCase("Index too big",
                    new ElementAtIndex<string>()
                    {
                        Index = Constant(2),
                        Array = Array("Hello", "World")
                    },
                    new ErrorBuilder("Index", ErrorCode.IndexOutOfBounds)
                    );


                yield return CreateDefaultErrorCase();
            } }
    }
}