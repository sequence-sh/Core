using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoTheory;
using MELT;
using Microsoft.Extensions.Logging;
using Moq;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.TestHarness;
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
            yield return new DeserializationTestInstance("'Hello World'", "Hello World");
            yield return new DeserializationTestInstance("123",           "123");
            yield return new DeserializationTestInstance("1 + 2",         "3");
            yield return new DeserializationTestInstance("3.3",           "3.3");
            yield return new DeserializationTestInstance("(double: 3.3)", "(double: 3.3)");

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

            yield return new DeserializationTestInstance( //Note the extra comma
                @"
- <ArrayVar> = ['abc', '123',]
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

            yield return new DeserializationTestInstance(@"['a','b','c'][0] | Print",     'a');
            yield return new DeserializationTestInstance(@"['a','b','c'][(1+1)] | Print", 'c');

            yield return new DeserializationTestInstance(@"(Foo:'a')['Foo'] | Print", 'a');
            yield return new DeserializationTestInstance(@"(Foo:'a')['FOO'] | Print", 'a');

            yield return new DeserializationTestInstance(@"(Foo:'a', Bar:'b')['Bar'] | Print", 'b');

            yield return new DeserializationTestInstance(
                @"(Foo:'a')[(StringToCase 'foo' 'Upper')] | Print",
                'a'
            );

            yield return new DeserializationTestInstance(
                @"ForEach ['a','b','c'] (Print <char>) <char>",
                "a",
                "b",
                "c"
            );

            yield return new DeserializationTestInstance("(Foo.Bar:'b')", "(Foo: (Bar: \"b\"))");

            yield return new DeserializationTestInstance(
                "(Foo.Bar.Baz:'b')",
                "(Foo: (Bar: (Baz: \"b\")))"
            );

            yield return new DeserializationTestInstance("(Foo.Bar.Baz:'b')['Foo.Bar.Baz']", "b");

            yield return new DeserializationTestInstance(
                "(Foo.Bar:'a' Foo.Baz:'b')",
                "(Foo: (Bar: \"a\" Baz: \"b\"))"
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

        public string Name => SCL;

        private IReadOnlyCollection<string> ExpectedLoggedValues { get; }

        /// <inheritdoc />
        public async Task RunAsync(ITestOutputHelper testOutputHelper)
        {
            testOutputHelper.WriteLine(SCL);

            var stepFactoryStore = StepFactoryStore.CreateUsingReflection(typeof(StepFactory));
            var loggerFactory    = TestLoggerFactory.Create();
            loggerFactory.AddXunit(testOutputHelper);
            var mockFactory = new MockRepository(MockBehavior.Strict);

            var runner = new SCLRunner(
                SCLSettings.EmptySettings,
                loggerFactory.CreateLogger("Test"),
                mockFactory.Create<IExternalProcessRunner>().Object,
                mockFactory.Create<IFileSystemHelper>().Object,
                stepFactoryStore
            );

            var result = await runner.RunSequenceFromTextAsync(SCL, CancellationToken.None);

            result.ShouldBeSuccessful(x => x.ToString()!);

            LogChecker.CheckLoggedValues(
                loggerFactory,
                LogLevel.Information,
                ExpectedLoggedValues
            );
        }
    }
}

}
