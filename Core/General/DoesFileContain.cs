﻿using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// Returns whether a file on the file system contains a particular string.
    /// </summary>
    public class DoesFileContain : CompoundStep<bool>
    {
        /// <inheritdoc />
        public override Result<bool, IRunErrors> Run(StateMonad stateMonad)
        {
            var pathResult = Path.Run(stateMonad);

            if (pathResult.IsFailure) return pathResult.ConvertFailure<bool>();

            if (!File.Exists(pathResult.Value))
                return new RunError($"File '{pathResult.Value}' does not exist", Name, null,
                    ErrorCode.ExternalProcessError);

            var textResult = Text.Run(stateMonad);
            if (textResult.IsFailure) return textResult.ConvertFailure<bool>();

            Maybe<RunError> error;
            string realText;

            try
            {
                realText = File.ReadAllText(pathResult.Value);
                error = Maybe<RunError>.None;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                realText = "";
                error = Maybe<RunError>.From(new RunError(e.Message, Name, null, ErrorCode.ExternalProcessError));
            }
#pragma warning restore CA1031 // Do not catch general exception types

            if (error.HasValue)
                return error.Value;

            var r = realText.Contains(textResult.Value);

            return r;

        }

        /// <summary>
        /// The path to the file or folder to check.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> Path { get; set; } = null!;


        /// <summary>
        /// The text to check for.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> Text { get; set; } = null!;


        /// <inheritdoc />
        public override IStepFactory StepFactory => DoesFileContainStepFactory.Instance;
    }

    /// <summary>
    /// Returns whether a file on the file system contains a particular string.
    /// </summary>
    public class DoesFileContainStepFactory : SimpleStepFactory<DoesFileContain, bool>
    {
        private DoesFileContainStepFactory() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static SimpleStepFactory<DoesFileContain, bool> Instance { get; } = new DoesFileContainStepFactory();
    }
}