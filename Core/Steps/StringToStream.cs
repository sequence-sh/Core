//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.IO;
//using System.Threading;
//using System.Threading.Tasks;
//using CSharpFunctionalExtensions;
//using Reductech.EDR.Core.Attributes;
//using Reductech.EDR.Core.Internal;
//using Reductech.EDR.Core.Internal.Errors;
//using Reductech.EDR.Core.Parser;

//namespace Reductech.EDR.Core.Steps
//{
//    /// <summary>
//    /// Converts a string to a stream.
//    /// </summary>
//    public sealed class StringToStream : CompoundStep<StringStream>
//    {
//        /// <inheritdoc />
//        public override async Task<Result<StringStream, IError>> Run(IStateMonad stateMonad,
//            CancellationToken cancellationToken)
//        {
//            var textResult = await String.Run(stateMonad, cancellationToken);

//            if (textResult.IsFailure)
//                return textResult.ConvertFailure<StringStream>();


//            var encodingResult = await Encoding.Run(stateMonad, cancellationToken);

//            if (encodingResult.IsFailure)
//                return encodingResult.ConvertFailure<StringStream>();

//            byte[] byteArray = encodingResult.Value.Convert().GetBytes(textResult.Value);
//            var stream = new MemoryStream(byteArray);

//            return new StringStream(stream, encodingResult.Value);
//        }

//        /// <summary>
//        /// The string to convert to a stream.
//        /// </summary>
//        [StepProperty(1)]
//        [Required]
//        public IStep<StringStream> String { get; set; } = null!;

//        /// <summary>
//        /// How the stream is encoded.
//        /// </summary>
//        [StepProperty(2)]
//        [DefaultValueExplanation("UTF8 no BOM")]
//        public IStep<EncodingEnum> Encoding { get; set; } = new Constant<EncodingEnum>(EncodingEnum.UTF8);

//        /// <inheritdoc />
//        public override IStepFactory StepFactory => StringToStreamStepFactory.Instance;
//    }

//    /// <summary>
//    /// Converts a string to a stream.
//    /// </summary>
//    public sealed class StringToStreamStepFactory : SimpleStepFactory<StringToStream, StringStream>
//    {
//        private StringToStreamStepFactory() { }

//        /// <summary>
//        /// The instance.
//        /// </summary>
//        public static SimpleStepFactory<StringToStream, StringStream> Instance { get; } = new StringToStreamStepFactory();
//    }
//}