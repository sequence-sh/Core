using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using Json.Schema;
using OneOf;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Entities.Schema
{

public abstract record SchemaNode
{
    public SchemaNode Create(JsonSchema Schema) { }

    public abstract Result<Maybe<EntityValue>, IErrorBuilder> TryTransform(
        string propertyName,
        EntityValue entityValue);
}

public record EntityNode(
    bool AllowExtra,
    IReadOnlyDictionary<string, (SchemaNode Node, bool Required)> Nodes) : SchemaNode
{
    /// <inheritdoc />
    public override Result<Maybe<EntityValue>, IErrorBuilder> TryTransform(
        string propertyName,
        EntityValue entityValue)
    {
        if (entityValue is not EntityValue.NestedEntity nestedEntity)
            return ErrorCode.SchemaViolation.ToErrorBuilder("Not an entity", propertyName);

        var errors             = new List<IErrorBuilder>();
        var requiredProperties = Nodes.Where(x => x.Value.Required).Select(x => x.Key).ToHashSet();

        var newEntity = nestedEntity.Value;

        foreach (var entityProperty in nestedEntity.Value)
        {
            if (Nodes.TryGetValue(entityProperty.Name, out var node))
            {
                var r = node.Node.TryTransform(
                    propertyName + "." + entityProperty.Name,
                    entityProperty.Value
                );

                if (r.IsFailure)
                    errors.Add(r.Error);
            }
            else if (!AllowExtra)
            {
                newEntity.RemoveProperty(entityProperty.Name);
            }
        }
    }
}

public record SchemaSettings(
    Formatter DateFormatter,
    Formatter TruthFormatter,
    Formatter FalseFormatter,
    Formatter NullFormatter,
    bool CaseSensitive,
    Maybe<bool> RemoveExtra) { }

public class Formatter : OneOfBase<IReadOnlyList<string>,
    IReadOnlyDictionary<string, IReadOnlyList<string>>>
{
    public Maybe<T> TryMatch<T>(
        string propertyName,
        string propertyValue,
        Func<(string value, string format), Maybe<T>> getValue)
    {
        if (!TryPickT0(out var list, out var dict))
        {
            if (!dict.TryGetValue(propertyName, out var list2))
            {
                return Maybe<T>.None;
            }

            list = list2;
        }

        foreach (var format in list)
        {
            var v = getValue((propertyValue, format));

            if (v.HasValue)
                return v;
        }

        return Maybe<T>.None;
    }

    public static Formatter Create(OneOf<string, IReadOnlyList<string>, Entity> data)
    {
        if (data.TryPickT0(out var s, out var r1))
        {
            return new Formatter(new List<string>() { s });
        }

        if (r1.TryPickT0(out var list, out var entity))
            return new Formatter(
                OneOf<IReadOnlyList<string>, IReadOnlyDictionary<string, IReadOnlyList<string>>>
                    .FromT0(list)
            );

        var dict = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var prop in entity)
        {
            if (prop.Value is EntityValue.NestedList nl)
                dict.Add(prop.Name, nl.Value.Select(x => x.GetPrimitiveString()).ToList());
            else
                dict.Add(prop.Name, new List<string>() { prop.Value.GetPrimitiveString() });
        }

        return new Formatter(dict);
    }

    /// <summary>
    /// Empty conversion mode
    /// </summary>
    public static Formatter Empty { get; } = new(new List<string>());

    /// <inheritdoc />
    protected Formatter(
        OneOf<IReadOnlyList<string>, IReadOnlyDictionary<string, IReadOnlyList<string>>> input) :
        base(input) { }
}

}
