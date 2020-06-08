using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using NUnit.Framework;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.mutable;
using Reductech.EDR.Utilities.Processes.mutable.enumerations;
using Reductech.EDR.Utilities.Processes.mutable.injection;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Utilities.Processes.Tests
{
    public class CSVReaderTest
    {
        public static readonly List<TestCase> TestCases = new List<TestCase>()
        {
            new TestCase("Basic CSV", "H1,H2\r\nOne,1\r\nTwo,2", ",", null, false, "One1", "Two2" ),
            new TestCase("Comment Token", "H1,H2\r\n#One,1\r\nTwo,2\r\nThree,3", ",", "#", false, "Two2", "Three3" ),
            new TestCase("Different Delimiter", "H1|H2\r\nOne|1\r\nTwo|2", "|", null, false, "One1", "Two2" ),
            new TestCase("A Field with Quotes", "H1,H2\r\nOne,1\r\n\"Two\",2", ",", null, false, "One1", "\"Two\"2" ),
            new TestCase("Has fields encased in quotes", "H1,H2\r\nOne,1\r\nTwo,2", ",", null, true, "One1", "Two2" ),
            new TestCase("Comma inside quotes", "H1,H2\r\n\"One,One\",1\r\nTwo,2", ",", null, true, "One,One1", "Two2" ),
        };

        [TestCaseSource(nameof(TestCases))]
        [Test]
        public async Task TestCSVReader(TestCase testCase)
        {
            var l = new Loop
            {
                Do = new EmitProcess(),
                For = new CSV
                {
                    CSVText = testCase.CSVText,
                    ColumnInjections = new List<ColumnInjection>
                    {
                        new ColumnInjection{Column = "H1",Property = nameof(EmitProcess.Term)},
                        new ColumnInjection{Column = "H2",Property = nameof(EmitProcess.Number)}
                    },
                    CommentToken = testCase.CommentToken,
                    Delimiter = testCase.Delimiter,
                    HasFieldsEnclosedInQuotes = testCase.HasFieldsEncasedInQuotes
                }
            };

            var actualList = new List<string>();

            var process = l.TryFreeze(EmptySettings.Instance).AssertSuccess();

            var output = process.ExecuteUntyped();

            await foreach (var o in output)
            {
                Assert.IsTrue(o.OutputType != OutputType.Error, o.Text);

                if(o.OutputType == OutputType.Message)
                    actualList.Add(o.Text);
            }

            CollectionAssert.AreEqual(testCase.ExpectedResults, actualList);
        }

        [TestCaseSource(nameof(TestCases))]
        [Test]
        public async Task TestLazyCSVReader(TestCase testCase)
        {
            var l = new Loop
            {
                Do = new EmitProcess(),
                For = new CSV
                {
                    CSVProcess = new EmitStringProcess {Output = testCase.CSVText},
                    ColumnInjections = new List<ColumnInjection>
                    {
                        new ColumnInjection{Column = "H1",Property = nameof(EmitProcess.Term)},
                        new ColumnInjection{Column = "H2",Property = nameof(EmitProcess.Number)}
                    },
                    CommentToken = testCase.CommentToken,
                    Delimiter = testCase.Delimiter,
                    HasFieldsEnclosedInQuotes = testCase.HasFieldsEncasedInQuotes
                }
            };

            var actualList = new List<string>();

            var process = l.TryFreeze(EmptySettings.Instance).AssertSuccess();

            var output = process.ExecuteUntyped();

            await foreach (var o in output)
            {
                Assert.IsTrue(o.OutputType != OutputType.Error, o.Text);

                if(o.OutputType == OutputType.Message)
                    actualList.Add(o.Text);
            }

            CollectionAssert.AreEqual(testCase.ExpectedResults, actualList);
        }

        [Test]
        public void TestCSVError()
        {
            var l = new Loop
            {
                Do = new EmitProcess(),
                For = new CSV
                {
                    CSVText = @"H1,H2
t,2,abc
t,4,def",
                    ColumnInjections = new List<ColumnInjection>()
                        {
                            new ColumnInjection{Column = "H1",Property = nameof(EmitProcess.Term)},
                            new ColumnInjection{Column = "H2",Property = nameof(EmitProcess.Number)}
                        },
                    Delimiter = ",",
                    HasFieldsEnclosedInQuotes = true
                }
            };

            var freezeResult = l.TryFreeze(EmptySettings.Instance);

            Assert.IsFalse(freezeResult.IsSuccess, "Should not have been able to freeze");

        }



#pragma warning disable CA1034 // Nested types should not be visible
        public class TestCase
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public TestCase(string name, string csvText, string delimiter, string? commentToken, bool hasFieldsEncasedInQuotes, params string[] expectedResults)
            {
                Name = name;
                CSVText = csvText;
                Delimiter = delimiter;
                CommentToken = commentToken;
                HasFieldsEncasedInQuotes = hasFieldsEncasedInQuotes;
                ExpectedResults = expectedResults;
            }

            public string Name { get; }

            public string CSVText { get; }

            public string Delimiter { get; }

            public string? CommentToken { get; }

            public bool HasFieldsEncasedInQuotes { get; }

            public IReadOnlyCollection<string>  ExpectedResults { get; }

            /// <inheritdoc />
            public override string ToString()
            {
                return Name;
            }
        }

#pragma warning disable CA1034 // Nested types should not be visible
        public class EmitStringProcess : Process
#pragma warning restore CA1034 // Nested types should not be visible
        {
            /// <inheritdoc />
            public override string GetReturnTypeInfo()
            {
                return nameof(String);
            }

            /// <inheritdoc />
            public override string GetName() => "Emit String";

            /// <inheritdoc />
            public override Result<ImmutableProcess, ErrorList> TryFreeze(IProcessSettings processSettings)
            {
                return Result.Success<ImmutableProcess, ErrorList>(new ImmutableEmitString(Output));
            }

            /// <inheritdoc />
            public override IEnumerable<string> GetRequirements() => Enumerable.Empty<string>();

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
            public string Output { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

            internal class ImmutableEmitString : ImmutableProcess<string>
            {
                public readonly string Output;

                public ImmutableEmitString(string output)
                {
                    Output = output;
                }

                /// <inheritdoc />
                public override string Name => "Emit String";

                /// <inheritdoc />
                public override IProcessConverter? ProcessConverter => null;

                /// <inheritdoc />
#pragma warning disable 1998
                public override async IAsyncEnumerable<IProcessOutput<string>> Execute()
#pragma warning restore 1998
                {
                    yield return ProcessOutput<string>.Success(Output);
                }
            }
        }


    }
}
