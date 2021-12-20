namespace Reductech.Sequence.Core.Tests.Steps;

public partial class StringToDateTests : StepTestBase<StringToDate, SCLDateTime>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Convert string using default format",
                new StringToDate { Date = Constant("2020/10/20 20:30:40") },
                new DateTime(2020, 10, 20, 20, 30, 40).ConvertToSCLObject()
            );

            yield return new StepCase(
                "Convert string using en-GB culture",
                new StringToDate
                {
                    Date = Constant("10/11/2020 20:30:40"), Culture = Constant("en-GB")
                },
                new DateTime(2020, 11, 10, 20, 30, 40).ConvertToSCLObject()
            );

            yield return new StepCase(
                "Convert string using en-US culture",
                new StringToDate
                {
                    Date = Constant("10/11/2020 20:30:40"), Culture = Constant("en-US")
                },
                new DateTime(2020, 10, 11, 20, 30, 40).ConvertToSCLObject()
            );

            yield return new StepCase(
                "Convert string using custom format",
                new StringToDate
                {
                    Date        = Constant("2020-10-11 10:30:40 AM"),
                    InputFormat = Constant("yyyy-dd-MM HH:mm:ss tt")
                },
                new DateTime(2020, 11, 10, 10, 30, 40).ConvertToSCLObject()
            );

            yield return new StepCase(
                "Convert string using custom format and culture",
                new StringToDate
                {
                    Date        = Constant("10|11|2020 10:30:40 AM"),
                    Culture     = Constant("en-US"),
                    InputFormat = Constant("dd|MM|yyyy HH:mm:ss tt")
                },
                new DateTime(2020, 11, 10, 10, 30, 40).ConvertToSCLObject()
            );
        }
    }

    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            // TODO: This is failing on linux, needs looking into - #145
            //yield return new ErrorCase("Can't parse culture",
            //    new StringToDate
            //    {
            //        Date = Constant("10/11/2020 20:30:40"),
            //        Culture = Constant("unknown")
            //    },
            //    new ErrorBuilder(
            //        "Culture is not supported. unknown is an invalid culture identifier.",
            //        ErrorCode.CouldNotParse
            //    )
            //);
            yield return new ErrorCase(
                "Can't parse Date",
                new StringToDate { Date = Constant("not a date") },
                new ErrorBuilder(
                    new FormatException(
                        "The string 'not a date' was not recognized as a valid DateTime. There is an unknown word starting at index '0'."
                    ),
                    ErrorCode.CouldNotParse
                )
            );

            yield return new ErrorCase(
                "Can't parse Date using inputFormat",
                new StringToDate
                {
                    Date = Constant("not a date"), InputFormat = Constant("yyyy/MM/dd")
                },
                new ErrorBuilder(
                    new FormatException(
                        "String 'not a date' was not recognized as a valid DateTime."
                    ),
                    ErrorCode.CouldNotParse
                )
            );
        }
    }
}
