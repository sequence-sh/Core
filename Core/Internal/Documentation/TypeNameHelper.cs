namespace Reductech.Sequence.Core.Internal.Documentation;

/// <summary>
/// Contains methods for getting readable names of types
/// </summary>
public static class TypeNameHelper
{
    /// <summary>
    /// Gets the name of a type as it should appear in markup (possibly containing a link to the documentation)
    /// </summary>
    public static string GetMarkupTypeName(Type t, DocumentationOptions options)
    {
        if (TypeAliases.TryGetValue(t, out var name))
            return $"`{name}`";

        if (!t.IsSignatureType && t.IsEnum)
        {
            var extension = options.IncludeExtensionsInLinks ? ".md" : "";

            if (string.IsNullOrWhiteSpace(options.RootUrl))
            {
                return $"[{t.Name}](../Enums/{t.Name}{extension})";
            }
            else
            {
                return $"[{t.Name}]({options.RootUrl.TrimEnd('/')}/Enums/{t.Name}{extension})";
            }
        }

        if (!t.IsGenericType)
            return $"`{t.Name}`";

        if (t.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var underlyingType = Nullable.GetUnderlyingType(t);

            if (underlyingType == null)
                return t.Name;

            return GetMarkupTypeName(underlyingType, options) + "?";
        }

        var typeName = t.Name.Split("`")[0];

        var arguments = t.GetGenericArguments().Select(x => GetMarkupTypeName(x, options)).ToList();
        var argumentsAreBackticked = arguments.All(x => x.StartsWith('`') && x.EndsWith('`'));

        switch (typeName)
        {
            case "SCLOneOf": return string.Join(" or ", arguments);
            case "SCLEnum":  return arguments.Single();
            case "Array" when argumentsAreBackticked:
            {
                var argumentsString =
                    $"<{string.Join(", ", arguments.Select(x => x.Trim('`')))}>";

                return $"`array{argumentsString}`";
            }
            default:
            {
                var argumentsString =
                    $"<{string.Join(", ", arguments)}>";

                return typeName + argumentsString;
            }
        }
    }

    /// <summary>
    /// Gets a human readable type name
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
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
            { typeof(void), "unit" },
            { typeof(SCLBool), "bool" },
            { typeof(SCLInt), "int" },
            { typeof(SCLDouble), "double" },
            { typeof(SCLDateTime), "dateTime" },
            { typeof(SCLNull), "null" },
            { typeof(Unit), "unit" },
        };

    /// <summary>
    /// Get a human readable type description for a step factory
    /// </summary>
    public static string GetHumanReadableTypeDescription(this IStepFactory stepFactory)
    {
        if (TypeAliasesByString.TryGetValue(stepFactory.OutputTypeExplanation, out var s))
            return $"`{s}`";

        return stepFactory.OutputTypeExplanation;
    }

    /// <summary>
    /// Get a human readable type description for a step parameter
    /// </summary>
    public static string GetHumanReadableTypeDescription(this IStepParameter stepParameter)
    {
        if (TypeAliases.TryGetValue(stepParameter.ActualType, out var v))
            return v;

        return "`" + stepParameter.ActualType.Name + "`";
    }

    /// <summary>
    /// Gets the human readable type name for the output type of a step
    /// </summary>
    public static readonly Dictionary<string, string> TypeAliasesByString =
        TypeAliases.ToDictionary(x => x.Key.Name, x => x.Value);
}
