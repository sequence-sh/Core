using System.Collections.Generic;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Utilities.Processes.immutable
{
    internal class Conditional<T> : ImmutableProcess<T>
    {
        /// <inheritdoc />
        public Conditional(string name, ImmutableProcess<bool> @if, ImmutableProcess<T> then, ImmutableProcess<T> @else) : base(name)
        {
            _if = @if;
            _then = then;
            _else = @else;
        }

        private readonly ImmutableProcess<bool> _if;
        private readonly ImmutableProcess<T> _then;
        private readonly ImmutableProcess<T> _else;

        /// <inheritdoc />
        public override async IAsyncEnumerable<IProcessOutput<T>> Execute()
        {
            yield return ProcessOutput<T>.Message($"Testing {_if}");

            bool? success = null;
            var anyErrors = false;

            await foreach (var r in _if.Execute())
            {
                if (r.OutputType == OutputType.Success)
                {
                    success = r.Value;
                }
                else
                {
                    if (r.OutputType == OutputType.Error)
                        anyErrors = true;

                    yield return r.ConvertTo<T>(); //These methods failing is expected so it should not produce an error
                }
            }

            if (!anyErrors)
            {
                if (success.HasValue)
                {
                    if (success.Value)
                    {
                        yield return ProcessOutput<T>.Message($"Condition met, executing {_then}");

                        await foreach (var r in _then.Execute())
                            yield return r;
                    }
                    else if (_else != null)
                    {
                        yield return ProcessOutput<T>.Message($"Condition not met, executing {_else}");

                        await foreach (var r in _else.Execute())
                            yield return r;
                    }
                }
                else
                    yield return ProcessOutput<T>.Error("Could not determine result of conditional");
            }

            
        }
    }
}