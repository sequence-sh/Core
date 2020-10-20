using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Serialization
{

    /// <summary>
    /// Deserializes a regex group into a constant of any type.
    /// </summary>
    public class AnyPrimitiveComponent :  ISerializerBlock,  IStepSerializerComponent
    {
        /// <summary>
        /// Creates a new AnyPrimitiveComponent
        /// </summary>
        /// <param name="propertyName"></param>
        public AnyPrimitiveComponent(string propertyName) => PropertyName = propertyName;

        /// <summary>
        /// The property name
        /// </summary>
        public string PropertyName { get; }

        /// <inheritdoc />
        public Result<string> TryGetText(FreezableStepData data) =>
            data.StepMembersDictionary
                .TryFindOrFail(PropertyName, null)
                .Bind(x => x.Join(VariableNameComponent.Serialize,
                    TrySerialize,
                    _ => Result.Failure<string>("Cannot serialize list")

                ));

        private static Result<string> TrySerialize(IFreezableStep step)
        {
            if (step is ConstantFreezableStep constantFreezableProcess)
                return SerializationMethods.TrySerializeConstant(constantFreezableProcess);
            if (step is CompoundFreezableStep compound && compound.StepConfiguration == null)
                return compound.StepFactory.Serializer.TrySerialize(compound.FreezableStepData);

            return Result.Failure<string>("Cannot serialize compound with a step configuration");
        }

        /// <inheritdoc />
        public ISerializerBlock? SerializerBlock => this;
    }
}