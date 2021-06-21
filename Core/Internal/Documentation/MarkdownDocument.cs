using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Reductech.EDR.Core.Internal.Documentation
{

[Serializable]
public record DocumentationCreationResult
(
    [JsonProperty] MainContents MainContents,
    [JsonProperty] IReadOnlyList<DocumentationCategory> Categories,
    [JsonProperty] IReadOnlyList<EnumPage> Enums) : IEntityConvertible { }

[Serializable]
public record DocumentationCategory(
    [JsonProperty] CategoryContents CategoryContents,
    [JsonProperty] IReadOnlyList<StepPage> Steps) : IEntityConvertible { }

[Serializable]
public record MainContents(
    [JsonProperty] string FileName,
    [JsonProperty] string Title,
    [JsonProperty] string FileText,
    [JsonProperty] string Directory) : ContentsPage(FileName, Title, FileText, Directory) { }

[Serializable]
public record CategoryContents(
    [JsonProperty] string FileName,
    [JsonProperty] string Title,
    [JsonProperty] string FileText,
    [JsonProperty] string Directory,
    [JsonProperty] string Category) : ContentsPage(FileName, Title, FileText, Directory) { }

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

[Serializable]
public record StepParameter(
    [JsonProperty] string Name,
    [JsonProperty] string Type,
    [JsonProperty] string Summary,
    [JsonProperty] bool Required,
    [JsonProperty] IReadOnlyList<string> Aliases) : IEntityConvertible { }

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

[Serializable]
public record EnumValue(
    [JsonProperty] string Name,
    [JsonProperty] string Summary) : IEntityConvertible { }

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

[Serializable]
public abstract record DocumentationPage(
    [JsonProperty] string FileName,
    [JsonProperty] string Title,
    [JsonProperty] string FileText,
    [JsonProperty] string Directory) : IEntityConvertible
{
    [JsonProperty] public abstract string PageType { get; }
}

///// <summary>
///// A Document Written in MarkDown
///// </summary>
//[Serializable]
//public record MarkdownDocument (
//    [JsonProperty] string FileName,
//    [JsonProperty] string Title,
//    [JsonProperty] string FileText,
//    [JsonProperty] string Directory,
//    [JsonProperty] string Category,
//    [JsonProperty] string PageType,
//    [JsonProperty] string? StepName,
//    [JsonProperty] IReadOnlyList<string>? Aliases,
//    [JsonProperty] string? Summary) : IEntityConvertible;

}
