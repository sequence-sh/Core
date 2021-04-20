using System;
using System.Collections.Generic;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class ToIDXTests : StepTestBase<ToIDX, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Single Property",
                new ToIDX
                {
                    Entity = Constant(Entity.Create(("DREREFERENCE", "abcd"), ("Foo", 1)))
                },
                "#DREREFERENCE abcd\r\n#DREFIELD Foo= 1\r\n#DREENDDOC\r\n#DREENDDATAREFERENCE"
            );

            yield return new StepCase(
                "Edgar Brown",
                new ToIDX
                {
                    Entity = Constant(
                        Entity.Create(
                            ("DREREFERENCE", "392348A0"),
                            ("authorname", new List<string>() { "Brown", "Edgar" }),
                            ("title", "Dr."),
                            ("DREDATE", new DateTime(1990, 01, 06)),
                            ("DRETITLE", "Jurassic Molecules"),
                            ("DRECONTENT", "abcde\r\nfghij"),
                            ("DREDBNAME", "Science")
                        )
                    )
                },
                "#DREREFERENCE 392348A0\r\n#DREDATE 1990/01/06\r\n#DRETITLE\r\nJurassic Molecules\r\n#DRECONTENT\r\nabcde\r\nfghij\r\n#DREDBNAME Science\r\n#DREFIELD authorname1= \"Brown\"\r\n#DREFIELD authorname2= \"Edgar\"\r\n#DREFIELD title= \"Dr.\"\r\n#DREENDDOC\r\n#DREENDDATAREFERENCE"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            var foo1 = Entity.Create(("Foo", 1));

            yield return new ErrorCase(
                "Single Property",
                new ToIDX() { Entity = Constant(foo1) },
                ErrorCode.SchemaViolationMissingProperty.ToErrorBuilder("DREREFERENCE", foo1)
            );
        }
    }
}

}
