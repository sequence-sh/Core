using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Namotion.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Reductech.EDR.Core.TestHarness
{

public static class CaseHelper
{
    public static async Task FindAndRunAsync<T>(
        this IEnumerable<T> cases,
        string caseName,
        ITestOutputHelper testOutputHelper,
        string? extraArgument = null) where T : ICase
    {
        if (string.IsNullOrWhiteSpace(caseName))
        {
            testOutputHelper.WriteLine("Case skipped - no data");
            return;
        }

        var possibleCases = cases.Where(x => x.Name == caseName).ToList();

        if (!possibleCases.Any())
            throw new XunitException($"No test with name {caseName}");

        if (possibleCases.Count > 1)
            throw new XunitException(
                $"{possibleCases.Count} {typeof(T).GetDisplayName()} with name {caseName}"
            );

        await possibleCases.Single().RunCaseAsync(testOutputHelper, extraArgument);
    }
}

}
