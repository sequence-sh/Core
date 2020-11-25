using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Reductech.EDR.Core.Entities;

namespace Reductech.EDR.Core
{
    /// <summary>
    /// Helps read blocks
    /// </summary>
    public static class CSVReader
    {
        /// <summary>
        /// Creates a block that will produce records from the CSV file.
        /// </summary>
        public static async IAsyncEnumerable<Entity> ReadCsv(Stream stream,
            Encoding encoding,
            bool ignoreQuotes,
            string delimiter, char? commentToken)

        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = delimiter,
                IgnoreQuotes = ignoreQuotes
            };

            if (commentToken.HasValue)
            {
                configuration.Comment = commentToken.Value;
                configuration.AllowComments = true;
            }

            var textReader = new StreamReader(stream, encoding);


            var reader = new CsvReader(textReader, configuration);


            await foreach (var row in reader.GetRecordsAsync<dynamic>())
            {
                var dict = row as IDictionary<string, object>;

                var entity = Entity.Create(dict!);
                yield return entity;
            }

            reader.Dispose();
        }
    }
}
