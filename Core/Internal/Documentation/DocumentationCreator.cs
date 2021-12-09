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
        DocumentationCreationResult CreateDocumentation(IEnumerable<IDocumentedStep> entities)
    {
        var enumTypes = new HashSet<Type>();

        MainContents mainContents;

        var categories = entities.GroupBy(x => x.DocumentationCategory).ToList();

        {
            var contentsStringBuilder = new StringBuilder();

            contentsStringBuilder.AppendLine("# EDR Steps");

            var contentsRows = categories.SelectMany(x => x)
                .OrderBy(x => x.Name)
                .Select(
                    x => new[]
                    {
                        $"[{x.Name}]({x.DocumentationCategory}/{x.FileName})",
                        x.DocumentationCategory, x.Summary
                    }
                )
                .ToList();

            var contentsHeader = new[] { "Step", "Connector", "Summary" }.Select(
                    x => Prettifier.Cell.Create(x, Prettifier.Alignment.LeftJustified)
                )
                .ToList();

            Prettifier.CreateMarkdownTable(contentsHeader, contentsRows, contentsStringBuilder);

            mainContents = new MainContents(
                "all.md",
                "all",
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

                contentsStringBuilder.AppendLine($"# {category.Key} Steps");

                var contentsRows = category
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
                    $"{category.Key}.md",
                    category.Key,
                    contentsStringBuilder.ToString().Trim(),
                    "",
                    category.Key
                );
            }

            List<StepPage> stepPages = new();

            //Individual Category Pages
            foreach (var doc in category)
            {
                enumTypes.UnionWith(
                    doc.Parameters.Select(x => x.ActualType)
                        .SelectMany(GetEnumTypes)
                );

                var stepPage = GetStepPage(doc);
                stepPages.Add(stepPage);
            }

            documentationCategories.Add(new DocumentationCategory(categoryContents, stepPages));
        }

        static IEnumerable<Type> GetEnumTypes(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;

            if (!type.IsSignatureType && type.IsEnum)
                yield return type;

            else if (type.IsGenericType)
                foreach (var gta in type.GenericTypeArguments)
                foreach (var enumType in GetEnumTypes(gta))
                    yield return enumType;
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
            .Replace("<",    @"&lt;")
            .Replace(">",    @"&gt;")
            .Replace("\r\n", " ")
            .Replace("\n",   " ");
    }

    /// <summary>
    /// Gets the documentation page for a step
    /// </summary>
    public static StepPage GetStepPage(IDocumentedStep doc)
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
                    TypeNameHelper.GetHumanReadableTypeName(x.ActualType),
                    x.Summary,
                    x.Required,
                    x.Aliases.ToList()
                )
            )
            .ToList();

        if (doc.Parameters.Any())
        {
            var extraParameterColumns = doc.Parameters.SelectMany(x => x.ExtraFields.Keys)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            var parameterHeaders = new List<Prettifier.Cell>()
            {
                Prettifier.Cell.Create("Parameter", Prettifier.Alignment.LeftJustified),
                Prettifier.Cell.Create("Type",      Prettifier.Alignment.Centre),
                Prettifier.Cell.Create("Required",  Prettifier.Alignment.Centre),
                Prettifier.Cell.Create("Position",  Prettifier.Alignment.Centre),
            };

            parameterHeaders.AddRange(
                extraParameterColumns.Select(
                    x => Prettifier.Cell.Create(x, Prettifier.Alignment.Centre)
                )
            );

            parameterHeaders.Add(
                Prettifier.Cell.Create("Summary", Prettifier.Alignment.LeftJustified)
            );

            var parameterRows = doc.Parameters
                .Select(
                    rp =>

                    {
                        var nameString = string.Join(
                            "<br/>",
                            rp.Aliases.Select(x => $"_{x}_").Prepend(rp.Name)
                        );

                        var r = new List<string?>
                        {
                            nameString,
                            TypeNameHelper.GetMarkupTypeName(rp.ActualType),
                            rp.Required ? "✔" : "",
                            rp.Order?.ToString() ?? ""
                        };

                        foreach (var extraColumn in extraParameterColumns)
                        {
                            var columnValue = rp.ExtraFields.TryGetValue(
                                extraColumn,
                                out var cv
                            )
                                ? cv
                                : null;

                            r.Add(Escape(columnValue));
                        }

                        r.Add(rp.Summary);

                        return r;
                    }
                )
                .ToList();

            Prettifier.CreateMarkdownTable(parameterHeaders, parameterRows, sb);
        }

        if (doc.Examples.Any())
        {
            sb.AppendLine();

            var showDescription    = doc.Examples.Any(x => x.Description is not null);
            var showExpectedOutput = doc.Examples.Any(x => x.ExpectedOutput is not null);

            var showExpectedLoggedValues =
                doc.Examples.Any(x => x.ExpectedLogs is not null && x.ExpectedLogs.Any());

            var exampleHeaders = new List<Prettifier.Cell>
            {
                Prettifier.Cell.Create("Example SCL", Prettifier.Alignment.LeftJustified),
            };

            if (showDescription)
                exampleHeaders.Add(
                    Prettifier.Cell.Create("Description", Prettifier.Alignment.Centre)
                );

            if (showExpectedOutput)
                exampleHeaders.Add(
                    Prettifier.Cell.Create("Expected Output", Prettifier.Alignment.Centre)
                );

            if (showExpectedLoggedValues)
                exampleHeaders.Add(
                    Prettifier.Cell.Create("Expected Logged Values", Prettifier.Alignment.Centre)
                );

            var exampleRows = doc.Examples.Select(
                    x =>
                    {
                        var list = new List<string>() { Escape(x.SCL) };

                        if (showDescription)
                            list.Add(Escape(x.Description ?? ""));

                        if (showExpectedOutput)
                            list.Add(Escape(x.ExpectedOutput ?? ""));

                        if (showExpectedLoggedValues)
                            list.Add(
                                x.ExpectedLogs is null
                                    ? ""
                                    : string.Join(
                                        "\r\n",
                                        x.ExpectedLogs.Select(Escape)
                                    )
                            );

                        return list;
                    }
                )
                .ToList();

            Prettifier.CreateMarkdownTable(exampleHeaders, exampleRows, sb);
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
            "Enums",
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
