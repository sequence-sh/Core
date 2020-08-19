using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes
{
    /// <summary>
    /// Deserializes a regex group into an enum.
    /// </summary>
    public class EnumDeserializerMapping<T> : IDeserializerMapping
        where T: Enum
    {
        public EnumDeserializerMapping(string groupName, string propertyName, Func<T, string> getName)
        {
            GroupName = groupName;
            PropertyName = propertyName;

            Dictionary = Extensions.GetEnumValues<T>().ToDictionary(getName, x => x);
        }

        /// <inheritdoc />
        public string GroupName { get; }

        /// <inheritdoc />
        public string PropertyName { get; }

        /// <summary>
        /// Enum mapping dictionary.
        /// </summary>
        public IReadOnlyDictionary<string, T> Dictionary { get; }

        /// <inheritdoc />
        public Result<ProcessMember> TryDeserialize(string groupText, ProcessFactoryStore processFactoryStore)
        {
            if (Dictionary.TryGetValue(groupText, out var t))
                return new ProcessMember(new ConstantFreezableProcess(t));
            return Result.Failure<ProcessMember>($"Could not parse '{t}' as a {typeof(T).Name}");
        }
    }
}