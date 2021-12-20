namespace Reductech.Sequence.Core.Internal;

/// <summary>
/// Default Comparer for step factories
/// </summary>
public class StepFactoryComparer : IEqualityComparer<IStepFactory>
{
    private StepFactoryComparer() { }

    /// <summary>
    /// The instance
    /// </summary>
    public static IEqualityComparer<IStepFactory> Instance { get; } = new StepFactoryComparer();

    /// <inheritdoc />
    public bool Equals(IStepFactory? x, IStepFactory? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x is null)
            return false;

        if (y is null)
            return false;

        if (x.GetType() != y.GetType())
            return false;

        return x.TypeName == y.TypeName;
    }

    /// <inheritdoc />
    public int GetHashCode(IStepFactory obj) => obj.TypeName.GetHashCode();
}
