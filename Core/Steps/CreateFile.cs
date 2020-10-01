using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Creates a file.
    /// </summary>
    public class CreateFile : CompoundStep<Unit>
    {
        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(StateMonad stateMonad)
        {
            var result = Path.Run(stateMonad)
                .Compose(() => Text.Run(stateMonad))
                .Bind(a=> CreateFile1(a.Item1, a.Item2));

            return result;

            static Result<Unit, IRunErrors> CreateFile1(string path, string text)
            {
                Result<Unit, IRunErrors> r1;

                try
                {
                    using var sw = File.CreateText(path);
                    sw.Write(text);
                    r1 = Unit.Default;
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception e)
                {
                    r1 = new RunError(e.Message, nameof(CreateFile), null, ErrorCode.ExternalProcessError);
                }
#pragma warning restore CA1031 // Do not catch general exception types

                return r1;
            }

        }


        /// <summary>
        /// The path to the file to create.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> Path { get; set; } = null!;


        /// <summary>
        /// The text to put in the file.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> Text { get; set; } = null!;

        /// <inheritdoc />
        public override IStepFactory StepFactory => CreateFileStepFactory.Instance;
    }

    /// <summary>
    /// Creates a file.
    /// </summary>
    public class CreateFileStepFactory : SimpleStepFactory<CreateFile, Unit>
    {
        private CreateFileStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<CreateFile, Unit> Instance { get; } = new CreateFileStepFactory();
    }
}