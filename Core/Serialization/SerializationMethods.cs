using System;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using System.Collections.Generic;
using System.Linq;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Serialization
{
    /// <summary>
    /// Deserializes text to primitive types.
    /// </summary>
    public static class SerializationMethods
    {
        /// <summary>
        /// Serialize a constant freezable step. Used in short form serialization.
        /// </summary>
        public static Result<string> TrySerializeConstant(ConstantFreezableStep cfp) => TrySerializeValue(cfp.Value);

        /// <summary>
        /// Serialize a value.
        /// </summary>
        private static Result<string> TrySerializeValue(object value)
        {
            if (value is string s)
            {
                var newS = $"'{Escape(s)}'";
                if (!newS.Contains('\n'))
                    return newS;
                return Result.Failure<string>("String constant contains newline");
            }

            if (value is Enum)
                return value.GetType().Name + "." + value;

            if (value is IEnumerable<object> enumerable)
            {

                var r = enumerable.Select(TrySerializeValue)
                    .Combine()
                    .Map(x=> string.Join(", ", x))
                    .Map(x=> $"[{x}]");

                return r;
            }

            return value.ToString() ?? string.Empty;
        }



        internal static object ConvertToSerializableType(ConstantFreezableStep cfp)
        {
            if (cfp.Value.GetType().IsEnum)
                return cfp.Value.GetType().Name + "." + cfp.Value;


            if (cfp.Value is string s)
                return new YamlString(s);

            return cfp.Value;
        }

        /// <summary>
        /// Escape single quotes
        /// </summary>
        private static string Escape(string s) => s.Replace("'", "''");
    }
}
