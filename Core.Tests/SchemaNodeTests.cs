using Sequence.Core.Entities.Schema;

namespace Sequence.Core.Tests;

[UseTestOutputHelper]
public partial class SchemaNodeTests
{
    [GenerateTheory("Successes")]
    public IEnumerable<SchemaNodeSuccessCase> RunSuccessCases
    {
        get
        {
            yield return new SchemaNodeSuccessCase(
                BooleanNode.Default,
                SCLBool.False
            );

            yield return new SchemaNodeSuccessCase(
                BooleanNode.Default,
                new StringStream("True")
            );

            yield return new SchemaNodeSuccessCase(
                BooleanNode.Default,
                new StringStream("false")
            );

            yield return new SchemaNodeSuccessCase(
                NullNode.Instance,
                new StringStream("null")
            );
        }
    }

    [GenerateTheory("Failures")]
    public IEnumerable<SchemaNodeFailureCase> RunFailCases
    {
        get
        {
            var empty = EnumeratedValuesNodeData.Empty;

            yield return new(
                new NumberNode(empty, new NumberRestrictions(Min: 6.0)),
                new SCLDouble(5.0)
            );

            yield return new(
                new NumberNode(empty, new NumberRestrictions(Max: 4.0)),
                new SCLDouble(5.0)
            );

            yield return new(
                new NumberNode(empty, new NumberRestrictions(ExclusiveMin: 6.0)),
                new SCLDouble(6.0)
            );

            yield return new(
                new NumberNode(empty, new NumberRestrictions(ExclusiveMax: 6.0)),
                new SCLDouble(6.0)
            );

            yield return new(
                new NumberNode(empty, new NumberRestrictions(MultipleOf: 4.0)),
                new SCLDouble(6.0)
            );

            yield return new(
                new NumberNode(empty, new NumberRestrictions(MultipleOf: 0)),
                new SCLDouble(0)
            );

            yield return new(
                new NumberNode(empty, new NumberRestrictions(Min: 6.0)),
                new SCLInt(5)
            );

            yield return new(
                new NumberNode(empty, new NumberRestrictions(Max: 4.0)),
                new SCLInt(5)
            );

            yield return new(
                new NumberNode(empty, new NumberRestrictions(ExclusiveMin: 6.0)),
                new SCLInt(6)
            );

            yield return new(
                new NumberNode(empty, new NumberRestrictions(ExclusiveMax: 6.0)),
                new SCLInt(6)
            );

            yield return new(
                new NumberNode(empty, new NumberRestrictions(MultipleOf: 4.0)),
                new SCLInt(6)
            );

            yield return new(
                new NumberNode(empty, new NumberRestrictions(MultipleOf: 0)),
                new SCLInt(0)
            );

            yield return new(
                BooleanNode.Default,
                new StringStream("abc")
            );

            yield return new(
                NullNode.Instance,
                new StringStream("something")
            );
        }
    }

    public abstract class SchemaNodeCase : ITestInstance
    {
        public SchemaNodeCase(
            SchemaNode node,
            ISCLObject entityValue,
            TransformSettings? settings = null)
        {
            Node        = node;
            EntityValue = entityValue;

            Settings = settings ?? new TransformSettings(
                Formatter.Empty,
                Formatter.Create(
                    Maybe<OneOf<string, IReadOnlyList<string>, Entity>>.From(
                        new List<string> { "True", "Yes", "1" }
                    )
                ),
                Formatter.Create(
                    Maybe<OneOf<string, IReadOnlyList<string>, Entity>>.From(
                        new List<string> { "False", "No", "0" }
                    )
                ),
                Formatter.Create(
                    Maybe<OneOf<string, IReadOnlyList<string>, Entity>>.From(
                        new List<string> { "Null" }
                    )
                ),
                Formatter.Create(
                    Maybe<OneOf<string, IReadOnlyList<string>, Entity>>.From(
                        new List<string> { "|" }
                    )
                ),
                false,
                0.01,
                true
            );
        }

        public SchemaNode Node { get; }
        public ISCLObject EntityValue { get; }
        public TransformSettings Settings { get; }

        /// <inheritdoc />
        public string Name => $"{Node} {EntityValue}";

        public abstract void Run(ITestOutputHelper testOutputHelper);
    }

    public class SchemaNodeSuccessCase : SchemaNodeCase
    {
        public SchemaNodeSuccessCase(
            SchemaNode node,
            ISCLObject entityValue,
            TransformSettings? settings = null) : base(node, entityValue, settings) { }

        /// <inheritdoc />
        public override void Run(ITestOutputHelper testOutputHelper)
        {
            testOutputHelper.WriteLine(EntityValue.Serialize(SerializeOptions.Name));

            var result = Node.TryTransform(
                "TestProperty",
                EntityValue,
                Settings,
                new TransformRoot(0, Entity.CreatePrimitive(EntityValue))
            );

            result.ShouldBeSuccessful();

            testOutputHelper.WriteLine(result.Value.ToString());
        }
    }

    public class SchemaNodeFailureCase : SchemaNodeCase
    {
        public SchemaNodeFailureCase(
            SchemaNode node,
            ISCLObject entityValue,
            TransformSettings? settings = null) : base(node, entityValue, settings) { }

        /// <inheritdoc />
        public override void Run(ITestOutputHelper testOutputHelper)
        {
            testOutputHelper.WriteLine(EntityValue.Serialize(SerializeOptions.Name));

            var result = Node.TryTransform(
                "TestProperty",
                EntityValue,
                Settings,
                new TransformRoot(0, Entity.CreatePrimitive(EntityValue))
            );

            result.ShouldBeFailure();

            testOutputHelper.WriteLine(result.Error.AsString);
        }
    }
}
