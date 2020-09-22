using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.Serialization
{
    /// <summary>
    /// Contributes to the serialized string
    /// </summary>
    public interface ISerializerBlock
    {
        /// <summary>
        /// Gets the segment of serialized text.
        /// </summary>
        public Result<string> TryGetText(FreezableProcessData data);
    }


    /// <summary>
    /// A custom process serializer.
    /// </summary>
    public class ProcessSerializer : IProcessSerializer
    {
        /// <summary>
        /// Create a new ProcessSerializer
        /// </summary>
        public ProcessSerializer(params IProcessSerializerComponent[] components) => Components = components;

        /// <summary>
        /// The component to use.
        /// </summary>
        public IReadOnlyCollection<IProcessSerializerComponent> Components { get; }

        /// <inheritdoc />
        public Result<string> TrySerialize(FreezableProcessData data)
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