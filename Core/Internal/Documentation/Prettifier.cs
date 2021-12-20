using System.Text;

namespace Reductech.Sequence.Core.Internal.Documentation;

internal static class Prettifier
{
    public record Cell(string EscapedText, int EscapedTextWidth, Alignment Alignment)
    {
        public static Cell Create(string? rawText, Alignment alignment)
        {
            var escapedText      = Escape(rawText);
            var escapedTextWidth = escapedText.EnumerateRunes().Count();
            return new Cell(escapedText, escapedTextWidth, alignment);
        }

        static string Escape(string? s)
        {
            return (s ?? string.Empty)
                .Replace(@"\",   @"\\")
                .Replace(@"*",   @"\*")
                .Replace("|",    @"\|")
                .Replace("\r\n", "<br/>")
                .Replace("\n",   " ");
        }
    }

    public enum Alignment { Centre, LeftJustified, RightJustified };

    internal static void CreateMarkdownTable(
        IReadOnlyList<Cell> headers,
        IEnumerable<IReadOnlyCollection<string?>> rawRows,
        StringBuilder sb)
    {
        var rows = rawRows.Select(CreateRow).ToList();

        IReadOnlyList<Cell> CreateRow(IReadOnlyCollection<string?> rawRow)
        {
            if (rawRow.Count > headers.Count)
                throw new Exception("Row is longer than the number of headers");

            return rawRow.Select((x, i) => Cell.Create(x, headers[i].Alignment))
                .ToList();
        }

        var maxRowWidths = headers.Select(
                (x, i) =>
                {
                    var maxCellWidth = rows.Select(r => r[i].EscapedTextWidth).Max();

                    return Math.Max(maxCellWidth, x.EscapedTextWidth);
                }
            )
            .ToList();

        CreateMarkDownRow(headers);

        sb.AppendLine();

        foreach (var (header, maxWidth) in headers.Zip(maxRowWidths))
        {
            sb.Append("|");
            var padding = header.Alignment == Alignment.Centre ? maxWidth - 2 : maxWidth - 1;

            padding = Math.Max(1, padding);

            if (header.Alignment != Alignment.RightJustified)
                sb.Append(":");

            sb.Append(new string('-', padding));

            if (header.Alignment != Alignment.LeftJustified)
                sb.Append(":");
        }

        sb.Append("|");

        foreach (var row in rows)
            CreateMarkDownRow(row);

        sb.AppendLine();

        void CreateMarkDownRow(IReadOnlyList<Cell> cells)
        {
            sb.AppendLine();

            for (var index = 0; index < cells.Count; index++)
            {
                var cell       = cells[index];
                var width      = maxRowWidths[index];
                var whitespace = new string(' ', width - cell.EscapedTextWidth);

                sb.Append('|');
                sb.Append(cell.EscapedText);
                sb.Append(whitespace);
            }

            sb.Append('|');
        }
    }
}
