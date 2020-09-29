using System.Linq;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// The default step name builder
    /// </summary>
    public class DefaultStepNameBuilder : IStepNameBuilder
    {
        /// <summary>
        /// The step type name.
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// Creates a new DefaultStepNameBuilder.
        /// </summary>
        public DefaultStepNameBuilder(string typeName) => TypeName = typeName;

        /// <inheritdoc />
        public string GetFromArguments(FreezableStepData freezableStepData)
        {
            var args = string.Join(", ", freezableStepData
                .Dictionary
                .OrderBy(x=>x.Key)
                .Select(x => $"{x.Key}: {x.Value.MemberString}"));

            return $"{TypeName}({args})";
        }
    }
}