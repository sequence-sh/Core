using System;

namespace Reductech.EDR.Core.Attributes
{

/// <summary>
/// An SCL example that will be displayed in the documentation and automatically tested
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class SCLExampleAttribute : Attribute
{
    /// <inheritdoc />
    public SCLExampleAttribute(
        string scl,
        string? description,
        string? expectedOutput,
        params string[]? expectedLogs)
    {
        SCL            = scl;
        Description    = description;
        ExpectedOutput = expectedOutput;
        ExpectedLogs   = expectedLogs;
    }

    /// <summary>
    /// The Example SCL
    /// </summary>
    public string SCL { get; set; }

    /// <summary>
    /// Description of the Example
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Expected output of the Example
    /// </summary>
    public string? ExpectedOutput { get; set; }

    /// <summary>
    /// Expected messages logged by the example
    /// </summary>
    public string[]? ExpectedLogs { get; set; }
}

}
