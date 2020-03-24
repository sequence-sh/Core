using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Utilities.Processes.immutable
{
    internal class ImmutableSequence : ImmutableProcess
    {
        private readonly IReadOnlyCollection<ImmutableProcess> _steps;

        /// <inheritdoc />
        public ImmutableSequence(string name, IReadOnlyCollection<ImmutableProcess> steps) : base(name)
        {
            _steps = steps;
        }

        /// <summary>
        /// Execute the steps in this process until a condition is not met or a step fails 
        /// </summary>
        /// <returns></returns>
        public override async IAsyncEnumerable<Result<string>> Execute()
        {
            foreach (var process in _steps)
            {
                var allGood = true;
                var resultLines = process.Execute();
                await foreach (var resultLine in resultLines)
                {
                    yield return resultLine;
                    allGood &= resultLine.IsSuccess;
                }
                if(!allGood)
                    yield break;
            }
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
        public override Result<ImmutableProcess> TryCombine(ImmutableProcess nextProcess)
        {
            if (_steps.Count == 0)
            {
                return Result.Success(nextProcess);
            }
            else if (nextProcess is ImmutableSequence nextSequence)
            {
                var allSteps = _steps.Concat(nextSequence._steps);

                var r = CombineSteps(allSteps);

                return Result.Success<ImmutableProcess>(r);
            }
            else
            {
                var r = CombineSteps(_steps.Concat(new[]{nextProcess}));

                return Result.Success<ImmutableProcess>(r);

            }
        }

        public static ImmutableSequence CombineSteps(IEnumerable<ImmutableProcess> steps)
        {
            var combinedProcesses = new List<ImmutableProcess>();

            ImmutableProcess? current = null;

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