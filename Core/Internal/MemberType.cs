namespace Reductech.Sequence.Core.Internal;

/// <summary>
/// The type of a step member.
/// </summary>
public enum MemberType
{
    /// <summary>
    /// This is not a member - some error has occured.
    /// </summary>
    NotAMember,

    /// <summary>
    /// The name of a variable
    /// </summary>
    VariableName,

    /// <summary>
    /// A step
    /// </summary>
    Step,

    /// <summary>
    /// A list of steps
    /// </summary>
    StepList,

    /// <summary>
    /// A lambda function
    /// </summary>
    Lambda
}
