﻿namespace Sequence.Core.Internal.Documentation;

/// <summary>
/// A wrapper for this documented object.
/// </summary>
public class StepWrapper : IDocumentedStep
{
    /// <summary>
    /// Creates a new StepWrapper.
    /// </summary>
    public StepWrapper(IGrouping<IStepFactory, string> grouping) : this(grouping.Key, grouping) { }

    /// <summary>
    /// Creates a new StepWrapper.
    /// </summary>
    public StepWrapper(IStepFactory stepFactory) : this(stepFactory, stepFactory.Names) { }

    private StepWrapper(IStepFactory stepFactory, IEnumerable<string> names)
    {
        Factory               = stepFactory;
        DocumentationCategory = stepFactory.Category;

        Parameters = Factory.ParameterDictionary
            .Values
            .Distinct(StepParameterComparer.Instance)
            .OrderBy(x => x.Order)
            .ThenBy(x => x.Name)
            .ToList();

        Requirements = stepFactory.Requirements.Select(x => $"Requires {x.GetText()}").ToList();

        TypeDetails = stepFactory.OutputTypeExplanation;

        AllNames = names.ToList();

        Examples = Factory.Examples.ToList();
    }

    private IStepFactory Factory { get; }

    /// <inheritdoc />
    public string DocumentationCategory { get; }

    /// <inheritdoc />
    public string Name => Factory.TypeName;

    /// <inheritdoc />
    public string FileName => Factory.TypeName;

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
