using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Namotion.Reflection;

namespace Reductech.EDR.Core.Internal.Documentation
{

/// <summary>
/// Contains helper methods for creating documentation
/// </summary>
public static class DocumentationCreator
{
    /// <summary>
    /// Creates documentation for a list of entities
    /// </summary>
    public static
        IEnumerable<MarkdownDocument> CreateDocumentation(IEnumerable<IDocumented> entities)
    {
        var enumTypes = new HashSet<Type>();

        var categories = entities.GroupBy(x => x.DocumentationCategory).ToList();

        var sb = new StringBuilder();

        sb.AppendLine("# Contents");

        var contentsRows = categories.SelectMany(x => x)
            .Select(x => new[] { $"[{x.Name}]({x.DocumentationCategory}/{x.FileName})", x.Summary })
            .ToList();

        var contentsHeader = new[] { "Step", "Summary" }.Select(
                x => Prettifier.Cell.Create(x, Prettifier.Alignment.LeftJustified)
            )
            .ToList();

        Prettifier.CreateMarkdownTable(contentsHeader, contentsRows, sb);

        yield return new("Contents.md", "Contents", sb.ToString().Trim(), "", "");

        foreach (var category in categories)
        {
            foreach (var doc in category)
            {
                enumTypes.UnionWith(
                    doc.Parameters.Select(x => x.Type)
                        .Select(x => Nullable.GetUnderlyingType(x) ?? x)
                        .Where(t => t.IsEnum)
                );

                var pageText = GetPageText(doc);

                yield return new(doc.FileName, doc.Name, pageText, category.Key, category.Key);
            }
        }

        if (enumTypes.Any())
        {
            foreach (var type in enumTypes.OrderBy(x => x.Name))
            {
                var enumPageText = GetEnumPageText(type);

                yield return new(type.Name + ".md", type.Name, enumPageText, "Enums", "Enums");
            }
        }
    }

    private static string Escape(string? s)
    {
        return (s ?? string.Empty)
            .Replace("|",    @"\|")
            .Replace("<",    @"\<")
            .Replace("\r\n", " ")
            .Replace("\n",   " ");
    }

    /// <summary>
    /// Gets the documentation page for a step
    /// </summary>
    public static string GetPageText(IDocumented doc)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"## {doc.Name}");

        var aliases = string.Join(@", ", doc.AllNames.Select(x => $"`{x}`"));

        sb.AppendLine($"_Alias_:{aliases}");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(doc.TypeDetails))
        {
            sb.AppendLine($"_Output_:`{doc.TypeDetails}`");
            sb.AppendLine();
        }

        foreach (var docRequirement in doc.Requirements)
        {
            sb.AppendLine($"*{docRequirement}*");
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(doc.Summary))
        {
            sb.AppendLine(Escape(doc.Summary).Trim());
            sb.AppendLine();
        }

        if (doc.Parameters.Any())
        {
            var extraColumns = doc.Parameters.SelectMany(x => x.ExtraFields.Keys)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            var headers = new List<Prettifier.Cell>()
            {
                Prettifier.Cell.Create("Parameter", Prettifier.Alignment.LeftJustified),
                Prettifier.Cell.Create("Type",      Prettifier.Alignment.Centre),
                Prettifier.Cell.Create("Required",  Prettifier.Alignment.Centre),
            };

            headers.AddRange(
                extraColumns.Select(x => Prettifier.Cell.Create(x, Prettifier.Alignment.Centre))
            );

            headers.Add(Prettifier.Cell.Create("Summary", Prettifier.Alignment.LeftJustified));

            var parameterRows = doc.Parameters
                .Select(
                    rp =>

                    {
                        var r = new List<string?>
                        {
                            rp.Name,
                            TypeNameHelper.GetMarkupTypeName(rp.Type),
                            rp.Required ? "✔" : "",
                        };

                        foreach (var extraColumn in extraColumns)
                        {
                            var columnValue = rp.ExtraFields.TryGetValue(
                                extraColumn,
                                out var cv
                            )
                                ? cv
                                : null;

                            r.Add(columnValue);
                        }

                        r.Add(rp.Summary);

                        return r;
                    }
                )
                .ToList();

            Prettifier.CreateMarkdownTable(headers, parameterRows, sb);
        }

        return sb.ToString().Trim();
    }

    private static string GetEnumPageText(Type type)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"## {Escape(type.Name)}");

        var summary = type.GetXmlDocsSummary();

        if (!string.IsNullOrWhiteSpace(summary))
        {
            sb.AppendLine(Escape(summary));
            sb.AppendLine();
        }

        var headers = new[] { "Name", "Summary" }.Select(
                x => Prettifier.Cell.Create(x, Prettifier.Alignment.LeftJustified)
            )
            .ToList();

        var parameterRows = type.GetFields(BindingFlags.Public | BindingFlags.Static)
            .OrderBy(GetEnumFieldValue)
            .Select(fieldInfo => new[] { fieldInfo.Name, fieldInfo.GetXmlDocsSummary() })
            .ToList();

        Prettifier.CreateMarkdownTable(headers, parameterRows, sb);

        return sb.ToString().Trim();

        static object GetEnumFieldValue(MemberInfo memberInfo)
        {
            var type = memberInfo.DeclaringType;

            var v = Enum.Parse(type!, memberInfo.Name);

            return v;
        }
    }
}

}
