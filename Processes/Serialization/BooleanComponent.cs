using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

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
        public Result<string> TryGetText(FreezableStepData data) =>
            data.Dictionary
                .TryFindOrFail(PropertyName, null)
                .Bind(x => x.Join(VariableNameComponent.Serialize,
                    TrySerialize,
                    _ => Result.Failure<string>("Cannot serialize list")

                ));

        private static Result<string> TrySerialize(IFreezableStep step)
        {
            if (step is ConstantFreezableStep constantFreezableProcess && constantFreezableProcess.Value is bool b)
                return b.ToString();
            if (step is CompoundFreezableStep compound && compound.StepConfiguration == null)
                return compound.StepFactory.Serializer.TrySerialize(compound.FreezableStepData);

            return Result.Failure<string>("Cannot serialize compound as a primitive");
        }

        /// <inheritdoc />
        public ISerializerBlock? SerializerBlock => this;
    }
}