using System.Collections.ObjectModel;
using MELT;
using Reductech.Sequence.Core.Internal.Logging;

namespace Reductech.Sequence.Core.Tests;

public class LogHelperTests
{
    [Fact]
    public void LogMessage_Enumerator_ReturnsProperties()
    {
        var message = new LogMessage(
            "Test",
            "params",
            "step",
            new TextLocation("Text", new TextPosition(1, 1, 1), new TextPosition(2, 2, 2)),
            LogSituation.SequenceStarted,
            DateTime.MinValue,
            new ReadOnlyDictionary<string, object>(new Dictionary<string, object>())
        );

        var props = message.Select(k => k.Key).ToList();

        Assert.Contains(nameof(message.Message),       props);
        Assert.Contains(nameof(message.MessageParams), props);
        Assert.Contains(nameof(message.StepName),      props);
        Assert.Contains(nameof(message.Location),      props);
        Assert.Contains(nameof(message.SequenceInfo),  props);
    }

    [Fact]
    public void LogError_WhenErrorHasException_LogsMessage()
    {
        var loggerFactory = TestLoggerFactory.Create();
        var logger        = loggerFactory.CreateLogger("Test");

        var error = new SingleError(
            ErrorLocation.EmptyLocation,
            new Exception("Test"),
            ErrorCode.WrongVariableType
        );

        logger.LogError(error);

        var logMessages = loggerFactory.Sink.LogEntries.ToList();

        Assert.NotNull(logMessages[0].Exception);

        Assert.Contains(
            logMessages,
            l => l.LogLevel == LogLevel.Error
              && l.Message!.Equals("Test (Step: n/a)")
        );
    }

    [Fact]
    public void LogError_WhenErrorHasNoException_LogsMessage()
    {
        var loggerFactory = TestLoggerFactory.Create();
        var logger        = loggerFactory.CreateLogger("Test");
        var errorCode     = ErrorCode.CannotConvertNestedEntity;

        var error = new SingleError(ErrorLocation.EmptyLocation, errorCode, "type");

        logger.LogError(error);

        var logMessages = loggerFactory.Sink.LogEntries.ToList();

        Assert.Null(logMessages[0].Exception);

        Assert.Contains(
            logMessages,
            l => l.LogLevel == LogLevel.Error
              && l.Message!.Equals($"{errorCode.GetFormattedMessage("type")} (Step: n/a)")
        );
    }

    [Fact]
    public void LogError_WhenErrorHasExceptionAndLocation_LogsMessage()
    {
        var loggerFactory = TestLoggerFactory.Create();
        var logger        = loggerFactory.CreateLogger("Test");

        const string stepName = "TestStep";

        var location = new ErrorLocation(
            stepName,
            new TextLocation("Text", new TextPosition(1, 1, 1), new TextPosition(2, 2, 2))
        );

        var expected = $"Test (Step: {stepName} {location.TextLocation})";

        var error = new SingleError(location, new Exception("Test"), ErrorCode.UnexpectedEnumValue);

        logger.LogError(error);

        var logMessages = loggerFactory.Sink.LogEntries.ToList();

        Assert.NotNull(logMessages[0].Exception);

        //Test (Step: TestStep Line: 1, Col: 1, Idx: 1 - Line: 2, Col: 2, Idx: 2 Text: Text)
        Assert.Contains(
            logMessages,
            l => l.LogLevel == LogLevel.Error && l.Message!.Equals(expected)
        );
    }

    [Fact]
    public void LogError_WhenErrorHasNoExceptionAndLocation_LogsMessage()
    {
        var loggerFactory = TestLoggerFactory.Create();
        var logger        = loggerFactory.CreateLogger("Test");
        var errorCode     = ErrorCode.ExternalProcessError;

        const string stepName = "TestStep";

        var location = new ErrorLocation(
            stepName,
            new TextLocation("Text", new TextPosition(1, 1, 1), new TextPosition(2, 2, 2))
        );

        var expected =
            $"{errorCode.GetFormattedMessage("error")} (Step: {stepName} {location.TextLocation})";

        var error = new SingleError(location, errorCode, "error");

        logger.LogError(error);

        var logMessages = loggerFactory.Sink.LogEntries.ToList();

        Assert.Null(logMessages[0].Exception);

        Assert.Contains(
            logMessages,
            l => l.LogLevel == LogLevel.Error && l.Message!.Equals(expected)
        );
    }
}
