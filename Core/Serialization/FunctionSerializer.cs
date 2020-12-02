using System.Collections.Generic;
using System.Linq;
using System.Text;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.Serialization
{
    /// <summary>
    /// The default step serializer for functions.
    /// Produces results like: Print(Value: 'Hello World')
    /// </summary>
    public sealed class FunctionSerializer : IStepSerializer
    {
        /// <summary>
        /// Creates a new FunctionSerializer
        /// </summary>
        /// <param name="name"></param>
        public FunctionSerializer(string name) { Name = name; }

        /// <summary>
        /// The name of the function.
        /// </summary>
        public string Name { get; }


        /// <inheritdoc />
        public string Serialize(IEnumerable<StepProperty> stepProperties)
        {
            var sb = new StringBuilder();
            sb.Append(Name);
            sb.Append("(");

            var first = true;


            foreach (var stepProperty in stepProperties.OrderBy(x => x.Index))
            {
                if (first)
                    first = false;
                else
                    sb.Append(", ");

                sb.Append(stepProperty.Name);
                sb.Append(" = ");

                sb.Append(stepProperty.SerializeValue());
            }
            sb.Append(")");

            return sb.ToString();
        }
    }
}