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
                new ToIDX()
                {
                    Entity = Constant(Entity.Create(("DREREFERENCE", "abcd"), ("Foo", 1)))
                },
                @"DREREFERENCE abcd
DREFIELD Foo= 1
#DREENDDOC
#DREENDDATAREFERENCE"
            );

            yield return new StepCase(
                "Edgar Brown",
                new ToIDX()
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
                @"DREREFERENCE 392348A0
DREDATE 1990/01/06
DRETITLE
Jurassic Molecules
DRECONTENT
abcde
fghij
DREDBNAME Science
DREFIELD authorname=1 ""Brown""
DREFIELD authorname=2 ""Edgar""
DREFIELD title= ""Dr.""
#DREENDDOC
#DREENDDATAREFERENCE"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield return new ErrorCase(
                "Single Property",
                new ToIDX() { Entity = Constant(Entity.Create(("Foo", 1))) },
                ErrorCode.SchemaViolationMissingProperty.ToErrorBuilder("DREREFERENCE")
            );
        }
    }
}

}
