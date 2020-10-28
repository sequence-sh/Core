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
    public class AppendStringTests : StepTestBase<AppendString, Unit>
    {
        /// <inheritdoc />
        public AppendStringTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper) {}

        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases {
            get
            {
                yield return new ErrorCase("Variable does not exist",
                    new AppendString
                    {
                        Variable = new VariableName("Foo"),
                        String = Constant("World")
                    }, new ErrorBuilder("Variable '<Foo>' does not exist.", ErrorCode.MissingVariable)
                );
            } }

        /// <inheritdoc />
        protected override IEnumerable<SerializeCase> SerializeCases {
            get
            {
                yield return CreateDefaultSerializeCase();

                //yield return new SerializeCase("Test Serialize", new AppendString()
                //{
                //    Variable = new VariableName("Hello"),
                //    String = Constant("World")
                //}, "AppendString(String = 'World', Variable = <Hello>)" );
            } }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases {
            get
            {
                yield return new StepCase("Append string to existing variable",
                    new AppendString
                    {
                        Variable = new VariableName("Foo"),
                        String = Constant("World")
                    }, Unit.Default)
                    .WithInitialState("Foo", "Hello")
                    .WithExpectedFinalState("Foo", "HelloWorld");
            } }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Short Form",
                    "AppendString(String = 'World', Variable = <Foo>)", Unit.Default)
                    .WithInitialState("Foo", "Hello")
                    .WithExpectedFinalState("Foo", "HelloWorld");

            }
        }
    }
}