namespace Reductech.Sequence.Core.Internal.Documentation;

internal static class TypeNameHelper
{
    public static string GetMarkupTypeName(Type t)
    {
        if (TypeAliases.TryGetValue(t, out var name))
            return $"`{name}`";

        if (!t.IsSignatureType && t.IsEnum)
            return $"[{t.Name}](../Enums/{t.Name}.md)";

        if (!t.IsGenericType)
            return $"`{t.Name}`";

        if (t.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var underlyingType = Nullable.GetUnderlyingType(t);

            if (underlyingType == null)
                return t.Name;

            return GetMarkupTypeName(underlyingType) + "?";
        }

        var typeName = t.Name.Split("`")[0];

        var arguments = $"<{string.Join(",", t.GetGenericArguments().Select(GetMarkupTypeName))}>";

        return typeName + arguments;
    }

    public static string GetHumanReadableTypeName(Type t)
    {
        if (!t.IsSignatureType && t.IsEnum)
            return t.Name;

        if (TypeAliases.TryGetValue(t, out var name))
            return name;

        if (!t.IsGenericType)
            return t.Name;

        if (t.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var underlyingType = Nullable.GetUnderlyingType(t);

            if (underlyingType == null)
                return t.Name;

            return GetHumanReadableTypeName(underlyingType) + "?";
        }

        var typeName = t.Name.Split("`")[0];

        var arguments =
            $"<{string.Join(",", t.GetGenericArguments().Select(GetHumanReadableTypeName))}>";

        return typeName + arguments;
    }

    private static readonly Dictionary<Type, string> TypeAliases =
        new()
        {
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(decimal), "decimal" },
            { typeof(object), "object" },
            { typeof(bool), "bool" },
            { typeof(char), "char" },
            { typeof(string), "string" },
            { typeof(StringStream), "string" },
            { typeof(Entity), "entity" },
            { typeof(DateTime), "dateTime" },
            { typeof(void), "void" }
        };
}
