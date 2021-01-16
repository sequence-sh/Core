using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoTheory;
using FluentAssertions;
using MELT;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Parser;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests
{

public partial class DeserializationTests
{
    [GenerateAsyncTheory("Deserialize")]
    public IEnumerable<DeserializationTestInstance> TestCases
    {
        get
        {
            yield return new DeserializationTestInstance(
                @"- <Foo> = 'Hello World'
- <Bar> = <Foo>
- Print <Bar>",
                "Hello World"
            );

            yield return new DeserializationTestInstance(
                @"Print 'Mark''s string'",
                "Mark's string"
            );

            yield return new DeserializationTestInstance(
                @"Print ""Mark's string""",
                "Mark's string"
            );

            yield return new DeserializationTestInstance(
                @"Print 'Comments!' #This is a comment",
                "Comments!"
            );

            yield return new DeserializationTestInstance(
                @"#This is a comment
Print 'Comments!'",
                "Comments!"
            );

            yield return new DeserializationTestInstance(
                @"/*This is a comment
block*/
Print 'Comments!'",
                "Comments!"
            );

            yield return new DeserializationTestInstance(
                @"Print /*Comment Block */ 'Comments!' ",
                "Comments!"
            );

            yield return new DeserializationTestInstance(@"Print (2 * 3)", 6);

            yield return new DeserializationTestInstance(@"Print(3 - 2)", 1);

            yield return new DeserializationTestInstance(@"print(2 * 3)", 6);

            yield return new DeserializationTestInstance(@"print(6 / 3)", 2);

            yield return new DeserializationTestInstance(@"print(6 ^ 2)", 36);

            yield return new DeserializationTestInstance(@"print(7 % 2)", 1);

            yield return
                new DeserializationTestInstance(
                    @"Print 'falsetto'",
                    "falsetto"
                ); //check 'false' delimiter

            yield return
                new DeserializationTestInstance(
                    @"Print 'notable'",
                    "notable"
                ); //check 'not' delimiter

            yield return new DeserializationTestInstance(@"print(2*3)", 6);

            yield return new DeserializationTestInstance(@"Print(2 ^ 3)", 8);

            yield return new DeserializationTestInstance(@"Print(not True)", false);

            yield return new DeserializationTestInstance(
                @"Print 2020-11-22T20:30:40.0000000",
                "2020-11-22T20:30:40.0000000"
            );

            yield return new DeserializationTestInstance(
                @"Print 2020-11-22",
                "2020-11-22T00:00:00.0000000"
            );

            yield return new DeserializationTestInstance(
                @"Print 2020-11-22T20:30:40",
                "2020-11-22T20:30:40.0000000"
            );

            yield return new DeserializationTestInstance(
                @"Print(stringToCase 'hello' textCase.Upper)",
                "HELLO"
            );

            yield return new DeserializationTestInstance(
                @"Print(stringToCase 'hello' 'Upper')",
                "HELLO"
            );

            yield return new DeserializationTestInstance(@"Print(2 >= 3)", false);
            yield return new DeserializationTestInstance(@"Print(4 >= 3)", true);
            yield return new DeserializationTestInstance(@"Print(3 >= 3)", true);

            yield return new DeserializationTestInstance(@"Print(3 > 3)", false);
            yield return new DeserializationTestInstance(@"Print(4 > 3)", true);
            yield return new DeserializationTestInstance(@"Print(3 < 3)", false);

            yield return new DeserializationTestInstance(@"Print(3 <= 3)", true);

            yield return new DeserializationTestInstance(@"Print(2 * (3 + 4))", 14);
            yield return new DeserializationTestInstance(@"Print((2 * 3) + 4)", 10);

            yield return new DeserializationTestInstance(@"Print((2 >= 3))", false);

            yield return new DeserializationTestInstance(@"Print((2 * (3 + 4)))", 14);
            yield return new DeserializationTestInstance(@"Print((2*(3+4)))",     14);
            yield return new DeserializationTestInstance(@"Print(((2 * 3) + 4))", 10);

            yield return new DeserializationTestInstance(@"Print(True && False)", false);

            yield return new DeserializationTestInstance(
                @"Print ((StringIsEmpty 'Hello') && (StringIsEmpty 'World'))",
                false
            );

            yield return new DeserializationTestInstance(
                @"Print((not True) && (not False))",
                false
            );

            yield return new DeserializationTestInstance(@"Print(true && false)", false);

            yield return new DeserializationTestInstance("Print(ArrayIsEmpty([]))", true);

            yield return new DeserializationTestInstance(@"<ArrayVar> = ['abc', '123']");

            yield return new DeserializationTestInstance("-<Seven> = 3 + 4\r\n- Print <Seven>", 7);

            yield return new DeserializationTestInstance("2 + 2 | Print", 4);

            yield return new DeserializationTestInstance("StringIsEmpty 'World' | Print", false);

            yield return new DeserializationTestInstance(
                "3 | ApplyMathOperator 'Add' 4 | ApplyMathOperator 'Multiply' 5 | Print",
                35
            );

            yield return new DeserializationTestInstance(
                "Print (3 | ApplyMathOperator 'Add' 4 | ApplyMathOperator 'Multiply' 5)",
                35
            );

            yield return new DeserializationTestInstance(
                @"
- <ArrayVar> = ['abc', '123']
- Print(ArrayLength <ArrayVar>)",
                2
            );

            yield return new DeserializationTestInstance(@"Print(ArrayLength ['abc', '123'])", 2);

            yield return new DeserializationTestInstance(
                "- <ArrayVar> =  ['abc', '123']\r\n- Print(ArrayLength <ArrayVar>)",
                2
            );

            yield return new DeserializationTestInstance(
                @"
- <ArrayVar> = ['abc', '123']
- Print(ArrayIsEmpty <ArrayVar>)",
                false
            );

            yield return new DeserializationTestInstance(
                @"
- <ArrayVar> =  ['abc', '123']
- Print(ElementAtIndex <ArrayVar> 1)",
                "123"
            );

            yield return new DeserializationTestInstance(
                @"
- <ArrayVar> = ['abc', '123']
- Print(FindElement <ArrayVar> '123')",
                "1"
            );

            yield return new DeserializationTestInstance(
                @"
- <ArrayVar> = ['abc', '123']
- Foreach <ArrayVar> (Print <Element>) <Element>",
                "abc",
                "123"
            );

            yield return new DeserializationTestInstance(
                @"
- <ArrayVar> = [(str: 'abc' num: '123')]
- Foreach <ArrayVar> (Print <Entity>)",
                "(str: \"abc\" num: \"123\")"
            );

            yield return new DeserializationTestInstance(
                @"
- <ArrayVar> = [(str: 'abc' num: '123')]
- EntityForeach <ArrayVar> (Print <Entity>)",
                "(str: \"abc\" num: \"123\")"
            );

            yield return new DeserializationTestInstance(
                @"
- <ArrayVar1> = ['abc', '123']
- <ArrayVar2> = (Repeat <ArrayVar1> 2)
- Foreach <ArrayVar2> (Print (ArrayLength <Element>)) <Element>",
                "2",
                "2"
            );

            yield return new DeserializationTestInstance(
                @"
- <ArrayVar> = ['abc', 'def']
- <Sorted> = (ArraySort <ArrayVar>)
- Print (ElementAtIndex <Sorted> 0)",
                "abc"
            );

            yield return new DeserializationTestInstance(
                @"
- <ConditionVar> = true
- If <ConditionVar> (Print 1) (Print 2)",
                "1"
            );

            //                yield return new DeserializationTestFunction(
            //                    @"Do: Print
            //Config:
            //  AdditionalRequirements: 
            //  TargetMachineTags:
            //  - Tag1
            //  DoNotSplit: false
            //  Priority: 1
            //Value: I have config", "I have config"
            //                )
            //                {
            //                    ExpectedConfiguration = new Configuration
            //                    {
            //                        TargetMachineTags = new List<string> {"Tag1"},
            //                        DoNotSplit = false,
            //                        Priority = 1
            //                    }
            //                };

            //                yield return new DeserializationTestFunction(@"Do: Print
            //Config:
            //  AdditionalRequirements:
            //  - Notes: ABC123
            //    Name: ValueIf
            //    MinVersion: 1.2.3.4
            //    MaxVersion: 5.6.7.8
            //  TargetMachineTags:
            //  - Tag1
            //  DoNotSplit: false
            //  Priority: 1
            //Value: I have config too", "I have config too")
            //                {
            //                    ExpectedConfiguration = new Configuration
            //                    {
            //                        TargetMachineTags = new List<string> { "Tag1" },
            //                        DoNotSplit = false,
            //                        Priority = 1,
            //                        AdditionalRequirements = new List<Requirement>
            //                        {
            //                            new Requirement
            //                            {
            //                                MaxVersion = new Version(5,6,7,8),
            //                                MinVersion = new Version(1,2,3,4),
            //                                Name = "ValueIf",
            //                                Notes = "ABC123"
            //                            }
            //                        }
            //                    }
            //                };

            yield return new DeserializationTestInstance(
                @"ForEach ['a','b','c'] (Print <char>) <char>",
                "a",
                "b",
                "c"
            );

            yield return new DeserializationTestInstance(
                @"ForEach
['a','b','c']
(Print <char>)
<char>",
                "a",
                "b",
                "c"
            );
        }
    }

    public record DeserializationTestInstance : IAsyncTestInstance
    {
        public DeserializationTestInstance(string scl, params object[] expectedLoggedValues)
        {
            SCL                  = scl;
            ExpectedLoggedValues = expectedLoggedValues.Select(x => x.ToString()!).ToList();
        }

        private string SCL { get; }

        public Configuration? ExpectedConfiguration { get; set; } = null!;

        private IReadOnlyCollection<string> ExpectedLoggedValues { get; }

        /// <inheritdoc />
        public async Task RunAsync(ITestOutputHelper testOutputHelper)
        {
            testOutputHelper.WriteLine(SCL);

            var stepFactoryStore = StepFactoryStore.CreateUsingReflection(typeof(StepFactory));
            var loggerFactory    = TestLoggerFactory.Create();
            loggerFactory.AddXunit(testOutputHelper);

            var deserializeResult = SCLParsing.ParseSequence(SCL);

            deserializeResult.ShouldBeSuccessful(x => x.AsString);

            var freezeResult = deserializeResult.Value.TryFreeze(stepFactoryStore);

            freezeResult.ShouldBeSuccessful(x => x.AsString);

            var unitStep = freezeResult.Value as IStep<Unit>;

            unitStep.Should().NotBeNull();

            using var stateMonad = new StateMonad(
                loggerFactory.CreateLogger("Test"),
                EmptySettings.Instance,
                ExternalProcessRunner.Instance,
                FileSystemHelper.Instance,
                stepFactoryStore
            );

            var runResult = await unitStep!
                .Run(stateMonad, CancellationToken.None);

            runResult.ShouldBeSuccessful(x => x.AsString);

            StaticHelpers.CheckLoggedValues(
                loggerFactory,
                LogLevel.Information,
                ExpectedLoggedValues
            );

            if (ExpectedConfiguration != null || freezeResult.Value.Configuration != null)
            {
                freezeResult.Value.Configuration.Should().BeEquivalentTo(ExpectedConfiguration);
            }
        }

        /// <inheritdoc />
        public void Deserialize(IXunitSerializationInfo info)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public void Serialize(IXunitSerializationInfo info)
        {
            throw new System.NotImplementedException();
        }
    }
}

}
