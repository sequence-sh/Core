using System.ComponentModel.DataAnnotations;
using System.IO;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Deletes a file or folder from the file system.
    /// </summary>
    public class DeleteItem : CompoundStep<Unit>
    {
        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(StateMonad stateMonad)
        {
            var pathResult = Path.Run(stateMonad);
            if (pathResult.IsFailure)
                return pathResult.ConvertFailure<Unit>();

            var path = pathResult.Value;


            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                stateMonad.Logger.LogInformation($"Directory '{path}' Deleted.");
            }
            else if (File.Exists(path))
            {
                File.Delete(path);
                stateMonad.Logger.LogInformation($"File '{path}' Deleted.");
            }
            else
                stateMonad.Logger.LogInformation($"Item '{path}' did not exist.");

            return Unit.Default;

        }

        /// <summary>
        /// The path to the file or folder to delete.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> Path { get; set; } = null!;


        /// <inheritdoc />
        public override IStepFactory StepFactory => DeleteItemStepFactory.Instance;
    }
}
