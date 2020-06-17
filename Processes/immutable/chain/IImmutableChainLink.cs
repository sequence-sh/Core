﻿using System.Collections.Generic;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Utilities.Processes.immutable.chain
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

    ///// <summary>
    ///// A immutableChain link where the output type is not defined.
    ///// </summary>
    //public interface IImmutableChainLink<in TInput>
    //{
    //    /// <summary>
    //    /// The name of this link in the immutableChain. Will include the names from subsequent links.
    //    /// </summary>
    //    string Name { get; }
    //}
}