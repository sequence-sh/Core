using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.mutable;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Utilities.Processes.immutable
{
    /// <summary>
    /// Executes each step, one after the another.
    /// Will stop if a process fails.
    /// </summary>
    public class Sequence : ImmutableProcess<Unit>
    {
        /// <summary>
        /// Steps that make up this sequence.
        /// </summary>
        public readonly IReadOnlyCollection<ImmutableProcess<Unit>> Steps;

        /// <inheritdoc />
        public Sequence(IReadOnlyCollection<ImmutableProcess<Unit>> steps)
        {
            Steps = steps;
        }

        /// <summary>
        /// Execute the steps in this process until a condition is not met or a step fails.
        /// </summary>
        /// <returns></returns>
        public override async IAsyncEnumerable<IProcessOutput<Unit>> Execute()
        {
            foreach (var process in Steps)
            {
                var allGood = true;
                var resultLines = process.Execute();
                await foreach (var resultLine in resultLines)
                {
                    if(resultLine.OutputType != OutputType.Success)
                        yield return resultLine;
                    allGood &= resultLine.OutputType != OutputType.Error;
                }
                if(!allGood)
                    yield break;
            }

            yield return ProcessOutput<Unit>.Success(Unit.Instance);
        }

        /// <inheritdoc />
        public override int GetHashCode() => Name.GetHashCode();

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is Sequence imSeq && Steps.Equals(imSeq.Steps);
        }

        /// <inheritdoc />
        public override Result<ImmutableProcess<Unit>> TryCombine(ImmutableProcess<Unit> nextProcess, IProcessSettings processSettings)
        {
            if (Steps.Count == 0)
                return Result.Success(nextProcess);

            var allSteps =
                nextProcess is Sequence nextSequence?  Steps.Concat(nextSequence.Steps) :
                    Steps.Concat(new[] {nextProcess});

            var r = CombineSteps(allSteps, processSettings);

            return Result.Success(r);
        }

        /// <summary>
        /// Combines steps to produce a sequence
        /// </summary>
        public static  ImmutableProcess<Unit> CombineSteps(IEnumerable<ImmutableProcess<Unit>> steps, IProcessSettings processSettings)
        {
            var combinedProcesses = new List<ImmutableProcess<Unit>>();

            ImmutableProcess<Unit>? current = null;

            foreach (var step in steps)
            {
                if (current == null) current = step;
                else
                {
                    var (isSuccess, _, value) = current.TryCombine(step, processSettings);
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

            if (combinedProcesses.Count == 1)
                return combinedProcesses.Single();

            return new Sequence(combinedProcesses);
        }

        /// <inheritdoc />
        public override string Name => ProcessNameHelper.GetSequenceName(Steps.Select(x => x.Name));

        /// <inheritdoc />
        public override IProcessConverter? ProcessConverter => null;
    }
}