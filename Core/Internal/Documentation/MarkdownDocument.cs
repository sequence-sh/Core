using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Reductech.EDR.Core.Internal.Documentation
{

/// <summary>
/// The result of Generating Documentation
/// </summary>
[DataContract]
public record DocumentationCreationResult
(
    [property: DataMember] MainContents MainContents,
    [property: DataMember] IReadOnlyList<DocumentationCategory> Categories,
    [property: DataMember] IReadOnlyList<EnumPage> Enums) : IEntityConvertible
{
    /// <summary>
    /// Every documentation page
    /// </summary>
    [property: DataMember]
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
[DataContract]
public record DocumentationCategory(
    [property: DataMember] CategoryContents CategoryContents,
    [property: DataMember] IReadOnlyList<StepPage> Steps) : IEntityConvertible;

/// <summary>
/// The main contents page
/// </summary>
[DataContract]
public record MainContents(
    string FileName,
    string Title,
    string FileText,
    string Directory) : ContentsPage(FileName, Title, FileText, Directory);

/// <summary>
/// The contents page for a category
/// </summary>
[DataContract]
public record CategoryContents(
    string FileName,
    string Title,
    string FileText,
    string Directory,
    [property: DataMember] string Category) : ContentsPage(FileName, Title, FileText, Directory);

/// <summary>
/// The documentation page for a single step
/// </summary>
[DataContract]
public record StepPage(
    string FileName,
    string Title,
    string FileText,
    string Directory,
    [property: DataMember] string Category,
    [property: DataMember] string StepName,
    [property: DataMember] IReadOnlyList<string> Aliases,
    [property: DataMember] string Summary,
    [property: DataMember] string ReturnType,
    [property: DataMember] IReadOnlyList<StepParameter> StepParameters) : DocumentationPage(
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
[DataContract]
public record StepParameter(
    [property: DataMember] string Name,
    [property: DataMember] string Type,
    [property: DataMember] string Summary,
    [property: DataMember] bool Required,
    [property: DataMember] IReadOnlyList<string> Aliases) : IEntityConvertible;

/// <summary>
/// The documentation page for an enum
/// </summary>
[DataContract]
public record EnumPage(
    string FileName,
    string Title,
    string FileText,
    string Directory,
    [property: DataMember] IReadOnlyList<EnumValue> Values) : DocumentationPage(
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
[DataContract]
public record EnumValue(
    [property: DataMember] string Name,
    [property: DataMember] string Summary) : IEntityConvertible;

/// <summary>
/// A contents page
/// </summary>
[DataContract]
public abstract record ContentsPage(
    string FileName,
    string Title,
    string FileText,
    string Directory) : DocumentationPage(FileName, Title, FileText, Directory)
{
    /// <inheritdoc />
    [property: DataMember]
    public override string PageType => "Contents";
}

/// <summary>
/// A documentation page
/// </summary>
[DataContract]
public abstract record DocumentationPage(
    [property: DataMember] string FileName,
    [property: DataMember] string Title,
    [property: DataMember] string FileText,
    [property: DataMember] string Directory) : IEntityConvertible
{
    /// <summary>
    /// The type of the page
    /// </summary>
    [property: DataMember]
    public abstract string PageType { get; }
}

}
