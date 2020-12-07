using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class CreateDirectoryTests : StepTestBase<CreateDirectory, Unit>
    {
        /// <inheritdoc />
        public CreateDirectoryTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Create Directory",
                    new CreateDirectory
                    {
                        Path = Constant("MyPath")
                    },
                    Unit.Default

                ).WithFileSystemAction(x=>
                    x.Setup(h=>h.CreateDirectory("MyPath"))
                        .Returns(Unit.Default));
        }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Create Directory", "CreateDirectory(Path: 'MyPath')", Unit.Default)
                    .WithFileSystemAction(x =>
                    x.Setup(h => h.CreateDirectory("MyPath"))
                        .Returns(Unit.Default));
            }
        }


        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases {
            get
            {
                yield return new ErrorCase("Error returned", new CreateDirectory
                {
                    Path = Constant("MyPath")
                }, new ErrorBuilder("ValueIf Error", ErrorCode.Test))
                    .WithFileSystemAction(x =>
                    x.Setup(h => h.CreateDirectory("MyPath"))
                        .Returns(new ErrorBuilder("ValueIf Error", ErrorCode.Test)));

            } }
    }
}
