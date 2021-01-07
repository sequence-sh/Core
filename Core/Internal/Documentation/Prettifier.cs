using System;
using System.Collections.Generic;
using System.Linq;

namespace Reductech.EDR.Core.Internal.Documentation
{

internal static class Prettifier
{
    internal static IEnumerable<string> CreateMarkdownTable(
        IEnumerable<IReadOnlyCollection<string?>> rows)
    {
        var data = rows.SelectMany(
                (row, rowNumber) =>
                    row.Select(
                        (text, columnNumber) =>
                            (rowNumber, columnNumber, text: Escape(text))
                    )
            )
            .ToList();

        var columnWidthDictionary = data.GroupBy(x => x.columnNumber)
            .ToDictionary(
                x => x.Key,
                x => x.Max(c => c.text.EnumerateRunes().Count())
            );

        foreach (var grouping in
            data.GroupBy(d => d.rowNumber).OrderBy(x => x.Key))
        {
            var terms = new List<string>();
            var i     = 0;

            foreach (var (_, columnNumber, text) in grouping.OrderBy(r => r.columnNumber))
            {
                while (i < columnNumber)
                {
                    terms.Add(new string(' ', columnWidthDictionary[i]));
                    i++;
                }

                terms.Add(text.PadRight(columnWidthDictionary[columnNumber]));
                i++;
            }

            while (i < columnWidthDictionary.Count)
            {
                terms.Add(new string(' ', columnWidthDictionary[i]));
                i++;
            }

            var s = $"|{string.Join('|', terms)}|";
            yield return s;

            if (grouping.Key == 0) //create dashes row
            {
                yield return @$"|{
                        string.Join('|', columnWidthDictionary
                                        .OrderBy(x => x.Key)
                                        .Select(x => ":" + new string('-', Math.Max(x.Value - 2, 1)) + ":"))
                    }|";
            }
        }

        static string Escape(string? s)
        {
            return (s ?? string.Empty)
                .Replace(@"\",   @"\\")
                .Replace(@"*",   @"\*")
                .Replace("|",    @"\|")
                .Replace("\r\n", "<br>")
                .Replace("\n",   " ");
        }
    }

    internal static IEnumerable<string> ArrangeIntoColumns(
        IEnumerable<IReadOnlyCollection<string?>> rows)
    {
        var data = rows.SelectMany(
                (row, rowNumber) =>
                    row.SelectMany(
                        (text, columnNumber) =>
                            (text ?? string.Empty).Split("\n")
                            .Select(
                                (line, lineNumber) => (rowNumber, lineNumber, columnNumber, line)
                            )
                    )
            )
            .ToList();

        var columnWidthDictionary = data.GroupBy(x => x.columnNumber)
            .ToDictionary(x => x.Key, x => x.Max(y => y.line.EnumerateRunes().Count()));

        foreach (var grouping in
            data.GroupBy(d => (d.rowNumber, d.lineNumber))
                .OrderBy(x => x.Key.rowNumber)
                .ThenBy(x => x.Key.lineNumber)
        )
        {
            var terms = new List<string>();
            var i     = 0;

            foreach (var (_, _, columnNumber, line) in grouping.OrderBy(r => r.columnNumber))
            {
                while (i < columnNumber)
                {
                    terms.Add(new string(' ', columnWidthDictionary[i]));
                    i++;
                }

                terms.Add(line.PadRight(columnWidthDictionary[columnNumber]));
                i++;
            }

            var s = string.Join(' ', terms).TrimEnd();
            yield return s;
        }
    }
}

}
