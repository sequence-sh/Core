using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

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

            //var startMark = reader.Current.Start; //TODO use these
            //var endMark = reader.Current.End;


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
                            .Map(x=> x.Select(m=>m.ConvertToStep(false)).ToList())
                            .Map(x=> new StepMember(x));

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
                        .MapFailure(e=> CreateError(reader.Current.Start, reader.Current.End, e, scalar.Value));

                    if (r.IsSuccess)
                    {
                        reader.MoveNext();
                        return r;
                    }

                    return r;
                }
                default: return new YamlException(reader.Current.Start, reader.Current.End, $"Cannot deserialize {reader.Current}");
            }
        }

        private static YamlException CreateError(Mark start, Mark end, StepMemberParseError error, string scalarValue)
        {

            var newStart = new Mark(start.Index + error.ErrorPosition.Absolute,
                start.Line + error.ErrorPosition.Line - 1,
                start.Column + error.ErrorPosition.Column - 1);

            return new YamlException(newStart, end, error.ErrorMessage?? $"Could not parse '{scalarValue}'" );

        }
    }
}