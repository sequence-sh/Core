using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.enumerations;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes
{
    /// <summary>
    /// Performs a nested process once for each element in an enumeration
    /// </summary>
    public class ForEach : Process
    {
        /// <inheritdoc />
        public override IEnumerable<string> GetArgumentErrors()
        {
            if (SubProcess == null)
                yield return $"{nameof(SubProcess)} is null";
            else
            {
                //foreach (var argumentError in SubProcess.GetArgumentErrors(settings)) //TODO look at this - its problematic. There seems to be no way to check the injected argument
                //    yield return argumentError;
            }
                

            if(Enumeration == null)
                yield return $"{nameof(Enumeration)} is null";
            else
                foreach (var argumentError in Enumeration.GetArgumentErrors())
                    yield return argumentError;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetSettingsErrors(IProcessSettings processSettings)
        {
            return SubProcess.GetSettingsErrors(processSettings);
        }



        /// <inheritdoc />
        public override string GetName() => $"Foreach in {Enumeration}, {SubProcess}";

        

        /// <summary>
        /// The enumeration to iterate through.
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 2)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public Enumeration Enumeration { get; set; }

        /// <summary>
        /// The process to run once for each element
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 5, Alias = "RunProcess")]
        public Process SubProcess { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <inheritdoc />
        public override async IAsyncEnumerable<Result<string>> Execute(IProcessSettings processSettings)
        {
            var (_, enumerationSuccess, elements, enumerationError) = Enumeration.Elements;

            if (enumerationSuccess)
            {
                foreach (var error in enumerationError)
                    yield return Result.Failure<string>(error);
                yield break;
            }

            foreach (var processInjector in elements)
            {
                var subProcess = SubProcess; //TODO if we ever try to run these in parallel we will need to clone the process

                var injectionResult = processInjector.Inject(subProcess);

                if (injectionResult.IsFailure)
                {
                    yield return injectionResult.ConvertFailure<string>();
                    yield break;
                }
                
                var resultLines = subProcess.Execute(processSettings);

                await foreach (var rl in resultLines)
                    yield return rl;
            }
        }


        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is ForEach fe && GetName() == fe.GetName();
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return GetName().GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return GetName();
        }
    }
}
