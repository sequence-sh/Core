using System.Collections.Generic;
using System.Threading;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class RunStepTests : StepTestBase<RunStep<Unit>, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Test Print",
                new RunStep<Unit>
                {
                    Step = new Log<StringStream> { Value = Constant("Hello World") }
                },
                Unit.Default,
                "Hello World"
            );

            yield return new StepCase(
                "Test Run String",
                new RunStep<StringStream> { Step = Constant("Hello World") },
                Unit.Default
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            var divide = new Divide { Terms = Array(1, 0) };

            yield return new ErrorCase(
                "Test Divide by zero",
                new RunStep<int> { Step = divide },
                ErrorCode.DivideByZero.ToErrorBuilder().WithLocation(divide)
            );

            yield return new ErrorCase(
                "Test Error in Array",
                new RunStep<Array<int>>
                {
                    Step = new ArrayMap<int>()
                    {
                        Array = Array(3, 2, 1, 0),
                        Function = new Divide()
                        {
                            Terms = new ArrayNew<int>()
                            {
                                Elements = new[]
                                {
                                    Constant(1),
                                    GetVariable<int>(VariableName.Entity)
                                }
                            }
                        }
                    }
                },
                ErrorCode.DivideByZero.ToErrorBuilder().WithLocation(divide)
            );
        }
    }

    public static TheoryData<object, bool> ReadToEndData
    {
        get
        {
            var data = new TheoryData<object, bool>
            {
                { true, false },
                { "Hello World", false },
                { new List<int> { 1, 2, 3 }.ToSCLArray(), false },
                { Entity.Create(("a", 1)), false }
            };

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(ReadToEndData))]
    public async void TestReadToEnd(object o, bool expectError)
    {
        var r = await RunStep<Unit>.ReadToEnd(o, CancellationToken.None);

        if (expectError)
            r.ShouldBeFailure();
        else
            r.ShouldBeSuccessful(x => x.AsString);
    }
}

}
