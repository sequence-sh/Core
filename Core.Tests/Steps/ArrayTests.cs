using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class ArrayTests : StepTestBase<Array<int>, List<int>>
    {
        /// <inheritdoc />
        public ArrayTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper) {}

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Three elements explicit form", "Array Elements: [1, 2, 3]", new List<int>{1,2,3});
                yield return new DeserializeCase("Three elements simple form", "Array Elements: [1, 2, 3]", new List<int>{1,2,3});
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases {
            get
            {
                yield return new StepCase("Empty",
                    new Array<int>
                    {
                        Elements = new List<IStep<int>>
                        {
                            Constant(1),
                            Constant(2),
                            Constant(3),
                        }
                    }, new List<int>{1,2,3} );
            } }

        /// <inheritdoc /> //TODO create a serialize case
        protected override IEnumerable<SerializeCase> SerializeCases
        {
            get
            {
                yield return new SerializeCase("Empty", new Array<int>
                {
                    Elements = ImmutableList<IStep<int>>.Empty
                }, "[]");

                yield return new SerializeCase("Three Elements", new Array<int>
                {
                    Elements = new List<IStep<int>>
                    {
                        Constant(1),
                        Constant(2),
                        Constant(3),
                    }
                }, "[1, 2, 3]");
            }
        }
    }
}