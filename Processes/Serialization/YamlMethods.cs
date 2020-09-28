using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.General;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Processes.Serialization
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

            var serializer = new Serializer();

            var r = serializer.Serialize(obj).Trim();

            return r;
        }

        /// <summary>
        /// Deserialize this yaml into a step.
        /// </summary>
        public static Result<IFreezableStep> DeserializeFromYaml(string yaml, StepFactoryStore stepFactoryStore)
        {
            var parser = new StepMemberParser(stepFactoryStore);

            var nodeDeserializer = new GeneralDeserializer(new ITypedYamlDeserializer []
            {
                new ProcessMemberDeserializer(parser),
                new FreezableStepDeserializer(parser),
            });

            var builder =
            new DeserializerBuilder()
                .WithNodeDeserializer(nodeDeserializer);


            var deserializer = builder.Build();

            StepMember stepMember;

            try
            {
                stepMember = deserializer.Deserialize<StepMember>(yaml);
            }
            catch (YamlException e)
            {
                return Result.Failure<IFreezableStep>(e.GetFullMessage());
            }


            return Result.Success(stepMember)
                .Bind(x=>x.TryConvert(MemberType.Step)
                    .Bind(y=>y.AsArgument("Step")));

        }

        internal const string TypeString = "Do";
        internal const string ConfigString = "Config";

        private static object SimplifyProcess(IFreezableStep step, bool isTopLevel)
        {
            switch (step)
            {
                case ConstantFreezableStep cfp:
                    return SerializationMethods.SerializeConstant(cfp, false);
                case CompoundFreezableStep compoundFreezableProcess:
                {
                    if (isTopLevel && compoundFreezableProcess.StepFactory == SequenceStepFactory.Instance &&
                        compoundFreezableProcess.FreezableStepData.Dictionary.TryGetValue(nameof(Sequence.Steps), out var processMember))
                        return ToSimpleObject(processMember);

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

                    foreach (var (name, m) in compoundFreezableProcess.FreezableStepData.Dictionary)
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
