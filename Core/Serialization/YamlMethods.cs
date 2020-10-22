using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Core.Serialization
{
    /// <summary>
    /// Contains methods for converting Processes to and from Yaml.
    /// </summary>
    public static class YamlMethods
    {
        /// <summary>
        /// Serialize this step to yaml.
        /// </summary>
        public static string SerializeToYaml(this IFreezableStep step)
        {
            var obj = SimplifyProcess(step, true);

            var builder = new SerializerBuilder()
                .WithTypeConverter(YamlStringTypeConverter.Instance)
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .WithTypeConverter(VersionTypeConverter.Instance);

            var serializer = builder.Build();

            var r = serializer.Serialize(obj).Trim();

            return r;
        }


        /// <summary>
        /// Deserialize this yaml into a step.
        /// </summary>
        public static Result<IFreezableStep, IError> DeserializeFromYaml(string yaml, StepFactoryStore stepFactoryStore)
        {
            if(string.IsNullOrWhiteSpace(yaml))
                return new SingleError("Yaml is empty.", ErrorCode.EmptySequence, EntireSequenceLocation.Instance);

            var parser = new StepMemberParser(stepFactoryStore);

            var nodeDeserializer = new GeneralDeserializer(new ITypedYamlDeserializer []
            {
                new StepMemberDeserializer(parser),
                new FreezableStepDeserializer(parser)
            });

            var builder =
            new DeserializerBuilder()
                .WithNodeDeserializer(nodeDeserializer)
                .WithTypeConverter(VersionTypeConverter.Instance);


            var deserializer = builder.Build();

            StepMember stepMember;

            try
            {
                stepMember = deserializer.Deserialize<StepMember>(yaml);
            }
            catch (YamlException e)
            {
                return new SingleError(e, ErrorCode.CouldNotDeserialize, new YamlErrorLocation(e));
            }

            return Result.Success<IFreezableStep, IError>(stepMember.ConvertToStep(true));
        }

        internal const string TypeString = "Do";
        internal const string ConfigString = "Config";

        private static object SimplifyProcess(IFreezableStep step, bool isTopLevel)
        {
            switch (step)
            {
                case ConstantFreezableStep cfp:
                    return SerializationMethods.ConvertToSerializableType(cfp);
                case CompoundFreezableStep compoundFreezableProcess:
                {
                    if (isTopLevel && compoundFreezableProcess.StepFactory == SequenceStepFactory.Instance &&
                        compoundFreezableProcess.FreezableStepData.StepMembersDictionary.TryGetValue(nameof(Sequence.Steps), out var stepMember))
                        return ToSimpleObject(stepMember);

                    if (compoundFreezableProcess.StepConfiguration == null)//Don't use custom serialization if you have configuration
                    {
                            var sr = compoundFreezableProcess.StepFactory.Serializer
                                    .TrySerialize(compoundFreezableProcess.FreezableStepData);
                            if (sr.IsSuccess) //Serialization will not always succeed.
                                return sr.Value;

                    }

                    IDictionary<string, object> expandoObject = new ExpandoObject();
                    expandoObject[TypeString] = compoundFreezableProcess.StepFactory.TypeName;

                    if (compoundFreezableProcess.StepConfiguration != null)
                        expandoObject[ConfigString] = compoundFreezableProcess.StepConfiguration;

                    foreach (var (name, m) in compoundFreezableProcess.FreezableStepData.StepMembersDictionary)
                        expandoObject[name] = ToSimpleObject(m);

                    return expandoObject;

                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }


        private static object ToSimpleObject(StepMember member) =>
            member.Join(x=> VariableNameComponent.Serialize(x).Value,
                x=> SimplifyProcess(x, false),
                l=>l.Select(x=>SimplifyProcess(x, false)).ToList());
    }
}
