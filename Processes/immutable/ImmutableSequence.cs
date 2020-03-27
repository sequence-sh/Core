using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Utilities.Processes.immutable
{
    internal class ImmutableSequence : ImmutableProcess<Unit>
    {
        private readonly IReadOnlyCollection<ImmutableProcess<Unit>> _steps;

        /// <inheritdoc />
        public ImmutableSequence(string name, IReadOnlyCollection<ImmutableProcess<Unit>> steps) : base(name)
        {
            _steps = steps;
        }

        /// <summary>
        /// Execute the steps in this process until a condition is not met or a step fails.
        /// </summary>
        /// <returns></returns>
        public override async IAsyncEnumerable<IProcessOutput<Unit>> Execute()
        {
            foreach (var process in _steps)
            {
                var allGood = true;
                var resultLines = process.ExecuteUntyped();
                await foreach (var resultLine in resultLines)
                {
                    if(resultLine.OutputType != OutputType.Success)
                        yield return resultLine.ConvertTo<Unit>();
                    allGood &= resultLine.OutputType != OutputType.Error;
                }
                if(!allGood)
                    yield break;
            }

            yield return ProcessOutput<Unit>.Success(Unit.Instance);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is ImmutableSequence imSeq && _steps.Equals(imSeq._steps);
        }

        /// <inheritdoc />
        public override Result<ImmutableProcess<Unit>> TryCombine(ImmutableProcess<Unit> nextProcess)
        {
            if (_steps.Count == 0)
            {
                return Result.Success(nextProcess);
            }
            else
            {

                var allSteps =
                    nextProcess is ImmutableSequence nextSequence?  _steps.Concat(nextSequence._steps) :
                        _steps.Concat(new[] {nextProcess});

                var r = CombineSteps(allSteps);

                return Result.Success<ImmutableProcess<Unit>>(r);
            }
        }

        public static ImmutableSequence CombineSteps(IEnumerable<ImmutableProcess<Unit>> steps)
        {
            var combinedProcesses = new List<ImmutableProcess<Unit>>();

            ImmutableProcess<Unit>? current = null;

            foreach (var step in steps)
            {
                if (current == null) current = step;
                else
                {
                    var (isSuccess, _, value) = current.TryCombine(step);
                    if (isSuccess)
                        current = value;
                    else
                    {
                        combinedProcesses.Add(current);
                        current = step;
                    }
                }
            }
            if(current != null) combinedProcesses.Add(current);

            var newName = string.Join(" then ", combinedProcesses.Select(cp => cp.Name));

            return new ImmutableSequence(newName, combinedProcesses);
        }
    }
}