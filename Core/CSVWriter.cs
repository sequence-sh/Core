using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Parser;

namespace Reductech.EDR.Core
{
    /// <summary>
    /// Helper methods for writing CSV files
    /// </summary>
    public static class CSVWriter
    {
        /// <summary>
        /// Writes entities from an entityStream to a stream in csv format.
        /// </summary>
        public static async Task<Result<DataStream, IError>> WriteCSV(
            IStateMonad stateMonad,
            IStep<EntityStream> entityStream,
            IStep<string> delimiter,
            IStep<EncodingEnum> encoding,
            IStep<string> quoteCharacter,
            IStep<bool> alwaysQuote,
            IStep<string> multiValueDelimiter,
            IStep<string> dateTimeFormat,
            IErrorLocation errorLocation,

            CancellationToken cancellationToken)
        {
            var entityStreamResult = await entityStream.Run(stateMonad, cancellationToken);

            if (entityStreamResult.IsFailure) return entityStreamResult.ConvertFailure<DataStream>();

            var delimiterResult = await delimiter.Run(stateMonad, cancellationToken);

            if (delimiterResult.IsFailure) return delimiterResult.ConvertFailure<DataStream>();

            var encodingResult = await encoding.Run(stateMonad, cancellationToken);

            if (encodingResult.IsFailure) return encodingResult.ConvertFailure<DataStream>();

            var quoteResult = await CSVReader.TryConvertToChar(quoteCharacter, "Quote Character", stateMonad, errorLocation, cancellationToken);
            if (quoteResult.IsFailure) return quoteResult.ConvertFailure<DataStream>();

            var multiValueResult = await CSVReader.TryConvertToChar(multiValueDelimiter, "MultiValue Delimiter", stateMonad, errorLocation, cancellationToken);
            if (multiValueResult.IsFailure) return multiValueResult.ConvertFailure<DataStream>();

            if(multiValueResult.Value is null)
                return new SingleError("MultiValue Delimiter is empty", ErrorCode.CSVError, errorLocation);


            var alwaysQuoteResult = await alwaysQuote.Run(stateMonad, cancellationToken);
            if (alwaysQuoteResult.IsFailure) return alwaysQuoteResult.ConvertFailure<DataStream>();

            var dateTimeResult = await dateTimeFormat.Run(stateMonad, cancellationToken);
            if (dateTimeResult.IsFailure) return dateTimeResult.ConvertFailure<DataStream>();


            var result = await WriteCSV(entityStreamResult.Value,
                encodingResult.Value.Convert(),
                delimiterResult.Value,
                quoteResult.Value,
                multiValueResult.Value.Value,
                alwaysQuoteResult.Value,
                dateTimeResult.Value, cancellationToken)
                    .Map(x=> new DataStream(x))
                ;

            return result;
        }



        /// <summary>
        /// Writes entities from an entityStream to a stream in csv format.
        /// </summary>
        public static async Task<Result<Stream, IError>> WriteCSV(
            EntityStream entityStream,
            Encoding encoding,
            string delimiter,
            char? quoteCharacter,
            char multiValueDelimiter,
            bool alwaysQuote,
            string dateTimeFormat,
            CancellationToken cancellationToken)
        {
            var results = await entityStream.TryGetResultsAsync(cancellationToken);

            if (results.IsFailure)
                return results.ConvertFailure<Stream>();

            var stream = new MemoryStream();

            if (!results.Value.Any())
                return stream;//empty stream

            var textWriter = new StreamWriter(stream, encoding);

            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = delimiter,
                Encoding = encoding,
                SanitizeForInjection = false,
                DetectColumnCountChanges = false
            };

            var options = new TypeConverterOptions { Formats = new[] { dateTimeFormat } };
            configuration.TypeConverterOptionsCache.AddOptions<DateTime>(options);

            if (quoteCharacter.HasValue)
            {
                configuration.Quote = quoteCharacter.Value;
                if (alwaysQuote)
                    configuration.ShouldQuote = (s, context) => true;
            }

            var writer = new CsvWriter(textWriter, configuration);

            var records = results.Value.Select(x => ConvertToObject(x, multiValueDelimiter, dateTimeFormat));

            await writer.WriteRecordsAsync(records);

            await textWriter.FlushAsync();

            return stream;


            static object ConvertToObject(Entity entity, char delimiter, string dateTimeFormat)
            {
                IDictionary<string, object> expandoObject = new ExpandoObject()!;

                foreach (var (key, value) in entity)
                {
                    value.Value.Switch(_ => { },
                        v => expandoObject[key] = v,
                        l => expandoObject[key] = string.Join(delimiter, l.Select(x => x.GetStringValue(dateTimeFormat))));
                }

                return expandoObject;
            }
        }
    }
}
