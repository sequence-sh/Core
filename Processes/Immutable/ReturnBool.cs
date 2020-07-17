using System.Collections.Generic;
using Reductech.EDR.Processes.Output;

namespace Reductech.EDR.Processes.Immutable
{
    /// <summary>
    /// Returns a given boolean.
    /// </summary>
    public class ReturnBool : IImmutableProcess<bool>
    {
        /// <summary>
        /// Create a new ReturnBool
        /// </summary>
        /// <param name="resultBool"></param>
        public ReturnBool(bool resultBool)
        {
            ResultBool = resultBool;
        }

        /// <inheritdoc />
#pragma warning disable 1998
        public async IAsyncEnumerable<IProcessOutput<bool>> Execute()
#pragma warning restore 1998
        {
            yield return ProcessOutput<bool>.Success(ResultBool);
        }

        private bool ResultBool { get; }


        /// <inheritdoc />
        public string Name => ProcessNameHelper.GetReturnBoolProcessName(ResultBool);

        /// <inheritdoc />
        public IProcessConverter? ProcessConverter => null;
    }
}