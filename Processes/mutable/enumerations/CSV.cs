using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.mutable.injection;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.mutable.enumerations
{
    /// <summary>
    /// Enumerates through a CSV file.
    /// </summary>
    public class CSV : Enumeration
    {

        private enum CSVSource
        {
            File, Text, Process   
        }

        internal static Result<EagerEnumerationElements, ErrorList> ConvertDataTable (DataTable dataTable, IReadOnlyDictionary<string, Injection> injectColumns)
        {
            var errors = new ErrorList();
            var injectors = new List<IProcessInjector>();

            var columnInjections = new List<(Injection injection, DataColumn column)>();

            foreach (var (columnName, value) in injectColumns)
            {
                var column = dataTable.Columns[columnName];
                if (column == null) errors.Add($"Could not find column '{columnName}'");
                else
                    columnInjections.Add((value, column));
            }

            if (errors.Any())
                return Result.Failure<EagerEnumerationElements, ErrorList>(errors);

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
            return Result.Success<EagerEnumerationElements, ErrorList>(new EagerEnumerationElements(injectors));
        }


        /// <inheritdoc />
        internal override Result<IEnumerationElements, ErrorList> TryGetElements(IProcessSettings processSettings)
        {
            var sources = new List<CSVSource>();

                if(CSVFilePath != null) sources.Add(CSVSource.File);
                if(CSVText != null) sources.Add(CSVSource.Text);
                if(CSVProcess != null) sources.Add(CSVSource.Process);

                if (sources.Count == 0)
                    return Result.Failure<IEnumerationElements, ErrorList>(new ErrorList
                        {$"Either {nameof(CSVFilePath)}, {nameof(CSVText)}, or {nameof(CSVProcess)} should be set."});
                if (sources.Count > 1)
                    return Result.Failure<IEnumerationElements, ErrorList>(new ErrorList
                    {
                        $"Only one of {nameof(CSVFilePath)}, {nameof(CSVText)}, and {nameof(CSVProcess)} may be set."
                    });


                Result<DataTable, ErrorList> csvResult;

                switch (sources.Single())
                {
                    case CSVSource.File when CSVFilePath != null:
                    {
                        csvResult = CsvReader.TryReadCSVFromFile(CSVFilePath, Delimiter, CommentToken, HasFieldsEnclosedInQuotes); }
                        break;
                    case CSVSource.Text when CSVText != null:
                        csvResult = CsvReader.TryReadCSVFromString(CSVText, Delimiter, CommentToken, HasFieldsEnclosedInQuotes);
                        break;
                    case CSVSource.Process when CSVProcess != null:
                    {
                        var subProcessFreezeResult = CSVProcess.TryFreeze(processSettings);

                        if (subProcessFreezeResult.IsFailure) return subProcessFreezeResult.ConvertFailure<IEnumerationElements>();
                        switch (subProcessFreezeResult.Value)
                        {
                            case ImmutableProcess<string> stringProcess:
                            {
                                var lazyElements = new LazyEnumerationElements(stringProcess, Delimiter, CommentToken, HasFieldsEnclosedInQuotes, 
                                    new ReadOnlyDictionary<string, Injection>(InjectColumns));
                                return Result.Success<IEnumerationElements, ErrorList>(lazyElements);
                            }
                            default:
                                return Result.Failure<IEnumerationElements, ErrorList>(new ErrorList
                                {
                                    $"{nameof(CSVProcess)} should have type string"
                                });
                        }
                    }
                    default: return Result.Failure<IEnumerationElements, ErrorList>(new ErrorList
                    {
                        "Something went wrong getting CSV elements"
                    });
                }

                using var dataTable = csvResult.Value;

                var r = ConvertDataTable(dataTable, InjectColumns);
                return r.Map(x => x as IEnumerationElements);
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
        /// Either this, CSVText, or CSVProcess must be set (but not more than one).
        /// </summary>
        
        [YamlMember]
        public string? CSVFilePath { get; set; }
        
        /// <summary>
        /// Raw CSV.
        /// Either this, CSVFilePath, or CSVProcess must be set (but not more than one).
        /// </summary>
        
        [YamlMember]
        public string? CSVText { get; set; }

        /// <summary>
        /// A process which produces a string in CSV format.
        /// Either this, CSVFilePath, or CSVText must be set (but not more than one).
        /// </summary>
        [YamlMember]
        public Process? CSVProcess { get; set; }


#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        /// <summary>
        /// List of mappings from CSV headers to property injection.
        /// </summary>
        [Required]
        [YamlMember]
        public Dictionary<string, Injection>  InjectColumns { get; set; }

        /// <summary>
        /// The delimiter used in the CSV file.
        /// </summary>
        [Required]
        
        [YamlMember]
        public string Delimiter { get; set; } = ",";
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <summary>
        /// A string that, when placed at the beginning of a line, indicates that the line is a comment and should be ignored by the parser.
        /// </summary>
        
        [YamlMember]
        public string? CommentToken { get; set; }

        /// <summary>
        /// Determines whether fields are enclosed in quotation marks.
        /// </summary>
        
        [YamlMember]
        public bool HasFieldsEnclosedInQuotes { get; set; } = false;
    }
}