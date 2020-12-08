using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        public async Task<string> SerializeAsync(IEnumerable<StepProperty> stepProperties, CancellationToken cancellationToken)
        {
            var sb = new StringBuilder();
            sb.Append(Name);
            //sb.Append('(');

            //var first = true;

            foreach (var stepProperty in stepProperties.OrderBy(x => x.Index))
            {
                //if (first)
                //    first = false;
                //else
                //    sb.Append(", ");
                sb.Append(' ');



                sb.Append(stepProperty.Name);
                sb.Append(": ");

                var value = await stepProperty.SerializeValueAsync(cancellationToken);

                 if (stepProperty.Value.IsT1 && stepProperty.Value.AsT1 is ICompoundStep cs && cs.ShouldBracketWhenSerialized)
                    value = '(' + value + ')';
                sb.Append(value );
            }
            //sb.Append(')');

            return sb.ToString();
        }
    }
}