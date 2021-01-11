using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public class DeleteItemTests : StepTestBase<DeleteItem, Unit>
{
    /// <inheritdoc />
    public DeleteItemTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                    "Delete Directory",
                    new DeleteItem { Path = Constant("My Path") },
                    Unit.Default
                    //, "Directory 'My Path' Deleted."
                )
                .WithFileSystemAction(
                    x => x.Setup(a => a.DoesDirectoryExist("My Path")).Returns(true)
                )
                .WithFileSystemAction(
                    x => x.Setup(a => a.DeleteDirectory("My Path", true)).Returns(Unit.Default)
                );

            yield return new StepCase(
                    "Delete File",
                    new DeleteItem { Path = Constant("My Path") },
                    Unit.Default
                    // , "File 'My Path' Deleted."
                )
                .WithFileSystemAction(
                    x => x.Setup(a => a.DoesDirectoryExist("My Path")).Returns(false)
                )
                .WithFileSystemAction(x => x.Setup(a => a.DoesFileExist("My Path")).Returns(true))
                .WithFileSystemAction(
                    x => x.Setup(a => a.DeleteFile("My Path")).Returns(Unit.Default)
                );

            yield return new StepCase(
                    "Item does not exist",
                    new DeleteItem { Path = Constant("My Path") },
                    Unit.Default
                    //, "Item 'My Path' did not exist."
                )
                .WithFileSystemAction(
                    x => x.Setup(a => a.DoesDirectoryExist("My Path")).Returns(false)
                )
                .WithFileSystemAction(x => x.Setup(a => a.DoesFileExist("My Path")).Returns(false));
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield return new ErrorCase(
                    "Could not delete file",
                    new DeleteItem { Path = Constant("My Path") },
                    new ErrorBuilder(ErrorCode.Test, "ValueIf Error")
                )
                .WithFileSystemAction(
                    x => x.Setup(a => a.DoesDirectoryExist("My Path")).Returns(true)
                )
                .WithFileSystemAction(
                    x => x.Setup(a => a.DeleteDirectory("My Path", true))
                        .Returns(new ErrorBuilder(ErrorCode.Test, "ValueIf Error"))
                );
        }
    }
}

}
