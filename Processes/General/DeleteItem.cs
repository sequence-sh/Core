using System.ComponentModel.DataAnnotations;
using System.IO;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Deletes a file or folder from the file system.
    /// </summary>
    public class DeleteItemProcessFactory : SimpleRunnableProcessFactory<DeleteItem, Unit>
    {
        private DeleteItemProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleRunnableProcessFactory<DeleteItem, Unit> Instance { get; } = new DeleteItemProcessFactory();
    }

    /// <summary>
    /// Deletes a file or folder from the file system.
    /// </summary>
    public class DeleteItem : CompoundRunnableProcess<Unit>
    {
        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(ProcessState processState)
        {
            var pathResult = Path.Run(processState);
            if (pathResult.IsFailure)
                return pathResult.ConvertFailure<Unit>();

            var path = pathResult.Value;


            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                processState.Logger.LogInformation($"Directory '{path}' Deleted.");
            }
            else if (File.Exists(path))
            {
                File.Delete(path);
                processState.Logger.LogInformation($"File '{path}' Deleted.");
            }
            else
                processState.Logger.LogInformation($"Item '{path}' did not exist.");

            return Unit.Default;

        }

        /// <summary>
        /// The path to the file or folder to delete.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<string> Path { get; set; } = null!;


        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => DeleteItemProcessFactory.Instance;
    }
}
