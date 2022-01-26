using Reductech.Sequence.Core.LanguageServer;
using Reductech.Sequence.Core.LanguageServer.Objects;

namespace Reductech.Sequence.Core.Tests.LanguageServer;

public class SignatureHelpTest
{
    [Theory]
    [InlineData("ArrayFilter", 0, 12, "ArrayFilter")]
    [InlineData("ArrayFilter", 0, 1,  null)]
    public void ShouldGiveCorrectSignatureHelp(
        string text,
        int line,
        int character,
        string? expectedLabel)
    {
        var sfs = StepFactoryStore.Create();

        var visitor = new SignatureHelpVisitor(new LinePosition(line, character), sfs);

        var signatureHelpResponse = visitor.LexParseAndVisit(
            text,
            x => x.RemoveErrorListeners(),
            x => x.RemoveErrorListeners()
        );

        if (expectedLabel is null)
        {
            signatureHelpResponse.Should().BeNull("Expected Signature Help is null");
            return;
        }

        signatureHelpResponse.Should().NotBeNull();
        signatureHelpResponse!.Signatures.First().Label.Should().Be(expectedLabel);
    }
}
