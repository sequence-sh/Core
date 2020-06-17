using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using NUnit.Framework;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.mutable;
using Reductech.EDR.Utilities.Processes.mutable.chain;
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
                If = new ReturnBool
                {
                    Value = true
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

            var immutableProcess = process.TryFreeze<Unit>(EmptySettings.Instance).AssertSuccess();
            var results = immutableProcess.Execute();

            var resultList = TestHelpers.AssertNoErrors(results).Result;

            CollectionAssert.Contains(resultList, "Yes");
        }

        [Test]
        public void TestElseBranch()
        {
            var process = new Conditional
            {
                If = new ReturnBool
                {
                    Value = false
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

            var immutableProcess = process.TryFreeze<Unit>(EmptySettings.Instance).AssertSuccess();

            var results = immutableProcess.Execute();

            var resultList = TestHelpers.AssertNoErrors(results).Result;

            CollectionAssert.Contains(resultList, "No");
        }
    }

    public class ReturnBool : Process
    {
        /// <inheritdoc />
        public override string GetReturnTypeInfo() => nameof(Boolean);
        public override string GetName() => Value.ToString();

        /// <inheritdoc />
        public override Result<ImmutableProcess<TFinal>> TryFreeze<TFinal>(IProcessSettings processSettings)
        {
            return TryConvertFreezeResult<TFinal, bool>(new ImmutableReturnBool(Value));
        }


        [YamlMember]
        [Required]
        public bool Value { get; set; }

        /// <inheritdoc />
        public override IEnumerable<string> GetRequirements()
        {
            yield break;
        }

        /// <inheritdoc />
        public override Result<ChainLinkBuilder<TInput, TFinal>> TryCreateChainLinkBuilder<TInput, TFinal>()
        {
            throw new NotImplementedException();
        }
    }

    public class ImmutableReturnBool : ImmutableProcess<bool>
    {
        /// <inheritdoc />
        public ImmutableReturnBool(bool value)
        {
            _value = value;
        }

        private readonly bool _value;

        /// <inheritdoc />
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async IAsyncEnumerable<IProcessOutput<bool>> Execute()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
                yield return ProcessOutput<bool>.Success(_value);
        }

        /// <inheritdoc />
        public override string Name => _value.ToString();

        /// <inheritdoc />
        public override IProcessConverter? ProcessConverter => null;
    }
}
