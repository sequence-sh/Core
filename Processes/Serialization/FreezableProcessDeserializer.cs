using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace Reductech.EDR.Processes.Serialization
{
    internal class FreezableProcessDeserializer : TypedYamlDeserializer<IFreezableProcess>
    {
        /// <inheritdoc />
        public FreezableProcessDeserializer(ProcessMemberParser processMemberParser) => ProcessMemberParser = processMemberParser;

        /// <summary>
        /// The process member parser
        /// </summary>
        public ProcessMemberParser ProcessMemberParser { get; }


        /// <inheritdoc />
        public override Result<IFreezableProcess, YamlException> TryDeserialize(IParser parser, Func<IParser, Type, object?> nestedObjectDeserializer)
        {
            if (parser.Current == null)
                return new YamlException("Reader is empty");

            var markStart = parser.Current.Start;
            var markEnd = parser.Current.End;

            var dictionary = new Dictionary<string, ProcessMember>();

            IRunnableProcessFactory? factory = null;
            ProcessConfiguration? configuration = null;

            parser.Consume<MappingStart>();
            while (!parser.TryConsume<MappingEnd>(out _))
            {
                //TODO handle promises

                var keyResult = TryDeserializeNested<string>(nestedObjectDeserializer, parser);

                if (keyResult.IsFailure)
                    return keyResult.ConvertFailure<IFreezableProcess>();

                if (keyResult.Value.Equals(YamlMethods.TypeString, StringComparison.OrdinalIgnoreCase))
                {
                    if (factory != null)
                        return new YamlException(markStart, markEnd, $"Duplicate property '{keyResult.Value}'");

                    var typeNameResult = TryDeserializeNested<string>(nestedObjectDeserializer, parser)
                        .Bind(x => ProcessMemberParser
                            .ProcessFactoryStore.Dictionary.TryFindOrFail(x, $"'{x}' is not the name of a Process")
                            .MapFailure(e => new YamlException(markStart, markEnd, e))
                        );

                    if (typeNameResult.IsFailure)
                        return typeNameResult.ConvertFailure<IFreezableProcess>();

                    factory = typeNameResult.Value;
                }
                else if (keyResult.Value.Equals(YamlMethods.ConfigString, StringComparison.OrdinalIgnoreCase))
                {
                    if (configuration != null)
                        return new YamlException(markStart, markEnd, $"Duplicate property '{keyResult.Value}'");

                    var configResult = TryDeserializeNested<ProcessConfiguration>(nestedObjectDeserializer, parser);

                    if (configResult.IsFailure)
                        return configResult.ConvertFailure<IFreezableProcess>();

                    configuration = configResult.Value;
                }
                else
                {
                    if (dictionary.ContainsKey(keyResult.Value))
                        return new YamlException(markStart, markEnd, $"Duplicate property '{keyResult.Value}'");


                    var memberResult = TryDeserializeNested<ProcessMember>(nestedObjectDeserializer, parser);

                    if (memberResult.IsFailure)
                        return memberResult.ConvertFailure<IFreezableProcess>();

                    dictionary.Add(keyResult.Value, memberResult.Value);
                }
            }


            if (factory == null)
                return new YamlException(markStart, markEnd, $"The '{YamlMethods.TypeString}' property must be set.");

            return new CompoundFreezableProcess(factory, new FreezableProcessData(dictionary), configuration);

        }
    }
}