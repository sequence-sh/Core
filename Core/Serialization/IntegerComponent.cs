using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Serialization
{
    /// <summary>
    /// Deserializes a regex group into an integer.
    /// </summary>
    public class IntegerComponent :  ISerializerBlock,  IStepSerializerComponent
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
        public Result<string> TryGetText(FreezableStepData data) =>
            data.StepMembersDictionary
                .TryFindOrFail(PropertyName, null)
                .Bind(x => x.Join(VariableNameComponent.Serialize,
                    TrySerialize,
                    _ => Result.Failure<string>("Cannot serialize list")

                ));

        private static Result<string> TrySerialize(IFreezableStep step)
        {
            if (step is ConstantFreezableStep constantFreezableProcess && constantFreezableProcess.Value is int i)
                return i.ToString();
            if (step is CompoundFreezableStep compound && compound.StepConfiguration == null)
                return compound.StepFactory.Serializer.TrySerialize(compound.FreezableStepData);

            return Result.Failure<string>("Cannot a step with configuration");
        }

        /// <inheritdoc />
        public ISerializerBlock? SerializerBlock => this;
    }
}