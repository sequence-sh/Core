using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.enumerations;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeInspectors;
using YamlDotNet.Serialization.TypeResolvers;

namespace Reductech.EDR.Utilities.Processes
{
    /// <summary>
    /// Contains methods for serializing and deserializing yaml
    /// </summary>
    public static class YamlHelper
    {
        private static IEnumerable<Type> SpecialTypes
        {
            get
            {
                var assemblies = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Where(x => x.CustomAttributes.Any(y => y.AttributeType == typeof(ProcessModuleAttribute)))
                    .ToList();

                var types = assemblies
                    .SelectMany(s => s.GetTypes())
                    .Where(type =>  
                        typeof(Process).IsAssignableFrom(type)
                        || typeof(Enumeration).IsAssignableFrom(type))
                    .Where(x=>!x.IsAbstract && ! x.IsInterface)
                    .ToList();

                return types;
            }
        }

        internal static readonly Lazy<ISet<Type>> SpecialTypesSet = new Lazy<ISet<Type>>(()=> new HashSet<Type>(SpecialTypes));

        /// <summary>
        /// Makes a new deserializer
        /// </summary>
        private static IDeserializer Deserializer
        {
            get
            {
                var deSerializerBuilder = new DeserializerBuilder();
                deSerializerBuilder =
                    SpecialTypes.Aggregate(deSerializerBuilder, 
                        (current, specialType) => current.WithTagMapping("!" + specialType.Name, specialType));

                var deserializer = new EdrNodeDeserializer(new CachedTypeInspector(new ReadablePropertiesTypeInspector(new DynamicTypeResolver())));
                deSerializerBuilder.WithNodeDeserializer(deserializer);

                return  deSerializerBuilder.Build();
            }
        }

        private static readonly Lazy<ISerializer> Serializer = new Lazy<ISerializer>(() =>
        {
            var serializerBuilder = new SerializerBuilder();
            serializerBuilder =
                SpecialTypes.Aggregate(serializerBuilder, 
                    (current, specialType) => current.WithTagMapping("!" + specialType.Name, specialType));
            serializerBuilder.ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull);
            return serializerBuilder.Build();
        });

        /// <summary>
        /// Creates a Yaml string representing a process.
        /// </summary>
        public static string ConvertToYaml(Process process)
        {
            var r = Serializer.Value.Serialize(process);
            return r;
        }

        /// <summary>
        /// Tries to create a process from a Yaml string.
        /// </summary>
        public static Result<Process> TryMakeFromYaml(string yaml)
        {
            return  Result.Try(() => Deserializer.Deserialize<Process>(yaml), GetInnermostExceptionMessage);
        }

        private static string GetInnermostExceptionMessage(Exception e)
        {
            while (true)
            {
                if (e.InnerException == null) return e.Message;
                e = e.InnerException;
            }
        }
    }
}