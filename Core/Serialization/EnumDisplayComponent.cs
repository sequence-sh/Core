using System;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Serialization
{

    /// <summary>
    /// Deserializes a regex group into an enum.
    /// </summary>
    public class EnumDisplayComponent<T>
        : ISerializerBlock,  IStepSerializerComponent
        where T : Enum
    {
        /// <summary>
        /// Creates a new EnumDisplayComponent
        /// </summary>
        /// <param name="propertyName"></param>
        public EnumDisplayComponent(string propertyName) => PropertyName = propertyName;

        /// <summary>
        /// The name of the property
        /// </summary>
        public string PropertyName { get; }

        /// <inheritdoc />
        public Result<string> TryGetText(FreezableStepData data) =>
            data.Dictionary
                .TryFindOrFail(PropertyName, null)
                .Bind(x => x.Join(VariableNameComponent.Serialize,
                    TrySerialize,
                    _ => Result.Failure<string>("Cannot serialize list")));

        private static Result<string> TrySerialize(IFreezableStep step)
        {

            if (step is ConstantFreezableStep constant && constant.Value is T t)
                return t.GetDisplayName();

            if (step is CompoundFreezableStep compound && compound.StepConfiguration == null)
            {
                var cSerializeResult = compound.StepFactory.Serializer.TrySerialize(compound.FreezableStepData);

                return cSerializeResult;
            }

            return Result.Failure<string>("Cannot serialize a step with a Configuration");
        }

        /// <inheritdoc />
        public ISerializerBlock? SerializerBlock => this;
    }
}