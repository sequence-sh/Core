namespace Reductech.Sequence.Core.LanguageServer.Objects;

public class CompletionRequest : Request
{
    public CompletionTriggerKind CompletionTrigger { get; set; }

    public char? TriggerCharacter { get; set; }
}
