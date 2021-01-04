using System;
using System.Collections.Generic;
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
        public static IStep<Unit> SetVariable(string name, string value) => new SetVariable<StringStream> { Variable = new VariableName(name), Value = Constant(value) };
        public static IStep<Unit> SetVariable(string name, int value) => new SetVariable<int> { Variable = new VariableName(name), Value = Constant(value) };
        public static IStep<Unit> SetVariable(string name, double value) => new SetVariable<double> { Variable = new VariableName(name), Value = Constant(value) };
        public static IStep<Unit> SetVariable(string name, bool value) => new SetVariable<bool> { Variable = new VariableName(name), Value = Constant(value) };
        public static IStep<Unit> SetVariable(string name, DateTime value) => new SetVariable<DateTime> { Variable = new VariableName(name), Value = Constant(value) };
        public static IStep<Unit> SetVariable(string name, Entity value) => new SetVariable<Entity> { Variable = new VariableName(name), Value = Constant(value) };
        public static IStep<Unit> SetVariable<T>(string name, T value) where T : Enum => new SetVariable<T> { Variable = new VariableName(name), Value = Constant(value) };


        public static StringConstant Constant(string value) => new StringConstant(new StringStream(value));
        public static IntConstant Constant(int value) => new IntConstant(value);
        public static DoubleConstant Constant(double value) => new DoubleConstant(value);
        public static BoolConstant Constant(bool value) => new BoolConstant((value));
        public static DateTimeConstant Constant(DateTime value) => new DateTimeConstant((value));
        public static EntityConstant Constant(Entity value) => new EntityConstant((value));
        public static EnumConstant<T> Constant<T>(T value) where T: Enum => new EnumConstant<T>((value));

        public static IStep<Array<int>> Array(params int[] elements) => new ArrayNew<int> { Elements = elements.Select(Constant).ToList() };
        public static IStep<Array<double>> Array(params double[] elements) => new ArrayNew<double> { Elements = elements.Select(Constant).ToList() };
        public static IStep<Array<bool>> Array(params bool[] elements) => new ArrayNew<bool> { Elements = elements.Select(Constant).ToList() };
        public static IStep<Array<DateTime>> Array(params DateTime[] elements) => new ArrayNew<DateTime> { Elements = elements.Select(Constant).ToList() };
        public static IStep<Array<Entity>> Array(params Entity[] elements) => new ArrayNew<Entity> { Elements = elements.Select(Constant).ToList() };
        //public static IStep<AsyncList<EntityStream>> Array(params EntityStream[] elements) => new Array<EntityStream> { Elements = elements.Select(Constant).ToList() };
        public static IStep<Array<StringStream>> Array(params string[] elements) => new ArrayNew<StringStream> { Elements = elements.Select(Constant).ToList() };
        public static IStep<Array<T>> Array<T>(params T[] elements) where T : Enum => new ArrayNew<T> { Elements = elements.Select(Constant).ToList() };

        public static IStep<TNew> GetVariable<TNew>(string variableName) => new GetVariable<TNew> { Variable = new VariableName(variableName) };
        public static IStep<TNew> GetVariable<TNew>(VariableName variableName) => new GetVariable<TNew> { Variable = variableName };

        public static IStep<Entity> GetEntityVariable => GetVariable<Entity>(VariableName.Entity);

        public static Entity CreateEntity(params (string key, object? value)[] pairs) => Entity.Create(pairs);

        public static Schema CreateSchema(string name, bool allowExtraProperties, params (string propertyName, SchemaPropertyType type, Multiplicity multiplicity)[] properties)
        {
            return CreateSchema(name, allowExtraProperties,
                properties.Select(p => (p.propertyName, p.type, null as string, p.multiplicity, null as string, null as List<string>))
                    .ToArray()
            );
        }

        public static Schema CreateSchema(string name, bool allowExtraProperties, params (string propertyName, SchemaPropertyType type, string? enumType, Multiplicity multiplicity, string? regex, List<string>? format)[] properties)
        {
            return new Schema
            {
                Name = name,
                AllowExtraProperties = allowExtraProperties,
                Properties = properties.ToDictionary(x => x.propertyName, x => new SchemaProperty
                {
                    EnumType = x.enumType,
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
