using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Microsoft.VisualBasic.FileIO;

namespace Reductech.EDR.Utilities.Processes
{
    internal static class CsvReader
    {

        public static Result<DataTable, ErrorList> TryReadCSVFromFile(
            string filePath, string delimiter, string? commentToken, bool enclosedInQuotes)
        {
            if(filePath == null)
                return Result.Failure<DataTable, ErrorList>(new ErrorList(){"File path is null."});
            if (!File.Exists(filePath))
                return Result.Failure<DataTable, ErrorList>(new ErrorList(){$"'{filePath}' does not exist."});
            
            using var csvParser = new TextFieldParser(filePath);

            return TryReadCSV(csvParser, delimiter, commentToken, enclosedInQuotes);
        }

        public static Result<DataTable, ErrorList> TryReadCSVFromString(
            string csvString, string delimiter, string? commentToken, bool enclosedInQuotes)
        {
            if(csvString == null)
                return Result.Failure<DataTable, ErrorList>(new ErrorList{"CSV string is null."});

            var byteArray = Encoding.UTF8.GetBytes( csvString );
            var stream = new MemoryStream( byteArray );
            
            using var csvParser = new TextFieldParser(stream);

            return TryReadCSV(csvParser, delimiter, commentToken, enclosedInQuotes);
        }

        public static Result<DataTable, ErrorList> TryReadCSV(TextFieldParser csvParser,
            string delimiter, string? commentToken, bool enclosedInQuotes)
        {
            var errorsSoFar = new ErrorList();

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
