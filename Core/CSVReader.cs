using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks.Dataflow;
using Microsoft.VisualBasic.FileIO;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core
{
    /// <summary>
    /// Helps read blocks
    /// </summary>
    public class CSVBlockHelper
    {
        /// <summary>
        /// Creates a block that will produce records from the CSV file.
        /// </summary>
        public static ISourceBlock<Entity> ReadCsv(Stream stream,
            Encoding encoding,
            string delimiter, string? commentToken, bool enclosedInQuotes, IErrorLocation errorLocation)

        {
            var textFieldParser = new TextFieldParser(stream, encoding);

            var block = new TransformManyBlock<TextFieldParser, Entity>(tfp=> TryReadCSV(tfp, delimiter, commentToken, enclosedInQuotes, errorLocation));

            block.Post(textFieldParser);

            block.Complete();

            return block;
        }

        /// <summary>
        /// Reads a csv file
        /// <throws>ErrorException</throws>
        /// </summary>
        private static IEnumerable<Entity> TryReadCSV(TextFieldParser csvParser,
            string delimiter, string? commentToken, bool enclosedInQuotes, IErrorLocation errorLocation)
        {

            if (commentToken != null)
                csvParser.CommentTokens = new[] { commentToken };

            csvParser.SetDelimiters(delimiter);
            csvParser.HasFieldsEnclosedInQuotes = enclosedInQuotes;

            string[] headers;

            try
            {
                headers = csvParser.ReadFields();
            }
            catch (MalformedLineException e)
            {
                throw new ErrorException(new ErrorBuilder(e, ErrorCode.CSVError).WithLocation(errorLocation));
            }

            var rowNumber = 1;
            while (!csvParser.EndOfData)
            {
                Entity row;

                // Read current line fields, pointer moves to the next line.
                try
                {
                    var fields = csvParser.ReadFields();

                    if (fields.Length != headers.Length)
                        throw new ErrorException(
                            new ErrorBuilder($"There were {fields.Length} columns in row {rowNumber} but we expected {headers.Length}.", ErrorCode.CSVError)
                        .WithLocation(errorLocation));

                    var pairs = headers.Zip(fields).Select(x => new KeyValuePair<string, string>(x.First, x.Second));

                    row = new Entity(pairs);

                }
                catch (MalformedLineException e)
                {
                    throw new ErrorException(new ErrorBuilder(e, ErrorCode.CSVError).WithLocation(errorLocation));
                }

                yield return row;

                rowNumber++;
            }
        }
    }
}
