using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class ApplyBooleanTests : StepTestBase<ApplyBooleanOperator, bool>
    {
        /// <inheritdoc />
        public ApplyBooleanTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<SerializeCase> SerializeCases
        {
            get
            {
                foreach (var arg1 in new[] {true, false})
                {
                    foreach (var arg2 in new[] {true, false})
                    {
                        foreach (var op in new[] {BooleanOperator.And, BooleanOperator.Or})
                        {
                            string expected = op switch
                            {
                                BooleanOperator.And => $"({arg1} && {arg2})",
                                BooleanOperator.Or => $"({arg1} || {arg2})",
                                _ => throw new ArgumentException($"Could not recognize {op}")
                            };

                            yield return new SerializeCase(expected, new ApplyBooleanOperator
                            {
                                Left = Constant(arg1),
                                Right = Constant(arg2),
                                Operator = Constant(op)
                            }, expected);
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
                foreach (var arg1 in new[] {true, false})
                {
                    foreach (var arg2 in new[] {true, false})
                    {
                        foreach (var op in new[] {BooleanOperator.And, BooleanOperator.Or})
                        {
                            bool expected = op switch
                            {
                                BooleanOperator.And => arg1 && arg2,
                                BooleanOperator.Or => arg1 || arg2,
                                _ => throw new ArgumentException($"Could not recognize {op}")
                            };

                            yield return new StepCase($"{arg1} {op.GetDisplayName()} {arg2}", new ApplyBooleanOperator
                            {
                                Left = Constant(arg1),
                                Right = Constant(arg2),
                                Operator = Constant(op)
                            }, expected);
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Short form with symbols", "true && true", true);
                //yield return new DeserializeCase("Short form with symbols but not spaces", "true&&true", true); TODO: this fails
                yield return new DeserializeCase("Short form with and", "true and true", true);
                yield return new DeserializeCase("Short form with or", "false or true", true);
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases
        {
            get
            {
                var noneStep = new ApplyBooleanOperator()
                {
                    Left = Constant(true),
                    Right = Constant(true),
                    Operator = Constant(BooleanOperator.None)

                };


                yield return new ErrorCase("BooleanOperator.None", noneStep,
                    new SingleError($"Could not apply '{BooleanOperator.None}'", ErrorCode.UnexpectedEnumValue, new StepErrorLocation(noneStep)));

                yield return CreateDefaultErrorCase();
            }
        }
    }
}