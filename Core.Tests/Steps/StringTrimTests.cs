using System.Collections.Generic;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps;

public partial class StringTrimTests : StepTestBase<StringTrim, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "StringTrim left",
                new StringTrim { Side = Constant(TrimSide.Start), String = Constant("  word  ") },
                "word  "
            );

            yield return new StepCase(
                "StringTrim right",
                new StringTrim { Side = Constant(TrimSide.End), String = Constant("  word  ") },
                "  word"
            );

            yield return new StepCase(
                "StringTrim both",
                new StringTrim { Side = Constant(TrimSide.Both), String = Constant("  word  ") },
                "word"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "StringTrim left",
                "StringTrim Side: TrimSide.Start String: '  word  '",
                "word  "
            );
        }
    }
}
