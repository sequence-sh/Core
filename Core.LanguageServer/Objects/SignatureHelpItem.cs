namespace Reductech.Sequence.Core.LanguageServer.Objects;

public class SignatureHelpItem
{
    public string Name { get; set; }

    public string Label { get; set; }

    public string Documentation { get; set; }

    public IEnumerable<SignatureHelpParameter> Parameters { get; set; }

    public override bool Equals(object obj) => obj is SignatureHelpItem signatureHelpItem
                                            && Name == signatureHelpItem.Name
                                            && Label == signatureHelpItem.Label
                                            && Documentation == signatureHelpItem.Documentation
                                            && Parameters
                                                   .SequenceEqual(signatureHelpItem.Parameters);

    public override int GetHashCode() => 17 * Name.GetHashCode()
                                       + 23 * Label.GetHashCode()
                                       + 31 * Documentation.GetHashCode()
                                       + Parameters.Aggregate(
                                             37,
                                             (
                                                 current,
                                                 element) => current + element.GetHashCode()
                                         );
}
