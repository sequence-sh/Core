using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.Serialization
{
    /// <summary>
    /// Deserializes a regex group into an integer.
    /// </summary>
    public class IntegerComponent :  ISerializerBlock,  ICustomSerializerComponent
    {
        /// <summary>
        /// Creates a new IntegerComponent
        /// </summary>
        /// <param name="propertyName"></param>
        public IntegerComponent(string propertyName) => PropertyName = propertyName;

        /// <summary>
        /// The name of the property.
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
            if (process is ConstantFreezableProcess constantFreezableProcess && constantFreezableProcess.Value is int i)
                return i.ToString();
            if (process is CompoundFreezableProcess compound && compound.ProcessConfiguration == null)
                return compound.ProcessFactory.Serializer.TrySerialize(compound.FreezableProcessData);

            return Result.Failure<string>("Cannot a process with configuration");
        }

        /// <inheritdoc />
        public ISerializerBlock? SerializerBlock => this;
    }
}