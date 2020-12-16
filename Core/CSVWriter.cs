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
        public static async Task<Result<StringStream, IError>> WriteCSV(
            IStateMonad stateMonad,
            IStep<EntityStream> entityStream,
            IStep<StringStream> delimiter,
            IStep<EncodingEnum> encoding,
            IStep<StringStream> quoteCharacter,
            IStep<bool> alwaysQuote,
            IStep<StringStream> multiValueDelimiter,
            IStep<StringStream> dateTimeFormat,
            IErrorLocation errorLocation,

            CancellationToken cancellationToken)
        {
            var entityStreamResult = await entityStream.Run(stateMonad, cancellationToken);

            if (entityStreamResult.IsFailure) return entityStreamResult.ConvertFailure<StringStream>();

            var delimiterResult = await delimiter.Run(stateMonad, cancellationToken)
                .Map(async x=> await x.GetStringAsync());

            if (delimiterResult.IsFailure) return delimiterResult.ConvertFailure<StringStream>();

            var encodingResult = await encoding.Run(stateMonad, cancellationToken);

            if (encodingResult.IsFailure) return encodingResult.ConvertFailure<StringStream>();

            var quoteResult = await CSVReader.TryConvertToChar(quoteCharacter, "Quote Character", stateMonad, errorLocation, cancellationToken);
            if (quoteResult.IsFailure) return quoteResult.ConvertFailure<StringStream>();

            var multiValueResult = await CSVReader.TryConvertToChar(multiValueDelimiter, "MultiValue Delimiter", stateMonad, errorLocation, cancellationToken);
            if (multiValueResult.IsFailure) return multiValueResult.ConvertFailure<StringStream>();

            if(multiValueResult.Value is null)
                return new SingleError("MultiValue Delimiter is empty", ErrorCode.CSVError, errorLocation);


            var alwaysQuoteResult = await alwaysQuote.Run(stateMonad, cancellationToken);
            if (alwaysQuoteResult.IsFailure) return alwaysQuoteResult.ConvertFailure<StringStream>();

            var dateTimeResult = await dateTimeFormat.Run(stateMonad, cancellationToken)
                .Map(async x=> await x.GetStringAsync());
            if (dateTimeResult.IsFailure) return dateTimeResult.ConvertFailure<StringStream>();


            var result = await WriteCSV(entityStreamResult.Value,
                encodingResult.Value.Convert(),
                delimiterResult.Value,
                quoteResult.Value,
                multiValueResult.Value.Value,
                alwaysQuoteResult.Value,
                dateTimeResult.Value, cancellationToken)
                    .Map(x=> new StringStream(x, encodingResult.Value));

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
                    configuration.ShouldQuote = (_,_) => true;
            }

            var writer = new CsvWriter(textWriter, configuration);

            var records = results.Value.Select(x => ConvertToObject(x, multiValueDelimiter, dateTimeFormat));

            await writer.WriteRecordsAsync(records);

            await textWriter.FlushAsync();

            stream.Seek(0, SeekOrigin.Begin);

            return stream;


            static object ConvertToObject(Entity entity, char delimiter, string dateTimeFormat)
            {
                IDictionary<string, object> expandoObject = new ExpandoObject()!;

                foreach (var entityProperty in entity)
                {
                    var s = GetString(entityProperty.BestValue, delimiter, dateTimeFormat);

                    expandoObject[entityProperty.Name] = s;
                }

                return expandoObject;

                static string GetString(EntityValue ev, char delimiter, string dateTimeFormat)
                {
                    var valueString = ev.Value.Match(
                   _ => "",
                   x => x,
                   x => x.ToString(),
                   x => x.ToString("G17"),
                   x => x.ToString(),
                   x => x.ToString(),
                   x => x.ToString(dateTimeFormat),
                   x => x.ToString(),
                   x =>
                       string.Join(delimiter, x.Select(ev1 => GetString(ev1, delimiter, dateTimeFormat)))
                       );

                    return valueString;
                }
            }
        }
    }
}
