namespace Reductech.Sequence.Core.Entities.Schema;

/// <summary>
/// Settings for using a Schema to Transform a value
/// </summary>
public record TransformSettings(
    Formatter DateFormatter,
    Formatter TruthFormatter,
    Formatter FalseFormatter,
    Formatter NullFormatter,
    Formatter MultiValueFormatter,
    bool CaseSensitive,
    bool AllowRounding,
    Maybe<bool> RemoveExtra);
