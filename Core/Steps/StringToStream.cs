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
    public sealed class StringToStream : CompoundStep<Stream>
    {
        /// <inheritdoc />
        public override async Task<Result<Stream, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var textResult = await String.Run(stateMonad, cancellationToken);

            if (textResult.IsFailure)
                return textResult.ConvertFailure<Stream>();


            var encodingResult = await Encoding.Run(stateMonad, cancellationToken);

            if (encodingResult.IsFailure)
                return encodingResult.ConvertFailure<Stream>();

            byte[] byteArray = encodingResult.Value.Convert().GetBytes(textResult.Value);
            var stream = new MemoryStream(byteArray);

            return stream;
        }

        /// <summary>
        /// The string to convert to a stream.
        /// </summary>
        [StepProperty(Order = 1)]
        [Required]
        public IStep<string> String { get; set; } = null!;

        /// <summary>
        /// How the stream is encoded.
        /// </summary>
        [StepProperty(Order = 2)]
        [DefaultValueExplanation("UTF8 no BOM")]
        public IStep<EncodingEnum> Encoding { get; set; } = new Constant<EncodingEnum>(EncodingEnum.UTF8);

        /// <inheritdoc />
        public override IStepFactory StepFactory => StringToStreamStepFactory.Instance;
    }

    /// <summary>
    /// Converts a string to a stream.
    /// </summary>
    public sealed class StringToStreamStepFactory : SimpleStepFactory<StringToStream, Stream>
    {
        private StringToStreamStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<StringToStream, Stream> Instance { get; } = new StringToStreamStepFactory();

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