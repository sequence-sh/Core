using System;
using System.Collections.Generic;
using System.IO;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class PathCombineTests : StepTestBase<PathCombine, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            var currentDirectory = Environment.CurrentDirectory;

            var expected = Path.Combine(currentDirectory, "Hello", "World");

            yield return new StepCase(
                "Non Relative",
                new PathCombine
                {
                    Paths = new ArrayNew<StringStream>
                    {
                        Elements = new List<IStep<StringStream>>()
                        {
                            Constant(currentDirectory),
                            Constant("Hello"),
                            Constant("World")
                        }
                    }
                },
                expected
            );

            yield return new StepCase(
                    "Relative",
                    new PathCombine
                    {
                        Paths = new ArrayNew<StringStream>
                        {
                            Elements = new List<IStep<StringStream>>()
                            {
                                Constant("Hello"), Constant("World")
                            }
                        }
                    },
                    expected
                )
                .WithFileSystemAction(
                    x => x.Setup(a => a.GetCurrentDirectory()).Returns(currentDirectory)
                );
        }
    }
}

}
