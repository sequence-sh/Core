using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

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

        /// <inheritdoc />
        public Result<string> TryGetText(FreezableStepData data) =>
            data.Dictionary
                .TryFindOrFail(PropertyName, null)
                .Bind(x => x.Join(VariableNameComponent.Serialize,
                    TrySerialize,
                    _ => Result.Failure<string>("Cannot serialize list")

                ));

        private static Result<string> TrySerialize(IFreezableStep step)
        {
            if (step is ConstantFreezableStep constantFreezableProcess)
                return SerializationMethods.SerializeConstant(constantFreezableProcess, true);
            if (step is CompoundFreezableStep compound && compound.StepConfiguration == null)
                return compound.StepFactory.Serializer.TrySerialize(compound.FreezableStepData);

            return Result.Failure<string>("Cannot serialize compound with a step configuration");
        }

        /// <inheritdoc />
        public ISerializerBlock? SerializerBlock => this;
    }
}