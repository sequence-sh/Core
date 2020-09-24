using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace Reductech.EDR.Processes.Serialization
{
    internal class ProcessMemberDeserializer : TypedYamlDeserializer<ProcessMember>
    {
        /// <summary>
        /// Creates a new ProcessMemberDeserializer
        /// </summary>
        public ProcessMemberDeserializer(ProcessMemberParser processMemberParser) => ProcessMemberParser = processMemberParser;

        /// <summary>
        /// The process member parser
        /// </summary>
        public ProcessMemberParser ProcessMemberParser { get; }

        /// <inheritdoc />
        public override Result<ProcessMember, YamlException> TryDeserialize(IParser reader, Func<IParser, Type, object?> nestedObjectDeserializer)
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
                        TryDeserializeNested<IFreezableProcess>(nestedObjectDeserializer, reader)
                            .Map(x=> new ProcessMember(x));

                    return r;

                }
                case SequenceStart _:
                {
                    var r =
                        TryDeserializeNested<List<ProcessMember>>(nestedObjectDeserializer, reader)
                            .Bind(x=>
                                x.Select(m=>ResultExtensions.Bind<ProcessMember, IFreezableProcess>(m.TryConvert(MemberType.Process), n=>n.AsArgument("Step")))
                                    .Combine()
                                    .MapFailure(e=> new YamlException(startMark, endMark, e)))
                            .Map(x=> new ProcessMember(x.ToList()));

                    return r;
                }
                case Scalar scalar:
                {

                    if (scalar.IsQuotedImplicit)
                    {
                        var member = new ProcessMember(new ConstantFreezableProcess(scalar.Value));
                        reader.MoveNext();
                        return member;
                    }
                    //otherwise try to deserialize this a as process member

                    var r = ProcessMemberParser.TryParse(scalar.Value)
                        .MapFailure(e=> new YamlException(reader.Current.Start, reader.Current.End, e));

                    if (r.IsSuccess)
                    {
                        reader.MoveNext();
                        return r;
                    }

                    //TODO remove this bit
                    var member2 = new ProcessMember(new ConstantFreezableProcess(scalar.Value));
                    reader.MoveNext();
                    return member2;
                }
                default: return new YamlException(reader.Current.Start, reader.Current.End, $"Cannot deserialize {reader.Current}");
            }
        }
    }
}