﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.General;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.Serialization
{

    /// <summary>
    /// Deserializes a regex group into a constant of any type.
    /// </summary>
    public class AnyPrimitiveComponent : IDeserializerMapping, ISerializerBlock, IDeserializerBlock, ICustomSerializerComponent
    {
        public AnyPrimitiveComponent(string propertyName) => PropertyName = propertyName;

        /// <inheritdoc />
        public string GetGroupName(int index) => "Value" + index;

        /// <inheritdoc />
        public string PropertyName { get; }

        /// <inheritdoc />
        public Result<ProcessMember> TryDeserialize(string groupText, ProcessFactoryStore processFactoryStore) => SerializationHelper.TryDeserialize(groupText, processFactoryStore);

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
                return SerializationHelper.SerializeConstant(constantFreezableProcess, true);
            if (process is CompoundFreezableProcess compound && compound.ProcessFactory == GetVariableProcessFactory.Instance) //Special case
                return compound.SerializeToYaml().Trim();

            return Result.Failure<string>("Cannot serialize compound as a primitive");
        }





        /// <inheritdoc />
        public string GetRegexText(int index) => @$"(?:(?<{GetGroupName(index)}>(?:[\w\d\._]+))|(?<{GetGroupName(index)}>[""'].*?[""'])|(?<{GetGroupName(index)}><[\w\d\._]+?>))";

        /// <inheritdoc />
        public ISerializerBlock? SerializerBlock => this;

        /// <inheritdoc />
        public IDeserializerBlock? DeserializerBlock  => this;

        /// <inheritdoc />
        public IDeserializerMapping? Mapping => this;
    }
}