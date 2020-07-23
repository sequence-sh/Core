using System.Collections.Generic;
using Reductech.EDR.Processes.Output;

namespace Reductech.EDR.Processes.Immutable
{
//    /// <summary>
//    /// Returns a given string. Useful for testing.
//    /// </summary>
//    public class ReturnString : ImmutableProcess<string>
//    {
//        /// <inheritdoc />
//        public ReturnString(string result)
//        {
//            _result = result;
//        }

//        private readonly string _result;

//        /// <inheritdoc />
//#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
//        public override async IAsyncEnumerable<IProcessOutput<string>> Execute()
//#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
//        {
//            yield return ProcessOutput<string>.Success(_result);
//        }

//        /// <inheritdoc />
//        public override string Name => ProcessNameHelper.GetReturnValueProcessName(_result);

//        /// <inheritdoc />
//        public override IProcessConverter? ProcessConverter => null;
//    }

}
