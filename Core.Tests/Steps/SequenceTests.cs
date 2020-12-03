using System;
using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class SequenceTests : StepTestBase<Sequence<string>, string>
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
                    new Sequence<string>
                {
                    InitialSteps = new List<IStep<Unit>>
                    {
                        new Print<string> {Value = Constant("Hello")},
                        new Print<string> {Value = Constant("World")}
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
                yield return new DeserializeCase("Print then print", "- Print(Value = 'Hello')\n- Print(Value = 'World')", Unit.Default, "Hello", "World");
            }

        }

        /// <inheritdoc />
        protected override IEnumerable<SerializeCase> SerializeCases
        {
            get
            {
                yield return new SerializeCase("Short form",
                    new Sequence<string>
                    {
                        InitialSteps = new List<IStep<Unit>>
                        {
                            new DoNothing(),
                            new DoNothing()
                        },
                        FinalStep = Constant("Hello World")
                    },
                    $"- DoNothing(){Environment.NewLine}- DoNothing(){Environment.NewLine}- DoNothing(){Environment.NewLine}- \"Hello World\"" );
            }
        }
    }
}