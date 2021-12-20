namespace Reductech.Sequence.Core.Internal.Documentation;

/// <summary>
/// A wrapper for this documented object.
/// </summary>
public class StepWrapper : IDocumentedStep
{
    /// <summary>
    /// Creates a new StepWrapper.
    /// </summary>
    public StepWrapper(IGrouping<IStepFactory, string> grouping)
    {
        Factory               = grouping.Key;
        DocumentationCategory = grouping.Key.Category;

        Parameters = Factory.ParameterDictionary
            .Values
            .Distinct(StepParameterComparer.Instance)
            .OrderBy(x => x.Order)
            .ThenBy(x => x.Name)
            .ToList();

        Requirements = grouping.Key.Requirements.Select(x => $"Requires {x}").ToList();

        TypeDetails = grouping.Key.OutputTypeExplanation;

        AllNames = grouping.ToList();

        Examples = Factory.Examples.ToList();
    }

    private IStepFactory Factory { get; }

    /// <inheritdoc />
    public string DocumentationCategory { get; }

    /// <inheritdoc />
    public string Name =>
        Factory.TypeName; // TypeNameHelper.GetHumanReadableTypeName(Factory.StepType);

    /// <inheritdoc />
    public string FileName => Factory.TypeName + ".md";

    /// <inheritdoc />
    public string Summary => Factory.Summary;

    /// <inheritdoc />
    public string? TypeDetails { get; }

    /// <inheritdoc />
    public IEnumerable<string> Requirements { get; }

    /// <inheritdoc />
    public IEnumerable<IStepParameter> Parameters { get; }

    /// <inheritdoc />
    public IReadOnlyList<string> AllNames { get; }

    /// <inheritdoc />
    public IReadOnlyList<SCLExample> Examples { get; }
}
