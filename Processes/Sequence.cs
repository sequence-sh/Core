using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using CSharpFunctionalExtensions;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes
{
    /// <summary>
    /// Executes each step in sequence until a condition is not met or a process fails.
    /// </summary>
    public class Sequence : Process
    {
        /// <summary>
        /// The name of this process
        /// </summary>
        public override string GetName()
        {
            return string.Join(" then ", Steps.Select(s=>s.GetName()));
        }

        /// <summary>
        /// Steps that make up this process. To be executed in order
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 3)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public List<Process> Steps { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <summary>
        /// Execute the steps in this process until a condition is not met or a step fails 
        /// </summary>
        /// <returns></returns>
        public override async IAsyncEnumerable<Result<string>> Execute(IProcessSettings processSettings) 
        {
            foreach (var process in Steps)
            {
                yield return Result.Success($"Executing '{process.GetName()}'");
                var allGood = true;
                var resultLines = process.Execute(processSettings);
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
        public override bool Equals(object? obj)
        {
            var r = obj is Sequence msp && Steps.SequenceEqual(msp.Steps);
            return r;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return GetName().GetHashCode();
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetArgumentErrors()
        {
            return Steps.SelectMany(process => process.GetArgumentErrors());
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetSettingsErrors(IProcessSettings processSettings)
        {
            return Steps.SelectMany(process => process.GetSettingsErrors(processSettings));
        }
    }
}