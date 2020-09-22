using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.Serialization
{

    /// <summary>
    /// Deserializes a regex group into a constant of any type.
    /// </summary>
    public class AnyPrimitiveComponent :  ISerializerBlock,  IProcessSerializerComponent
    {
        /// <summary>
        /// Creates a new AnyPrimitiveComponent
        /// </summary>
        /// <param name="propertyName"></param>
        public AnyPrimitiveComponent(string propertyName) => PropertyName = propertyName;

        ///// <inheritdoc />
        //public string GetGroupName(int index) => "Value" + index;

        /// <summary>
        /// The property name
        /// </summary>
        public string PropertyName { get; }

        ///// <inheritdoc />
        //public Result<ProcessMember> TryDeserialize(string groupText, ProcessFactoryStore processFactoryStore) => SerializationMethods.TryDeserialize(groupText, processFactoryStore);

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
            if (process is ConstantFreezableProcess constantFreezableProcess)
                return SerializationMethods.SerializeConstant(constantFreezableProcess, true);
            if (process is CompoundFreezableProcess compound && compound.ProcessConfiguration == null)
                return compound.ProcessFactory.Serializer.TrySerialize(compound.FreezableProcessData);

            return Result.Failure<string>("Cannot serialize compound with a process configuration");
        }


        ///// <inheritdoc />
        //public string GetRegexText(int index) => @$"(?:(?<{GetGroupName(index)}>(?:[\w\d\._]+))|(?<{GetGroupName(index)}>[""'].*?[""'])|(?<{GetGroupName(index)}><[\w\d\._]+?>))";

        /// <inheritdoc />
        public ISerializerBlock? SerializerBlock => this;

        ///// <inheritdoc />
        //public IDeserializerBlock? DeserializerBlock  => this;

        ///// <inheritdoc />
        //public IDeserializerMapping? Mapping => this;
    }
}