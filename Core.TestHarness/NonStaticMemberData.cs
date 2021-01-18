namespace Reductech.EDR.Core.TestHarness
{

//public sealed class NonStaticMemberData : MemberDataAttributeBase
//{
//    /// <inheritdoc />
//    public NonStaticMemberData(
//        string memberName,
//        bool addDefaultIfNoValues,
//        params object[] parameters) : base(memberName, parameters)
//    {
//        AddDefaultIfNoValues = addDefaultIfNoValues;
//    }

//    public bool AddDefaultIfNoValues { get; }

//    /// <inheritdoc />
//    public override IEnumerable<object?[]> GetData(MethodInfo testMethod)
//    {
//        var reflectedType = testMethod.ReflectedType;

//        if (reflectedType == null)
//            throw new ArgumentException($"{testMethod.Name} does not belong to a type");

//        var accessor = GetPropertyAccessor(reflectedType)
//                    ?? GetFieldAccessor(reflectedType) ?? GetMethodAccessor(reflectedType);

//        if (accessor == null)
//        {
//            var parameterText = Parameters?.Length > 0
//                ? $" with parameter types: {string.Join(", ", Parameters.Select(p => p?.GetType().FullName ?? "(null)"))}"
//                : "";

//            throw new ArgumentException(
//                $"Could not find public static member (property, field, or method) named '{MemberName}' on {reflectedType.FullName}{parameterText}"
//            );
//        }

//        var obj = accessor();

//        if (obj == null)
//            throw new ArgumentException($"{reflectedType.GetDisplayName()}.{MemberName} is null");

//        if (obj is not IEnumerable dataItems)
//            throw new ArgumentException(
//                $"Property {reflectedType.GetDisplayName()}.{MemberName} did not return IEnumerable"
//            );

//        var result = new List<object?[]>();

//        foreach (var item in dataItems)
//        {
//            var converted = ConvertDataItem1(item, reflectedType);
//            result.Add(converted);
//        }

//        if (result.Count == 0)
//        {
//            if (AddDefaultIfNoValues)
//                result.Add(testMethod.GetParameters().Select(x => x.DefaultValue).ToArray());
//            else
//            {
//                throw new ArgumentException(
//                    $"Property {reflectedType.GetDisplayName()}.{MemberName} did not have any members"
//                );
//            }
//        }

//        return result;
//    }

//    Func<object?>? GetFieldAccessor(Type type)
//    {
//        FieldInfo? fieldInfo = null;

//        for (var reflectionType = type;
//             reflectionType != null;
//             reflectionType = reflectionType.BaseType)
//        {
//            fieldInfo = reflectionType.GetRuntimeField(MemberName);

//            if (fieldInfo != null)
//                break;
//        }

//        if (fieldInfo == null)
//            return null;

//        var instance = GetInstance(type);

//        return () => fieldInfo.GetValue(instance);
//    }

//    private Func<object?>? GetMethodAccessor(Type type)
//    {
//        MethodInfo? methodInfo = null;

//        var parameterTypes =
//            (Parameters ?? Array.Empty<object>()).Select(p => p.GetType()).ToArray();

//        for (var reflectionType = type;
//             reflectionType != null;
//             reflectionType = reflectionType.BaseType)
//        {
//            methodInfo =
//                reflectionType
//                    .GetRuntimeMethods()
//                    .FirstOrDefault(
//                        m => m.Name == MemberName && ParameterTypesCompatible(
//                            m.GetParameters(),
//                            parameterTypes
//                        )
//                    );

//            if (methodInfo != null)
//                break;
//        }

//        if (methodInfo == null)
//            return null;

//        var instance = GetInstance(type);

//        return () => methodInfo.Invoke(instance, Parameters);
//    }

//    Func<object?>? GetPropertyAccessor(Type type)
//    {
//        PropertyInfo? propInfo = null;

//        for (var reflectionType = type;
//             reflectionType != null;
//             reflectionType = reflectionType.BaseType)
//        {
//            propInfo = reflectionType.GetRuntimeProperty(MemberName);

//            if (propInfo != null)
//                break;
//        }

//        if (propInfo == null || propInfo.GetMethod == null)
//            return null;

//        var instance = GetInstance(type);

//        return () => propInfo.GetValue(instance, null);
//    }

//    static object GetInstance(Type type)
//    {
//        var constructors = type.GetConstructors();

//        if (constructors.Length != 1)
//            throw new Exception($"{type.GetDisplayName()} does not have exactly one constructor");

//        var args = constructors.Single()
//            .GetParameters()
//            .Select(x => x.DefaultValue)
//            .Select(x => x is DBNull ? null : x)
//            .ToArray();

//        var instance = constructors.Single().Invoke(args);

//        return instance;
//    }

//    private object?[] ConvertDataItem1(object? item, Type type)
//    {
//        if (item == null)
//            return Array.Empty<object>();

//        if (item is not object?[] array)
//            throw new ArgumentException(
//                $"Property {MemberName} on {MemberType ?? type} yielded an item that is not an object?[] (it was {item.GetType().GetDisplayName()})"
//            );

//        return array;
//    }

//    /// <inheritdoc/>
//    protected override object?[] ConvertDataItem(
//        MethodInfo testMethod,
//        object? item)
//    {
//        return ConvertDataItem1(item, testMethod.DeclaringType!);
//    }

//    static bool ParameterTypesCompatible(
//        IReadOnlyList<ParameterInfo> parameters,
//        IReadOnlyList<Type?> parameterTypes)
//    {
//        if (parameters.Count != parameterTypes.Count)
//            return false;

//        for (var idx = 0; idx < parameters.Count; ++idx)
//            if (parameterTypes[idx] != null
//             && !parameters[idx].ParameterType.IsAssignableFrom(parameterTypes[idx]!))
//                return false;

//        return true;
//    }
//}

}
