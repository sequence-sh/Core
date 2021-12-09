namespace Reductech.EDR.Core.Internal;

/// <summary>
/// Builds step names.
/// </summary>
public interface IStepNameBuilder
{
    /// <summary>
    /// Gets the name of the step from the step arguments
    /// </summary>
    string GetFromArguments(FreezableStepData freezableStepData, IStepFactory stepFactory);
}
