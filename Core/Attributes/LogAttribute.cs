namespace Reductech.Sequence.Core.Attributes;

/// <summary>
/// Controls when a property value will output by the logger.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class LogAttribute : Attribute
{
    /// <summary>
    /// Create a new LogAttribute
    /// </summary>
    public LogAttribute(LogOutputLevel logOutputLevel) => LogOutputLevel = logOutputLevel;

    /// <summary>
    /// At what level should this property be output.
    /// </summary>
    public LogOutputLevel LogOutputLevel { get; }
}

/// <summary>
/// Controls when property values should be output
/// </summary>
public enum LogOutputLevel
{
    /// <summary>
    /// Never output this property value
    /// </summary>
    None,

    /// <summary>
    /// Output this property value when logging is set to trace
    /// </summary>
    Trace
}
