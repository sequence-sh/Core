using System;
using System.Collections.Generic;
using FluentAssertions;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.TestHarness;
using Xunit;

namespace Reductech.EDR.Core.Tests
{

[AutoTheory.UseTestOutputHelper]
public partial class ExternalContextTests
{
    [Fact]
    public void ContextShouldReturnInjectedObject()
    {
        var objectToInject = new List<string>() { "Hello World" };

        var context = new ExternalContext(null!, null!, ("list", objectToInject));

        var retrievedObject = context.TryGetContext<List<string>>("list");

        retrievedObject.ShouldBeSuccessful();

        retrievedObject.Value.Should().BeEquivalentTo(objectToInject);
    }

    [Fact]
    public void AttemptToRetrieveMissingObjectShouldReturnError()
    {
        var context = new ExternalContext(null!, null!);

        var retrievedObject = context.TryGetContext<string>("list");

        retrievedObject.ShouldBeFailure(ErrorCode.MissingContext.ToErrorBuilder(nameof(String)));
    }
}

}
