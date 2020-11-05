using System.Collections.Generic;
using System.Threading;
using Moq;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class CreateFileTests : StepTestBase<CreateFile, Unit>
    {
        /// <inheritdoc />
        public CreateFileTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Create File",
                        new CreateFile
                        {
                            Path = Constant("My Path"),
                            Text = Constant("My Text")
                        }, Unit.Default)
                    .WithFileSystemAction(x=>x
                        .Setup(a=>a.CreateFileAsync("My Path", "My Text", It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Unit.Default));
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases {
            get
            {
                yield return new ErrorCase("Error Returned",
                        new CreateFile
                        {
                            Path = Constant("My Path"),
                            Text = Constant("My Text")
                        }, new ErrorBuilder("Test Error", ErrorCode.Test))
                    .WithFileSystemAction(x => x
                        .Setup(a => a.CreateFileAsync("My Path", "My Text", It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new ErrorBuilder("Test Error", ErrorCode.Test)));
            } }
    }
}