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
        /// Serialize this process to yaml.
        /// </summary>
        public static string SerializeToYaml(this IFreezableProcess process)
        {
            var obj = SimplifyProcess(process, true);

            var serializer = new Serializer();

            var r = serializer.Serialize(obj).Trim();

            return r;
        }

        /// <summary>
        /// Deserialize this yaml into a process.
        /// </summary>
        public static Result<IFreezableProcess> DeserializeFromYaml(string yaml, ProcessFactoryStore processFactoryStore)
        {
            var parser = new ProcessMemberParser(processFactoryStore);

            var nodeDeserializer = new GeneralDeserializer(new ITypedYamlDeserializer []
            {
                new ProcessMemberDeserializer(parser),
                new FreezableProcessDeserializer(parser),
            });

            var builder =
            new DeserializerBuilder()
                .WithNodeDeserializer(nodeDeserializer);


            var deserializer = builder.Build();

            ProcessMember processMember;

            try
            {
                processMember = deserializer.Deserialize<ProcessMember>(yaml);
            }
            catch (YamlException e)
            {
                return Result.Failure<IFreezableProcess>(e.GetFullMessage());
            }


            return Result.Success(processMember)
                .Bind(x=>x.TryConvert(MemberType.Process)
                    .Bind(y=>y.AsArgument("Process")));

        }

        internal const string TypeString = "Do";
        internal const string ConfigString = "Config";

        private static object SimplifyProcess(IFreezableProcess process, bool isTopLevel)
        {
            switch (process)
            {
                case ConstantFreezableProcess cfp:
                    return SerializationMethods.SerializeConstant(cfp, false);
                case CompoundFreezableProcess compoundFreezableProcess:
                {
                    if (isTopLevel && compoundFreezableProcess.ProcessFactory == SequenceProcessFactory.Instance &&
                        compoundFreezableProcess.FreezableProcessData.Dictionary.TryGetValue(nameof(Sequence.Steps), out var processMember))
                        return ToSimpleObject(processMember);

                    if (compoundFreezableProcess.ProcessConfiguration == null)//Don't use custom serialization if you have configuration
                    {
                            var sr = compoundFreezableProcess.ProcessFactory.Serializer
                                    .TrySerialize(compoundFreezableProcess.FreezableProcessData);
                            if (sr.IsSuccess) //Serialization will not always succeed.
                                return sr.Value;

                    }

                    IDictionary<string, object> expandoObject = new ExpandoObject();
                    expandoObject[TypeString] = compoundFreezableProcess.ProcessFactory.TypeName;

                    if (compoundFreezableProcess.ProcessConfiguration != null)
                        expandoObject[ConfigString] = compoundFreezableProcess.ProcessConfiguration;

                    foreach (var (name, m) in compoundFreezableProcess.FreezableProcessData.Dictionary)
                        expandoObject[name] = ToSimpleObject(m);

                    return expandoObject;

                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(process));
            }
        }


        private static object ToSimpleObject(ProcessMember member) =>
            member.Join(x=> VariableNameComponent.Serialize(x).Value,
                x=> SimplifyProcess(x, false),
                l=>l.Select(x=>SimplifyProcess(x, false)).ToList());
    }
}
