using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Util.IDX
{

/// <summary>
/// The Idx parser configuration
/// </summary>
public record IdxParserConfiguration(
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        string FieldDelimiter,
        string FieldNameDelimiter,
        string StringDelimiter)
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    /// <summary>
    /// Default Configuration for IDX parsers
    /// </summary>
    public static readonly IdxParserConfiguration Default = new("#DRE", "=", "\"");
}

/// <summary>
/// Contains methods for parsing IDX
/// </summary>
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public record IdxParser(IdxParserConfiguration Config)
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    private static readonly Regex FieldRegex = new(
        @"(?<name>\w+)(?<number>\d+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    /// <summary>
    /// Parse an IDX string as an entity.
    /// </summary>
    public Result<Entity, IErrorBuilder> TryParseEntity(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return ErrorCode.CouldNotParse.ToErrorBuilder("", "IDX");

        var propertyList = new Dictionary<string, EntityProperty>();

        var currentOrder = 0;

        void AddField(string fieldName, string fieldValue)
        {
            var fieldMatch = FieldRegex.Match(fieldName);
            var newValue   = EntityValue.CreateFromObject(fieldValue);

            if (fieldMatch.Success)
                fieldName = fieldMatch.Groups["name"].Value;

            if (propertyList.TryGetValue(fieldName, out var ep))
            {
                var combinedValue = ep.BestValue.Combine(newValue);

                propertyList[fieldName] =
                    new EntityProperty(
                        fieldName,
                        combinedValue,
                        null,
                        currentOrder
                    );
            }
            else
            {
                propertyList.Add(
                    fieldName,
                    new EntityProperty(
                        fieldName,
                        newValue,
                        null,
                        currentOrder
                    )
                );
            }

            currentOrder++;
        }

        foreach (string fieldBlock in input.Split(Config.FieldDelimiter))
        {
            // skip any potential empty lines at the start of the document
            if (string.IsNullOrWhiteSpace(fieldBlock))
                continue;

            var fieldNameEnd = fieldBlock.IndexOfAny(new[] { ' ', '\n' });

            if (fieldNameEnd < 0)
                continue;

            var fieldName  = fieldBlock.Substring(0, fieldNameEnd).TrimEnd('\r');
            var fieldValue = fieldBlock[(fieldNameEnd + 1)..];

            fieldValue = fieldValue.Trim();

            switch (fieldName)
            {
                case "CONTENT":
                    AddField("DRECONTENT", fieldValue);
                    break;
                case "DATE":
                    AddField("DREDATE", fieldValue);
                    break;
                case "DBNAME":
                    AddField("DREDBNAME", fieldValue);
                    break;
                case "REFERENCE":
                    AddField("DREREFERENCE", fieldValue);
                    break;
                case "SECTION":
                    AddField("DRESECTION", fieldValue);
                    break;
                case "TITLE":
                    AddField("DRETITLE", fieldValue);
                    break;
                case "FIELD":
                    var fieldEnd = fieldValue.IndexOf(
                        Config.FieldNameDelimiter,
                        StringComparison.Ordinal
                    );

                    var field = fieldEnd == -1 ? fieldValue : fieldValue.Substring(0, fieldEnd);
                    var value = "";

                    if (fieldEnd != -1)
                    {
                        value = fieldValue[(fieldEnd + 1)..];
                        value = value.Trim();

                        if (value.StartsWith(Config.StringDelimiter))
                            value = value[1..];

                        if (value.EndsWith(Config.StringDelimiter))
                            value = value.Remove(value.Length - 1);
                    }

                    AddField(field, value);
                    break;
                case "ENDDOC":
                    // TODO: Maybe a check if there are any fields AFTER the end tag
                    break;
                default:
                    AddField(fieldName, fieldValue);
                    break;
            }
        }

        var newPropertyValues = propertyList.Values.OrderBy(x => x.Order)
            .Select((x, i) => new EntityProperty(x.Name, x.BaseValue, null, i));

        var entity = new Entity(newPropertyValues);
        return entity;
    }
}

}
