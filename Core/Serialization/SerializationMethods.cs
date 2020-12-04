﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Reductech.EDR.Core.Entities;

namespace Reductech.EDR.Core.Serialization
{
    /// <summary>
    /// Serializes primitive types
    /// </summary>
    public static class SerializationMethods
    {
        ///// <summary>
        ///// SerializeAsync a constant freezable step. Used in short form serialization.
        ///// </summary>
        //public static Result<string> TrySerializeConstant(ConstantFreezableStep cfp) => SerializeAsync(cfp.Value);

        ///// <summary>
        ///// SerializeAsync a value.
        ///// </summary>
        //private static Result<string> SerializeAsync(object value)
        //{
        //    if (value is string s)
        //        return TrySerializeShortFormString(s);

        //    if (value is IEnumerable enumerable)
        //    {


        //        var r = TrySerializeSimpleList(enumerable.OfType<object>());
        //        return r;
        //    }

        //    if (value is Entity entity)
        //        return entity.SerializeAsync();

        //    if (value is Stream)
        //        return Result.Failure<string>("Streams cannot be serialized in short form");

        //    if (value is EntityStream)
        //        return Result.Failure<string>("EntityStreams cannot be serialized in short form");


        //    return TrySerializeSimple(value);
        //}


        ///// <summary>
        ///// Try to deserialize list of simple objects.
        ///// </summary>
        //public static Result<string> TrySerializeSimpleList(IEnumerable<object> values)
        //{
        //    return values.Select(SerializeAsync)
        //            .Combine()
        //            .Map(x => string.Join(", ", x))
        //            .Map(x => $"[{x}]");
        //}

        ///// <summary>
        ///// Try to serialize a simple object.
        ///// </summary>
        //public static Result<string> TrySerializeSimple(object value)
        //{
        //    if (value is Enum)
        //        return value.GetType().Name + "." + value;
        //    if (value is bool b)
        //        return b.ToString();
        //    if (value is int i)
        //        return i.ToString();
        //    if (value is double d)
        //        return d.ToString("G17", CultureInfo.InvariantCulture);


        //    return Result.Failure<string>($"Could not serialize {value.GetType().Name}");
        //}


        //private static object ConvertToSerializableType(Entity entity)
        //{
        //    var shortFormEntity = entity.SerializeAsync();
        //    if (shortFormEntity.IsSuccess)
        //        return shortFormEntity.Value;

        //    return entity.ToSimpleObject();
        //}




        //internal static async Task<object> ConvertToSerializableTypeAsync(ConstantFreezableStep cfp, CancellationToken cancellationToken)
        //{
        //    return await Convert(cfp.Value, cancellationToken);


        //    static async Task<object> Convert(object value, CancellationToken cancellationToken)
        //    {
        //        if (value is string s)
        //            return new YamlString(s);

        //        if (value is Entity entity)
        //            return ConvertToSerializableType(entity);

        //        if (value is EntityStream entityStream)
        //        {
        //            var task = await entityStream.TryGetResultsAsync(cancellationToken);

        //            var entities = task
        //                //This should work, but maybe not in some environments
        //                .Map(x => x.Select(ConvertToSerializableType).ToList());

        //            if (entities.IsFailure)
        //                throw new SerializationException(entities.Error.AsString);

        //            return entities.Value;
        //        }


        //        if (value is Stream stream)
        //        {
        //            var streamString = StreamToString(stream, Encoding.UTF8);
        //            return new YamlString(streamString); //This will be a string - convert it to a stream
        //        }

        //        if (value is Schema schema)
        //            return schema; //Yaml serializable

        //        if (value is IEnumerable enumerable)
        //        {
        //            var list = new List<object>();

        //            foreach (var member in enumerable.OfType<object>())
        //            {
        //                var o = await Convert(member, cancellationToken);
        //                list.Add(o);
        //            }

        //            return list;
        //        }


        //        var r = TrySerializeSimple(value);

        //        if (r.IsSuccess)
        //            return r.Value;

        //        throw new SerializationException(r.Error);//This is unexpected
        //    }
        //}

        /// <summary>
        /// SerializeAsync a list
        /// </summary>
        public static string SerializeList(IEnumerable<string> serializedMembers)
        {
            var sb2 = new StringBuilder();

            sb2.Append('[');
            sb2.AppendJoin(", ", serializedMembers);
            sb2.Append(']');

            return sb2.ToString();
        }

        /// <summary>
        /// Quote with single quotes
        /// </summary>
        public static string SingleQuote(string s) => "'" + s.Replace("'", "''") + "'";

        /// <summary>
        /// Quote with double quotes
        /// </summary>
        public static string DoubleQuote(string s) =>

            "\"" + (s
            .Replace("\\", @"\\")
            .Replace("\r", @"\r")
            .Replace("\n", @"\n")) + "\"";

        /// <summary>
        /// Serialize an entityStream
        /// </summary>
        public static async Task<string> SerializeEntityStreamAsync(EntityStream entityStream, CancellationToken cancellationToken)
        {
            var enumerable = await entityStream.SourceEnumerable.Select(x=>x.Serialize()) .ToListAsync(cancellationToken);

            return SerializeList(enumerable);
        }
    }
}
