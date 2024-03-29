﻿namespace Sequence.Core.Tests.Steps;

public partial class RunStepTests : StepTestBase<RunStep<Unit>, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Test Print",
                new RunStep<Unit> { Step = new Log { Value = Constant("Hello World") } },
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
                new RunStep<SCLInt> { Step = divide },
                ErrorCode.DivideByZero.ToErrorBuilder().WithLocation(divide)
            );

            yield return new ErrorCase(
                "Test Error in Array",
                new RunStep<Array<SCLInt>>
                {
                    Step = new ArrayMap<SCLInt, SCLInt>()
                    {
                        Array = Array(3, 2, 1, 0),
                        Function = new LambdaFunction<SCLInt, SCLInt>(
                            null,
                            new Divide()
                            {
                                Terms = new ArrayNew<SCLInt>()
                                {
                                    Elements = new[]
                                    {
                                        Constant(1),
                                        GetVariable<SCLInt>(VariableName.Item)
                                    }
                                }
                            }
                        )
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
                {
                    new List<int> { 1, 2, 3 }.Select(x => x.ConvertToSCLObject()).ToSCLArray(),
                    false
                },
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
            r.ShouldBeSuccessful();
    }
}
