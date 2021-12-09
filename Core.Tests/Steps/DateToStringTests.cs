using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps;

public partial class DateToStringTests : StepTestBase<DateToString, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Convert DateTime using default Format",
                new DateToString { Date = Constant(new DateTime(2020, 11, 22, 20, 30, 40)) },
                "2020/11/22 20:30:40"
            );

            yield return new StepCase(
                "Convert DateTime using custom Format",
                new DateToString
                {
                    Date   = Constant(new DateTime(2020, 11, 22, 20, 30, 40)),
                    Format = Constant("MM/dd/yy hh.mm.ss")
                },
                "11/22/20 08.30.40"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "ArrayLength 3",
                "DateToString Date: 2020-11-22T20:30:40",
                "2020/11/22 20:30:40"
            );
        }
    }
}
