using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
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
        public static async Task<string> SerializeToYamlAsync(this IFreezableStep step, CancellationToken cancellationToken)
        {
            var obj = await SimplifyProcessAsync(step, true, cancellationToken);

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
            catch (GeneralSerializerYamlException e)
            {
                return Result.Failure<IFreezableStep, IError>(e.Error);
            }
            catch (YamlException e)
            {
                return new SingleError(e.GetRealMessage(), ErrorCode.CouldNotParse, new YamlRegionErrorLocation(e.Start, e.End));
            }

            return Result.Success<IFreezableStep, IError>(stepMember.ConvertToStep(true));
        }

        internal const string TypeString = "Do";
        internal const string ConfigString = "Config";

        private static async Task<object> SimplifyProcessAsync(IFreezableStep step, bool isTopLevel, CancellationToken cancellationToken)
        {
            switch (step)
            {
                case ConstantFreezableStep cfp:
                    return await SerializationMethods.ConvertToSerializableTypeAsync(cfp, cancellationToken);
                case CompoundFreezableStep compoundFreezableProcess:
                {
                    if (isTopLevel && compoundFreezableProcess.StepFactory == SequenceStepFactory.Instance &&
                        compoundFreezableProcess.FreezableStepData.StepMembersDictionary.TryGetValue(nameof(Sequence.Steps), out var stepMember))
                        return await ToSimpleObject(stepMember, cancellationToken);

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
                        expandoObject[name] = await ToSimpleObject(m, cancellationToken);

                    return expandoObject;

                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }


        private static Task<object> ToSimpleObject(StepMember member, CancellationToken cancellationToken) =>
            member.Join(x=> Task.FromResult<object>(VariableNameComponent.Serialize(x).Value),
                x=> SimplifyProcessAsync(x, false, cancellationToken),
                l=> SimplifyProcessListAsync(l, cancellationToken));


        private static async Task<object> SimplifyProcessListAsync(IEnumerable<IFreezableStep> steps, CancellationToken cancellationToken)
        {
            var list = new List<object>();

            foreach (var freezableStep in steps)
            {
                var o = await SimplifyProcessAsync(freezableStep, false, cancellationToken);
                list.Add(o);
            }

            return list;

        }
    }
}
