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
        stringBuilder.AppendLine();

        return stringBuilder.ToString();
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
        stringBuilder.AppendLine();

        return stringBuilder.ToString();
    }

    private static Result<Unit, IErrorBuilder> TryAppendValues(
        Entity entity,
        StringBuilder stringBuilder)
    {
        foreach (var dreField in OrderedDREFields)
        {
            var value = entity.TryGetValue(dreField);

            if (value.HasValue)
            {
                var v = TryAppendValue(
                    stringBuilder,
                    dreField,
                    value.Value,
                    false,
                    dreField != DREReference
                );

                if (v.IsFailure)
                    return v;
            }
        }

        foreach (var entityProperty in entity)
        {
            if (!DREFieldsSet.Contains(entityProperty.Name))
            {
                var v = TryAppendValue(
                    stringBuilder,
                    $"{DREField} {entityProperty.Name}=",
                    entityProperty.BestValue,
                    true,
                    true
                );

                if (v.IsFailure)
                    return v;
            }
        }

        return Unit.Default;
    }

    private static Result<Unit, IErrorBuilder> TryAppendValue(
        StringBuilder sb,
        string fieldName,
        EntityValue entityValue,
        bool allowList,
        bool quoteString)
    {
        if (entityValue.TryPickT0(out _, out var r0))
            return Unit.Default;

        if (r0.TryPickT0(out var s, out var r1))
        {
            //TODO escape value
            var line = $"{fieldName} \"{s}\"";
            sb.AppendLine(line);
            return Unit.Default;
        }

        if (r1.TryPickT0(out var i, out var r2))
        {
            var line = $"{fieldName} {i}";
            sb.AppendLine(line);
            return Unit.Default;
        }

        if (r2.TryPickT0(out var d, out var r3))
        {
            var line = $"{fieldName} {d}";
            sb.AppendLine(line);
            return Unit.Default;
        }

        if (r3.TryPickT0(out var b, out var r4))
        {
            var line = $"{fieldName} {b}";
            sb.AppendLine(line);
            return Unit.Default;
        }

        if (r4.TryPickT0(out var e, out var r5))
        {
            //TODO escape value
            var line = $"{fieldName} \"{e}\"";
            sb.AppendLine(line);
            return Unit.Default;
        }

        if (r5.TryPickT0(out var dt, out var r6))
        {
            var line = $"{fieldName} {dt:yyyy/MM/dd}";
            sb.AppendLine(line);
            return Unit.Default;
        }

        if (r6.TryPickT0(out _, out var list))
            return ErrorCode.CannotConvertNestedEntity.ToErrorBuilder("IDX");

        if (allowList)
        {
            for (var j = 0; j < list.Count; j++)
            {
                var member       = list[j];
                var newFieldName = $"{fieldName}{j + 1}";
                TryAppendValue(sb, newFieldName, member, false, true);
            }
        }

        return ErrorCode.CannotConvertNestedList.ToErrorBuilder("IDX");
    }

    private const string DREEndData = "DREENDDATA";

    private const string DREField = "DREFIELD";
    private const string DREEndDoc = "DREENDDOC";
    private const string DREEndDataReference = "DREENDDATAREFERENCE";

    private const string DREReference = "DREREFERENCE";

    private static readonly IReadOnlyList<string> OrderedDREFields = new List<string>()
    {
        // ReSharper disable StringLiteralTypo
        DREReference,
        "DREDATE",
        "DRETITLE",
        "DRECONTENT",
        "DREDBNAME"
        // ReSharper restore StringLiteralTypo
    };

    private static readonly IReadOnlyCollection<string> DREFieldsSet = OrderedDREFields.ToHashSet();
}

}
