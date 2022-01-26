namespace Reductech.Sequence.Core.LanguageServer.Objects;

public class SignatureHelpParameter
{
    public string Name { get; set; }

    public string Label { get; set; }

    public string Documentation { get; set; }

    public override bool Equals(object? obj) => obj is SignatureHelpParameter signatureHelpParameter
                                             && Name == signatureHelpParameter.Name
                                             && Label == signatureHelpParameter.Label
                                             && Documentation
                                             == signatureHelpParameter.Documentation;

    public override int GetHashCode() => 17 * Name.GetHashCode()
                                       + 23 * Label.GetHashCode()
                                       + 31 * Documentation.GetHashCode();

    
}
