using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using NUnit.Framework;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.mutable;
using Reductech.EDR.Utilities.Processes.output;
using YamlDotNet.Serialization;
using Conditional = Reductech.EDR.Utilities.Processes.mutable.Conditional;

namespace Reductech.EDR.Utilities.Processes.Tests
{
    public class ConditionalTest
    {

        [Test]
        public void TestThenBranch()
        {
            var process = new Conditional
            {
                If = new Assertion
                {
                    Success = true
                } ,
                Then = new EmitProcess
                {
                    Term = "Yes"
                },
                Else = new EmitProcess
                {
                    Term = "No"
                }
            };

            var immutableProcess = process.TryFreeze(EmptySettings.Instance).AssertSuccess();
            var results = immutableProcess.ExecuteUntyped();

            var resultList = TestHelpers.AssertNoErrors(results).Result;

            CollectionAssert.Contains(resultList, "Yes");
        }

        [Test]
        public void TestElseBranch()
        {
            var process = new Conditional
            {
                If = new Assertion
                {
                    Success = false
                } ,
                Then = new EmitProcess
                {
                    Term = "Yes"
                },
                Else = new EmitProcess
                {
                    Term = "No"
                }
            };

            var immutableProcess = process.TryFreeze(EmptySettings.Instance).AssertSuccess();

            var results = immutableProcess.ExecuteUntyped();

            var resultList = TestHelpers.AssertNoErrors(results).Result;

            CollectionAssert.Contains(resultList, "No");
        }
    }

    public class Assertion : Process
    {
        /// <inheritdoc />
        public override string GetReturnTypeInfo() => nameof(Unit);
        public override string GetName() => Success.ToString();

        /// <inheritdoc />
        public override Result<ImmutableProcess, ErrorList> TryFreeze(IProcessSettings processSettings)
        {
            return Result.Success<ImmutableProcess, ErrorList>(new ImmutableAssertion(GetName(), Success));
        }


        [YamlMember]
        [Required]
        public bool Success { get; set; }
    }

    public class ImmutableAssertion : ImmutableProcess<Unit>
    {
        /// <inheritdoc />
        public ImmutableAssertion(string name, bool success) : base(name)
        {
            _success = success;
        }

        private readonly bool _success;

        /// <inheritdoc />
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async IAsyncEnumerable<IProcessOutput<Unit>> Execute()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (_success)
                yield return ProcessOutput<Unit>.Success(Unit.Instance);
            else yield return ProcessOutput<Unit>.Error("Assertion failed.");
        }
    }
}
