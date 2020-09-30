using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Serialization;
using Reductech.Utilities.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests
{

    public class DeserializationErrorTests : DeserializationErrorTestCases
    {
        public DeserializationErrorTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;


        /// <inheritdoc />
        [Theory]
        [ClassData(typeof(DeserializationErrorTestCases))]
        public override void Test(string key) => base.Test(key);
    }

    public class DeserializationErrorTestCases : TestBase
    {
        /// <inheritdoc />
        protected override IEnumerable<ITestBaseCase> TestCases
        {
            get
            {
                yield return new DeserializationErrorCase("", "Yaml is empty.");

                yield return new DeserializationErrorCase("MyMegaFunction(Value = true)", "(Line: 1, Col: 1, Idx: 0) - (Line: 1, Col: 29, Idx: 28): Could not parse 'MyMegaFunction(Value = true)'");
            }
        }


        private class DeserializationErrorCase : ITestBaseCase
        {
            private readonly string _expectedError;

            public DeserializationErrorCase(string yaml, string expectedError)
            {
                Name = yaml;
                _expectedError = expectedError;
            }

            /// <inheritdoc />
            public string Name { get; }

            /// <inheritdoc />
            public void Execute(ITestOutputHelper testOutputHelper)
            {
                var sfs = StepFactoryStore.CreateUsingReflection(typeof(IFreezableStep));

                var result = YamlMethods.DeserializeFromYaml(Name, sfs);

                result.ShouldBeFailure(_expectedError);
            }
        }

    }
}
