using System.Collections.Generic;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A runnable step that is not a constant.
    /// </summary>
    public interface ICompoundStep : IStep
    {
        /// <summary>
        /// The factory used to create steps of this type.
        /// </summary>
        IStepFactory StepFactory { get; }

        /// <summary>
        /// Requirements for this step that can only be determined at runtime.
        /// </summary>
        IEnumerable<Requirement> RuntimeRequirements { get; }
    }

    /// <summary>
    /// A runnable step that is not a constant or enumerable.
    /// </summary>
    public interface ICompoundStep<T> : IStep<T>, ICompoundStep
    {
    }
}