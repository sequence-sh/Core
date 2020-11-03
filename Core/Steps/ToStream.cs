using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Converts a string to a stream.
    /// </summary>
    public sealed class ToStream : CompoundStep<Stream>
    {
        /// <inheritdoc />
        public override async Task<Result<Stream, IError>> Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var textResult = await Text.Run(stateMonad, cancellationToken);

            if (textResult.IsFailure)
                return textResult.ConvertFailure<Stream>();


            var encodingResult = await Encoding.Run(stateMonad, cancellationToken);

            if (encodingResult.IsFailure)
                return encodingResult.ConvertFailure<Stream>();

            // convert string to stream
            byte[] byteArray = encodingResult.Value.Convert().GetBytes(textResult.Value);
            //byte[] byteArray = Encoding.ASCII.GetBytes(contents);
            var stream = new MemoryStream(byteArray);

            return stream;
        }

        /// <summary>
        /// The text to convert to a stream.
        /// </summary>
        [StepProperty(Order = 1)]
        [Required]
        public IStep<string> Text { get; set; } = null!;

        /// <summary>
        /// How to encode the string.
        /// </summary>
        [StepProperty(Order = 2)]
        [DefaultValueExplanation("The default encoding")]
        public IStep<EncodingEnum> Encoding { get; set; } = new Constant<EncodingEnum>(EncodingEnum.Default);

        /// <inheritdoc />
        public override IStepFactory StepFactory => ToStreamStepFactory.Instance;
    }

    /// <summary>
    /// Converts a string to a stream.
    /// </summary>
    public sealed class ToStreamStepFactory : SimpleStepFactory<ToStream, Stream>
    {
        private ToStreamStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<ToStream, Stream> Instance { get; } = new ToStreamStepFactory();

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes
        {
            get
            {
                yield return typeof(EncodingEnum);
            }
        }
    }
}