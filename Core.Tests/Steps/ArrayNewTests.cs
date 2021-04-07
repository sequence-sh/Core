using System.Collections.Generic;
using System.Collections.Immutable;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class ArrayNewTests : StepTestBase<ArrayNew<int>, Array<int>>
{
    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Three elements explicit form",
                "Array Elements: [1, 2, 3]",
                new List<int> { 1, 2, 3 }.ToSCLArray()
            );

            yield return new DeserializeCase(
                "Three elements simple form",
                "Array Elements: [1, 2, 3]",
                new List<int> { 1, 2, 3 }.ToSCLArray()
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Empty",
                new ArrayNew<int>
                {
                    Elements = new List<IStep<int>> { Constant(1), Constant(2), Constant(3), }
                },
                new List<int> { 1, 2, 3 }.ToSCLArray()
            );
        }
    }

    /// <inheritdoc /> //TODO create a serialize case
    protected override IEnumerable<SerializeCase> SerializeCases
    {
        get
        {
            yield return new SerializeCase(
                "Empty",
                new ArrayNew<int> { Elements = ImmutableList<IStep<int>>.Empty },
                "[]"
            );

            yield return new SerializeCase(
                "Three Elements",
                new ArrayNew<int>
                {
                    Elements = new List<IStep<int>> { Constant(1), Constant(2), Constant(3), }
                },
                "[1, 2, 3]"
            );
        }
    }
}

}
