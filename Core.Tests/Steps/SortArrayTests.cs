﻿using System.Collections.Generic;
using System.Linq;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class SortArrayTests : StepTestBase<ArraySort<int>, Sequence<int>>
    {
        /// <inheritdoc />
        public SortArrayTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Ascending", new ArraySort<int>()
                {
                    Array = Array(8,6,7,5,3,0,9),
                    Descending = Constant(false)

                }, new List<int>(){0,3,5,6,7,8,9}.ToSequence() );

                yield return new StepCase("Descending", new ArraySort<int>()
                {
                    Array = Array(8, 6, 7, 5, 3, 0, 9),
                    Descending = Constant(true)

                }, new List<int>() { 9,8,7,6,5,3,0 }.ToSequence());
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Sort Ascending",
                    "ArraySort Array: [8,6,7,5,3,0,9]",
                    new List<int>(){0,3,5,6,7,8,9}.ToSequence());

            }

        }

    }
}