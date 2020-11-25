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
using Reductech.EDR.Core.Internal.Errors;

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
        /// <returns></returns>
        public static async Task<Result<Stream, IError>> WriteCSV(
            EntityStream entityStream,
            string delimiter,
            Encoding encoding,
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
                SanitizeForInjection = false
            };

            var writer = new CsvWriter(textWriter, configuration);

            var records = results.Value.Select(x => x.ToSimpleObject());

            await writer.WriteRecordsAsync(records);

            await textWriter.FlushAsync();

            return stream;

        }
    }
}
