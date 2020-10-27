using System.Collections.Generic;
using JetBrains.Annotations;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class ApplyMathOperatorTests : StepTestBase<ApplyMathOperator, int>
    {
        /// <inheritdoc />
        public ApplyMathOperatorTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases {
            get
            {
                yield return new ErrorCase("Divide by zero",
                    new ApplyMathOperator
                    {
                        Left = Constant(1),
                        Operator = Constant(MathOperator.Divide),
                        Right = Constant(0)
                    },
                    new ErrorBuilder("Divide by Zero Error", ErrorCode.DivideByZero));

                yield return new ErrorCase("Math Operator.None", new ApplyMathOperator
                {
                    Left = Constant(1),
                    Operator = Constant(MathOperator.None),
                    Right = Constant(2)
                }, new ErrorBuilder($"Could not apply '{MathOperator.None}'", ErrorCode.UnexpectedEnumValue) );
            } }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                var cases = new[]
                {
                    ("1 + 2", 3),
                    ("2 - 1", 1),
                    ("2 - -1", 3),
                    ("2 - 3", -1),
                    ("2 * 3", 6),
                    ("-2 * 3", -6),
                    ("2 * -3", -6),
                    ("4 / 2", 2),
                    ("5 / 2", 2),
                    ("5 % 2", 1),
                    ("2 ^ 5", 32),
                    ("2 ^ 0", 1),
                    ("2 ^ -1", 0),
                };

                foreach (var (func, expectedValue) in cases)
                    yield return new DeserializeCase(func, func, expectedValue);
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                var cases = new[]
                {
                    (1,2, MathOperator.Add, 3),
                    (2, 1, MathOperator.Subtract, 1),
                    (2, -1, MathOperator.Subtract, 3),
                    (2, 3, MathOperator.Subtract, -1),
                    (2, 3, MathOperator.Multiply, 6),
                    (-2, 3, MathOperator.Multiply, -6),
                    (2, -3, MathOperator.Multiply, -6),
                    (4,2, MathOperator.Divide, 2),
                    (5,2, MathOperator.Divide, 2),
                    (5,2, MathOperator.Modulo, 1),
                    (2,5, MathOperator.Power, 32),
                    (2,0, MathOperator.Power, 1),
                    (2,-1, MathOperator.Power, 0),
                };

                foreach (var (left, right, op, expectedOutput) in cases)
                {
                    yield return new StepCase($"{left} {op} {right}",
                        new ApplyMathOperator
                        {
                            Left = Constant(left),
                            Right = Constant(right),
                            Operator = Constant(op)
                        }, expectedOutput );
                }
            }

        }

        /// <inheritdoc />
        protected override IEnumerable<SerializeCase> SerializeCases
        {
            get
            {
                yield return new SerializeCase("Add", new ApplyMathOperator()
                {
                    Left = Constant(1),
                    Operator = Constant(MathOperator.Add),
                    Right = Constant(2)
                }, "(1 + 2)");

                yield return new SerializeCase("Add negative number", new ApplyMathOperator()
                {
                    Left = Constant(1),
                    Operator = Constant(MathOperator.Add),
                    Right = Constant(-2)
                }, "(1 + -2)");

                yield return new SerializeCase("Subtract negative number", new ApplyMathOperator()
                {
                    Left = Constant(1),
                    Operator = Constant(MathOperator.Subtract),
                    Right = Constant(-2)
                }, "(1 - -2)");

            }
        }
    }
}