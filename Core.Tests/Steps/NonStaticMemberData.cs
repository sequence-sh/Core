using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Namotion.Reflection;
using Xunit;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class NonStaticMemberData : MemberDataAttributeBase
    {
        /// <inheritdoc />
        public NonStaticMemberData(string memberName, params object[] parameters) : base(memberName, parameters)
        {
        }

        /// <inheritdoc />
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {

            var type = testMethod.ReflectedType;
            if (type == null)
                return null;

            var accessor = GetPropertyAccessor(type) ?? GetFieldAccessor(type) ?? GetMethodAccessor(type);
            if (accessor == null)
            {
                var parameterText = Parameters?.Length > 0 ? $" with parameter types: {string.Join(", ", Parameters.Select(p => p?.GetType().FullName ?? "(null)"))}" : "";
                throw new ArgumentException($"Could not find public static member (property, field, or method) named '{MemberName}' on {type.FullName}{parameterText}");
            }

            var obj = accessor();
            if (obj == null)
                return null;

            if (!(obj is IEnumerable dataItems))
                throw new ArgumentException($"Property {MemberName} on {type.FullName} did not return IEnumerable");

            return dataItems.Cast<object?>().Select(item => ConvertDataItem(testMethod, item));
        }



        Func<object?>? GetFieldAccessor(Type type)
        {
            FieldInfo? fieldInfo = null;
            for (var reflectionType = type; reflectionType != null; reflectionType = reflectionType.BaseType)
            {
                fieldInfo = reflectionType.GetRuntimeField(MemberName);
                if (fieldInfo != null)
                    break;
            }

            if (fieldInfo == null)
                return null;

            var instance = GetInstance(type);

            return () => fieldInfo.GetValue(instance);
        }

        Func<object?>? GetMethodAccessor(Type type)
        {
            MethodInfo? methodInfo = null;
            var parameterTypes = Parameters == null ? new Type[0] : Parameters.Select(p => p?.GetType()).ToArray();
            for (var reflectionType = type; reflectionType != null; reflectionType = reflectionType.BaseType)
            {
                methodInfo =
                    reflectionType
                        .GetRuntimeMethods()
                        .FirstOrDefault(m => m.Name == MemberName && ParameterTypesCompatible(m.GetParameters(), parameterTypes));

                if (methodInfo != null)
                    break;
            }

            if (methodInfo == null )
                return null;

            var instance = GetInstance(type);

            return () => methodInfo.Invoke(instance, Parameters);
        }

        Func<object?>? GetPropertyAccessor(Type type)
        {
            PropertyInfo? propInfo = null;
            for (var reflectionType = type; reflectionType != null; reflectionType = reflectionType.BaseType)
            {
                propInfo = reflectionType.GetRuntimeProperty(MemberName);
                if (propInfo != null)
                    break;
            }


            if (propInfo == null || propInfo.GetMethod == null)
                return null;

            var instance = GetInstance(type);

            return () => propInfo.GetValue(instance, null);
        }

        object GetInstance(Type type)
        {
            var constructors = type.GetConstructors();

            if(constructors.Length != 1)
                throw new Exception($"{type.GetDisplayName()} does not have exactly one constructor");

            var args = constructors.Single().GetParameters()
                .Select(x => x.DefaultValue)
                .Select(x=> x is DBNull? null : x)
                .ToArray();

            var instance = constructors.Single().Invoke(args);

            return instance;
        }

        /// <inheritdoc/>
        protected override object?[] ConvertDataItem(
            MethodInfo testMethod,
            object? item)
        {
            if (item == null)
                return new object[0];

            if (!(item is object?[] array))
                throw new ArgumentException($"Property {MemberName} on {MemberType ?? testMethod.DeclaringType} yielded an item that is not an object?[] (it was {item.GetType().GetDisplayName()})");

            return array;
        }

        static bool ParameterTypesCompatible(
            IReadOnlyList<ParameterInfo> parameters,
            IReadOnlyList<Type?> parameterTypes)
        {
            if (parameters?.Count != parameterTypes.Count)
                return false;

            for (var idx = 0; idx < parameters.Count; ++idx)
                if (parameterTypes[idx] != null && !parameters[idx].ParameterType.IsAssignableFrom(parameterTypes[idx]!))
                    return false;

            return true;
        }
    }
}