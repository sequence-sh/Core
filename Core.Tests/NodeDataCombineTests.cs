using Reductech.Sequence.Core.Entities.Schema;

namespace Reductech.Sequence.Core.Tests;

public partial class NodeDataCombineTests
{
    [GenerateTheory("Combinations")]
    public IEnumerable<ITestInstance> RunCombinationCases
    {
        get
        {
            yield return new NodeDataCombinationCase<NumberRestrictions>(
                new NumberRestrictions(1),
                new NumberRestrictions(2),
                new NumberRestrictions(1)
            );

            yield return new NodeDataCombinationCase<NumberRestrictions>(
                new NumberRestrictions(Max: 1),
                new NumberRestrictions(Max: 2),
                new NumberRestrictions(Max: 2)
            );

            yield return new NodeDataCombinationCase<NumberRestrictions>(
                new NumberRestrictions(ExclusiveMax: 1),
                new NumberRestrictions(ExclusiveMax: 2),
                new NumberRestrictions(ExclusiveMax: 2)
            );

            yield return new NodeDataCombinationCase<NumberRestrictions>(
                new NumberRestrictions(ExclusiveMin: 1),
                new NumberRestrictions(ExclusiveMin: 2),
                new NumberRestrictions(ExclusiveMin: 1)
            );

            yield return new NodeDataCombinationCase<NumberRestrictions>(
                new NumberRestrictions(MultipleOf: 3),
                new NumberRestrictions(MultipleOf: 5),
                new NumberRestrictions(MultipleOf: 15)
            );

            yield return new NodeDataCombinationCase<StringRestrictions>(
                new StringRestrictions(1, null, null),
                new StringRestrictions(2, null, null),
                new StringRestrictions(2, null, null)
            );

            yield return new NodeDataCombinationCase<StringRestrictions>(
                new StringRestrictions(null, 1, null),
                new StringRestrictions(null, 2, null),
                new StringRestrictions(null, 1, null)
            );

            yield return new NodeDataCombinationCase<StringRestrictions>(
                new StringRestrictions(null, 1, null),
                new StringRestrictions(null, 1, null),
                new StringRestrictions(null, 1, null)
            );

            yield return new NodeDataCombinationCase<StringRestrictions>(
                StringRestrictions.NoRestrictions,
                new StringRestrictions(null, 1, null),
                new StringRestrictions(null, 1, null)
            );

            yield return new NodeDataCombinationCase<StringRestrictions>(
                new StringRestrictions(null, 1, null),
                StringRestrictions.NoRestrictions,
                new StringRestrictions(null, 1, null)
            );

            //TODO fix regex combination
            //yield return new NodeDataCombinationCase<StringRestrictions>(
            //    new StringRestrictions(null, null, new Regex("a")),
            //    new StringRestrictions(null, null, new Regex("b")),
            //    new StringRestrictions(null, 1,    new Regex("a|b"))
            //);

            yield return new NodeDataCombinationCase<ItemsData>(
                new ItemsData(new List<SchemaNode>() { BooleanNode.Default, }, NullNode.Instance),
                new ItemsData(
                    new List<SchemaNode>() { BooleanNode.Default, IntegerNode.Default },
                    NullNode.Instance
                ),
                new ItemsData(
                    new List<SchemaNode>()
                    {
                        BooleanNode.Default, TrueNode.Instance
                    }, //TrueNode because we're combining types
                    NullNode.Instance
                )
            ) { NameOverride = "Combine Items Shortest First" };

            yield return new NodeDataCombinationCase<ItemsData>(
                new ItemsData(
                    new List<SchemaNode>() { BooleanNode.Default, IntegerNode.Default },
                    NullNode.Instance
                ),
                new ItemsData(new List<SchemaNode>() { BooleanNode.Default, }, NullNode.Instance),
                new ItemsData(
                    new List<SchemaNode>()
                    {
                        BooleanNode.Default, TrueNode.Instance
                    }, //TrueNode because we're combining types
                    NullNode.Instance
                )
            ) { NameOverride = "Combine Items Longest First" };

            yield return new NodeDataCombinationCase<EnumeratedValuesNodeData>(
                new EnumeratedValuesNodeData(new List<ISCLObject>() { SCLBool.True }),
                new EnumeratedValuesNodeData(null),
                new EnumeratedValuesNodeData(new List<ISCLObject>() { SCLBool.True })
            );

            yield return new NodeDataCombinationCase<EnumeratedValuesNodeData>(
                new EnumeratedValuesNodeData(null),
                new EnumeratedValuesNodeData(new List<ISCLObject>() { SCLBool.True }),
                new EnumeratedValuesNodeData(new List<ISCLObject>() { SCLBool.True })
            );

            yield return new NodeDataCombinationCase<EnumeratedValuesNodeData>(
                new EnumeratedValuesNodeData(new List<ISCLObject>() { SCLBool.True }),
                new EnumeratedValuesNodeData(new List<ISCLObject>() { SCLBool.True }),
                new EnumeratedValuesNodeData(new List<ISCLObject>() { SCLBool.True })
            );

            yield return new NodeDataCombinationCase<EnumeratedValuesNodeData>(
                new EnumeratedValuesNodeData(new List<ISCLObject>() { SCLBool.True }),
                new EnumeratedValuesNodeData(new List<ISCLObject>() { SCLBool.False }),
                new EnumeratedValuesNodeData(new List<ISCLObject>() { SCLBool.True, SCLBool.False })
            );
        }
    }

    public record NodeDataCombinationCase<T>(
        T Nr1,
        T Nr2,
        T Expected) : ITestInstance where T : NodeData<T>
    {
        /// <inheritdoc />
        public string Name => NameOverride ?? $"{Nr1} + {Nr2} = {Expected}";

        public string? NameOverride { get; set; }

        public void Run(ITestOutputHelper testOutputHelper)
        {
            var actual = Nr1.Combine(Nr2);

            actual.Should().Be(Expected);
        }
    }
}
