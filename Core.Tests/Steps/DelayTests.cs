﻿namespace Sequence.Core.Tests.Steps;

public partial class DelayTests : StepTestBase<Delay, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Short Delay",
                new Delay() { Milliseconds = new SCLConstant<SCLInt>(10.ConvertToSCLObject()) },
                Unit.Default
            );
        }
    }
}
