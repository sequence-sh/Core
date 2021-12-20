using Reductech.Sequence.Core.Internal.Analytics;

namespace Reductech.Sequence.Core.Tests;

[UseTestOutputHelper]
public partial class AnalyticsTests
{
    [Fact]
    public async Task SuccessfulSequenceMustProduceAnalytics()
    {
        var testLogger      = TestOutputHelper.BuildLogger();
        var analyticsLogger = new AnalyticsLogger();
        var multiLogger     = new MultiLogger(testLogger, analyticsLogger);

        var sfs      = StepFactoryStore.Create();
        var mockRepo = new MockRepository(MockBehavior.Strict);

        var runner = new SCLRunner(
            multiLogger,
            sfs,
            mockRepo.OneOf<IExternalContext>()
        );

        var scl = "- <a> = 2 + 2\r\n- <b> = 3 + 4\r\n- [1,2,3] | Foreach (<>=> <b> = <> + <b>)";

        var result = await runner.RunSequenceFromTextAsync(
            scl,
            new Dictionary<string, object>(),
            CancellationToken.None
        );

        result.ShouldBeSuccessful();

        var analysis = analyticsLogger.SequenceAnalytics.Synthesize();

        TestOutputHelper.WriteLine(analysis.ToString());

        var expectedSumTimesRuns = new[] { 1, 1, 3 };

        var actualSumTimesRuns = analysis.Steps
            .Where(x => x.StepName == "Sum")
            .Select(x => x.TimesRun)
            .ToArray();

        actualSumTimesRuns.Should().BeEquivalentTo(expectedSumTimesRuns);
    }

    [Fact]
    public async Task FailedSequenceMustProduceAnalytics()
    {
        var testLogger      = TestOutputHelper.BuildLogger();
        var analyticsLogger = new AnalyticsLogger();
        var multiLogger     = new MultiLogger(testLogger, analyticsLogger);

        var sfs      = StepFactoryStore.Create();
        var mockRepo = new MockRepository(MockBehavior.Strict);

        var runner = new SCLRunner(
            multiLogger,
            sfs,
            mockRepo.OneOf<IExternalContext>()
        );

        var scl = "- <a> = 2 / 0";

        var result = await runner.RunSequenceFromTextAsync(
            scl,
            new Dictionary<string, object>(),
            CancellationToken.None
        );

        result.ShouldBeFailure();

        var analysis = analyticsLogger.SequenceAnalytics.Synthesize();

        TestOutputHelper.WriteLine(analysis.ToString());

        analysis.Steps.Count.Should().Be(3);

        analysis.Steps.SelectMany(x => x.Errors)
            .Should()
            .NotBeNullOrEmpty()
            .And.AllBe("Attempt to Divide by Zero.");
    }
}
