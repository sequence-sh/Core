namespace Reductech.Sequence.Core.LanguageServer.Objects;

public class CompletionItem
{
    public string Label { get; set; }

    public CompletionItemKind Kind { get; set; }

    public IReadOnlyList<CompletionItemTag>? Tags { get; set; }

    public string? Detail { get; set; }

    public string? Documentation { get; set; }

    public bool Preselect { get; set; }

    public string? SortText { get; set; }

    public string? FilterText { get; set; }

    public InsertTextFormat? InsertTextFormat { get; set; }

    public LinePositionSpanTextChange? TextEdit { get; set; }

    public IReadOnlyList<char>? CommitCharacters { get; set; }

    public IReadOnlyList<LinePositionSpanTextChange>? AdditionalTextEdits { get; set; }

    public int Data { get; set; }

    public override string ToString() => $"{{ Label = {Label}, CompletionItemKind = {Kind} }}";

    //public static CompletionItem Create(
    //    OmniSharp.Extensions.LanguageServer.Protocol.Models.CompletionItem ci)
    //{
    //    return new CompletionItem
    //    {
    //        Label            = ci.Label,
    //        Kind             = (CompletionItemKind)(int)ci.Kind,
    //        Detail           = ci.Detail,
    //        Documentation    = ci.Documentation?.ConvertToString(),
    //        Preselect        = ci.Preselect,
    //        SortText         = ci.SortText,
    //        FilterText       = ci.FilterText,
    //        InsertTextFormat = (InsertTextFormat)(int)ci.InsertTextFormat,
    //        TextEdit         = Convert(ci.TextEdit),
    //        CommitCharacters = ci.CommitCharacters?.Select(x => x.First()).ToList(),
    //        AdditionalTextEdits = ci.AdditionalTextEdits
    //            ?.Select(x => Convert(x.NewText, x.Range))
    //            .ToList()
    //    };
    //}

    //private static LinePositionSpanTextChange? Convert(TextEditOrInsertReplaceEdit? edit)
    //{
    //    if (edit is null)
    //        return null;

    //    if (edit.IsTextEdit)
    //        return Convert(edit.TextEdit!.NewText, edit.TextEdit.Range);

    //    if (edit.IsInsertReplaceEdit)
    //    {
    //        return Convert(
    //            edit.InsertReplaceEdit?.NewText ?? "",
    //            edit.InsertReplaceEdit?.Insert
    //         ?? edit.InsertReplaceEdit?.Replace ?? new Range(0, 0, 0, 0)
    //        );
    //    }

    //    return null;
    //}

    //private static LinePositionSpanTextChange Convert(string text, Range range)
    //{
    //    return new LinePositionSpanTextChange
    //    {
    //        NewText     = text,
    //        StartColumn = range.Start.Character,
    //        StartLine   = range.Start.Line,
    //        EndColumn   = range.End.Character,
    //        EndLine     = range.End.Line
    //    };
    //}
}
