using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// An object which can combine a step with the next step in the sequence.
    /// </summary>
    public interface IStepCombiner
    {
        /// <summary>
        /// Tries to combine this step with the next step in the sequence.
        /// </summary>
        public Result<IStep<Unit>> TryCombine(IStep<Unit> p1, IStep<Unit> p2);

    }
}
