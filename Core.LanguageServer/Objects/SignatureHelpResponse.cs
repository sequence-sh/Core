namespace Reductech.Sequence.Core.LanguageServer.Objects;

public class SignatureHelpResponse
{
    public IEnumerable<SignatureHelpItem> Signatures { get; set; }

    public int ActiveSignature { get; set; }

    public int ActiveParameter { get; set; }
}
