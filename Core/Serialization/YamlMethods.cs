//using System;
//using System.Collections.Generic;
//using System.Dynamic;
//using System.Threading;
//using System.Threading.Tasks;
//using CSharpFunctionalExtensions;
//using Reductech.EDR.Core.InitialSteps;
//using Reductech.EDR.Core.Internal;
//using Reductech.EDR.Core.Internal.Errors;
//using Reductech.EDR.Core.Util;

//namespace Reductech.EDR.Core.Serialization
//{
//    /// <summary>
//    /// Contains methods for converting Processes to and from Yaml.
//    /// </summary>
//    public static class YamlMethods
//    {
//        /// <summary>
//        /// Serialize this step to yaml.
//        /// </summary>
//        public static async Task<string> SerializeToYamlAsync(this IFreezableStep step, CancellationToken cancellationToken)
//        {
//            step.


//            //var obj = await SimplifyProcessAsync(step, true, cancellationToken);

//            //var builder = new SerializerBuilder()
//            //    .WithTypeConverter(YamlStringTypeConverter.Instance)
//            //    .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
//            //    .WithTypeConverter(VersionTypeConverter.Instance);

//            //var serializer = builder.Build();

//            //var r = serializer.Serialize(obj).Trim();

//            //return r;
//        }


//        /// <summary>
//        /// Deserialize this yaml into a step.
//        /// </summary>
//        public static Result<IFreezableStep, IError> DeserializeFromYaml(string yaml, StepFactoryStore stepFactoryStore)
//        {
//            throw new NotImplementedException();
//            //if(string.IsNullOrWhiteSpace(yaml))
//            //    return new SingleError("Yaml is empty.", ErrorCode.EmptySequence, EntireSequenceLocation.Instance);

//            //var parser = new StepMemberParser(stepFactoryStore);

//            //var nodeDeserializer = new GeneralDeserializer(new ITypedYamlDeserializer []
//            //{
//            //    new StepMemberDeserializer(parser),
//            //    new FreezableStepDeserializer(parser)
//            //});

//            //var builder =
//            //new DeserializerBuilder()
//            //    .WithNodeDeserializer(nodeDeserializer)
//            //    .WithTypeConverter(VersionTypeConverter.Instance);


//            //var deserializer = builder.Build();

//            //FreezableStepProperty stepMember;

//            //try
//            //{
//            //    stepMember = deserializer.Deserialize<FreezableStepProperty>(yaml);
//            //}
//            //catch (GeneralSerializerYamlException e)
//            //{
//            //    return Result.Failure<IFreezableStep, IError>(e.Error);
//            //}
//            //catch (YamlException e)
//            //{
//            //    return new SingleError(e.GetRealMessage(), ErrorCode.CouldNotParse, new YamlRegionErrorLocation(e.Start, e.End));
//            //}

//            //return Result.Success<IFreezableStep, IError>(stepMember.ConvertToStep(true));
//        }

//        internal const string TypeString = "Do";
//        internal const string ConfigString = "Config";

//        private static async Task<object> SimplifyProcessAsync(IFreezableStep step, bool isTopLevel, StepFactoryStore stepFactoryStore, CancellationToken cancellationToken)
//        {
//            switch (step)
//            {
//                case ConstantFreezableStep cfp:
//                    return await SerializationMethods.ConvertToSerializableTypeAsync(cfp, cancellationToken);
//                case CompoundFreezableStep compound:
//                {
//                    if (isTopLevel && compound.StepName == SequenceStepFactory.Instance.TypeName &&
//                        compound.FreezableStepData.InitialSteps.TryGetValue(nameof(Sequence.InitialSteps), out var stepMember))
//                        return await ToSimpleObject(stepMember, stepFactoryStore, cancellationToken);

//                    if (compound.StepConfiguration == null)//Don't use custom serialization if you have configuration
//                    {
//                         var sr =   compound.TryGetStepFactory(stepFactoryStore)
//                    .MapError1(x => x.AsString)
//                    .Bind(x =>
//                        x.Serializer.TrySerialize(compound.FreezableStepData, stepFactoryStore));
//                            if (sr.IsSuccess) //Serialization will not always succeed.
//                                return sr.Value;

//                    }

//                    IDictionary<string, object> expandoObject = new ExpandoObject();
//                    expandoObject[TypeString] = compound.StepName;

//                    if (compound.StepConfiguration != null)
//                        expandoObject[ConfigString] = compound.StepConfiguration;

//                    foreach (var (name, m) in compound.FreezableStepData.InitialSteps)
//                        expandoObject[name] = await ToSimpleObject(m, stepFactoryStore, cancellationToken);

//                    return expandoObject;

//                    }
//                default:
//                    throw new ArgumentOutOfRangeException(nameof(step));
//            }
//        }


//        private static Task<object> ToSimpleObject(FreezableStepProperty member, StepFactoryStore stepFactoryStore, CancellationToken cancellationToken) =>
//            member.Match(x=> Task.FromResult<object>(VariableNameComponent.Serialize(x).Value),
//                x=> SimplifyProcessAsync(x, false, stepFactoryStore, cancellationToken),
//                l=> SimplifyProcessListAsync(l, stepFactoryStore, cancellationToken));


//        private static async Task<object> SimplifyProcessListAsync(IEnumerable<IFreezableStep> steps,StepFactoryStore stepFactoryStore, CancellationToken cancellationToken)
//        {
//            var list = new List<object>();

//            foreach (var freezableStep in steps)
//            {
//                var o = await SimplifyProcessAsync(freezableStep, false, stepFactoryStore, cancellationToken);
//                list.Add(o);
//            }

//            return list;

//        }
//    }
//}
