using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Namotion.Reflection;

namespace Reductech.EDR.Core.Internal.Documentation
{

public static class DocumentationCreator
{
    /// <summary>
    /// Creates documentation for a list of entities
    /// </summary>
    public static
        IEnumerable<(string fileName, string title, string fileText, string directory, string
            category)> CreateDocumentation(IEnumerable<IDocumented> entities)
    {
        var enumTypes = new HashSet<Type>();

        var categories = entities.GroupBy(x => x.DocumentationCategory).ToList();

        var contentsLines = new List<string> { $"# Contents" };

        var contentsRows = categories.SelectMany(x => x)
            .Select(x => new[] { $"[{x.Name}]({x.DocumentationCategory}/{x.FileName})", x.Summary })
            .Prepend(new[] { "Step", "Summary" }) //Header row
            .ToList();

        var contentsTableLines = Prettifier.CreateMarkdownTable(contentsRows);
        contentsLines.AddRange(contentsTableLines);

        yield return ("Contents.md", "Contents", JoinLines(contentsLines), "", "");

        foreach (var category in categories)
        {
            foreach (var doc in category)
            {
                enumTypes.UnionWith(
                    doc.Parameters.Select(x => x.Type)
                        .Select(x => Nullable.GetUnderlyingType(x) ?? x)
                        .Where(t => t.IsEnum)
                );

                var pageLines = GetPageLines(doc);

                yield return (doc.FileName, doc.Name, JoinLines(pageLines), category.Key,
                              category.Key);
            }
        }

        if (enumTypes.Any())
        {
            foreach (var type in enumTypes.OrderBy(x => x.Name))
            {
                var enumLines = GetEnumLines(type);

                yield return (type.Name + ".md", type.Name, JoinLines(enumLines), "Enums", "Enums");
            }
        }

        static string JoinLines(IEnumerable<string> lines) => string.Join("\r\n", lines).Trim();
    }

    private static string Escape(string? s)
    {
        return (s ?? string.Empty)
            .Replace("|",    @"\|")
            .Replace("<",    @"\<")
            .Replace("\r\n", " ")
            .Replace("\n",   " ");
    }

    public static IEnumerable<string> GetPageLines(IDocumented doc)
    {
        yield return $"## {doc.Name}";

        var names = string.Join(@" \| ", doc.AllNames.Select(x => $"*{x}*"));

        yield return $"{names}";

        if (!string.IsNullOrWhiteSpace(doc.TypeDetails))
        {
            yield return $"**{doc.TypeDetails}**";
            yield return "";
        }

        foreach (var docRequirement in doc.Requirements)
        {
            yield return $"*{docRequirement}*";
            yield return "";
        }

        if (!string.IsNullOrWhiteSpace(doc.Summary))
        {
            yield return Escape(doc.Summary);
            yield return "";
        }

        if (doc.Parameters.Any())
        {
            var extraColumns = doc.Parameters.SelectMany(x => x.ExtraFields.Keys)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            var headers = new List<string?> { "Parameter", "Type", "Required", "Summary" };
            headers.AddRange(extraColumns);
            var parameterRows = new List<List<string?>> { headers };

            parameterRows.AddRange(
                doc.Parameters
                    .Select(
                        rp =>

                        {
                            var r = new List<string?>
                            {
                                rp.Name,
                                TypeNameHelper.GetMarkupTypeName(rp.Type),
                                rp.Required ? "☑️" : "",
                                rp.Summary
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

                            return r;
                        }
                    )
            );

            var table = Prettifier.CreateMarkdownTable(parameterRows).ToList();

            foreach (var line in table)
            {
                yield return line;
            }

            yield return "";
        }
    }

    private static IEnumerable<string> GetEnumLines(Type type)
    {
        yield return $"## {Escape(type.Name)}";

        var summary = type.GetXmlDocsSummary();

        if (!string.IsNullOrWhiteSpace(summary))
        {
            yield return Escape(summary);
            yield return string.Empty;
        }

        var parameterRows = new List<string?[]> { new[] { "Name", "Summary" } };

        parameterRows.AddRange(
            type.GetFields(BindingFlags.Public | BindingFlags.Static)
                .OrderBy(GetEnumFieldValue)
                .Select(fieldInfo => new[] { fieldInfo.Name, fieldInfo.GetXmlDocsSummary() })
        );

        var table = Prettifier.CreateMarkdownTable(parameterRows).ToList();

        foreach (var l in table)
            yield return l;

        yield return string.Empty;

        static object GetEnumFieldValue(MemberInfo memberInfo)
        {
            var type = memberInfo.DeclaringType;

            var v = Enum.Parse(type!, memberInfo.Name);

            return v;
        }
    }
}

}
