using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes.mutable;
using Reductech.EDR.Utilities.Processes.mutable.enumerations;
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
        private static IReadOnlyDictionary<string, Type> GetSpecialTypes(bool includeAliases)
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

                .SelectMany(t=> GetAllNames(t).Select(n=>(t,n)))
                .ToDictionary(x=>x.n, x=>x.t);

            return types;

            IEnumerable<string> GetAllNames(Type t)
            {
                yield return t.Name;

                if(includeAliases)
                    foreach (var name in t.GetCustomAttributes(typeof(YamlProcessAttribute), true)
                    .OfType<YamlProcessAttribute>()
                    .Select(x=>x.Alias)
                    .Where(x=> !string.IsNullOrWhiteSpace(x))
                    .Distinct())
                    yield return name??"";
            }
        }

        internal static readonly Lazy<ISet<Type>> SpecialTypesSet = new Lazy<ISet<Type>>(()=> new HashSet<Type>(GetSpecialTypes(false).Values));

        /// <summary>
        /// Makes a new deserializer.
        /// ONLY USE THIS ONCE
        /// We have to make a new one each time to keep track of defaults properly.
        /// </summary>
        private static IDeserializer Deserializer
        {
            get
            {
                var deSerializerBuilder = new DeserializerBuilder();
                deSerializerBuilder =
                    GetSpecialTypes(true)
                        .Aggregate(deSerializerBuilder, 
                        (current, specialType) => current.WithTagMapping("!" + specialType.Key, specialType.Value));

                var deserializer = 


                new EdrNodeDeserializer(new CachedTypeInspector(
                    new CompositeTypeInspector(
                        new YamlAttributesTypeInspector(new ReadablePropertiesTypeInspector(new DynamicTypeResolver()))
                        ))
                );
                deSerializerBuilder.WithNodeDeserializer(deserializer);

                return  deSerializerBuilder.Build();
            }
        }

        private static readonly Lazy<ISerializer> Serializer = new Lazy<ISerializer>(() =>
        {
            var serializerBuilder = new SerializerBuilder();
            serializerBuilder =
                GetSpecialTypes(false).Aggregate(serializerBuilder, 
                    (current, specialType) => current.WithTagMapping("!" + specialType.Key, specialType.Value));
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