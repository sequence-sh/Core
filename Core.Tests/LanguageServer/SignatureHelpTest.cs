using Reductech.Sequence.Core.LanguageServer;
using Reductech.Sequence.Core.LanguageServer.Objects;

namespace Reductech.Sequence.Core.Tests.LanguageServer;

public class SignatureHelpTest
{
    [Theory]
    [InlineData("ArrayFilter", 0, 12, "ArrayFilter")]
    [InlineData("ArrayFilter", 0, 1,  null)]
    //[InlineData("- log ",      0, 7,  "Value")]
    public void ShouldGiveCorrectSignatureHelp(
        string text,
        int line,
        int character,
        string? expectedLabel)
    {
        var sfs = StepFactoryStore.Create();

        var linePosition = new LinePosition(line, character);

        var response = SignatureHelpHelper.GetSignatureHelpResponse(text, linePosition, sfs);

        if (expectedLabel is null)
        {
            response.Signatures.Should().BeEmpty("Expected Signature Help is empty");
            return;
        }

        response.Signatures.First().Label.Should().Be(expectedLabel);
    }
}
