using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.injection;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.enumerations
{
    /// <summary>
    /// Enumerates through a CSV file 
    /// </summary>
    public class CSV : Enumeration
    {
        internal override Result<IReadOnlyCollection<IProcessInjector>, ErrorList> Elements
        {
            get
            {
                Result<DataTable, ErrorList> csvResult;

                if(CSVFilePath != null)
                    if(CSVText != null)
                        return Result.Failure<IReadOnlyCollection<IProcessInjector>, ErrorList>(new ErrorList{$"Both {nameof(CSVFilePath)} and {nameof(CSVText)} are set."});
                    else
                        csvResult = CsvReader.TryReadCSVFromFile(CSVFilePath, Delimiter, CommentToken, HasFieldsEnclosedInQuotes);
                else if (CSVText != null)
                    csvResult = CsvReader.TryReadCSVFromString(CSVText, Delimiter, CommentToken, HasFieldsEnclosedInQuotes);
                else
                    return Result.Failure<IReadOnlyCollection<IProcessInjector>, ErrorList>(new ErrorList{$"Either {nameof(CSVFilePath)} or {nameof(CSVText)} should be set."});

                if (csvResult.IsFailure)
                    return csvResult.ConvertFailure<IReadOnlyCollection<IProcessInjector>>();


                using var dataTable = csvResult.Value;
                var errors = new ErrorList();
                var injectors = new List<IProcessInjector>();

                var columnInjections = new List<(Injection injection, DataColumn column)>();

                foreach (var hi in InjectColumn)
                {
                    var column = dataTable.Columns[hi.Key];
                    if (column == null) errors.Add($"Could not find column '{hi.Key}'");
                    else
                        columnInjections.Add((hi.Value, column));
                }

                if (errors.Any())
                    return Result
                        .Failure<IReadOnlyCollection<IProcessInjector>,
                            ErrorList>(errors);


                foreach (var dataTableRow in dataTable.Rows.Cast<DataRow>())
                {
                    if (dataTableRow == null) continue;
                    var processInjector = new ProcessInjector();
                    foreach (var (injection, column) in columnInjections)
                    {
                        var val = dataTableRow[column];
                        processInjector.Add(val?.ToString()??string.Empty, injection);
                    }
                    injectors.Add(processInjector);
                }
                return Result.Success<IReadOnlyCollection<IProcessInjector>, ErrorList>(injectors);
            }
        }


        internal override string Name
        {
            get
            {
                if (CSVFilePath != null)
                    return $"Csv from '{CSVFilePath}'";

                else if (CSVText != null)
                    return CSVText.Split("\n")[0];

                return "CSV";
            }
        }

        internal override IEnumerable<string> GetArgumentErrors()
        {
            Result<DataTable, ErrorList> csvResult;

            if(CSVFilePath != null)
                if (CSVText != null)
                {
                    yield return $"Both {nameof(CSVFilePath)} and {nameof(CSVText)} are set.";
                    yield break;
                }
                else
                    csvResult = CsvReader.TryReadCSVFromFile(CSVFilePath, Delimiter, CommentToken, HasFieldsEnclosedInQuotes);
            else if (CSVText != null)
                csvResult = CsvReader.TryReadCSVFromString(CSVText, Delimiter, CommentToken, HasFieldsEnclosedInQuotes);
            else
            {
                yield return $"Either {nameof(CSVFilePath)} or {nameof(CSVText)} should be set.";
                yield break;
            }

            if (csvResult.IsFailure)
                foreach (var errorString in csvResult.Error)
                    yield return errorString;

        }

        /// <summary>
        /// The path to the CSV file.
        /// Either this or CSVText must be set (but not both).
        /// </summary>
        [DataMember]
        [YamlMember]
        public string? CSVFilePath { get; set; }
        
        /// <summary>
        /// Raw Csv.
        /// Either this or CSVFilePath must be set (but not both).
        /// </summary>
        [DataMember]
        [YamlMember]
        public string? CSVText { get; set; }


#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        /// <summary>
        /// List of mappings from headers to property injection
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember]
        public Dictionary<string, Injection>  InjectColumn { get; set; }

        /// <summary>
        /// The delimiter used in the CSV file
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember]
        public string Delimiter { get; set; } = ",";
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <summary>
        /// A string that, when placed at the beginning of a line, indicates that the line is a comment and should be ignored by the parser.
        /// </summary>
        [DataMember]
        [YamlMember]
        public string? CommentToken { get; set; }

        /// <summary>
        /// Determines whether fields are enclosed in quotation marks.
        /// </summary>
        [DataMember]
        [YamlMember]
        public bool HasFieldsEnclosedInQuotes { get; set; } = false;
    }
}