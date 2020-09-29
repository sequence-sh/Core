using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using Version = System.Version;

namespace Reductech.EDR.Core.Serialization
{

    internal class StepMemberDeserializer : TypedYamlDeserializer<StepMember>
    {
        /// <summary>
        /// Creates a new StepMemberDeserializer
        /// </summary>
        public StepMemberDeserializer(StepMemberParser stepMemberParser) => StepMemberParser = stepMemberParser;

        /// <summary>
        /// The step member parser
        /// </summary>
        public StepMemberParser StepMemberParser { get; }

        /// <inheritdoc />
        public override Result<StepMember, YamlException> TryDeserialize(IParser reader, Func<IParser, Type, object?> nestedObjectDeserializer)
        {
            if(reader.Current == null)
                return new YamlException("Reader is empty");

            var startMark = reader.Current.Start;
            var endMark = reader.Current.End;


            switch (reader.Current)
            {
                case MappingStart _:
                {
                    var r =
                        TryDeserializeNested<IFreezableStep>(nestedObjectDeserializer, reader)
                            .Map(x=> new StepMember(x));

                    return r;

                }
                case SequenceStart _:
                {
                    var r =
                        TryDeserializeNested<List<StepMember>>(nestedObjectDeserializer, reader)
                            .Bind(x=>
                                x.Select(m=>ResultExtensions.Bind<StepMember, IFreezableStep>(m.TryConvert(MemberType.Step), n=>n.AsArgument("Step")))
                                    .Combine()
                                    .MapFailure(e=> new YamlException(startMark, endMark, e)))
                            .Map(x=> new StepMember(x.ToList()));

                    return r;
                }
                case Scalar scalar:
                {

                    if (scalar.IsQuotedImplicit)
                    {
                        var member = new StepMember(new ConstantFreezableStep(scalar.Value));
                        reader.MoveNext();
                        return member;
                    }
                    //otherwise try to deserialize this a as step member

                    var r = StepMemberParser.TryParse(scalar.Value)
                        .MapFailure(e=> new YamlException(reader.Current.Start, reader.Current.End, e));

                    if (r.IsSuccess)
                    {
                        reader.MoveNext();
                        return r;
                    }

                    //TODO remove this bit
                    var member2 = new StepMember(new ConstantFreezableStep(scalar.Value));
                    reader.MoveNext();
                    return member2;
                }
                default: return new YamlException(reader.Current.Start, reader.Current.End, $"Cannot deserialize {reader.Current}");
            }
        }
    }
}