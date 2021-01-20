using System;
using System.Collections.Generic;
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
    public static readonly IdxParserConfiguration Default = new("#DRE", "=", "\"");
}

/// <summary>
/// Contains methods for parsing IDX
/// </summary>
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public record IdxParser(IdxParserConfiguration Config)
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    /// <summary>
    /// Parse an IDX string as an entity.
    /// </summary>
    public Result<Entity, IErrorBuilder> TryParseEntity(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return ErrorCode.CouldNotParse.ToErrorBuilder("", "IDX");

        var propertyList = new List<EntityProperty>();

        var currentOrder = 0;

        void AddField(string fieldName, string fieldValue)
        {
            propertyList.Add(
                new EntityProperty(
                    fieldName,
                    EntityValue.CreateFromObject(fieldValue),
                    null,
                    currentOrder
                )
            );

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
            var fieldValue = fieldBlock.Substring(fieldNameEnd + 1);

            fieldValue = fieldValue.Trim();

            //if (fieldValue.EndsWith('\n'))
            //    fieldValue = fieldValue.Remove(fieldValue.Length - 1);

            //if (fieldValue.EndsWith('\r'))
            //    fieldValue = fieldValue.Remove(fieldValue.Length - 1);

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
                            value = value.Substring(1);

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

        return new Entity(propertyList);
    }
}

}
