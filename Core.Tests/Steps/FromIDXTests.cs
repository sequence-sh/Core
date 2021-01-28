using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class FromIDXTests : StepTestBase<FromIDX, Entity>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Single Property",
                new FromIDX
                {
                    Stream = StaticHelpers.Constant(
                        @"#DREREFERENCE abcd
#DREFIELD Foo= 1
#DREENDDOC
#DREENDDATAREFERENCE"
                    )
                },
                Entity.Create(("DREREFERENCE", "abcd"), ("Foo", "1"))
            );

            yield return new StepCase(
                "Edgar Brown",
                new FromIDX
                {
                    Stream = StaticHelpers.Constant(
                        "#DREREFERENCE 392348A0\r\n#DREDATE 1990/01/06\r\n#DRETITLE\r\nJurassic Molecules\r\n#DRECONTENT\r\nabcde\r\nfghij\r\n#DREDBNAME Science\r\n#DREFIELD authorname1= \"Brown\"\r\n#DREFIELD authorname2= \"Edgar\"\r\n#DREFIELD title= \"Dr.\"\r\n#DREENDDOC\r\n#DREENDDATAREFERENCE"
                    )
                },
                Entity.Create(
                    ("DREREFERENCE", "392348A0"),
                    ("DREDATE", "1990/01/06"),
                    ("DRETITLE", "Jurassic Molecules"),
                    ("DRECONTENT", "abcde\r\nfghij"),
                    ("DREDBNAME", "Science"),
                    ("authorname", new List<string>() { "Brown", "Edgar" }),
                    ("title", "Dr.")
                )
            );
        }
    }
}

}
