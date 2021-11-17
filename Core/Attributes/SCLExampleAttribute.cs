using System;
using System.Linq;

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
        string? expectedOutput = null,
        string? description = null,
        params string[]? expectedLogs) : this(
        scl,
        expectedOutput,
        description,
        null,
        null,
        expectedLogs:
        expectedLogs
    ) { }

    public SCLExampleAttribute(
        string scl,
        string? expectedOutput,
        string? description,
        string[]? variableNamesToInject,
        string[]? variableValuesToInject,
        params string[]? expectedLogs)
    {
        SCL            = scl;
        Description    = description;
        ExpectedOutput = expectedOutput;

        if (variableNamesToInject is not null && variableValuesToInject is not null)
        {
            VariableInjections = variableNamesToInject.Zip(variableValuesToInject)
                .Select(x => (x.First, x.Second))
                .ToArray();
        }

        ExpectedLogs = expectedLogs;
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

    /// <summary>
    /// Variables to inject into the test
    /// </summary>
    public (string VariableName, string Value)[]? VariableInjections { get; set; }

    /// <summary>
    /// Whether this example will be executed in unit tests. (Deserialization will still be tested)
    /// </summary>
    public bool ExecuteInTests { get; set; } = true;

    /// <summary>
    /// Convert this attribute to an SCL Example
    /// </summary>
    public SCLExample ToSCLExample => new(
        SCL,
        Description,
        ExpectedOutput,
        ExpectedLogs,
        VariableInjections,
        ExecuteInTests
    );

    //private object? ConvertValue(object? o)
    //{
    //    var ev = EntityValue.CreateFromObject(o);

    //    if (ev is EntityValue.NestedList nl)
    //    {
    //        return nl.Value.Select(x => ConvertValue(x.ObjectValue)).ToSCLArray();
    //    }

    //    var ov = ev.ObjectValue;

    //    return ov;
    //}
}

/// <summary>
/// An SCL Example of using a particular step
/// </summary>
public record SCLExample(
    string SCL,
    string? Description,
    string? ExpectedOutput,
    string[]? ExpectedLogs,
    (string VariableName, string Value)[]? VariableInjections,
    bool ExecuteInTests);

}
