using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Serialization
{
    /// <summary>
    /// A custom step serializer.
    /// </summary>
    public class StepSerializer : IStepSerializer
    {
        /// <summary>
        /// Create a new StepSerializer
        /// </summary>
        public StepSerializer(params IStepSerializerComponent[] components) => Components = components;

        /// <summary>
        /// The component to use.
        /// </summary>
        public IReadOnlyCollection<IStepSerializerComponent> Components { get; }

        /// <inheritdoc />
        public Result<string> TrySerialize(FreezableStepData data)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var serializerBlock in Components.Select(x => x.SerializerBlock).WhereNotNull())
            {
                var r = serializerBlock.TryGetText(data);
                if (r.IsFailure)
                    return r;
                sb.Append(r.Value);
            }

            if (sb.Length == 0)
                return Result.Failure<string>("Serialized string was empty");

            return sb.ToString();
        }
    }
}