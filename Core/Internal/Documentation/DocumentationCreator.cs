using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Namotion.Reflection;

namespace Reductech.EDR.Core.Internal.Documentation
{

internal static class DocumentationCreator
{
    private static IEnumerable<string> GetPageLines(IDocumented doc)
    {
        var pageLines = new List<string>
        {
            $"<a name=\"{doc.Name}\"></a>", $"## {doc.Name}", string.Empty
        };

        if (!string.IsNullOrWhiteSpace(doc.TypeDetails))
        {
            pageLines.Add($"**{doc.TypeDetails}**");
            pageLines.Add(string.Empty);
        }

        foreach (var docRequirement in doc.Requirements)
        {
            pageLines.Add($"*{docRequirement}*");
            pageLines.Add(string.Empty);
        }

        if (!string.IsNullOrWhiteSpace(doc.Summary))
        {
            pageLines.Add(Escape(doc.Summary));
            pageLines.Add(string.Empty);
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
            pageLines.AddRange(table);
            pageLines.Add(string.Empty);
        }

        return pageLines;
    }

    /// <summary>
    /// Creates documentation for a list of entities
    /// </summary>
    public static IEnumerable<(string fileName, string fileText)> CreateDocumentation(
        IEnumerable<IDocumented> entities)
    {
        var enumTypes = new HashSet<Type>();

        var categories = entities.GroupBy(x => x.DocumentationCategory).ToList();

        var contentsLines = new List<string> { $"# Contents" };

        var contentsRows = categories.SelectMany(x => x)
            .Select(x => new[] { $"[{x.Name}](#{x.Name})", x.Summary })
            .Prepend(new[] { "Step", "Summary" }) //Header row
            .ToList();

        var contentsTableLines = Prettifier.CreateMarkdownTable(contentsRows);
        contentsLines.AddRange(contentsTableLines);

        yield return ("Contents", JoinLines(contentsLines));

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

                yield return (doc.Name, JoinLines(pageLines));
            }
        }

        if (enumTypes.Any())
        {
            //lines.Add($"# Enums");

            foreach (var type in enumTypes.OrderBy(x => x.Name))
            {
                var enumLines = new List<string>();
                enumLines.Add($"<a name=\"{type.Name}\"></a>");
                enumLines.Add($"## {type.Name}");
                var summary = type.GetXmlDocsSummary();

                if (!string.IsNullOrWhiteSpace(summary))
                {
                    enumLines.Add(Escape(summary));
                    enumLines.Add(string.Empty);
                }

                var parameterRows = new List<string?[]> { new[] { "Name", "Summary" } };

                parameterRows.AddRange(
                    type.GetFields(BindingFlags.Public | BindingFlags.Static)
                        .OrderBy(GetEnumFieldValue)
                        .Select(
                            fieldInfo => new[] { fieldInfo.Name, fieldInfo.GetXmlDocsSummary() }
                        )
                );

                var table = Prettifier.CreateMarkdownTable(parameterRows).ToList();
                enumLines.AddRange(table);
                enumLines.Add(string.Empty);

                yield return (type.Name, JoinLines(enumLines));
            }
        }

        static string JoinLines(IEnumerable<string> lines) => string.Join("\r\n", lines).Trim();

        static object GetEnumFieldValue(MemberInfo memberInfo)
        {
            var type = memberInfo.DeclaringType;

            var v = Enum.Parse(type!, memberInfo.Name);

            return v;
        }
    }

    private static string Escape(string? s)
    {
        return (s ?? string.Empty)
            .Replace("|",    @"\|")
            .Replace("\r\n", " ")
            .Replace("\n",   " ");
    }
}

}
