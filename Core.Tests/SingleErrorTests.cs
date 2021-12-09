using System;
using Reductech.EDR.Core.Internal.Errors;
using Xunit;

namespace Reductech.EDR.Core.Tests;

public class SingleErrorTests
{
    [Fact]
    public void Exception_WhenErrorDataIsException_ReturnsException()
    {
        var ex    = new Exception("test");
        var error = new SingleError(ErrorLocation.EmptyLocation, ex, ErrorCode.IndexOutOfBounds);
        Assert.Same(ex, error.Exception);
    }

    [Fact]
    public void Exception_WhenErrorDataIsNotException_ReturnsNull()
    {
        var ex    = new Exception("test");
        var error = new SingleError(ErrorLocation.EmptyLocation, ErrorCode.UnitExpected);
        Assert.Null(error.Exception);
    }

    [Fact]
    public void ToString_ReturnsMessageAndLocation()
    {
        var location = new ErrorLocation("test");
        var error    = new SingleError(location, ErrorCode.StepDoesNotExist, "test");
        Assert.Equal($"{error.Message} in {location.AsString()}", error.ToString());
    }
}
