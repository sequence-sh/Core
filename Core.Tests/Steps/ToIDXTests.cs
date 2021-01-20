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
#DREENDDATAREFERENCE

"
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
