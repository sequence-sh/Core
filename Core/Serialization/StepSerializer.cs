using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;

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
        public StepSerializer(string typeName, params ISerializerBlock[] components)
        {
            TypeName = typeName;
            Blocks = components;
        }

        /// <summary>
        /// The name of the step to serialize
        /// </summary>
        public string TypeName { get; }

        /// <inheritdoc />
        public override string ToString() => TypeName;

        /// <summary>
        /// The component to use.
        /// </summary>
        public IReadOnlyCollection<ISerializerBlock> Blocks { get; }


        /// <inheritdoc />
        public  async Task<string> SerializeAsync(IEnumerable<StepProperty> stepProperties, CancellationToken cancellationToken)
        {
            var dict = stepProperties.
                ToDictionary(x => x.Name);


            var result = await Blocks
                .Select(x => x.TryGetSegmentTextAsync(dict, cancellationToken))
                .Combine();

            if (result.IsSuccess) //The custom serialization worked
                return string.Join("", result.Value);

            var defaultSerializer = new FunctionSerializer(TypeName);

            var r = await defaultSerializer.SerializeAsync(dict.Values, cancellationToken);

            return r;
        }
    }
}