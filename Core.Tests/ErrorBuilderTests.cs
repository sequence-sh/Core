using System;
using Reductech.EDR.Core.Internal.Errors;
using Xunit;

namespace Reductech.EDR.Core.Tests;

public class ErrorBuilderTests
{
    [Fact]
    public void EqualsIErrorBuilder_WhenObjectIsErrorBuilder_ReturnsTrue()
    {
        var eb = new ErrorBuilder(
            ErrorCode.Unknown,
            new ErrorData.ExceptionData(new Exception())
        );

        Assert.True(eb.Equals((IErrorBuilder)eb));
    }

    [Fact]
    public void EqualsIErrorBuilder_WhenObjectIsErrorBuilderList_ReturnsTrue()
    {
        var eb = new ErrorBuilder(
            ErrorCode.Unknown,
            new ErrorData.ExceptionData(new Exception())
        );

        var el = new ErrorBuilderList(new[] { eb });

        Assert.True(eb.Equals((IErrorBuilder)el));
    }

    [Fact]
    public void EqualsErrorBuilder_WhenObjectIsNull_ReturnsFalse()
    {
        var eb = new ErrorBuilder(
            ErrorCode.Unknown,
            new ErrorData.ExceptionData(new Exception())
        );

        Assert.False(eb.Equals(null));
    }

    [Fact]
    public void EqualsIErrorBuilder_WhenObjectIsNull_ReturnsFalse()
    {
        var eb = new ErrorBuilder(
            ErrorCode.Unknown,
            new ErrorData.ExceptionData(new Exception())
        );

        Assert.False(eb.Equals((IErrorBuilder?)null));
    }

    [Fact]
    public void ErrorData_AsString_ReturnsExceptionMessage()
    {
        var ec = ErrorCode.Unknown;

        var eb = new ErrorBuilder(
            ec,
            new ErrorData.ExceptionData(new Exception("Message!"))
        );

        Assert.Equal("Message!", eb.Data.AsString(ec));
    }

    [Fact]
    public void ExceptionData_Equals_WhenObjectIsNull_ReturnsFalse()
    {
        var ob = new ErrorData.ExceptionData(new Exception());

        Assert.False(ob.Equals(null));
    }

    [Fact]
    public void ObjectData_Equals_WhenObjectIsNull_ReturnsFalse()
    {
        var ob = new ErrorData.ObjectData(Array.Empty<object>());

        Assert.False(ob.Equals(null));
    }
}
