using System.Collections.Generic;
using System.Threading.Tasks;
using Reductech.EDR.Processes.Output;

namespace Reductech.EDR.Processes.Immutable
{
    /// <summary>
    /// Delays for a given amount of time.
    /// </summary>
    public class Delay : ImmutableProcess<Unit>
    {

        /// <summary>
        /// Create a new Delay process.
        /// </summary>
        /// <param name="milliseconds"></param>
        public Delay(int milliseconds)
        {
            Milliseconds = milliseconds;
        }

        /// <summary>
        /// The number of milliseconds to delay
        /// </summary>
        private int Milliseconds { get; }

        /// <inheritdoc />
        public override async IAsyncEnumerable<IProcessOutput<Unit>> Execute()
        {
            await Task.Delay(Milliseconds);

            yield return ProcessOutput<Unit>.Success(Unit.Instance);
        }

        /// <inheritdoc />
        public override string Name => ProcessNameHelper.GetDelayProcessName(Milliseconds);

        /// <inheritdoc />
        public override IProcessConverter? ProcessConverter => null;
    }
}