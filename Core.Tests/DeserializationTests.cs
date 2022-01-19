using MELT;

namespace Reductech.Sequence.Core.Tests;

public partial class DeserializationTests
{
    public record DeserializationTestInstance : IAsyncTestInstance, ICaseWithSetup
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

            var stepFactoryStore = StepFactoryStore.Create();
            var loggerFactory    = TestLoggerFactory.Create();
            loggerFactory.AddXunit(testOutputHelper);
            var repository = new MockRepository(MockBehavior.Strict);

            var restClient        = RESTClientSetupHelper.GetRESTClient(repository, FinalChecks);
            var restClientFactory = new SingleRestClientFactory(restClient);

            var externalContext =
                ExternalContextSetupHelper.GetExternalContext(repository, restClientFactory);

            var runner = new SCLRunner(
                loggerFactory.CreateLogger("Test"),
                stepFactoryStore,
                externalContext
            );

            var result = await runner.RunSequenceFromTextAsync(
                SCL,
                new Dictionary<string, object>(),
                CancellationToken.None
            );

            result.ShouldBeSuccessful();

            foreach (var finalCheck in FinalChecks)
            {
                finalCheck();
            }

            LogChecker.CheckLoggedValues(
                loggerFactory,
                LogLevel.Information,
                ExpectedLoggedValues
            );
        }

        /// <inheritdoc />
        public ExternalContextSetupHelper ExternalContextSetupHelper { get; } = new();

        /// <inheritdoc />
        public RESTClientSetupHelper RESTClientSetupHelper { get; } = new();

        /// <inheritdoc />
        public List<Action> FinalChecks { get; } = new();
    }

    [GenerateAsyncTheory("Deserialize")]
    public IEnumerable<DeserializationTestInstance> TestCases
    {
        get
        {
            yield return new DeserializationTestInstance("1.1 + 1.2",     "2.3");
            yield return new DeserializationTestInstance("1.1 + 1",       "2.1");
            yield return new DeserializationTestInstance("(1.2 - 1) > 0", true);

            yield return new DeserializationTestInstance("2.1 == 2.5",    false);
            yield return new DeserializationTestInstance("2.0 == 2",      true);
            yield return new DeserializationTestInstance("true == false", false);
            yield return new DeserializationTestInstance("'yes' == 'no'", false);
            yield return new DeserializationTestInstance("1 == 2",        false);
            yield return new DeserializationTestInstance("2.5 < 2.1",     false);

            yield return new DeserializationTestInstance("Log ('a' + 'b')", "ab");

            yield return new DeserializationTestInstance(
                "- <foo> = (a:'1') + (b:'2')\r\n- Log <foo>",
                "('a': \"1\" 'b': \"2\")"
            );

            yield return new DeserializationTestInstance(
                "Log ((a:'1') + (b:'2'))",
                "('a': \"1\" 'b': \"2\")"
            );

            yield return new DeserializationTestInstance("(Foo: 1)['Foo'] + 3",               4);
            yield return new DeserializationTestInstance("(Foo: (Bar: 1))['Foo']['Bar'] + 3", 4);
            yield return new DeserializationTestInstance("(Foo: (Bar: 1))['Foo.Bar'] + 3",    4);

            yield return new DeserializationTestInstance("ArrayLength (Foo: [1,2,3])['Foo']", 3);
            yield return new DeserializationTestInstance("3 * (Foo: [1,2,3])['Foo'][2]",      9);

            yield return new DeserializationTestInstance(
                "3 * (Foo:(Bar:([1,2,3])))['Foo.Bar'][2]",
                9
            );

            yield return new DeserializationTestInstance(
                "3 * (Foo:(Bar:([[1,1,1],[2,2,2],[3,4,5]])))['Foo.Bar'][2][0]",
                9
            );

            yield return new DeserializationTestInstance("(Foo: [1,2])['Foo']", "[1, 2]");
            yield return new DeserializationTestInstance("(Foo: [1,2])",        "('Foo': [1, 2])");

            yield return new DeserializationTestInstance("ArrayLength([1,2] + [3,4])",         "4");
            yield return new DeserializationTestInstance("ArrayLength Array: ([1,2] + [3,4])", "4");

            yield return new DeserializationTestInstance("ArrayLength Array: [1,2,3,4]", "4");

            yield return new DeserializationTestInstance("'Hello World'", "Hello World");
            yield return new DeserializationTestInstance("123",           "123");
            yield return new DeserializationTestInstance("1 + 2",         "3");
            yield return new DeserializationTestInstance("3.3",           "3.3");
            yield return new DeserializationTestInstance("(double: 3.3)", "('double': 3.3)");

            yield return new DeserializationTestInstance(
                @"- <Foo> = 'Hello World'
- <Bar> = <Foo>
- Log <Bar>",
                "Hello World"
            );

            yield return new DeserializationTestInstance(@"(a:'x')['a'] | Log", "x");
            yield return new DeserializationTestInstance(@"(a:'')['a'] | Log",  "");

            yield return new DeserializationTestInstance(
                @"- <textCase> = TextCase.Lower
- <result> = StringToCase 'HELLO' <textCase>
- log <result>",
                "hello"
            );

            yield return new DeserializationTestInstance(
                @"Log 'Mark''s string'",
                "Mark's string"
            );

            yield return new DeserializationTestInstance(
                @"Log ""Mark's string""",
                "Mark's string"
            );

            yield return new DeserializationTestInstance(
                @"Log 'Comments!' #This is a comment",
                "Comments!"
            );

            yield return new DeserializationTestInstance(
                @"#This is a comment
Log 'Comments!'",
                "Comments!"
            );

            yield return new DeserializationTestInstance(
                @"/*This is a comment
block*/
Log 'Comments!'",
                "Comments!"
            );

            yield return new DeserializationTestInstance(
                @"Log /*Comment Block */ 'Comments!' ",
                "Comments!"
            );

            yield return new DeserializationTestInstance(@"Log (2 * 3)", 6);

            yield return new DeserializationTestInstance(@"Log(3 - 2)", 1);

            yield return new DeserializationTestInstance(@"Log(2 * 3)", 6);

            yield return new DeserializationTestInstance(@"Log(6 / 3)", 2);

            yield return new DeserializationTestInstance(@"Log(6 ^ 2)", 36);

            yield return new DeserializationTestInstance(@"Log(7 % 2)", 1);

            yield return
                new DeserializationTestInstance(
                    @"Log 'falsetto'",
                    "falsetto"
                ); //check 'false' delimiter

            yield return
                new DeserializationTestInstance(
                    @"Log 'notable'",
                    "notable"
                ); //check 'not' delimiter

            yield return new DeserializationTestInstance(@"Log(2*3)", 6);

            yield return new DeserializationTestInstance(@"Log Value: 2", 2);

            yield return new DeserializationTestInstance(@"Log Value: not True", false);

            yield return new DeserializationTestInstance(@"Log(2 ^ 3)", 8);

            yield return new DeserializationTestInstance(@"Log(not True)", false);

            yield return new DeserializationTestInstance(
                @"Log 2020-11-22T20:30:40.0000000",
                "2020-11-22T20:30:40.0000000"
            );

            yield return new DeserializationTestInstance(
                @"Log 2020-11-22",
                "2020-11-22T00:00:00.0000000"
            );

            yield return new DeserializationTestInstance(
                @"Log 2020-11-22T20:30:40",
                "2020-11-22T20:30:40.0000000"
            );

            yield return new DeserializationTestInstance(
                @"Log(stringToCase 'hello' textCase.Upper)",
                "HELLO"
            );

            yield return new DeserializationTestInstance(
                @"Log(stringToCase 'hello' 'Upper')",
                "HELLO"
            );

            yield return new DeserializationTestInstance(@"Log(2 >= 3)", false);
            yield return new DeserializationTestInstance(@"Log(4 >= 3)", true);
            yield return new DeserializationTestInstance(@"Log(3 >= 3)", true);

            yield return new DeserializationTestInstance(@"Log(3 > 3)", false);
            yield return new DeserializationTestInstance(@"Log(4 > 3)", true);
            yield return new DeserializationTestInstance(@"Log(3 < 3)", false);

            yield return new DeserializationTestInstance(@"Log(3 <= 3)", true);

            yield return new DeserializationTestInstance(@"Log(2 * (3 + 4))", 14);
            yield return new DeserializationTestInstance(@"Log((2 * 3) + 4)", 10);

            yield return new DeserializationTestInstance(@"Log((2 >= 3))", false);

            yield return new DeserializationTestInstance(@"Log((2 * (3 + 4)))", 14);
            yield return new DeserializationTestInstance(@"Log((2*(3+4)))",     14);
            yield return new DeserializationTestInstance(@"Log(((2 * 3) + 4))", 10);

            yield return new DeserializationTestInstance(@"Log(True && False)", false);

            yield return new DeserializationTestInstance(
                @"Log ((StringIsEmpty 'Hello') && (StringIsEmpty 'World'))",
                false
            );

            yield return new DeserializationTestInstance(
                @"Log((not True) && (not False))",
                false
            );

            yield return new DeserializationTestInstance(@"Log(true && false)", false);

            yield return new DeserializationTestInstance("Log(ArrayIsEmpty([]))", true);

            yield return new DeserializationTestInstance(@"<ArrayVar> = ['abc', '123']");

            yield return new DeserializationTestInstance("-<Seven> = 3 + 4\r\n- Log <Seven>", 7);

            yield return new DeserializationTestInstance("2 + 2 | Log", 4);

            yield return new DeserializationTestInstance("StringIsEmpty 'World' | Log", false);

            yield return new DeserializationTestInstance(
                @"
- <ArrayVar> = ['abc', '123']
- Log(ArrayLength <ArrayVar>)",
                2
            );

            yield return new DeserializationTestInstance( //Note the extra comma
                @"
- <ArrayVar> = ['abc', '123',]
- Log(ArrayLength <ArrayVar>)",
                2
            );

            yield return new DeserializationTestInstance(@"Log(ArrayLength ['abc', '123'])", 2);

            yield return new DeserializationTestInstance(
                "- <ArrayVar> =  ['abc', '123']\r\n- Log(ArrayLength <ArrayVar>)",
                2
            );

            yield return new DeserializationTestInstance(
                @"
- <ArrayVar> = ['abc', '123']
- Log(ArrayIsEmpty <ArrayVar>)",
                false
            );

            yield return new DeserializationTestInstance(
                @"
- <ArrayVar> =  ['abc', '123']
- Log(ArrayElementAtIndex <ArrayVar> 1)",
                "123"
            );

            yield return new DeserializationTestInstance(
                @"
- <ArrayVar> = ['abc', '123']
- Log(ArrayFind <ArrayVar> '123')",
                "1"
            );

            yield return new DeserializationTestInstance(
                @"
- <ArrayVar> = ['abc', '123']
- Foreach <ArrayVar> (Log <Item>)",
                "abc",
                "123"
            );

            yield return new DeserializationTestInstance(
                @"
- <ArrayVar> = [(str: 'abc' num: '123')]
- Foreach <ArrayVar> (Log <item>)",
                "('str': \"abc\" 'num': \"123\")"
            );

            yield return new DeserializationTestInstance(
                @"
- <ArrayVar> = [(str: 'abc' num: '123')]
- EntityForeach <ArrayVar> (Log <item>)",
                "('str': \"abc\" 'num': \"123\")"
            );

            yield return new DeserializationTestInstance(
                @"
- <ArrayVar1> = ['abc', '123']
- <ArrayVar2> = (Repeat <ArrayVar1> 2)
- Foreach <ArrayVar2> (Log (ArrayLength <Item>))",
                "2",
                "2"
            );

            yield return new DeserializationTestInstance(
                @"
- <ArrayVar> = ['abc', 'def']
- <Sorted> = (ArraySort <ArrayVar>)
- Log (ArrayElementAtIndex <Sorted> 0)",
                "abc"
            );

            yield return new DeserializationTestInstance(
                @"
- <ConditionVar> = true
- If <ConditionVar> (Log 1) (Log 2)",
                "1"
            );

            yield return new DeserializationTestInstance(
                @"
- <docs> = DocumentationCreate
- log <docs>['MainContents']['FileName']
",
                "all.md"
            );

            yield return new DeserializationTestInstance(
                @"
- (DocumentationCreate)['AllPages'] | ArrayFilter (<>['FileName'] == 'Not.md') | ForEach (
    - <path> = $""{<>['Directory']}/{<>['FileName']}""
    - log <path>
)
",
                "Core/Not.md"
            );

            yield return new DeserializationTestInstance(@"['a','b','c'][0] | Log",     'a');
            yield return new DeserializationTestInstance(@"['a','b','c'][(1+1)] | Log", 'c');

            yield return new DeserializationTestInstance(@"(Foo:'a')['Foo'] | Log", 'a');
            yield return new DeserializationTestInstance(@"(Foo:'a')['FOO'] | Log", 'a');

            yield return new DeserializationTestInstance(@"(Foo:'a', Bar:'b')['Bar'] | Log", 'b');

            yield return new DeserializationTestInstance(
                @"(Foo:'a')[(StringToCase 'foo' 'Upper')] | Log",
                'a'
            );

            yield return new DeserializationTestInstance(
                @"ForEach ['a','b','c'] (Log <item>)",
                "a",
                "b",
                "c"
            );

            yield return new DeserializationTestInstance(
                @"ForEach ['a' 'b' 'c'] (Log <item>)",
                "a",
                "b",
                "c"
            );

            yield return new DeserializationTestInstance(
                "(Foo: 'a')['Foo'] == (Foo: 'a')['Foo']",
                true
            );

            yield return new DeserializationTestInstance(
                "(Foo: 'a')['Foo'] == (Foo: 'b')['Foo']",
                false
            );

            yield return new DeserializationTestInstance(
                "(Foo: 1)['Foo'] == (Foo: '1')['Foo']",
                true
            );

            yield return new DeserializationTestInstance(
                "(Foo: 1)['Foo'] < (Foo: 2)['Foo']",
                true
            );

            yield return new DeserializationTestInstance(
                "(Foo.Bar:'b')",
                "('Foo': ('Bar': \"b\"))"
            );

            yield return new DeserializationTestInstance(
                "(Foo.Bar:(Baz: 'a' Zab: 'b'))",
                "('Foo': ('Bar': ('Baz': \"a\" 'Zab': \"b\")))"
            );

            yield return new DeserializationTestInstance("'a' + 'b' + 'c'", "abc");

            yield return new DeserializationTestInstance(
                "(Foo: 'a') + (Bar: 'b')",
                "('Foo': \"a\" 'Bar': \"b\")"
            );

            yield return new DeserializationTestInstance(
                "(Foo: 'a') + (Bar: 'b') + (Baz: 'c')",
                "('Foo': \"a\" 'Bar': \"b\" 'Baz': \"c\")"
            );

            yield return new DeserializationTestInstance("[1,2,3][2]", "3");

            yield return new DeserializationTestInstance(
                "([1,2] + [3,4] + [5,6])[5]",
                "6"
            );

            yield return new DeserializationTestInstance(
                "((foo:'a') + (bar:'b'))['bar']",
                "b"
            );

            yield return new DeserializationTestInstance(
                "(Foo.Bar.Baz:'b')",
                "('Foo': ('Bar': ('Baz': \"b\")))"
            );

            yield return new DeserializationTestInstance(
                "(Foo.Bar: 'a' Foo.Baz: 'b')",
                "('Foo': ('Bar': \"a\" 'Baz': \"b\"))"
            );

            yield return new DeserializationTestInstance("(Foo.Bar.Baz:'b')['Foo.Bar.Baz']",   "b");
            yield return new DeserializationTestInstance("('Foo.Bar.Baz':'b')['Foo.Bar.Baz']", "b");

            yield return new DeserializationTestInstance(
                "(\"Foo.Bar.Baz\":'b')['Foo.Bar.Baz']",
                "b"
            );

            yield return new DeserializationTestInstance(
                "(\"Foo\".Bar.'Baz':'b')['Foo.Bar.Baz']",
                "b"
            );

            yield return new DeserializationTestInstance(
                "(Foo.Bar:'a' Foo.Baz:'b')",
                "('Foo': ('Bar': \"a\" 'Baz': \"b\"))"
            );

            yield return new DeserializationTestInstance(
                @"ForEach
['a','b','c']
(Log <item>)",
                "a",
                "b",
                "c"
            );

            yield return new DeserializationTestInstance(
                @"
- <bar> = 'b'
- ['a', <bar>, 'c']
",
                "[\"a\", \"b\", \"c\"]"
            );

            yield return new DeserializationTestInstance(
                @"
- <bar> = ('letter': 'b')['letter']
- ['a', <bar>, 'c']
",
                "[\"a\", \"b\", \"c\"]"
            );

            yield return new DeserializationTestInstance(
                "Foreach (foo: [(bar:1), (bar:2), (bar:3)])['foo'] (log <>['bar'])",
                1,
                2,
                3
            );

            yield return new DeserializationTestInstance(
                "Foreach (foo: [(bar:1), (bar:2), (bar:3)])['foo'] (log <item>['bar'])",
                1,
                2,
                3
            );

            yield return new DeserializationTestInstance(
                "Foreach (foo: [(bar:1), (bar:2), (bar:3)])['foo'] (<myVar> => log <myVar>['bar'])",
                1,
                2,
                3
            );

            yield return new DeserializationTestInstance(
                "- foreach [1,2,3] (log <item>)\r\n- foreach [true, false] (log <item>)",
                1,
                2,
                3,
                true,
                false
            );

            yield return new DeserializationTestInstance(
                "- foreach [1,2,3] (log (<item> + 1))\r\n- foreach [true, false] (log (not <item>))",
                2,
                3,
                4,
                false,
                true
            );

            yield return new DeserializationTestInstance(
                @"
- <num> = (num: 12345)['num']
- IncrementVariable <num>
- log <num>
",
                12346
            );

            yield return new DeserializationTestInstance(
                @"
- <var> = false
- ArraySort Array: [1,3,2] Descending: <var>
| foreach (log <>)
",
                1,
                2,
                3
            );

            yield return new DeserializationTestInstance(
                @"
- <var> = SortOrder.Ascending
- ArraySort Array: [1,3,2] Descending: <var>
| foreach (log <>)
",
                1,
                2,
                3
            );

            yield return new DeserializationTestInstance(
                @"
- <entity> = (direction: false)
- <var> = <entity>['direction']
- ArraySort Array: [1,3,2] Descending: <var>
| foreach (log <>)
",
                1,
                2,
                3
            );

            yield return new DeserializationTestInstance(
                @"stringcontains 'a' + 'b' stringtocase 'b' textcase.upper",
                false
            );

            yield return new DeserializationTestInstance(
                @"stringcontains ('a' + 'b') stringtocase 'b' textcase.upper",
                false
            );

            yield return new DeserializationTestInstance(
                @"stringcontains (stringtrim ' abc ') (stringtocase 'b' textcase.upper)",
                false
            );

            yield return new DeserializationTestInstance(
                @"stringcontains stringtrim ' abc ' substring: stringtocase 'b' textcase.upper",
                false
            );

            yield return new DeserializationTestInstance(
                @"ArrayTake ['a' 'b' 'c'] 2 | foreach log <>",
                "a",
                "b"
            );

            yield return new DeserializationTestInstance(
                @"ArraySkip ['a' 'b' 'c'] 2 | foreach log <>",
                "c"
            );

            yield return new DeserializationTestInstance(
                @"
- <entity> = (a: 1)
- <output> = <entity> + (b: 2)
- log <output>
",
                "('a': 1 'b': 2)"
            );

            yield return new DeserializationTestInstance(
                @"
- <entity1> = (a: 1)
- <entity2> = (b: 2)
- <output> = <entity1> + <entity2> + (c: 3)
- log <output>
",
                "('a': 1 'b': 2 'c': 3)"
            );

            yield return new DeserializationTestInstance(
                @"
- <docs> = (DocumentationCreate)['AllPages'] | ArrayTake 50
- log (ArrayLength <docs>) 
",
                "50"
            );

            yield return new DeserializationTestInstance(
                @"
- <nums> = (a: [1,2,3])['a'] | arrayskip 0
- log <nums>
",
                "[1, 2, 3]"
            );

            yield return new DeserializationTestInstance(
                @"
- <nums> = (a: 1,2,3)['a']
- log <nums>
",
                "[1, 2, 3]"
            );

            yield return new DeserializationTestInstance(
                @"
- <docs> = (DocumentationCreate)['AllPages'] | arraytake 1
- <docs> 
 | ForEach (<entityVar> =>
    log (stringtrim <entityVar>['FileName'])
  )",
                "all.md"
            );

            yield return new DeserializationTestInstance(
                "ValueIf  condition:true then:(log 't') else: (log 'f')",
                "t"
            );

            yield return new DeserializationTestInstance(
                "ValueIf  condition:true then:(log 't')",
                "t"
            );

            yield return new DeserializationTestInstance(
                "ValueIf  condition:false then:(log 't') else: (log 'f')",
                "f"
            );

            yield return new DeserializationTestInstance("ValueIf  condition:false then:(log 't')");

            yield return new DeserializationTestInstance(
                @"
- <array> = [1,2,3] | ArrayMap (<> * 2)
- Foreach <array> (log <>)
- Foreach <array> (log <>)
",
                2,
                4,
                6,
                2,
                4,
                6
            );

            yield return new DeserializationTestInstance(
                @"
- <array> = 1,2,3 | ArrayMap (<> * 2)
- Foreach <array> (log <>)
- Foreach <array> (log <>)
",
                2,
                4,
                6,
                2,
                4,
                6
            );
        }
    }
}
