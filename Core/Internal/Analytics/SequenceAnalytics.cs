namespace Reductech.Sequence.Core.Internal.Analytics;

/// <summary>
/// Analysis produced by a Sequence Logger
/// </summary>
public class SequenceAnalytics
{
    /// <summary>
    /// The starts of steps
    /// </summary>
    public readonly List<StepStart> StepStarts = new();

    /// <summary>
    /// The ends of steps
    /// </summary>
    public readonly List<StepEnd> StepEnds = new();

    /// <summary>
    /// Get Detailed Analysis of this Sequence
    /// </summary>
    public DetailedAnalysis Synthesize()
    {
        var groupedStarts = StepStarts.GroupBy(x => (x.StepName, x.Location));

        var groupedEnds = StepEnds.GroupBy(x => (x.StepName, x.Location))
            .ToDictionary(x => x.Key);

        var results = new List<DetailedStepAnalysis>();

        foreach (var startGroup in groupedStarts)
        {
            if (groupedEnds.TryGetValue(startGroup.Key, out var endGroup))
            {
                var timesRun = Math.Min(startGroup.Count(), endGroup.Count());

                var totalTime = startGroup.Take(timesRun)
                    .Select(x => x.Time)
                    .Zip(endGroup.Take(timesRun).Select(x => x.Time), (start, end) => end - start)
                    .Aggregate(TimeSpan.Zero, (agg, ts) => agg + ts);

                var errors = endGroup.Select(x => x.StepError)
                    .Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .ToList();

                var dsa = new DetailedStepAnalysis(
                    startGroup.Key.StepName,
                    startGroup.Key.Location,
                    totalTime,
                    timesRun,
                    errors
                );

                results.Add(dsa);
            }
        }

        return new DetailedAnalysis(results);
    }
}
