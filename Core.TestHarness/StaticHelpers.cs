namespace Reductech.Sequence.Core.TestHarness;

public static class StaticHelpers
{
    public static IStep<Unit> SetVariable(string name, string value) =>
        new SetVariable<StringStream>
        {
            Variable = new VariableName(name), Value = Constant(value)
        };

    public static IStep<Unit> SetVariable(string name, int value) =>
        new SetVariable<SCLInt> { Variable = new VariableName(name), Value = Constant(value) };

    public static IStep<Unit> SetVariable(string name, double value) =>
        new SetVariable<SCLDouble> { Variable = new VariableName(name), Value = Constant(value) };

    public static IStep<Unit> SetVariable(string name, bool value) =>
        new SetVariable<SCLBool> { Variable = new VariableName(name), Value = Constant(value) };

    public static IStep<Unit> SetVariable(string name, DateTime value) =>
        new SetVariable<SCLDateTime> { Variable = new VariableName(name), Value = Constant(value) };

    public static IStep<Unit> SetVariable(string name, Entity value) =>
        new SetVariable<Entity> { Variable = new VariableName(name), Value = Constant(value) };

    public static IStep<Unit> SetVariable<T>(string name, T value) where T : struct, Enum =>
        new SetVariable<SCLEnum<T>> { Variable = new VariableName(name), Value = Constant(value) };

    public static SCLConstant<StringStream> Constant(string value) => new(value);
    public static SCLConstant<SCLInt> Constant(int value) => new(new SCLInt(value));
    public static SCLConstant<SCLDouble> Constant(double value) => new(new SCLDouble(value));
    public static SCLConstant<SCLBool> Constant(bool value) => new(SCLBool.Create(value));
    public static SCLConstant<SCLDateTime> Constant(DateTime value) => new(new SCLDateTime(value));
    public static SCLConstant<Entity> Constant(Entity value) => new((value));

    public static SCLConstant<SCLEnum<T>> Constant<T>(T value) where T : struct, Enum =>
        new(new SCLEnum<T>(value));

    public static IStep<Array<SCLInt>> Array(params int[] elements) =>
        new ArrayNew<SCLInt> { Elements = elements.Select(Constant).ToList() };

    public static IStep<Array<SCLDouble>> Array(params double[] elements) =>
        new ArrayNew<SCLDouble> { Elements = elements.Select(Constant).ToList() };

    public static IStep<Array<SCLBool>> Array(params bool[] elements) =>
        new ArrayNew<SCLBool> { Elements = elements.Select(Constant).ToList() };

    public static IStep<Array<SCLDateTime>> Array(params DateTime[] elements) =>
        new ArrayNew<SCLDateTime> { Elements = elements.Select(Constant).ToList() };

    public static IStep<Array<Entity>> Array(params Entity[] elements) =>
        new ArrayNew<Entity> { Elements = elements.Select(Constant).ToList() };

    public static IStep<Array<StringStream>> Array(params string[] elements) =>
        new ArrayNew<StringStream> { Elements = elements.Select(Constant).ToList() };

    public static IStep<Array<SCLEnum<T>>> Array<T>(params T[] elements) where T : struct, Enum =>
        new ArrayNew<SCLEnum<T>> { Elements = elements.Select(Constant).ToList() };

    public static IStep<TNew> GetVariable<TNew>(string variableName) where TNew : ISCLObject =>
        new GetVariable<TNew> { Variable = new VariableName(variableName) };

    public static IStep<TNew> GetVariable<TNew>(VariableName variableName)
        where TNew : ISCLObject => new GetVariable<TNew> { Variable = variableName };

    public static IStep<Entity> GetEntityVariable =>
        GetVariable<Entity>(VariableName.Item); //TODO rename
}
