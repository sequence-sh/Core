using System;
using System.Collections.Generic;
using System.Linq;
using Generator.Equals;

namespace Reductech.EDR.Core.Internal.Analytics
{

/// <summary>
/// Detailed analysis of how a step is used in a sequence
/// </summary>
[Equatable]
public partial record DetailedStepAnalysis(
    string StepName,
    TextLocation TextLocation,
    TimeSpan TotalTime,
    int TimesRun,
    [property: OrderedEquality] IReadOnlyList<string> Errors)
{
    /// <inheritdoc />
    public override string ToString()
    {
        if (Errors.Any())
            return new
            {
                StepName,
                TextLocation,
                TotalTime,
                TimesRun,
                Errors = string.Join(";", Errors)
            }.ToString()!;

        return new { StepName, TextLocation, TotalTime, TimesRun }.ToString()!;
    }
}

}
