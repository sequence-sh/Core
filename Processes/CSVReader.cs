using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Microsoft.VisualBasic.FileIO;

namespace Reductech.EDR.Processes
{
    internal static class CsvReader
    {

        public static Result<DataTable> TryReadCSVFromFile(
            string filePath, string delimiter, string? commentToken, bool enclosedInQuotes)
        {
            if(filePath == null)
                return Result.Failure<DataTable>("File path is null.");
            if (!File.Exists(filePath))
                return Result.Failure<DataTable>($"'{filePath}' does not exist.");

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
        public static Result<DataTable> TryReadCSVFromString(
            string csvString, string delimiter, string? commentToken, bool enclosedInQuotes)
        {
            if(csvString == null)
                return Result.Failure<DataTable>("CSV string is null.");

            var byteArray = Encoding.UTF8.GetBytes( csvString );
            var stream = new MemoryStream( byteArray );

            using var csvParser = new TextFieldParser(stream);

            return TryReadCSV(csvParser, delimiter, commentToken, enclosedInQuotes);
        }

        public static Result<DataTable> TryReadCSV(TextFieldParser csvParser,
            string delimiter, string? commentToken, bool enclosedInQuotes)
        {
            var errorsSoFar = new List<string>();

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
                return Result.Failure<DataTable>(e.Message);
            }

            var rowNumber = 1;
            while (!csvParser.EndOfData)
            {
                // Read current line fields, pointer moves to the next line.
                try
                {
                    var fields = csvParser.ReadFields();

                    if (fields.Length != dataTable.Columns.Count)
                        errorsSoFar.Add($"There were {fields.Length} columns in row {rowNumber} but we expected {dataTable.Columns.Count}.");
                    else
                        dataTable.Rows.Add(fields.ToArray<object>());

                }
                catch (MalformedLineException e)
                {
                    errorsSoFar.Add(e.Message);
                }

                rowNumber++;
            }

            if (errorsSoFar.Any())
                return Result.Failure<DataTable>(string.Join("\r\n", errorsSoFar));

            return Result.Success(dataTable);
        }
    }
}
