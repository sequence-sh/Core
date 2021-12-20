namespace Reductech.Sequence.Core.Internal.Errors;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public record ErrorLocation(string? StepName, TextLocation? TextLocation)
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    /// <summary>
    /// Creates an ErrorLocation from a TextLocation
    /// </summary>
    private ErrorLocation(TextLocation textLocation) : this(
        null,
        textLocation
    ) { }

    /// <summary>
    /// Creates an ErrorLocation from a freezable step
    /// </summary>
    public ErrorLocation(IFreezableStep freezableStep) : this(
        freezableStep.StepName,
        freezableStep.TextLocation
    ) { }

    /// <summary>
    /// Creates an ErrorLocation from a  step
    /// </summary>
    public ErrorLocation(IStep step) : this(step.Name, step.TextLocation) { }

    /// <summary>
    /// Creates an ErrorLocation from a freezableErrorLocation
    /// </summary>
    public ErrorLocation(IStepFactory stepFactory, FreezableStepData data) : this(
        stepFactory.TypeName,
        data.Location
    ) { }

    /// <summary>
    /// Creates an ErrorLocation from a StepName
    /// </summary>
    public ErrorLocation(string stepName) : this(stepName, null) { }

    /// <summary>
    /// An empty location
    /// </summary>
    public static ErrorLocation EmptyLocation => new(null, null as TextLocation);

    /// <summary>
    /// Creates a new ErrorLocation from a TextLocation
    /// </summary>
    public static implicit operator ErrorLocation(TextLocation textLocation) => new(textLocation);

    /// <summary>
    /// String representation of this Error Location
    /// </summary>
    public string AsString()
    {
        if (string.IsNullOrWhiteSpace(StepName))
        {
            if (TextLocation is null)
                return "Unknown Location";

            return TextLocation.AsString;
        }

        if (TextLocation is null)
            return StepName;

        return $"{StepName} - {TextLocation}";
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return AsString();
    }
}
