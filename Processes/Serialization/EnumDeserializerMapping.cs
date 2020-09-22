using System;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.Serialization
{

    /// <summary>
    /// Deserializes a regex group into an enum.
    /// </summary>
    public class EnumDisplayComponent<T>
        : ISerializerBlock,  ICustomSerializerComponent
        where T : Enum
    {
        /// <summary>
        /// Creates a new EnumDisplayComponent
        /// </summary>
        /// <param name="propertyName"></param>
        public EnumDisplayComponent(string propertyName) => PropertyName = propertyName;

        ///// <inheritdoc />
        //public string GetGroupName(int index) => "Value" + index;

        /// <summary>
        /// The name of the property
        /// </summary>
        public string PropertyName { get; }

        ///// <inheritdoc />
        //public Result<ProcessMember> TryDeserialize(string groupText, ProcessFactoryStore processFactoryStore)
        //{
        //    if (Dictionary.TryGetValue(groupText.Trim(), out var r))
        //        return new ProcessMember(new ConstantFreezableProcess(r));

        //    return Result.Failure<ProcessMember>($"Could not parse '{groupText}' as an enum.");
        //}

        //private IReadOnlyDictionary<string, T> Dictionary { get; } = Extensions.GetEnumValues<T>()
        //    .ToDictionary(x=>x.GetDisplayName(), x=>x);


        /// <inheritdoc />
        public Result<string> TryGetText(FreezableProcessData data) =>
            data.Dictionary
                .TryFindOrFail(PropertyName, null)
                .Bind(x => x.Join(VariableNameComponent.Serialize,
                    TrySerialize,
                    _ => Result.Failure<string>("Cannot serialize list")));

        private Result<string> TrySerialize(IFreezableProcess process)
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

        ///// <inheritdoc />
        //public string GetRegexText(int index) => @$"(?:(?<{GetGroupName(index)}>(?:{string.Join('|', Dictionary.Keys.Select(Regex.Escape))}))|(?<{GetGroupName(index)}><[\w\d\._]+?>))";

        /// <inheritdoc />
        public ISerializerBlock? SerializerBlock => this;

        ///// <inheritdoc />
        //public IDeserializerBlock? DeserializerBlock => this;

        ///// <inheritdoc />
        //public IDeserializerMapping? Mapping => this;
    }
}