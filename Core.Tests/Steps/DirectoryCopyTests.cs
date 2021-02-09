using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Thinktecture.IO;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class DirectoryCopyTests : StepTestBase<DirectoryCopy, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                        "Copy Directory",
                        new DirectoryCopy()
                        {
                            SourceDirectory      = StaticHelpers.Constant("MySource"),
                            DestinationDirectory = StaticHelpers.Constant("MyDestination"),
                            Overwrite            = StaticHelpers.Constant(true),
                            CopySubDirectories   = StaticHelpers.Constant(true)
                        },
                        Unit.Default
                    )
                    .WithDirectoryAction(x => x.Setup(d => d.Exists("MySource")).Returns(true))
                    .WithDirectoryAction(
                        x => x.Setup(d => d.GetDirectories("MySource"))
                            .Returns(new[] { "MySource\\Sub" })
                    )
                    .WithDirectoryAction(
                        x => x.Setup(d => d.CreateDirectory("MyDestination"))
                            .Returns((null as IDirectoryInfo)!)
                    )
                    .WithDirectoryAction(
                        x => x.Setup(d => d.GetFiles("MySource"))
                            .Returns(new[] { "MySource\\f1", "MySource\\f2" })
                    )
                    .WithFileAction(
                        x => x.Setup(f => f.Copy("MySource\\f1", "MyDestination\\f1", true))
                    )
                    .WithFileAction(
                        x => x.Setup(f => f.Copy("MySource\\f2", "MyDestination\\f2", true))
                    )
                    .WithDirectoryAction(x => x.Setup(d => d.Exists("MySource\\Sub")).Returns(true))
                    .WithDirectoryAction(
                        x => x.Setup(d => d.GetDirectories("MySource\\Sub"))
                            .Returns(new string[] { })
                    )
                    .WithDirectoryAction(
                        x => x.Setup(d => d.CreateDirectory("MyDestination\\Sub"))
                            .Returns((null as IDirectoryInfo)!)
                    )
                    .WithDirectoryAction(
                        x => x.Setup(d => d.GetFiles("MySource\\Sub"))
                            .Returns(new[] { "MySource\\Sub\\f3" })
                    )
                    .WithFileAction(
                        x => x.Setup(
                            f => f.Copy("MySource\\Sub\\f3", "MyDestination\\Sub\\f3", true)
                        )
                    )
                ;
        }
    }
}

}
