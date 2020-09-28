using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Reductech.EDR.Processes.General;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Internal.Documentation;
using Reductech.EDR.Processes.Test.Extensions;
using Xunit;
using Xunit.Abstractions;
using ITestCase = Reductech.EDR.Processes.Test.Extensions.ITestCase;

namespace Reductech.EDR.Processes.Test
{
    public class DocumentationTests : DocumentationTestCases
    {
        public DocumentationTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;


        /// <inheritdoc />
        [Theory]
        [ClassData(typeof(DocumentationTestCases))]
        public override void Test(string key) => base.Test(key);
    }

    public class DocumentationTestCases : TestBase
    {
        /// <inheritdoc />
        protected override IEnumerable<ITestCase> TestCases
        {
            get
            {
                yield return new DocumentationTestCase(@"# Processes
<a name=""Not""></a>
## Not

**Boolean**

Negation of a boolean value.

|Parameter|Type  |Required|Summary             |
|:-------:|:----:|:------:|:------------------:|
|Boolean  |`bool`|☑️      |The value to negate.|", NotProcessFactory.Instance);
            }
        }



        private class DocumentationTestCase : ITestCase
        {
            /// <summary>
            /// Create a new DocumentationTestCase
            /// </summary>
            public DocumentationTestCase(string text,
                params IRunnableProcessFactory[] entities)
            {
                Text = text;
                Entities = entities;
                Name = string.Join(" ", Entities.Select(x => x.TypeName).OrderBy(x=>x));
            }

            /// <inheritdoc />
            public string Name { get; }

            public string Text { get; }

            public IReadOnlyCollection<IRunnableProcessFactory> Entities { get; }

            /// <inheritdoc />
            public void Execute(ITestOutputHelper testOutputHelper)
            {
                var documented = Entities.Select(x => new ProcessWrapper(x, new DocumentationCategory("Processes")));

                var lines = DocumentationCreator.CreateDocumentationLines(documented);

                foreach (var line in lines)
                {
                    testOutputHelper.WriteLine(line);
                }

                var textLines = string.Join("\r\n", lines);

                textLines.Trim().Should().Be(Text.Trim());

            }
        }
    }

}
