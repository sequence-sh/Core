using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Serialization;

namespace Reductech.EDR.Core.Entities
{
    /// <summary>
    /// A piece of data.
    /// </summary>
    public sealed class Entity : IEnumerable<IGrouping<string, string>>
    {
        private readonly ILookup<string, string> _fields;

        /// <summary>
        /// Create a new record.
        /// </summary>
        public Entity(params KeyValuePair<string, string>[] fields) : this(fields.AsEnumerable()) {}

        /// <summary>
        /// Create a new record.
        /// </summary>
        public Entity(IEnumerable<KeyValuePair<string, string>> fields) =>
            _fields = fields.ToLookup(x => x.Key, x => x.Value);

        /// <summary>
        /// Gets the names of different fields on this object.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetFieldNames() => _fields.Select(x => x.Key);

        /// <summary>
        /// Creates a copy of this with the new fields added or updated.
        /// </summary>
        /// <param name="newFields"></param>
        /// <returns></returns>
        public Entity WithFields(IReadOnlyCollection<KeyValuePair<string, string>> newFields)
        {
            var usedKeys = new HashSet<string>(newFields.Select(x => x.Key));

            var allNewKeyValuePairs = newFields
                .Concat(_fields.Where(x => !usedKeys.Contains(x.Key))
                    .SelectMany(group => group.Select(x => new KeyValuePair<string, string>(group.Key, x))));

            return new Entity(allNewKeyValuePairs);
        }

        /// <summary>
        /// Gets the values of a particular field.
        /// </summary>
        public IEnumerable<string> this[string key] => _fields[key];

        /// <summary>
        /// Gets the values of a particular field.
        /// </summary>
        public IEnumerable<string> GetField(string key) => this[key];

        IEnumerator<IGrouping<string, string>> IEnumerable<IGrouping<string, string>>.GetEnumerator() => _fields.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _fields.GetEnumerator();

        /// <inheritdoc />
        public override string ToString() => AsString();


        /// <summary>
        /// Serialize this record.
        /// </summary>
        /// <returns></returns>
        public Result<string> TrySerializeShortForm()
        {
            var sb = new StringBuilder();

            sb.Append("(");

            var pairs = _fields.SelectMany(grouping => grouping.Select(value => (grouping.Key, value)))
                .Select(x=> SerializationMethods.TrySerializeShortFormString(x.value).Map(k=> $"{x.Key} = {k}"))
                .Combine();

            if (pairs.IsFailure)
                return pairs.ConvertFailure<string>();

            sb.AppendJoin(",", pairs.Value);

            sb.Append(")");

            return sb.ToString();
        }

        /// <summary>
        /// Convert this entity to an object that can be serialized
        /// </summary>
        /// <returns></returns>
        public object ToSimpleObject()
        {
            IDictionary<string, object> expandoObject = new ExpandoObject();

            foreach (var field in _fields)
            {
                if (field.Count() == 1)
                    expandoObject[field.Key] = field.Single();
                else
                    expandoObject[field.Key] = field.ToList();
            }

            return expandoObject;
        }

        /// <summary>
        /// Converts this record into a string.
        /// </summary>
        public string AsString()
        {
            var result = string.Join(", ",
                _fields.Select(field => $"{field.Key}: {string.Join(";", field)}"));

            return result;
        }

    }
}
