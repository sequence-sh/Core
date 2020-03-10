using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using CSharpFunctionalExtensions;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes
{
    /// <summary>
    /// A process that runs a process depending on the success of an assertion.
    /// </summary>
    public class Conditional : Process
    {
        /// <summary>
        /// The process to use as the assertion
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 1)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public Process If { get; set; }

        /// <summary>
        /// If the 'If' process was successful then run this.
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 2)]
        public Process Then { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <summary>
        /// If the 'If' process was unsuccessful then run this.
        /// </summary>
        [DataMember]
        [YamlMember(Order = 3)]
        public Process? Else { get; set; }

        /// <inheritdoc />
        public override IEnumerable<string> GetArgumentErrors()
        {
            return If.GetArgumentErrors().Concat(Then.GetArgumentErrors()).Concat(Else?.GetArgumentErrors()??Enumerable.Empty<string>());
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetSettingsErrors(IProcessSettings processSettings)
        {
            return If.GetSettingsErrors(processSettings).Concat(Then.GetSettingsErrors(processSettings)).Concat(Else?.GetSettingsErrors(processSettings)??Enumerable.Empty<string>());
        }
        /// <inheritdoc />
        public override string GetName() => Else == null? $"If ({If}) then ({Then})" : $"If ({If}) then ({Then}) else ({Else})";
            
        /// <inheritdoc />
        public override async IAsyncEnumerable<Result<string>> Execute(IProcessSettings processSettings)
        {
            yield return Result.Success($"Testing {If}");

            var success = true;
            await foreach (var r in If.Execute(processSettings))
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
                yield return Result.Success($"Assertion Succeeded, executing {Then}");

                await foreach (var r in Then.Execute(processSettings))
                    yield return r;
            }
            else if (Else != null)
            {
                yield return Result.Success($"Assertion Failed, executing {Else}");

                await foreach (var r in Else.Execute(processSettings))
                    yield return r;
            }
            else
                yield return Result.Success("Assertion Failed");
        }
    }
}