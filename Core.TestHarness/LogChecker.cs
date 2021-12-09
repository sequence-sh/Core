using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using FluentAssertions;
using MELT;
using Microsoft.Extensions.Logging;

namespace Reductech.EDR.Core.TestHarness;

public static class LogChecker
{
    public static void CheckLoggedValues(
        ITestLoggerFactory loggerFactory,
        LogLevel minLogLevel,
        IReadOnlyCollection<string> expectedLoggedValues)
    {
        var infoOrHigherEntries =
            loggerFactory.Sink.LogEntries.Where(x => x.LogLevel >= minLogLevel);

        var assertions = expectedLoggedValues.Select(
            expected =>
            {
                return new Action<LogEntry>(
                    entry =>
                    {
                        var actual    = SpaceCompressor.CompressSpaces(entry.Message!);
                        var expected2 = SpaceCompressor.CompressSpaces(expected);

                        actual.Should().Be(expected2);
                    }
                );
            }
        );

        if (expectedLoggedValues.IsNullOrEmpty())
            infoOrHigherEntries.Should().BeEmpty("Log values should be empty");
        else
            infoOrHigherEntries.Should()
                .SatisfyRespectively(assertions, "Log value should match expected");
    }
}
