using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.Serialization
{
    /// <summary>
    /// Deserializes a regex group into a bool.
    /// </summary>
    public class BooleanComponent : ISerializerBlock, IProcessSerializerComponent
    {
        /// <summary>
        /// Creates a new BooleanComponent
        /// </summary>
        public BooleanComponent(string propertyName) => PropertyName = propertyName;

        /// <summary>
        /// The property name
        /// </summary>
        public string PropertyName { get; }

        /// <inheritdoc />
        public Result<string> TryGetText(FreezableProcessData data) =>
            data.Dictionary
                .TryFindOrFail(PropertyName, null)
                .Bind(x => x.Join(VariableNameComponent.Serialize,
                    TrySerialize,
                    _ => Result.Failure<string>("Cannot serialize list")

                ));

        private static Result<string> TrySerialize(IFreezableProcess process)
        {
            if (process is ConstantFreezableProcess constantFreezableProcess && constantFreezableProcess.Value is bool b)
                return b.ToString();
            if (process is CompoundFreezableProcess compound && compound.ProcessConfiguration == null)
                return compound.ProcessFactory.Serializer.TrySerialize(compound.FreezableProcessData);

            return Result.Failure<string>("Cannot serialize compound as a primitive");
        }

        /// <inheritdoc />
        public ISerializerBlock? SerializerBlock => this;
    }
}