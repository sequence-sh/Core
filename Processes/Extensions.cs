using System;
using System.Collections.Generic;
using System.ComponentModel;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes
{
    /// <summary>
    /// Helper methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets the description of an enum value from the Description Attribute.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum value)
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            if (name == null) return value.ToString();
            var field = type.GetField(name);
            if (field == null) return value.ToString();
            var attr =
                Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            if (attr is DescriptionAttribute da)
            {
                return da.Description;
            }
            return value.ToString();
        }

        /// <summary>
        /// Tries to get the element. Returns a failure if it is not present.
        /// </summary>
#pragma warning disable 8714
        public static Result<TValue> TryFindOrFail<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, string? error)
            =>  dictionary.TryFind(key).ToResult(error??$"The element '{key}' was not present.");
#pragma warning restore 8714
    }
}
