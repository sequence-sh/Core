using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Reductech.EDR.Core.Internal.Documentation
{

/// <summary>
/// The result of Generating Documentation
/// </summary>
[Serializable]
public record DocumentationCreationResult
(
    [JsonProperty] MainContents MainContents,
    [JsonProperty] IReadOnlyList<DocumentationCategory> Categories,
    [JsonProperty] IReadOnlyList<EnumPage> Enums) : IEntityConvertible
{
    /// <summary>
    /// Every documentation page
    /// </summary>
    [JsonProperty]
    public IReadOnlyList<DocumentationPage> AllPages
    {
        get
        {
            IEnumerable<DocumentationPage> GetPages()
            {
                yield return MainContents;

                foreach (var documentationCategory in Categories)
                {
                    yield return documentationCategory.CategoryContents;

                    foreach (var page in documentationCategory.Steps)
                    {
                        yield return page;
                    }
                }

                foreach (var enumPage in Enums)
                {
                    yield return enumPage;
                }
            }

            return GetPages().ToList();
        }
    }
}

/// <summary>
/// The steps from a single connector
/// </summary>
[Serializable]
public record DocumentationCategory(
    [JsonProperty] CategoryContents CategoryContents,
    [JsonProperty] IReadOnlyList<StepPage> Steps) : IEntityConvertible;

/// <summary>
/// The main contents page
/// </summary>
[Serializable]
public record MainContents(
    [JsonProperty] string FileName,
    [JsonProperty] string Title,
    [JsonProperty] string FileText,
    [JsonProperty] string Directory) : ContentsPage(FileName, Title, FileText, Directory);

/// <summary>
/// The contents page for a category
/// </summary>
[Serializable]
public record CategoryContents(
    [JsonProperty] string FileName,
    [JsonProperty] string Title,
    [JsonProperty] string FileText,
    [JsonProperty] string Directory,
    [JsonProperty] string Category) : ContentsPage(FileName, Title, FileText, Directory);

/// <summary>
/// The documentation page for a single step
/// </summary>
[Serializable]
public record StepPage(
    [JsonProperty] string FileName,
    [JsonProperty] string Title,
    [JsonProperty] string FileText,
    [JsonProperty] string Directory,
    [JsonProperty] string Category,
    [JsonProperty] string StepName,
    [JsonProperty] IReadOnlyList<string> Aliases,
    [JsonProperty] string Summary,
    [JsonProperty] string ReturnType,
    [JsonProperty] IReadOnlyList<StepParameter> StepParameters) : DocumentationPage(
    FileName,
    Title,
    FileText,
    Directory
)
{
    /// <inheritdoc />
    public override string PageType => "Step";
}

/// <summary>
/// Documentation for a step parameter
/// </summary>
[Serializable]
public record StepParameter(
    [JsonProperty] string Name,
    [JsonProperty] string Type,
    [JsonProperty] string Summary,
    [JsonProperty] bool Required,
    [JsonProperty] IReadOnlyList<string> Aliases) : IEntityConvertible;

/// <summary>
/// The documentation page for an enum
/// </summary>
[Serializable]
public record EnumPage(
    [JsonProperty] string FileName,
    [JsonProperty] string Title,
    [JsonProperty] string FileText,
    [JsonProperty] string Directory,
    [JsonProperty] IReadOnlyList<EnumValue> Values) : DocumentationPage(
    FileName,
    Title,
    FileText,
    Directory
)
{
    /// <inheritdoc />
    public override string PageType => "Enums";
}

/// <summary>
/// The value for an enum
/// </summary>
[Serializable]
public record EnumValue(
    [JsonProperty] string Name,
    [JsonProperty] string Summary) : IEntityConvertible;

/// <summary>
/// A contents page
/// </summary>
[Serializable]
public abstract record ContentsPage(
    [JsonProperty] string FileName,
    [JsonProperty] string Title,
    [JsonProperty] string FileText,
    [JsonProperty] string Directory) : DocumentationPage(FileName, Title, FileText, Directory)
{
    /// <inheritdoc />
    [JsonProperty]
    public override string PageType => "Contents";
}

/// <summary>
/// A documentation page
/// </summary>
[Serializable]
public abstract record DocumentationPage(
    [JsonProperty] string FileName,
    [JsonProperty] string Title,
    [JsonProperty] string FileText,
    [JsonProperty] string Directory) : IEntityConvertible
{
    /// <summary>
    /// The type of the page
    /// </summary>
    [JsonProperty]
    public abstract string PageType { get; }
}

}
