namespace Reductech.Sequence.Core.LanguageServer.Objects;

public class DocumentationComment
{
    public static readonly DocumentationComment Empty = new();

    public string SummaryText { get; }

    public DocumentationItem[] TypeParamElements { get; }

    public DocumentationItem[] ParamElements { get; }

    public string ReturnsText { get; }

    public string RemarksText { get; }

    public string ExampleText { get; }

    public string ValueText { get; }

    public DocumentationItem[] Exception { get; }

    internal class DocumentationItemBuilder
    {
        public string Name { get; set; }

        public StringBuilder Documentation { get; set; }

        public DocumentationItemBuilder() => Documentation = new StringBuilder();

        public DocumentationItem ConvertToDocumentedObject() => new(Name, Documentation.ToString());
    }

    public DocumentationComment(
        string summaryText = "",
        DocumentationItem[]? typeParamElements = null,
        DocumentationItem[]? paramElements = null,
        string returnsText = "",
        string remarksText = "",
        string exampleText = "",
        string valueText = "",
        DocumentationItem[]? exception = null)
    {
        SummaryText       = summaryText;
        TypeParamElements = typeParamElements ?? Array.Empty<DocumentationItem>();
        ParamElements     = paramElements ?? Array.Empty<DocumentationItem>();
        ReturnsText       = returnsText;
        RemarksText       = remarksText;
        ExampleText       = exampleText;
        ValueText         = valueText;
        Exception         = exception ?? Array.Empty<DocumentationItem>();
    }

    public static DocumentationComment From(
        string xmlDocumentation,
        string lineEnding)
    {
        if (string.IsNullOrEmpty(xmlDocumentation))
            return Empty;

        var input          = new StringReader("<docroot>" + xmlDocumentation + "</docroot>");
        var stringBuilder1 = new StringBuilder();
        var source1        = new List<DocumentationItemBuilder>();
        var source2        = new List<DocumentationItemBuilder>();
        var stringBuilder2 = new StringBuilder();
        var stringBuilder3 = new StringBuilder();
        var stringBuilder4 = new StringBuilder();
        var stringBuilder5 = new StringBuilder();
        var source3        = new List<DocumentationItemBuilder>();

        using (var xmlReader = XmlReader.Create(input))
        {
            try
            {
                xmlReader.Read();
                string        str            = null;
                StringBuilder stringBuilder6 = null;

                do
                {
                    if (xmlReader.NodeType == XmlNodeType.Element)
                    {
                        str = xmlReader.Name.ToLowerInvariant();

                        switch (str)
                        {
                            case "br":
                            case "para":
                                stringBuilder6.Append(lineEnding);
                                break;
                            case "example":
                                stringBuilder6 = stringBuilder4;
                                break;
                            case "exception":
                                var documentationItemBuilder1 = new DocumentationItemBuilder();

                                documentationItemBuilder1.Name =
                                    GetCref(xmlReader["cref"]).TrimEnd();

                                stringBuilder6 = documentationItemBuilder1.Documentation;
                                source3.Add(documentationItemBuilder1);
                                break;
                            case "filterpriority":
                                xmlReader.Skip();
                                break;
                            case "param":
                                var documentationItemBuilder2 = new DocumentationItemBuilder();

                                documentationItemBuilder2.Name = TrimMultiLineString(
                                    xmlReader["name"],
                                    lineEnding
                                );

                                stringBuilder6 = documentationItemBuilder2.Documentation;
                                source2.Add(documentationItemBuilder2);
                                break;
                            case "paramref":
                                stringBuilder6.Append(xmlReader["name"]);
                                stringBuilder6.Append(" ");
                                break;
                            case "remarks":
                                stringBuilder6 = stringBuilder3;
                                break;
                            case "returns":
                                stringBuilder6 = stringBuilder2;
                                break;
                            case "see":
                                stringBuilder6.Append(GetCref(xmlReader["cref"]));
                                stringBuilder6.Append(xmlReader["langword"]);
                                break;
                            case "seealso":
                                stringBuilder6.Append("See also: ");
                                stringBuilder6.Append(GetCref(xmlReader["cref"]));
                                break;
                            case "summary":
                                stringBuilder6 = stringBuilder1;
                                break;
                            case "typeparam":
                                var documentationItemBuilder3 = new DocumentationItemBuilder();

                                documentationItemBuilder3.Name = TrimMultiLineString(
                                    xmlReader["name"],
                                    lineEnding
                                );

                                stringBuilder6 = documentationItemBuilder3.Documentation;
                                source1.Add(documentationItemBuilder3);
                                break;
                            case "typeparamref":
                                stringBuilder6.Append(xmlReader["name"]);
                                stringBuilder6.Append(" ");
                                break;
                            case "value":
                                stringBuilder6 = stringBuilder5;
                                break;
                        }
                    }
                    else if (xmlReader.NodeType == XmlNodeType.Text && stringBuilder6 != null)
                    {
                        if (str == "code")
                            stringBuilder6.Append(xmlReader.Value);
                        else
                            stringBuilder6.Append(TrimMultiLineString(xmlReader.Value, lineEnding));
                    }
                } while (xmlReader.Read());
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        return new DocumentationComment(
            stringBuilder1.ToString(),
            source1.Select(s => s.ConvertToDocumentedObject()).ToArray(),
            source2.Select(s => s.ConvertToDocumentedObject()).ToArray(),
            stringBuilder2.ToString(),
            stringBuilder3.ToString(),
            stringBuilder4.ToString(),
            stringBuilder5.ToString(),
            source3.Select(s => s.ConvertToDocumentedObject()).ToArray()
        );
    }

    private static string TrimMultiLineString(string input, string lineEnding)
    {
        var source = input.Split(
            new string[2] { "\n", "\r\n" },
            StringSplitOptions.RemoveEmptyEntries
        );

        return string.Join(lineEnding, source.Select(TrimStartRetainingSingleLeadingSpace));
    }

    private static string GetCref(string? cref)
    {
        if (cref == null || cref.Trim().Length == 0)
            return "";

        if (cref.Length < 2)
            return cref;

        return cref.Substring(1, 1) == ":" ? cref.Substring(2, cref.Length - 2) + " " : cref + " ";
    }

    private static string TrimStartRetainingSingleLeadingSpace(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return !char.IsWhiteSpace(input[0]) ? input : " " + input.TrimStart();
    }

    public string GetParameterText(string name) =>
        Array.Find(ParamElements, parameter => parameter.Name == name)?.Documentation
     ?? string.Empty;

    public string GetTypeParameterText(string name) =>
        Array.Find(TypeParamElements, typeParam => typeParam.Name == name)?.Documentation
     ?? string.Empty;
}
