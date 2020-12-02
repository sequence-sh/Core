using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using CSharpFunctionalExtensions;
using OneOf;

namespace Reductech.EDR.Core.Util
{
    /// <summary>
    /// SerializationMethods methods.
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
        /// Gets a full message from an exception.
        /// </summary>
        public static string GetFullMessage(this Exception exception)
        {
            if (exception.InnerException == null)
                return exception.Message;

            var innerMessage = exception.InnerException.GetFullMessage();

            var message = $"{exception.Message}\r\n{innerMessage}";

            return message;
        }


        /// <summary>
        /// Gets the name of an enum value from the display attribute if it is present.
        /// </summary>
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())?
                            .First(x=>x.MemberType == MemberTypes.Field)?
                            .GetCustomAttribute<DisplayAttribute>()?
                            .GetName()?? enumValue.ToString();
        }

        /// <summary>
        /// Returns the string, unless it is null or whitespace, in which case the backup is returned.
        /// </summary>
        public static string DefaultIfNullOrWhitespace(this string s, string backup)
        {
            if (string.IsNullOrWhiteSpace(s))
                return backup;
            return s;
        }

        /// <summary>
        /// Tries to parse the enum value. Uses both the name of the enum and the display name.
        /// </summary>
        public static Maybe<T> TryParseValue<T>(string s) where T : Enum
        {
            if (Enum.TryParse(typeof(T), s, true, out var r) && r is T t)
                return CSharpFunctionalExtensions.Maybe<T>.From(t);

            foreach (var value in Enum.GetValues(typeof(T)).Cast<T>())
            {
                if(value.ToString().Equals(s, StringComparison.OrdinalIgnoreCase))
                    return CSharpFunctionalExtensions.Maybe<T>.From(value);

                var displayName = value.GetDisplayName();

                if(displayName.Equals(s, StringComparison.OrdinalIgnoreCase))
                    return CSharpFunctionalExtensions.Maybe<T>.From(value);
            }

            return CSharpFunctionalExtensions.Maybe<T>.None;

        }

        /// <summary>
        /// Gets all possible values of this enum.
        /// </summary>
        public static IEnumerable<T> GetEnumValues<T>() where T: Enum => Enum.GetValues(typeof(T)).Cast<T>();

        /// <summary>
        /// Tries to get the element. Returns a failure if it is not present.
        /// </summary>
#pragma warning disable 8714
        public static Result<TValue> TryFindOrFail<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary,
            TKey key, string? error)
        {
            var r = dictionary.TryFind(key);

            return r.ToResult(error??$"The element '{key}' was not present.");
        }


        /// <summary>
        /// Tries to get the element. Returns a failure if it is not present.
        /// </summary>
#pragma warning disable 8714
        public static Result<TValue, TError> TryFindOrFail<TKey, TValue, TError>(this IReadOnlyDictionary<TKey, TValue> dictionary,
            TKey key, Func<TError> error)
        {
            var r = dictionary.TryFind(key);

            if (r.HasValue) return r.Value!;

            return error()!;
        }

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
