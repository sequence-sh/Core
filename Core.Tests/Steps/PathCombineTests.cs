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
                var expected1 = Path.Combine("Hello", "World");


                yield return new StepCase("Non Relative", new PathCombine
                {
                    Paths = new Constant<List<string>>(new List<string>{"Hello", "World"})
                }, expected1
                    );

                var currentDirectory = Environment.CurrentDirectory;

                var expected2 = Path.Combine(currentDirectory, "Hello", "World");

                yield return new StepCase("Relative", new PathCombine
                {
                    Paths = Constant(new List<string> { "Hello", "World" }),
                    IsRelative = Constant(true)
                }, expected2)
                    .WithFileSystemAction(x=>x.Setup(a=>a.GetCurrentDirectory()).Returns(currentDirectory));
            }
        }

        public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

    }
}