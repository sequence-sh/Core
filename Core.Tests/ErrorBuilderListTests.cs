namespace Reductech.EDR.Core.Tests;

public class ErrorBuilderListTests
{
    private const string ErrorSeparator = "; ";

    [Fact]
    public void WithLocation_AddsLocationToErrors()
    {
        var errorBuilders = new[]
        {
            new ErrorBuilder(ErrorCode.DirectoryNotFound, "directory"),
            new ErrorBuilder(ErrorCode.MissingContext,    "context")
        };

        var eb = new ErrorBuilderList(errorBuilders);

        var el = new ErrorLocation("test");

        var errors = eb.WithLocation(el).GetAllErrors();

        foreach (var err in errors)
            Assert.Equal(el, err.Location);
    }

    [Fact]
    public void AsString_JoinsAllErrors()
    {
        var errorBuilders = new[]
        {
            new ErrorBuilder(ErrorCode.RequirementNotMet,     "req"),
            new ErrorBuilder(ErrorCode.ConflictingParameters, "1", "2")
        };

        var eb = new ErrorBuilderList(errorBuilders);

        Assert.Equal(
            eb.ToString(),
            $"{errorBuilders[0].AsString}{ErrorSeparator}{errorBuilders[1].AsString}"
        );
    }
}
