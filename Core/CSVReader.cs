using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using CsvHelper;
using CsvHelper.Configuration;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Entity = Reductech.EDR.Core.Entities.Entity;

namespace Reductech.EDR.Core
{
    /// <summary>
    /// Helps read blocks
    /// </summary>
    public static class CSVReader
    {
        /// <summary>
        /// Reads a CSV stream to an entity stream based on all the input steps.
        /// </summary>
        /// <returns></returns>
        public static async Task<Result<EntityStream, IError>> ReadCSV(
            IStateMonad stateMonad,
            IStep<Stream> stream,
            IStep<string> delimiter,
            IStep<EncodingEnum> encoding,
            IStep<string> commentCharacter,
            IStep<string> quoteCharacter,
            IStep<string> multiValueDelimiter,
            IErrorLocation errorLocation,
            CancellationToken cancellationToken)
        {
            var testStreamResult = await stream.Run(stateMonad, cancellationToken);
            if (testStreamResult.IsFailure) return testStreamResult.ConvertFailure<EntityStream>();

            var delimiterResult = await delimiter.Run(stateMonad, cancellationToken);
            if (delimiterResult.IsFailure) return delimiterResult.ConvertFailure<EntityStream>();

            var encodingResult = await encoding.Run(stateMonad, cancellationToken);
            if (encodingResult.IsFailure) return encodingResult.ConvertFailure<EntityStream>();

            var quoteResult = await TryConvertToChar(quoteCharacter, "Quote Character", stateMonad, errorLocation, cancellationToken);
            if (quoteResult.IsFailure) return quoteResult.ConvertFailure<EntityStream>();

            var commentResult = await TryConvertToChar(commentCharacter, "Comment Character", stateMonad, errorLocation, cancellationToken);
            if (commentResult.IsFailure) return commentResult.ConvertFailure<EntityStream>();

            var multiValueResult = await TryConvertToChar(multiValueDelimiter, "MultiValue Delimiter", stateMonad, errorLocation, cancellationToken);
            if (multiValueResult.IsFailure) return multiValueResult.ConvertFailure<EntityStream>();


            var block = ReadCSV(testStreamResult.Value,
                encodingResult.Value.Convert(),
                delimiterResult.Value,
                quoteResult.Value,
                commentResult.Value,
                multiValueResult.Value,
                errorLocation);

            var recordStream = new EntityStream(block);

            return recordStream;



        }

        /// <summary>
        /// Tries to convert a string step to a single nullable character.
        /// </summary>
        public static async Task<Result<char?, IError>> TryConvertToChar(IStep<string> step,
            string propertyName,
            IStateMonad stateMonad,
            IErrorLocation errorLocation,
            CancellationToken cancellationToken)
        {
            var charResult = await step.Run(stateMonad, cancellationToken);
            if (charResult.IsFailure) return charResult.ConvertFailure<char?>();

            char? resultChar;

            if (charResult.Value.Length == 0)
                resultChar = null;
            else if (charResult.Value.Length == 1)
                resultChar = charResult.Value.Single();
            else return new SingleError($"{propertyName} must be a single character.", ErrorCode.CSVError, errorLocation);

            return resultChar;
        }

        /// <summary>
        /// Creates a block that will produce records from the CSV file.
        /// </summary>
        public static async IAsyncEnumerable<Entity> ReadCSV(Stream stream,
            Encoding encoding,
            string delimiter,
            char? quoteCharacter,
            char? commentCharacter,
            char? multiValueDelimiter,
            IErrorLocation location)

        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = delimiter,
                Encoding = encoding,
                SanitizeForInjection = false,
                DetectColumnCountChanges = false,
                ReadingExceptionOccurred = HandleException,
            };

            if (quoteCharacter.HasValue)
            {
                configuration.Quote = quoteCharacter.Value;
                configuration.IgnoreQuotes = false;
            }
            else configuration.IgnoreQuotes = true;

            if (commentCharacter.HasValue)
            {
                configuration.Comment = commentCharacter.Value;
                configuration.AllowComments = true;
            }
            else configuration.AllowComments = false;

            var textReader = new StreamReader(stream, encoding);


            var reader = new CsvReader(textReader, configuration);


            await foreach (var row in reader.GetRecordsAsync<dynamic>())
            {
                var dict = row as IDictionary<string, object>;

                var entity = Entity.Create(dict!, multiValueDelimiter);
                yield return entity;
            }

            reader.Dispose();

            bool HandleException(CsvHelperException exception)
            {
                throw new ErrorException(new SingleError(exception, ErrorCode.CSVError, location));
            }
        }
    }
}
