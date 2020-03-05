using System.Data;
using System.IO;
using System.Linq;
using CSharpFunctionalExtensions;
using Microsoft.VisualBasic.FileIO;

namespace Processes
{
    internal static class CsvReader
    {

        public static Result<DataTable, ErrorList> TryReadCsv(
            string filePath, string delimiter, string? commentToken, bool enclosedInQuotes)
        {
            var errorsSoFar = new ErrorList();

            if(filePath == null)
                errorsSoFar.Add("File path is null.");
            else if (!File.Exists(filePath))
                errorsSoFar.Add($"'{filePath}' does not exist.");

            if(errorsSoFar.Any())
                return Result.Failure<DataTable, ErrorList>(errorsSoFar);


            using var csvParser = new TextFieldParser(filePath);
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
                return Result.Failure<DataTable, ErrorList>(new ErrorList{e.Message});
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
                            $"There were {fields.Length} columns in row {rowNumber} but we expected {dataTable.Columns.Count}.");
                    else
                    {
                        dataTable.Rows.Add(fields.ToArray<object>());
                    }

                }
                catch (MalformedLineException e)
                {
                    errorsSoFar.Add(e.Message);
                }

                rowNumber++;
            }

            return Result.Success<DataTable, ErrorList>(dataTable);
        }

    }
}
