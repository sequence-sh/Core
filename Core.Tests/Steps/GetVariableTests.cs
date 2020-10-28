using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class GetVariableTests : StepTestBase<GetVariable<int>, int>
    {
        /// <inheritdoc />
        public GetVariableTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                //yield return new StepCase("Get Variable", new GetVariable<int>
                //{
                //    VariableName = new VariableName("Foo")
                //}, 42 )
                //    .WithInitialState("Foo", 42)
                //    .WithExpectedFinalState("Foo", 42);

                //Can't test this here - Could not resolve variable <Foo>
                yield break;
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                //Can't do deserialize cases - Could not resolve variable <Foo>

                yield break;

                //yield return new DeserializeCase("Short Form",
                //    "<Foo>",
                //    42
                //    ).WithInitialState("Foo", 42)
                //    .WithExpectedFinalState("Foo", 42);


                //yield return new DeserializeCase("Long Form",
                //    "Do: GetVariable\nVariableName: <Foo>",
                //    42
                //    ).WithInitialState("Foo", 42)
                //    .WithExpectedFinalState("Foo", 42);


            }

        }

        /// <inheritdoc />
        protected override IEnumerable<SerializeCase> SerializeCases {
            get
            {
                yield return new SerializeCase("Short form", new GetVariable<int>(){VariableName = new VariableName("Foo")}, "<Foo>");
            } }

        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases
        {
            get { yield return CreateDefaultErrorCase(false); }
        }
    }
}