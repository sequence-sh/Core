using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.Serialization
{
    /// <summary>
    /// Serializer for processes that should not use short form serialization.
    /// </summary>
    public sealed class NoSpecialSerializer : IProcessSerializer
    {
        private NoSpecialSerializer() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static IProcessSerializer Instance { get; } = new NoSpecialSerializer();

        /// <inheritdoc />
        public Result<string> TrySerialize(FreezableProcessData data) => Result.Failure<string>("This process does not support special serialization");
    }


    /// <summary>
    /// The default process serializer for functions.
    /// Produces results like: Print(Value: 'Hello World')
    /// </summary>
    public sealed class FunctionSerializer : IProcessSerializer
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
        public Result<string> TrySerialize(FreezableProcessData data)
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


            static Result<string> SerializeProcess(IFreezableProcess freezableProcess)
            {
                return freezableProcess switch
                {
                    ConstantFreezableProcess constant => SerializationMethods.SerializeConstant(constant, true),
                    CompoundFreezableProcess compoundFreezableProcess => compoundFreezableProcess.ProcessFactory
                        .Serializer.TrySerialize(compoundFreezableProcess.FreezableProcessData),
                    _ => Result.Failure<string>("Cannot serialize")
                };
            }

            static Result<string> SerializeList(IReadOnlyList<IFreezableProcess> list)
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