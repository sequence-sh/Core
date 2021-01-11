using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public class ApplyBooleanTests : StepTestBase<ApplyBooleanOperator, bool>
{
    /// <inheritdoc />
    public ApplyBooleanTests([NotNull] ITestOutputHelper testOutputHelper) :
        base(testOutputHelper) { }

    /// <inheritdoc />
    protected override IEnumerable<SerializeCase> SerializeCases
    {
        get
        {
            foreach (var arg1 in new[] { true, false })
            {
                foreach (var arg2 in new[] { true, false })
                {
                    foreach (var op in new[] { BooleanOperator.And, BooleanOperator.Or })
                    {
                        string expected = op switch
                        {
                            BooleanOperator.And => $"{arg1} && {arg2}",
                            BooleanOperator.Or => $"{arg1} || {arg2}",
                            _ => throw new ArgumentException($"Could not recognize {op}")
                        };

                        yield return new SerializeCase(
                            expected,
                            new ApplyBooleanOperator
                            {
                                Left     = Constant(arg1),
                                Right    = Constant(arg2),
                                Operator = Constant(op)
                            },
                            expected
                        );
                    }
                }
            }
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            foreach (var arg1 in new[] { true, false })
            {
                foreach (var arg2 in new[] { true, false })
                {
                    foreach (var op in new[] { BooleanOperator.And, BooleanOperator.Or })
                    {
                        var expected = op switch
                        {
                            BooleanOperator.And => arg1 && arg2,
                            BooleanOperator.Or => arg1 || arg2,
                            _ => throw new ArgumentException($"Could not recognize {op}")
                        };

                        yield return new StepCase(
                            $"{arg1} {op.GetDisplayName()} {arg2}",
                            new ApplyBooleanOperator
                            {
                                Left     = Constant(arg1),
                                Right    = Constant(arg2),
                                Operator = Constant(op)
                            },
                            expected
                        );
                    }
                }
            }

            yield return new StepCase(
                "Left is a func",
                new ApplyBooleanOperator()
                {
                    Left     = new StringIsEmpty() { String = Constant("") },
                    Operator = Constant(BooleanOperator.And),
                    Right    = Constant(true)
                },
                true
            );

            yield return new StepCase(
                "Right is a func",
                new ApplyBooleanOperator()
                {
                    Left     = Constant(true),
                    Operator = Constant(BooleanOperator.And),
                    Right    = new StringIsEmpty() { String = Constant("") },
                },
                true
            );

            yield return new StepCase(
                "Both sides are functions",
                new ApplyBooleanOperator()
                {
                    Left     = new StringIsEmpty() { String = Constant("") },
                    Operator = Constant(BooleanOperator.And),
                    Right    = new StringIsEmpty() { String = Constant("") },
                },
                true
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase("Short form with symbols", "true && true", true);

            yield return new DeserializeCase(
                "Short form with symbols but not spaces",
                "true&&true",
                true
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            var noneStep = new ApplyBooleanOperator()
            {
                Left     = Constant(true),
                Right    = Constant(true),
                Operator = Constant(BooleanOperator.None)
            };

            yield return new ErrorCase(
                "BooleanOperator.None",
                noneStep,
                new SingleError(
                    new StepErrorLocation(noneStep),
                    ErrorCode.UnexpectedEnumValue,
                    nameof(ApplyBooleanOperator.Operator),
                    BooleanOperator.None
                )
            );

            //Do not do default cases as some errors are not propagated due to lazy evaluation

            yield return new ErrorCase(
                "Left is error",
                new ApplyBooleanOperator
                {
                    Left     = new FailStep<bool> { ErrorMessage = "Left Fail" },
                    Right    = Constant(true),
                    Operator = Constant(BooleanOperator.And)
                },
                new SingleError(
                    EntireSequenceLocation.Instance,
                    ErrorCode.Test,
                    "Left Fail"
                )
            );

            yield return new ErrorCase(
                "Operator is error",
                new ApplyBooleanOperator
                {
                    Left     = Constant(true),
                    Right    = Constant(true),
                    Operator = new FailStep<BooleanOperator> { ErrorMessage = "Operator Fail" },
                },
                new SingleError(
                    EntireSequenceLocation.Instance,
                    ErrorCode.Test,
                    "Operator Fail"
                )
            );

            yield return new ErrorCase(
                "Right is error",
                new ApplyBooleanOperator
                {
                    Left     = Constant(true),
                    Right    = new FailStep<bool> { ErrorMessage = "Right Fail" },
                    Operator = Constant(BooleanOperator.And)
                },
                new SingleError(
                    EntireSequenceLocation.Instance,
                    ErrorCode.Test,
                    "Right Fail"
                )
            );
        }
    }
}

}
