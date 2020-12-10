using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.TestHarness
{
    public static class StaticHelpers
    {
        public static IStep<Unit> SetVariable<TNew>(string name, TNew value) => new SetVariable<TNew> { Variable = new VariableName(name), Value = Constant(value) };
        public static Constant<StringStream> Constant(string value) => new Constant<StringStream>(new StringStream(value));
        public static Constant<TNew> Constant<TNew>(TNew value) => new Constant<TNew>(value);

        public static IStep<List<TNew>> Array<TNew>(params TNew[] elements) => new Array<TNew> { Elements = elements.Select(Constant).ToList() };

        public static IStep<TNew> GetVariable<TNew>(string variableName) => new GetVariable<TNew> { Variable = new VariableName(variableName) };
        public static IStep<TNew> GetVariable<TNew>(VariableName variableName) => new GetVariable<TNew> { Variable = variableName };

        public static IStep<Entity> GetEntityVariable => GetVariable<Entity>(VariableName.Entity);

        public static Entity CreateEntity(params (string key, string value)[] pairs)
        {
            var evs = pairs
                .GroupBy(x => x.key, x => x.value)
                .Select(x => new KeyValuePair<string, EntityValue>(x.Key, EntityValue.Create(x)));

            return new Entity(evs.ToImmutableList());
        }

        public static Schema CreateSchema(string name, bool allowExtraProperties, params (string propertyName, SchemaPropertyType type, Multiplicity multiplicity)[] properties)
        {
            return new Schema
            {
                Name = name,
                AllowExtraProperties = allowExtraProperties,
                Properties = properties.ToDictionary(x => x.propertyName, x => new SchemaProperty
                {
                    Multiplicity = x.multiplicity,
                    Type = x.type
                })
            };
        }

        public static Schema CreateSchema(string name, bool allowExtraProperties, params (string propertyName, SchemaPropertyType type, Multiplicity multiplicity, string? regex, List<string>? format)[] properties)
        {
            return new Schema
            {
                Name = name,
                AllowExtraProperties = allowExtraProperties,
                Properties = properties.ToDictionary(x => x.propertyName, x => new SchemaProperty
                {
                    Multiplicity = x.multiplicity,
                    Type = x.type,
                    Regex = x.regex,
                    Format = x.format
                })
            };
        }

        public static string CompressNewlines(string s) => s.Replace("\r\n", "\n");
    }
}
