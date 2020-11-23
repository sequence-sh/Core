﻿using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class ToCaseTests : StepTestBase<ToCase, string>
    {
        /// <inheritdoc />
        public ToCaseTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("To upper", new ToCase()
                {
                    Case = Constant(TextCase.Upper), String = Constant("hello")
                }, "HELLO");

                yield return new StepCase("To lower", new ToCase()
                {
                    Case = Constant(TextCase.Lower),
                    String = Constant("HELLO")
                }, "hello");

                yield return new StepCase("To title", new ToCase()
                {
                    Case = Constant(TextCase.Title),
                    String = Constant("hELLo")
                }, "Hello");

            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("To title", "ToCase(Case = TextCase.Title, String = 'hELLo')", "Hello");

            }

        }

    }
}