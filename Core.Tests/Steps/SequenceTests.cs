using System;
using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class SequenceTests : StepTestBase<Core.Steps.Sequence<StringStream>, StringStream>
    {
        /// <inheritdoc />
        public SequenceTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Print then print",
                    new Core.Steps.Sequence<StringStream>
                {
                    InitialSteps = new List<IStep<Unit>>
                    {
                        new Print<StringStream> {Value = Constant("Hello")},
                        new Print<StringStream> {Value = Constant("World")}
                    },
                    FinalStep = Constant("Goodbye")
                },
                    "Goodbye",
                    "Hello", "World" );


            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Print then print", "- Print Value: 'Hello'\n- Print Value: 'World'", Unit.Default, "Hello", "World");
            }

        }

        /// <inheritdoc />
        protected override IEnumerable<SerializeCase> SerializeCases
        {
            get
            {
                yield return new SerializeCase("Short form",
                    new Core.Steps.Sequence<StringStream>
                    {
                        InitialSteps = new List<IStep<Unit>>
                        {
                            new DoNothing(),
                            new DoNothing(),
                            new DoNothing()
                        },
                        FinalStep = Constant("Hello World")
                    },
                    $"- DoNothing{Environment.NewLine}- DoNothing{Environment.NewLine}- DoNothing{Environment.NewLine}- \"Hello World\"{Environment.NewLine}" );
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases
        {
            get
            {
                yield return new ErrorCase("Initial steps error",
                    new Core.Steps.Sequence<StringStream>
                    {
                        InitialSteps = new List<IStep<Unit>> {new FailStep<Unit> {ErrorMessage = "Initial step Fail"}},
                        FinalStep = Constant("Final")
                    },
                    new SingleError("Initial step Fail", ErrorCode.Test, EntireSequenceLocation.Instance));

                yield return new ErrorCase("Final steps error",
                    new Core.Steps.Sequence<StringStream>
                    {
                        InitialSteps = new List<IStep<Unit>> {new DoNothing() },
                        FinalStep =  new FailStep<StringStream> { ErrorMessage = "Final step Fail" }
                    },
                    new SingleError("Final step Fail", ErrorCode.Test, EntireSequenceLocation.Instance));
            }
        }
    }
}