using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Utilities.Processes.immutable
{
    internal class Conditional : ImmutableProcess
    {
        /// <inheritdoc />
        public Conditional(string name, ImmutableProcess @if, ImmutableProcess then, ImmutableProcess @else) : base(name)
        {
            _if = @if;
            _then = then;
            _else = @else;
        }

        private readonly ImmutableProcess _if;
        private readonly ImmutableProcess _then;
        private readonly ImmutableProcess _else;

        /// <inheritdoc />
        public override async IAsyncEnumerable<Result<string>> Execute()
        {
            yield return Result.Success($"Testing {_if}");

            var success = true;
            await foreach (var r in _if.Execute())
            {
                if (r.IsSuccess)
                    yield return r;
                else
                {
                    success = false;
                    yield return Result.Success(r.Error); //These methods failing is expected so it should not produce an error
                }
            }

            if (success)
            {
                yield return Result.Success($"Assertion Succeeded, executing {_then}");

                await foreach (var r in _then.Execute())
                    yield return r;
            }
            else if (_else != null)
            {
                yield return Result.Success($"Assertion Failed, executing {_else}");

                await foreach (var r in _else.Execute())
                    yield return r;
            }
            else
                yield return Result.Success("Assertion Failed");
        }
    }
}