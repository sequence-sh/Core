using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
        public Entity(params KeyValuePair<string, string>[] fields) : this(fields.AsEnumerable())
        {

        }

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
        /// Converts this record into a string.
        /// </summary>
        /// <returns></returns>
        public string AsString()
        {
            var result = string.Join(", ",
                _fields.Select(field => $"{field.Key}: {string.Join(";", field)}"));

            return result;
        }

    }
}
