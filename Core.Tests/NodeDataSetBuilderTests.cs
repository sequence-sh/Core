using System.Text.RegularExpressions;
using Json.More;
using Reductech.Sequence.Core.Entities.Schema;

namespace Reductech.Sequence.Core.Tests;

public class NodeDataSetBuilderTests
{
    [Fact]
    public void TestNumberRestrictions()
    {
        var nr = new NumberRestrictions(1, 2, 3, 4, 5);

        SetAndTest(
            nr,
            @"{""minimum"":1,""maximum"":2,""exclusiveMinimum"":3,""exclusiveMaximum"":4,""multipleOf"":5}"
        );
    }

    [Fact]
    public void TestStringRestrictions()
    {
        var nr = new StringRestrictions(1, 2, new Regex("abc"));

        SetAndTest(
            nr,
            @"{""minLength"":1,""maxLength"":2,""pattern"":""abc""}"
        );
    }

    [Fact]
    public void TestItemsData()
    {
        var itemsData = new ItemsData(
            new List<SchemaNode>() { BooleanNode.Default, NumberNode.Default },
            IntegerNode.Default
        );

        SetAndTest(
            itemsData,
            @"{""items"":{""type"":""integer""},""prefixItems"":[{""type"":""boolean""},{""type"":""number""}]}"
        );
    }

    [Fact]
    public void TestEnumeratedValuesData()
    {
        var evsData = new EnumeratedValuesNodeData(new List<ISCLObject>() { SCLBool.True });

        SetAndTest(
            evsData,
            @"{""const"":true}"
        );
    }

    private void SetAndTest<T>(NodeData<T> nodeData, string expected) where T : NodeData<T>
    {
        var builder = new JsonSchemaBuilder();
        nodeData.SetBuilder(builder);
        var schema = builder.Build();

        var actual = schema.ToJsonDocument().RootElement.ToString();

        actual.Should()
            .Be(expected);
    }
}
