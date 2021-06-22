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
        DocumentationCreationResult CreateDocumentation(IEnumerable<IDocumented> entities)
    {
        var enumTypes = new HashSet<Type>();

        MainContents mainContents;

        var categories = entities.GroupBy(x => x.DocumentationCategory).ToList();

        {
            var contentsStringBuilder = new StringBuilder();

            contentsStringBuilder.AppendLine("# Contents");

            var contentsRows = categories.SelectMany(x => x)
                .Select(
                    x => new[] { $"[{x.Name}]({x.DocumentationCategory}/{x.FileName})", x.Summary }
                )
                .ToList();

            var contentsHeader = new[] { "Step", "Summary" }.Select(
                    x => Prettifier.Cell.Create(x, Prettifier.Alignment.LeftJustified)
                )
                .ToList();

            Prettifier.CreateMarkdownTable(contentsHeader, contentsRows, contentsStringBuilder);

            mainContents = new MainContents(
                "Contents.md",
                "Contents",
                contentsStringBuilder.ToString().Trim(),
                ""
            );
        }

        var documentationCategories = new List<DocumentationCategory>();

        foreach (var category in categories)
        {
            CategoryContents categoryContents;

            //Category Contents Page
            {
                var contentsStringBuilder = new StringBuilder();

                contentsStringBuilder.AppendLine("# Contents");

                var contentsRows = categories.SelectMany(x => x)
                    .Select(
                        x => new[]
                        {
                            $"[{x.Name}]({x.DocumentationCategory}/{x.FileName})", x.Summary
                        }
                    )
                    .ToList();

                var contentsHeader = new[] { "Step", "Summary" }.Select(
                        x => Prettifier.Cell.Create(x, Prettifier.Alignment.LeftJustified)
                    )
                    .ToList();

                Prettifier.CreateMarkdownTable(contentsHeader, contentsRows, contentsStringBuilder);

                categoryContents = new CategoryContents(
                    "Contents.md",
                    "Contents",
                    contentsStringBuilder.ToString().Trim(),
                    category.Key,
                    category.Key
                );
            }

            List<StepPage> stepPages = new();

            //Individual Category Pages
            foreach (var doc in category)
            {
                enumTypes.UnionWith(
                    doc.Parameters.Select(x => x.Type)
                        .Select(x => Nullable.GetUnderlyingType(x) ?? x)
                        .Where(t => !t.IsSignatureType && t.IsEnum)
                );

                var stepPage = GetStepPage(doc);
                stepPages.Add(stepPage);
            }

            documentationCategories.Add(new DocumentationCategory(categoryContents, stepPages));
        }

        var enums = new List<EnumPage>();

        if (enumTypes.Any())
        {
            foreach (var type in enumTypes.OrderBy(x => x.Name))
            {
                var enumData = GetEnumPage(type);
                enums.Add(enumData);
            }
        }

        var result =
            new DocumentationCreationResult(mainContents, documentationCategories, enums);

        return result;
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
    public static StepPage GetStepPage(IDocumented doc)
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

        var stepParameters = doc.Parameters.Select(
                x => new StepParameter(
                    x.Name,
                    TypeNameHelper.GetHumanReadableTypeName(x.Type),
                    x.Summary,
                    x.Required,
                    x.Aliases.ToList()
                )
            )
            .ToList();

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
                        var nameString = string.Join(
                            "<br>",
                            rp.Aliases.Select(x => $"_{x}_").Prepend(rp.Name)
                        );

                        var r = new List<string?>
                        {
                            nameString,
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

        var stepPage = new StepPage(
            doc.FileName,
            doc.Name,
            sb.ToString().Trim(),
            doc.DocumentationCategory,
            doc.DocumentationCategory,
            doc.Name,
            doc.AllNames,
            doc.Summary,
            doc.TypeDetails ?? "",
            stepParameters
        );

        return stepPage;
    }

    private static EnumPage GetEnumPage(Type type)
    {
        var textStringBuilder = new StringBuilder();
        textStringBuilder.AppendLine($"## {Escape(type.Name)}");

        var summary = type.GetXmlDocsSummary();

        if (!string.IsNullOrWhiteSpace(summary))
        {
            textStringBuilder.AppendLine(Escape(summary));
            textStringBuilder.AppendLine();
        }

        var headers = new[] { "Name", "Summary" }.Select(
                x => Prettifier.Cell.Create(x, Prettifier.Alignment.LeftJustified)
            )
            .ToList();

        var enumValues = type.GetFields(BindingFlags.Public | BindingFlags.Static)
            .OrderBy(GetEnumFieldValue)
            .Select(x => new EnumValue(x.Name, x.GetXmlDocsSummary()))
            .ToList();

        var parameterRows = enumValues
            .Select(fieldInfo => new[] { fieldInfo.Name, fieldInfo.Summary })
            .ToList();

        Prettifier.CreateMarkdownTable(headers, parameterRows, textStringBuilder);

        return new EnumPage(
            type.Name + ".md",
            type.Name,
            textStringBuilder.ToString().Trim(),
            "",
            enumValues
        );

        static object GetEnumFieldValue(MemberInfo memberInfo)
        {
            var type = memberInfo.DeclaringType;

            var v = Enum.Parse(type!, memberInfo.Name);

            return v;
        }
    }
}

}
