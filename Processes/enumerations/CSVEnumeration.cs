using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using CSharpFunctionalExtensions;
using YamlDotNet.Serialization;

namespace Processes.enumerations
{
    /// <summary>
    /// Enumerates through a CSV file 
    /// </summary>
    public class CsvEnumeration : Enumeration
    {
        internal override Result<IReadOnlyCollection<IProcessInjector>,
            ErrorList> Elements
        {
            get
            {
                var csvResult = CsvReader.TryReadCsv(FilePath, Delimiter, CommentToken, HasFieldsEnclosedInQuotes);

                return csvResult.Bind(TryConvert);

                Result<IReadOnlyCollection<IProcessInjector>,
                    ErrorList>
                    TryConvert
                    (DataTable dataTable)
                {
                    var errors = new ErrorList();
                    var injectors = new List<IProcessInjector>();

                    var columnInjections = new List<(Injection injection, DataColumn column)>();

                    foreach (var hi in HeaderInjections)
                    {
                        var column = dataTable.Columns[hi.Header];
                        if (column == null) errors.Add($"Could not find column '{hi.Header}'");
                        else
                            columnInjections.Add((hi, column));
                    }

                    if (errors.Any())
                        return Result
                            .Failure<IReadOnlyCollection<IProcessInjector>,
                                ErrorList>(errors);

#pragma warning disable CS8606 // Possible null reference assignment to iteration variable
                    foreach (DataRow dataTableRow in dataTable.Rows)
#pragma warning restore CS8606 // Possible null reference assignment to iteration variable
                    {
                        if (dataTableRow != null)
                        {
                            var processInjector = new ProcessInjector();
                            foreach (var (injection, column) in columnInjections)
                            {
                                var val = dataTableRow[column];
                                processInjector.Add(val?.ToString()??string.Empty, injection);
                            }
                            injectors.Add(processInjector);
                        }
                    }

                    return Result.Success<IReadOnlyCollection<IProcessInjector>, ErrorList>(injectors);
                }
            }
        }


        internal override string Name => FilePath;

        /// <summary>
        /// The path to the CSV file
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string FilePath { get; set; }

        /// <summary>
        /// List of mappings from headers to property injections
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember]
        public List<ColumnInjection> HeaderInjections { get; set; }

        /// <summary>
        /// The delimiter used in the CSV file
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember]
        public string Delimiter { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <summary>
        /// A string that, when placed at the beginning of a line, indicates that the line is a comment and should be ignored by the parser.
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember]
        public string? CommentToken { get; set; }

        /// <summary>
        /// Determines whether fields are enclosed in quotation marks.
        /// </summary>
        [DataMember] [YamlMember] public bool HasFieldsEnclosedInQuotes { get; set; }

        ///// <summary>
        ///// Whether to automatically remove duplicate combinations of properties.
        ///// </summary>
        //[DataMember] [YamlMember] public bool RemoveDuplicates { get; set; }
    }
}