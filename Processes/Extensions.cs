using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
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
        /// Gets the name of an enum value from the display attribute if it is present.
        /// </summary>
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())?
                            .First()?
                            .GetCustomAttribute<DisplayAttribute>()?
                            .GetName()?? enumValue.ToString();
        }



        /// <summary>
        /// Gets all possible values of this enum.
        /// </summary>
        public static IEnumerable<T> GetEnumValues<T>() where T: Enum => Enum.GetValues(typeof(T)).Cast<T>();

        /// <summary>
        /// Tries to get the element. Returns a failure if it is not present.
        /// </summary>
#pragma warning disable 8714
        public static Result<TValue> TryFindOrFail<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, string? error)
            =>  dictionary.TryFind(key).ToResult(error??$"The element '{key}' was not present.");

        /// <summary>
        /// Returns this nullable value as a maybe.
        /// </summary>
        public static Maybe<T> Maybe<T>(this T? str) where T : struct => str.HasValue ? CSharpFunctionalExtensions.Maybe<T>.From(str.Value) : CSharpFunctionalExtensions.Maybe<T>.None;

        /// <summary>
        /// Gets each element paired with it's index.
        /// </summary>
        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self) => self.Select((item, index) => (item, index));


        /// <summary>
        /// Returns the values of elements of the sequence.
        /// </summary>
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : struct => from val in source where val.HasValue select val.Value;

        /// <summary>
        /// Returns the elements of the sequence which are not null.
        /// </summary>
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : class => (from val in source where val != null select val)!;


        /// <summary>
        /// Tries to get this value of an enum type. Returns a failure if it is not present.
        /// </summary>
        public static Result<object> TryGetEnumValue(Type enumType, string value)
        {
            if (Enum.TryParse(enumType, value, true, out var r))
                return r!;

            return Result.Failure<object>($"{enumType.Name} does not have a value '{value}'");

        }
#pragma warning restore 8714
    }
}
