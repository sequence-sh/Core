namespace Reductech.Sequence.Core.LanguageServer;

/// <summary>
/// Visits SCL to find errors
/// </summary>
public class DiagnosticsVisitor : SCLBaseVisitor<ImmutableList<SingleError>>
{
    /// <inheritdoc />
    public override ImmutableList<SingleError> VisitErrorNode(IErrorNode node)
    {
        return DefaultResult.Add(
            ErrorCode.SCLSyntaxError.ToErrorBuilder(node.GetText())
                .WithLocationSingle(new TextLocation(node.Symbol))
        );
    }

    /// <inheritdoc />
    protected override ImmutableList<SingleError> AggregateResult(
        ImmutableList<SingleError> aggregate,
        ImmutableList<SingleError> nextResult)
    {
        return aggregate.AddRange(nextResult);
    }

    /// <inheritdoc />
    protected override ImmutableList<SingleError> DefaultResult { get; } =
        ImmutableList<SingleError>.Empty;
}
