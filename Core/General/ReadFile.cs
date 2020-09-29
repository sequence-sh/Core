using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// Reads text from a file.
    /// </summary>
    public sealed class ReadFile : CompoundStep<string>
    {
        /// <inheritdoc />
        public override Result<string, IRunErrors> Run(StateMonad stateMonad)
        {
            var data = Folder.Run(stateMonad).Compose(() => FileName.Run(stateMonad));

            if (data.IsFailure)
                return data.ConvertFailure<string>();


            var path = Path.Combine(data.Value.Item1, data.Value.Item2);

            Result<string, IRunErrors> result;
            try
            {
                result = File.ReadAllText(path);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                result = new RunError(e.Message, Name, null, ErrorCode.ExternalProcessError);
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
}