using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using NUnit.Framework;
using Reductech.EDR.Utilities.Processes.mutable;
using Reductech.EDR.Utilities.Processes.mutable.enumerations;
using Reductech.EDR.Utilities.Processes.mutable.injection;

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
                    InjectColumns = new Dictionary<string, Injection>
                    {
                        {"H1", new Injection {Property = nameof(EmitProcess.Term)} },
                        {"H2", new Injection {Property = nameof(EmitProcess.Number)} },
                    },
                    CommentToken = testCase.CommentToken,
                    Delimiter = testCase.Delimiter,
                    HasFieldsEnclosedInQuotes = testCase.HasFieldsEncasedInQuotes
                }
            };

            var actualList = new List<string>();

            var process = l.TryFreeze(EmptySettings.Instance).AssertSuccess();

            var resultList = process.Execute();

            await foreach (var (isSuccess, _, value, error) in resultList)
            {
                Assert.IsTrue(isSuccess, error);
                actualList.Add(value);
            }

            CollectionAssert.AreEqual(testCase.ExpectedResults, actualList);
        }


        public class TestCase
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

    }
}
