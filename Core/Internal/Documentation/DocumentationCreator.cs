using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Namotion.Reflection;

namespace Reductech.EDR.Core.Internal.Documentation
{
    internal static class DocumentationCreator
    {
        /// <summary>
        /// Dynamically Gets all steps and related entities from an assembly.
        /// For use with InstantConsole.
        /// </summary>
        public static IEnumerable<IDocumented> GetAllDocumented(DocumentationCategory documentationCategory, StepFactoryStore stepFactoryStore)
        {
            var wrappers = stepFactoryStore.Dictionary.Values
                .Select(x => new StepWrapper(x, documentationCategory)).ToList();

            return wrappers;
        }


        /// <summary>
        /// Creates documentation for a list of entities
        /// </summary>
        /// <param name="entities"></param>
        public static List<string> CreateDocumentationLines(IEnumerable<IDocumented> entities)
        {
            var lines = new List<string>();

            var enumTypes = new HashSet<Type>();

            var categories = entities.GroupBy(x => x.DocumentationCategory).ToList();

            lines.Add($"# Contents");

            var contentsRows = categories.SelectMany(x => x)
                .Select(x => new[]
                {
                    $"<a name=\"{x.Name}\">{x.Name}</a>",
                    x.Summary
                }).ToList();

            var contentsTableLines = Prettifier.CreateMarkdownTable(contentsRows);

            lines.AddRange(contentsTableLines);



            foreach (var category in categories)
            {
                if (category.Key.Anchor != null)
                    lines.Add($"<a name=\"{TypeNameHelper.GetHumanReadableTypeName(category.Key.Anchor)}\"></a>");
                lines.Add($"# {category.Key.Header}");

                foreach (var doc in category)
                {
                    lines.Add($"<a name=\"{doc.Name}\"></a>");
                    lines.Add($"## {doc.Name}");
                    lines.Add("");
                    if (!string.IsNullOrWhiteSpace(doc.TypeDetails))
                    {
                        lines.Add($"**{doc.TypeDetails}**");
                        lines.Add("");
                    }

                    foreach (var docRequirement in doc.Requirements)
                    {
                        lines.Add($"*{docRequirement}*");
                        lines.Add("");
                    }

                    if (!string.IsNullOrWhiteSpace(doc.Summary))
                    {
                        lines.Add(Escape(doc.Summary));
                        lines.Add("");
                    }

                    if (doc.Parameters.Any())
                    {
                        var extraColumns = doc.Parameters.SelectMany(x => x.ExtraFields.Keys).Distinct().OrderBy(x => x).ToList();

                        var headers = new List<string?> { "Parameter", "Type", "Required", "Summary" };
                        headers.AddRange(extraColumns);
                        var parameterRows = new List<List<string?>> { headers };

                        parameterRows.AddRange(
                            doc.Parameters
                                .Select(rp =>

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
                                        var columnValue = rp.ExtraFields.TryGetValue(extraColumn, out var cv) ? cv : null;
                                        r.Add(columnValue);
                                    }
                                    return r;
                                }));

                        var table = Prettifier.CreateMarkdownTable(parameterRows).ToList();
                        lines.AddRange(table);
                        lines.Add("");
                        enumTypes.UnionWith(doc.Parameters.Select(x => x.Type)
                            .Select(x => Nullable.GetUnderlyingType(x) ?? x).Where(t => t.IsEnum));
                    }
                }
            }

            if (enumTypes.Any())
            {
                lines.Add($"# Enums");

                foreach (var type in enumTypes.OrderBy(x => x.Name))
                {
                    lines.Add($"<a name=\"{type.Name}\"></a>");
                    lines.Add($"## {type.Name}");
                    var summary = type.GetXmlDocsSummary();
                    if (!string.IsNullOrWhiteSpace(summary))
                    {
                        lines.Add(Escape(summary));
                        lines.Add("");
                    }

                    var parameterRows = new List<string?[]> { new[] { "Name", "Summary" } };
                    parameterRows.AddRange(
                        type.GetFields(BindingFlags.Public | BindingFlags.Static)
                            .Select(fieldInfo => new[] { fieldInfo.Name, fieldInfo.GetXmlDocsSummary() }));

                    var table = Prettifier.CreateMarkdownTable(parameterRows).ToList();
                    lines.AddRange(table);
                    lines.Add("");
                }
            }



            return lines;

            static string Escape(string? s)
            {
                return (s ?? string.Empty)
                    .Replace("|", @"\|")
                    .Replace("\r\n", " ")
                    .Replace("\n", " ");
            }
        }

    }
}
