using System;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.Serialization
{

    /// <summary>
    /// Deserializes a regex group into an enum.
    /// </summary>
    public class EnumDisplayComponent<T>
        : ISerializerBlock,  IProcessSerializerComponent
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
        public Result<string> TryGetText(FreezableProcessData data) =>
            data.Dictionary
                .TryFindOrFail(PropertyName, null)
                .Bind(x => x.Join(VariableNameComponent.Serialize,
                    TrySerialize,
                    _ => Result.Failure<string>("Cannot serialize list")));

        private static Result<string> TrySerialize(IFreezableProcess process)
        {

            if (process is ConstantFreezableProcess constant && constant.Value is T t)
                return t.GetDisplayName();

            if (process is CompoundFreezableProcess compound && compound.ProcessConfiguration == null)
            {
                var cSerializeResult = compound.ProcessFactory.Serializer.TrySerialize(compound.FreezableProcessData);

                return cSerializeResult;
            }

            return Result.Failure<string>("Cannot serialize a process with a ProcessConfiguration");
        }

        /// <inheritdoc />
        public ISerializerBlock? SerializerBlock => this;
    }
}