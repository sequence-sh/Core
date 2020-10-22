using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
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
        public override Result<StepMember, IError> TryDeserialize(IParser reader, Func<IParser, Type, object?> nestedObjectDeserializer)
        {
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

                    var r = StepMemberParser.TryParse(scalar.Value, reader.Current.Start, reader.Current.End);

                    if (r.IsSuccess)
                    {
                        reader.MoveNext();
                        return r;
                    }

                    return r;
                }
                default: return new SingleError($"Cannot deserialize {reader.Current}", ErrorCode.CouldNotParse,
                    new YamlRegionErrorLocation(reader.Current!.Start, reader.Current.End));
            }
        }

    }
}