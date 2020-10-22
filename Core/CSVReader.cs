using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Microsoft.VisualBasic.FileIO;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core
{
    internal static class CsvReader
    {

        public static Result<DataTable, IErrorBuilder> TryReadCSVFromFile(
            string filePath, string delimiter, string? commentToken, bool enclosedInQuotes)
        {
            if(filePath == null)
                return new ErrorBuilder("File path is null.", ErrorCode.CSVError);
            if (!File.Exists(filePath))
                return new ErrorBuilder($"'{filePath}' does not exist.", ErrorCode.CSVError);

            using var csvParser = new TextFieldParser(filePath);

            return TryReadCSV(csvParser, delimiter, commentToken, enclosedInQuotes);
        }

        /// <summary>
        /// Extracts data from a CSV string
        /// </summary>
        /// <param name="csvString">The CSV string</param>
        /// <param name="delimiter">The delimiter to use</param>
        /// <param name="commentToken">The token to indicate a comment</param>
        /// <param name="enclosedInQuotes">Whether the csv fields are enclosed in quotes</param>
        /// <returns></returns>
        public static Result<DataTable, IErrorBuilder> TryReadCSVFromString(
            string csvString, string delimiter, string? commentToken, bool enclosedInQuotes)
        {
            if(csvString == null)
                return new ErrorBuilder("CSV string is null.", ErrorCode.CSVError);

            var byteArray = Encoding.UTF8.GetBytes( csvString );
            var stream = new MemoryStream( byteArray );

            using var csvParser = new TextFieldParser(stream);

            return TryReadCSV(csvParser, delimiter, commentToken, enclosedInQuotes);
        }

        public static Result<DataTable, IErrorBuilder> TryReadCSV(TextFieldParser csvParser,
            string delimiter, string? commentToken, bool enclosedInQuotes)
        {
            var errorsSoFar = new List<IErrorBuilder>();

            if(commentToken != null)
                csvParser.CommentTokens = new[] {commentToken};

            csvParser.SetDelimiters(delimiter);
            csvParser.HasFieldsEnclosedInQuotes = enclosedInQuotes;

            var dataTable = new DataTable();

            try
            {
                var headers = csvParser.ReadFields();
                foreach (var header in headers)
                    dataTable.Columns.Add(header, typeof(string));
            }
            catch (MalformedLineException e)
            {
                return new ErrorBuilder(e, ErrorCode.CSVError);
            }

            var rowNumber = 1;
            while (!csvParser.EndOfData)
            {
                // Read current line fields, pointer moves to the next line.
                try
                {
                    var fields = csvParser.ReadFields();

                    if (fields.Length != dataTable.Columns.Count)
                        errorsSoFar.Add(
                            new ErrorBuilder($"There were {fields.Length} columns in row {rowNumber} but we expected {dataTable.Columns.Count}.", ErrorCode.CSVError));
                    else
                        dataTable.Rows.Add(fields.ToArray<object>());

                }
                catch (MalformedLineException e)
                {
                    errorsSoFar.Add(new ErrorBuilder(e, ErrorCode.CSVError));
                }

                rowNumber++;
            }

            if (errorsSoFar.Any())
                return Result.Failure<DataTable, IErrorBuilder>(ErrorBuilderList.Combine(errorsSoFar));

            return dataTable;
        }
    }
}
