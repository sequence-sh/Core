using System;
using System.Collections.Generic;
using System.IO;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class PathCombineTests : StepTestBase<PathCombine, string>
    {
        /// <inheritdoc />
        public PathCombineTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) {}

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                var root = "C:/";
                if (!Path.IsPathFullyQualified(root))
                    root = @"C:\";


                var expected1 = Path.Combine(root, "Hello", "World");


                yield return new StepCase("Non Relative", new PathCombine
                {
                    Paths = new Constant<List<string>>(new List<string>{root, "Hello", "World"})
                }, expected1
                    );

                var currentDirectory = Environment.CurrentDirectory;

                var expected2 = Path.Combine(currentDirectory, "Hello", "World");

                yield return new StepCase("Relative", new PathCombine
                {
                    Paths = Constant(new List<string> { "Hello", "World" }),
                }, expected2)
                    .WithFileSystemAction(x=>x.Setup(a=>a.GetCurrentDirectory()).Returns(currentDirectory));
            }
        }

    }
}