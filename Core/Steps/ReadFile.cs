using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Reads text from a file.
    /// </summary>
    public sealed class ReadFile : CompoundStep<string>
    {
        /// <inheritdoc />
        public override async Task<Result<string, IError>>  Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var data = await Folder.Run(stateMonad, cancellationToken).Compose(() => FileName.Run(stateMonad, cancellationToken));

            if (data.IsFailure)
                return data.ConvertFailure<string>();


            var path = Path.Combine(data.Value.Item1, data.Value.Item2);

            Result<string, IError> result;
            try
            {
                result = await File.ReadAllTextAsync(path, cancellationToken);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                result = new SingleError(e.Message, Name, null, ErrorCode.ExternalProcessError);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return result;
        }


        /// <summary>
        /// The name of the folder.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> Folder { get; set; } = null!;

        /// <summary>
        /// The name of the file to write to.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> FileName { get; set; } = null!;

        /// <inheritdoc />
        public override IStepFactory StepFactory => ReadFileStepFactory.Instance;
    }

    /// <summary>
    /// Reads text from a file.
    /// </summary>
    public sealed class ReadFileStepFactory : SimpleStepFactory<ReadFile, string>
    {
        private ReadFileStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<ReadFile, string> Instance { get; } = new ReadFileStepFactory();
    }
}