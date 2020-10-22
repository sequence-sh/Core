using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Serialization;
using Reductech.EDR.Core.Util;
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



                yield return new DeserializationErrorCase(@"
- <ArrayVar1> = Array(Elements = ['abc', '123'])
- <ArrayVar2> = Array(Elements = ['abc', '123'])
- Print(Value = (<ArrayVar1> == <ArrayVar2>))", "Cannot compare objects of type 'ListOfString'");




                yield return new DeserializationErrorCase("MyMegaFunction(Value = true)", "(Line: 1, Col: 1, Idx: 0) - (Line: 1, Col: 29, Idx: 28): Could not parse 'MyMegaFunction(Value = true)'");


                yield return new DeserializationErrorCase(@"- >
  ForEach(
    Array = ['a','b','c'],
    VariableName = <char>,
    Action = Print(Value = <char>)
  )",
@"'ForEach( Array = ['a','b','c'], VariableName = <char>, Action = Print(Value = <char>) )' is a 'String' but it should be a 'Unit' to be a member of 'Sequence'");

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

                var result = YamlMethods.DeserializeFromYaml(Name, sfs)
                    .Bind(x=>x.TryFreeze())
                    .MapError(x=>x.AsString);

                result.ShouldBeFailure(_expectedError);
            }
        }

    }
}
