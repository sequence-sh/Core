using System.Collections.Immutable;
using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps;

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
            //Note, we can't really test empty array here (there's no way to know it's meant to be int[]. It is tested in general deserialize tests)

            yield return new StepCase(
                "Three Constant Elements",
                new ArrayNew<int>
                {
                    Elements =
                        new List<IStep<SCLInt>> { Constant(1), Constant(2), Constant(3), }
                },
                new List<int> { 1, 2, 3 }.ToSCLArray()
            );

            yield return new StepCase(
                "Sum Elements",
                new ArrayNew<int>
                {
                    Elements = new List<IStep<SCLInt>>
                    {
                        new Sum { Terms = Array(1, 2) }, new Sum { Terms = Array(2, 3) }
                    }
                },
                new List<int> { 3, 5 }.ToSCLArray()
            );

            yield return new StepCase(
                "Function Elements",
                new ArrayNew<int>
                {
                    Elements = new List<IStep<SCLInt>>
                    {
                        new StringLength() { String = Constant("hello") },
                        new StringLength() { String = Constant("goodbye") }
                    }
                },
                new List<int> { 5, 7 }.ToSCLArray()
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<SerializeCase> SerializeCases
    {
        get
        {
            yield return new SerializeCase(
                "Empty",
                new ArrayNew<int> { Elements = ImmutableList<IStep<SCLInt>>.Empty },
                "[]"
            );

            yield return new SerializeCase(
                "Three Elements",
                new ArrayNew<int>
                {
                    Elements =
                        new List<IStep<SCLInt>> { Constant(1), Constant(2), Constant(3), }
                },
                "[1, 2, 3]"
            );
        }
    }
}
