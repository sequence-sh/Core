using System.Collections.Generic;
using Reductech.EDR.Processes.Output;

namespace Reductech.EDR.Processes.Immutable.Chain
{
    /// <summary>
    /// A immutableChain link where the output type is not defined.
    /// </summary>
    public interface IImmutableChainLink<in TInput, out TFinal>
    {
        /// <summary>
        /// Execute this link and all subsequent links.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        IAsyncEnumerable<IProcessOutput<TFinal>> Execute(TInput input);

        /// <summary>
        /// The name of this link in the immutableChain. Will include the names from subsequent links.
        /// </summary>
        string Name { get; }
    }
}