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
//    /// Creates a string from a stream.
//    /// </summary>
//    public sealed class StringFromStream : CompoundStep<StringStream>
//    {
//        /// <inheritdoc />
//        public override async Task<Result<string, IError>> Run(IStateMonad stateMonad,
//            CancellationToken cancellationToken)
//        {
//            var streamResult = await Stream.Run(stateMonad, cancellationToken);

//            if (streamResult.IsFailure)
//                return streamResult.ConvertFailure<string>();

//            streamResult.Value.Stream.Seek(0, SeekOrigin.Begin);

//            var encodingResult = await Encoding.Run(stateMonad, cancellationToken);

//            if (encodingResult.IsFailure)
//                return encodingResult.ConvertFailure<string>();

//            using StreamReader reader = new StreamReader(streamResult.Value.Stream, encodingResult.Value.Convert());
//            var text = await reader.ReadToEndAsync();

//            return text;

//        }

//        /// <summary>
//        /// The stream to read.
//        /// </summary>
//        [StepProperty(1)]
//        [Required]
//        public IStep<StringStream> Stream { get; set; } = null!;

//        /// <summary>
//        /// How the stream is encoded.
//        /// </summary>
//        [StepProperty(2)]
//        [DefaultValueExplanation("UTF8 no BOM")]
//        public IStep<EncodingEnum> Encoding { get; set; } = new Constant<EncodingEnum>(EncodingEnum.UTF8); //TODO move away

//        /// <inheritdoc />
//        public override IStepFactory StepFactory => StringFromStreamFactory.Instance;
//    }

//    /// <summary>
//    /// Creates a string from a stream.
//    /// </summary>
//    public sealed class StringFromStreamFactory : SimpleStepFactory<StringFromStream, string>
//    {
//        private StringFromStreamFactory() { }

//        /// <summary>
//        /// The instance.
//        /// </summary>
//        public static SimpleStepFactory<StringFromStream, string> Instance { get; } = new StringFromStreamFactory();
//    }
//}


