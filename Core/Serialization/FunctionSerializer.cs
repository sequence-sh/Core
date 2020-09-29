using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
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
        public Result<string> TrySerialize(FreezableStepData data)
        {
            var sb = new StringBuilder();
            sb.Append(Name);
            sb.Append("(");

            var first = true;


            foreach (var (propertyName, value) in
                data.Dictionary.OrderBy(x=>x.Key))
            {
                if (first)
                    first = false;
                else
                    sb.Append(", ");

                sb.Append(propertyName);
                sb.Append(" = ");


                var valueSerializeResult =
                    value.Join(
                        VariableNameComponent.Serialize,
                        SerializeProcess,
                        SerializeList);

                if (valueSerializeResult.IsFailure)
                    return valueSerializeResult;

                sb.Append(valueSerializeResult.Value);
            }
            sb.Append(")");

            return sb.ToString();


            static Result<string> SerializeProcess(IFreezableStep freezableProcess)
            {
                return freezableProcess switch
                {
                    ConstantFreezableStep constant => SerializationMethods.SerializeConstant(constant, true),
                    CompoundFreezableStep compoundFreezableProcess => compoundFreezableProcess.StepFactory
                        .Serializer.TrySerialize(compoundFreezableProcess.FreezableStepData),
                    _ => Result.Failure<string>("Cannot serialize")
                };
            }

            static Result<string> SerializeList(IReadOnlyList<IFreezableStep> list)
            {
                var elementResult = list.Select(SerializeProcess).Combine();
                if (elementResult.IsFailure)
                    return elementResult.ConvertFailure<string>();

                var sb2 = new StringBuilder();

                sb2.Append("[");
                sb2.AppendJoin(", ", elementResult.Value);
                sb2.Append("]");

                return sb2.ToString();
            }
        }
    }
}