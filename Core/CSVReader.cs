using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks.Dataflow;
using CsvHelper;
using CsvHelper.Configuration;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core
{
    /// <summary>
    /// Helps read blocks
    /// </summary>
    public static class CSVBlockHelper
    {
        /// <summary>
        /// Creates a block that will produce records from the CSV file.
        /// </summary>
        public static ISourceBlock<Entity> ReadCsv(Stream stream,
            Encoding encoding,
            bool ignoreQuotes,
            string delimiter, char? commentToken, IErrorLocation errorLocation)

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
            var block = new TransformManyBlock<TextReader, Entity>(tr=> TryReadCSV(tr, configuration, errorLocation));

            block.Post(textReader);

            block.Complete();

            return block;
        }

        /// <summary>
        /// Reads a csv file
        /// <throws>ErrorException</throws>
        /// </summary>
        private static IEnumerable<Entity> TryReadCSV(TextReader textReader, CsvConfiguration configuration, IErrorLocation errorLocation)
        {
            var reader = new CsvReader(textReader, configuration);


            foreach (var row in reader.GetRecords<dynamic>())
            {
                var dict = row as IDictionary<string, object>;

                yield return Entity.Create(dict!);
            }

            reader.Dispose();
            //try
            //{
            //    var rows = reader.GetRecords<dynamic>()
            //    .Select(x =>
            //    {

            //    });

            //    return rows;
            //}
            //catch (Exception e)
            //{
            //    throw new ErrorException(new ErrorBuilder(e, ErrorCode.CSVError).WithLocation(errorLocation));
            //}

            //if (commentToken != null)
            //    csvParser.CommentTokens = new[] { commentToken };

            //csvParser.SetDelimiters(delimiter);
            //csvParser.HasFieldsEnclosedInQuotes = enclosedInQuotes;

            //string[] headers;

            //try
            //{
            //    headers = csvParser.ReadFields();
            //}
            //catch (MalformedLineException e)
            //{
            //    throw new ErrorException(new ErrorBuilder(e, ErrorCode.CSVError).WithLocation(errorLocation));
            //}

            //var rowNumber = 1;
            //while (!csvParser.EndOfData)
            //{
            //    Entity row;

            //    // Read current line fields, pointer moves to the next line.
            //    try
            //    {
            //        var fields = csvParser.ReadFields();

            //        if (fields.Length != headers.Length)
            //            throw new ErrorException(
            //                new ErrorBuilder($"There were {fields.Length} columns in row {rowNumber} but we expected {headers.Length}.", ErrorCode.CSVError)
            //            .WithLocation(errorLocation));

            //        var pairs = headers.Zip(fields).Select(x => new KeyValuePair<string, EntityValue>(x.First, EntityValue.Create(x.Second)));

            //        row = new Entity(pairs);

            //    }
            //    catch (MalformedLineException e)
            //    {
            //        throw new ErrorException(new ErrorBuilder(e, ErrorCode.CSVError).WithLocation(errorLocation));
            //    }

            //    yield return row;

            //    rowNumber++;
            //}
        }
    }
}
