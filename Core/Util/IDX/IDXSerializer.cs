using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Util.IDX
{

/// <summary>
/// Contains methods for serializing entities to IDX
/// </summary>
public static class IDXSerializer
{
    /// <summary>
    /// Convert this entity to an IDX Document.
    /// Will fail if the entity contains nested entities or doubly nested lists.
    /// </summary>
    public static Result<string, IErrorBuilder> TryConvertToIDXDocument(this Entity entity)
    {
        var stringBuilder = new StringBuilder();

        var r = TryAppendValues(entity, stringBuilder);

        if (r.IsFailure)
            return r.ConvertFailure<string>();

        stringBuilder.AppendLine($"#{DREEndDoc}");
        stringBuilder.AppendLine($"#{DREEndDataReference}");

        return stringBuilder.ToString().Trim();
    }

    /// <summary>
    /// Convert this entity to IDX Data.
    /// Will fail if the entity contains nested entities or doubly nested lists.
    /// </summary>
    public static Result<string, IErrorBuilder> TryConvertToIDXData(this Entity entity)
    {
        var stringBuilder = new StringBuilder();

        var r = TryAppendValues(entity, stringBuilder);

        if (r.IsFailure)
            return r.ConvertFailure<string>();

        stringBuilder.AppendLine($"#{DREEndData}");

        return stringBuilder.ToString().Trim();
    }

    private static Result<Unit, IErrorBuilder> TryAppendValues(
        Entity entity,
        StringBuilder stringBuilder)
    {
        foreach (var dreField in OrderedDREFields)
        {
            var value = entity.TryGetValue(dreField.Name);

            if (value.HasValue)
            {
                var v = TryAppendValue(
                    stringBuilder,
                    value.Value,
                    dreField
                );

                if (v.IsFailure)
                    return v;
            }
            else if (dreField.Mandatory)
                return ErrorCode.SchemaViolationMissingProperty.ToErrorBuilder(dreField.Name);
        }

        foreach (var entityProperty in entity)
        {
            if (!DREFieldsSet.Contains(entityProperty.Name))
            {
                var fieldValue = new SpecialField(
                    $"{DREField} {entityProperty.Name}=",
                    StringHandling.Quote,
                    false,
                    true
                );

                var v = TryAppendValue(
                    stringBuilder,
                    entityProperty.BestValue,
                    fieldValue
                );

                if (v.IsFailure)
                    return v;
            }
        }

        return Unit.Default;
    }

    private static Result<Unit, IErrorBuilder> TryAppendValue(
        StringBuilder sb,
        EntityValue entityValue,
        SpecialField field)
    {
        if (entityValue.TryPickT0(out _, out var r0))
            return Unit.Default;

        void AppendString(string s1) //TODO escape value
        {
            if (s1.Contains('\n') || field.StringHandling == StringHandling.Paragraph)
            {
                sb.AppendLine("#" + field.Name);
                sb.AppendLine(s1);
            }
            else
            {
                var maybeQuotedString =
                    field.StringHandling == StringHandling.Quote ? $"\"{s1}\"" : s1;

                var line = $"#{field.Name} {maybeQuotedString}";
                sb.AppendLine(line);
            }
        }

        if (r0.TryPickT0(out var s, out var r1))
        {
            AppendString(s);
            return Unit.Default;
        }

        if (r1.TryPickT0(out var i, out var r2))
        {
            var line = $"#{field.Name} {i}";
            sb.AppendLine(line);
            return Unit.Default;
        }

        if (r2.TryPickT0(out var d, out var r3))
        {
            var line = $"#{field.Name} {d}";
            sb.AppendLine(line);
            return Unit.Default;
        }

        if (r3.TryPickT0(out var b, out var r4))
        {
            var line = $"#{field.Name} {b}";
            sb.AppendLine(line);
            return Unit.Default;
        }

        if (r4.TryPickT0(out var e, out var r5))
        {
            AppendString(e.ToString());
            return Unit.Default;
        }

        if (r5.TryPickT0(out var dt, out var r6))
        {
            var line = $"#{field.Name} {dt:yyyy/MM/dd}";
            sb.AppendLine(line);
            return Unit.Default;
        }

        if (r6.TryPickT0(out _, out var list))
            return ErrorCode.CannotConvertNestedEntity.ToErrorBuilder("IDX");

        if (!field.AllowList)
            return ErrorCode.CannotConvertNestedList.ToErrorBuilder("IDX");

        for (var j = 0; j < list.Count; j++)
        {
            var member       = list[j];
            var newFieldName = $"{field.Name}{j + 1}";
            TryAppendValue(sb, member, field with { AllowList = false, Name = newFieldName });
        }

        return Unit.Default;
    }

    private enum StringHandling
    {
        Quote,
        Paragraph,
        InlineUnquoted
    }

    private record SpecialField(
        string Name,
        StringHandling StringHandling,
        bool Mandatory,
        bool AllowList);

    private const string DREEndData = "DREENDDATA";

    private const string DREField = "DREFIELD";
    private const string DREEndDoc = "DREENDDOC";
    private const string DREEndDataReference = "DREENDDATAREFERENCE";

    private static readonly IReadOnlyList<SpecialField> OrderedDREFields = new List<SpecialField>()
    {
        // ReSharper disable StringLiteralTypo
        new("DREREFERENCE", StringHandling.InlineUnquoted, true, false),
        new("DREDATE", StringHandling.InlineUnquoted, false, false),
        new("DRETITLE", StringHandling.Paragraph, false, false),
        new("DRECONTENT", StringHandling.Paragraph, false, false),
        new("DREDBNAME", StringHandling.InlineUnquoted, false, false)
        // ReSharper restore StringLiteralTypo
    };

    private static readonly IReadOnlyCollection<string> DREFieldsSet =
        OrderedDREFields.Select(x => x.Name).ToHashSet();
}

}
