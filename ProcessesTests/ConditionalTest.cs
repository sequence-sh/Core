using FluentAssertions;
using NUnit.Framework;
using Reductech.EDR.Processes.Mutable;
using Reductech.EDR.Processes.Mutable.Chain;
using Reductech.EDR.Processes.Mutable.Injections;
using Conditional = Reductech.EDR.Processes.Mutable.Conditional;

namespace Reductech.EDR.Processes.Tests
{
    public class ChainTest
    {
        [Test]
        public void TestEmptyChain()
        {
            var process = new Chain();

            process.TryFreeze<Unit>(EmptySettings.Instance).AssertFailure();
        }

        [Test]
        public void TestSingleElementUnitChain()
        {
            var process = new Chain
            {
                Process = new EmitProcess{Term = "Yes"}
            };

            var immutableProcess = process.TryFreeze<Unit>(EmptySettings.Instance).AssertSuccess();
            var results = immutableProcess.Execute();

            var resultList = TestHelpers.AssertNoErrors(results).Result;

            resultList.Should().Contain(x=>x.Contains("Yes"));
        }

        [Test]
        public void TestSingleElementStringChain()
        {
            var process = new Chain
            {
                Process = new CSVReaderTest.EmitStringProcess{Output = "Yes"}
            };

            var immutableProcess = process.TryFreeze<string>(EmptySettings.Instance).AssertSuccess();
            var results = immutableProcess.Execute();

            var resultList = TestHelpers.AssertNoErrors(results).Result;

            resultList.Should().Contain(x=>x.Contains("Yes"));
        }

        [Test]
        public void TestTwoElementUnitChain()
        {
            var process = new Chain
            {
                Process = new CSVReaderTest.EmitStringProcess{Output = "Yes"},

                Into = new ChainLink
                {
                    Inject = new Injection
                    {
                        Property = nameof(EmitProcess.Term),
                        Regex = @"\w",
                        Template = "$1ou are welcome"
                    },
                    Process = new EmitProcess{Term = "No"}
                }

            };

            var immutableProcess = process.TryFreeze<Unit>(EmptySettings.Instance).AssertSuccess();
            var results = immutableProcess.Execute();

            var resultList = TestHelpers.AssertNoErrors(results).Result;

            resultList.Should().Contain(x=> x.Contains("You are welcome"));
        }

        [Test]
        public void TestTwoElementStringChain()
        {
            var process = new Chain
            {
                Process = new CSVReaderTest.EmitStringProcess{Output = "Yes"},

                Into = new ChainLink
                {
                    Inject = new Injection
                    {
                        Property = nameof(CSVReaderTest.EmitStringProcess.Output),
                        Regex = @"\w",
                        Template = "$1ou are welcome"
                    },
                    Process = new CSVReaderTest.EmitStringProcess{Output = "No"}
                }
            };

            var immutableProcess = process.TryFreeze<string>(EmptySettings.Instance).AssertSuccess();
            var results = immutableProcess.Execute();

            var resultList = TestHelpers.AssertNoErrors(results).Result;

            resultList.Should().Contain(x=>x.Contains("You are welcome"));
        }
    }


    public class ConditionalTest
    {

        [Test]
        public void TestThenBranch()
        {
            var process = new Conditional
            {
                If = new ReturnBool
                {
                    ResultBool = true
                },
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
                    ResultBool = false
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
}
