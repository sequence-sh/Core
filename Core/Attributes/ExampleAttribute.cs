using System;

namespace Reductech.EDR.Core.Attributes
{

/// <summary>
/// Indicates an example value for this parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class ExampleAttribute : StepPropertyMetadataAttribute
{
    /// <summary>
    /// Creates a new ExampleAttribute.
    /// </summary>
    /// <param name="example"></param>
    public ExampleAttribute(string example)
    {
        Example = example;
    }

    /// <summary>
    /// The example value.
    /// </summary>
    public string Example { get; }

    /// <inheritdoc />
    public override string MetadataFieldName => "Example";

    /// <inheritdoc />
    public override string MetadataFieldValue => Example;
}

}
