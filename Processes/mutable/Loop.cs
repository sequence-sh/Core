using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.mutable.enumerations;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.mutable
{
    /// <summary>
    /// Performs a nested process once for each element in an enumeration.
    /// </summary>
    public class Loop : Process
    {
        /// <inheritdoc />
        public override Result<ImmutableProcess, ErrorList> TryFreeze(IProcessSettings processSettings)
        {
            var initialErrors = new ErrorList();

            if (Do == null)
                initialErrors.Add($"{nameof(Do)} is null");

            if(For == null)
                initialErrors.Add($"{nameof(For)} is null");
            else
                initialErrors.AddRange(For.GetArgumentErrors());

            if (initialErrors.Any() || For == null || Do == null)
                return Result.Failure<ImmutableProcess, ErrorList>(initialErrors);
            
            //TODO Allow loop to be compiled lazily

            var (_, isEnumerationFailure, elements, enumerationError) = For.Elements;

            if (isEnumerationFailure) return Result.Failure<ImmutableProcess, ErrorList>(enumerationError);

            var finalProcesses = new List<ImmutableProcess<Unit>>();

            foreach (var processInjector in elements)
            {
                var subProcess = Do;

                var (_, isInjectionFailure, injectionError) = processInjector.Inject(subProcess);

                if (isInjectionFailure)
                    return Result.Failure<ImmutableProcess, ErrorList>(new ErrorList(injectionError));

                var freezeResult = subProcess.TryFreeze(processSettings);

                if (freezeResult.IsFailure) return freezeResult;

                if(freezeResult.Value is ImmutableProcess<Unit> unitProcess)
                    finalProcesses.Add(unitProcess);
                else
                {
                    return Result.Failure<ImmutableProcess, ErrorList>(new ErrorList(
                        $"Process '{freezeResult.Value.Name}' has result type {freezeResult.Value.ResultType.Name} but members of a loop should have result type void."));
                }
            }

            var finalSequence = immutable.Sequence.CombineSteps(finalProcesses, processSettings);

            return Result.Success<ImmutableProcess, ErrorList>(finalSequence);
        }


        /// <inheritdoc />
        public override string GetReturnTypeInfo() => nameof(Unit);

        /// <inheritdoc />
        public override string GetName() => $"Foreach in {For}, {Do}";
        
        /// <summary>
        /// The enumeration to iterate through.
        /// </summary>
        [Required]
        
        [YamlMember(Order = 2)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public Enumeration For { get; set; }

        /// <summary>
        /// The process to run once for each element.
        /// </summary>
        [Required]
        
        [YamlMember(Order = 5)]
        public Process Do { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        /// <inheritdoc />
        public override string ToString()
        {
            return GetName();
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetRequirements()
        {
            return Do.GetRequirements();
        }
    }
}
