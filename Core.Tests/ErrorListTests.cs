using System.Linq;
using Reductech.EDR.Core.Internal.Errors;
using Xunit;

namespace Reductech.EDR.Core.Tests;

public class ErrorListTests
{
    private const string ErrorSeparator = "; ";

    [Fact]
    public void AsString_JoinsAllErrors()
    {
        var errors = new[]
        {
            new SingleError(ErrorLocation.EmptyLocation, ErrorCode.CannotInferType, "Blah"),
            new SingleError(ErrorLocation.EmptyLocation, ErrorCode.CSVError)
        };

        var errorList = new ErrorList(errors);

        Assert.Equal(
            errorList.ToString(),
            $"{errors[0].AsString}{ErrorSeparator}{errors[1].AsString}"
        );
    }

    [Fact]
    public void AsStringWithLocation_JoinsAllErrors()
    {
        var errors = new[]
        {
            new SingleError(ErrorLocation.EmptyLocation, ErrorCode.CannotInferType, "Blah"),
            new SingleError(ErrorLocation.EmptyLocation, ErrorCode.CSVError)
        };

        var errorList = new ErrorList(errors);

        Assert.Equal(
            errorList.AsStringWithLocation,
            $"{errors[0].AsStringWithLocation}{ErrorSeparator}{errors[1].AsStringWithLocation}"
        );
    }

    [Fact]
    public void ToErrorBuilder_get_ReturnsAllErrors()
    {
        var errors = new[]
        {
            new SingleError(ErrorLocation.EmptyLocation, ErrorCode.CannotInferType, "Blah"),
            new SingleError(ErrorLocation.EmptyLocation, ErrorCode.CSVError)
        };

        var errorList = new ErrorList(errors);

        var eb = errorList.ToErrorBuilder;

        Assert.Equal(2, eb.GetErrorBuilders().Count());

        Assert.Equal(
            eb.AsString,
            $"{errors[0].AsString}{ErrorSeparator}{errors[1].AsString}"
        );
    }
}
