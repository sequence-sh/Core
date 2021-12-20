namespace Reductech.Sequence.Core.Entities;

/// <summary>
/// Extension methods to help with schemas
/// </summary>
public static class SchemaExtensions
{
    /// <summary>
    /// Convert a schema to an entity
    /// </summary>
    public static Entity ConvertToEntity(this JsonSchema schema)
    {
        return Entity.Create(schema.ToJsonDocument().RootElement);
    }

    /// <summary>
    /// The default validation results
    /// </summary>
    public static readonly ValidationOptions DefaultValidationOptions = new()
    {
        OutputFormat = OutputFormat.Verbose, RequireFormatValidation = true,
    };

    /// <summary>
    /// Get error messages from validation results
    /// </summary>
    public static IEnumerable<(string message, string location)> GetErrorMessages(
        this ValidationResults validationResults)
    {
        if (!validationResults.IsValid)
        {
            if (validationResults.Message is not null)
                yield return (validationResults.Message,
                              validationResults.SchemaLocation.ToString());

            foreach (var nestedResult in validationResults.NestedResults)
            foreach (var errorMessage in GetErrorMessages(nestedResult))
                yield return errorMessage;
        }
    }
}
