using System.ComponentModel;

namespace Reductech.Sequence.Core.LanguageServer.Objects;

public enum CompletionTriggerKind
{
    Invoked = 1,
    TriggerCharacter = 2,

    [EditorBrowsable(EditorBrowsableState.Never)]
    TriggerForIncompleteCompletions = 3,
}
