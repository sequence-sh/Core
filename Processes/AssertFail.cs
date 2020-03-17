using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes
{
    /// <summary>
    /// Asserts that a particular process will fail.
    /// </summary>
    public class AssertFail : Process
    {
        /// <summary>
        /// The process that is expected to fail.
        /// </summary>
        [Required]
        [YamlMember(Order = 1)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public Process Process { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <inheritdoc />
        public override IEnumerable<string> GetArgumentErrors()
        {
            if (Process == null)
                yield return $"{nameof(Process)} is null.";
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetSettingsErrors(IProcessSettings processSettings)
        {
            yield break;
        }

        /// <inheritdoc />
        public override string GetName()
        {
            return $"Assert Fail: {Process?.GetName()}";
        }

        /// <inheritdoc />
        public override async IAsyncEnumerable<Result<string>> Execute(IProcessSettings processSettings)
        {
            if (Process == null)
                yield return Result.Failure<string>($"{nameof(Process)} is null.");
            else
            {
                var failed = false;

                var results = Process.Execute(processSettings);
                await foreach (var line in results)
                {
                    if (line.IsSuccess)
                        yield return line;
                    else
                    {
                        yield return Result.Success(line.Error);
                        failed = true;
                    }
                }

                if (failed)
                    yield return Result.Success("Assertion Succeeded");
                else
                {
                    yield return Result.Failure<string>("Assertion Failed - Process was unexpectedly successful");
                }
            }
        }
    }
}