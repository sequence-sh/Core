namespace Reductech.EDR.Core.Tests;

[UseTestOutputHelper]
public partial class ISCLObjectTests
{
    private class GetStringData : TheoryData<object, string, Maybe<int>, Maybe<double>, Maybe<bool>,
        Maybe<EncodingEnum>, Maybe<DateTime>, Maybe<Entity>, Maybe<IReadOnlyList<string>>>
    {
        public GetStringData()
        {
            Add(
                null!,
                "null",
                Maybe<int>.None,
                Maybe<double>.None,
                Maybe<bool>.None,
                Maybe<EncodingEnum>.None,
                Maybe<DateTime>.None,
                Maybe<Entity>.None,
                Maybe<IReadOnlyList<string>>.None
            );

            Add(
                "Foo",
                "Foo",
                Maybe<int>.None,
                Maybe<double>.None,
                Maybe<bool>.None,
                Maybe<EncodingEnum>.None,
                Maybe<DateTime>.None,
                Maybe<Entity>.None,
                Maybe<IReadOnlyList<string>>.None
            );

            Add(
                "2",
                "2",
                2,
                2.0,
                Maybe<bool>.None,
                Maybe<EncodingEnum>.None,
                Maybe<DateTime>.None,
                Maybe<Entity>.None,
                Maybe<IReadOnlyList<string>>.None
            );

            Add(
                1,
                "1",
                1,
                1.0,
                Maybe<bool>.None,
                Maybe<EncodingEnum>.None,
                Maybe<DateTime>.None,
                Maybe<Entity>.None,
                Maybe<IReadOnlyList<string>>.None
            );

            Add(
                3.142,
                "3.142",
                Maybe<int>.None,
                3.142,
                Maybe<bool>.None,
                Maybe<EncodingEnum>.None,
                Maybe<DateTime>.None,
                Maybe<Entity>.None,
                Maybe<IReadOnlyList<string>>.None
            );

            Add(
                false,
                "False",
                Maybe<int>.None,
                Maybe<double>.None,
                false,
                Maybe<EncodingEnum>.None,
                Maybe<DateTime>.None,
                Maybe<Entity>.None,
                Maybe<IReadOnlyList<string>>.None
            );

            Add(
                true,
                "True",
                Maybe<int>.None,
                Maybe<double>.None,
                true,
                Maybe<EncodingEnum>.None,
                Maybe<DateTime>.None,
                Maybe<Entity>.None,
                Maybe<IReadOnlyList<string>>.None
            );

            Add(
                EncodingEnum.Unicode,
                "EncodingEnum.Unicode",
                Maybe<int>.None,
                Maybe<double>.None,
                Maybe<bool>.None,
                EncodingEnum.Unicode,
                Maybe<DateTime>.None,
                Maybe<Entity>.None,
                Maybe<IReadOnlyList<string>>.None
            );

            Add(
                new DateTime(1990, 1, 6),
                "1990-01-06T00:00:00.0000000",
                Maybe<int>.None,
                Maybe<double>.None,
                Maybe<bool>.None,
                Maybe<EncodingEnum>.None,
                new DateTime(1990, 1, 6),
                Maybe<Entity>.None,
                Maybe<IReadOnlyList<string>>.None
            );

            Add(
                Entity.Create(("a", 1), ("b", 2)),
                "('a': 1 'b': 2)",
                Maybe<int>.None,
                Maybe<double>.None,
                Maybe<bool>.None,
                Maybe<EncodingEnum>.None,
                Maybe<DateTime>.None,
                Entity.Create(("a", 1), ("b", 2)),
                Maybe<IReadOnlyList<string>>.None
            );

            Add(
                new List<object>() { "a", 1 },
                "[\"a\", 1]",
                Maybe<int>.None,
                Maybe<double>.None,
                Maybe<bool>.None,
                Maybe<EncodingEnum>.None,
                Maybe<DateTime>.None,
                Maybe<Entity>.None,
                new List<string>() { "a", "1" }
            );
        }
    }

    //[Theory]
    //[ClassData(typeof(GetStringData))]
    //public void TestGetExpected(
    //    object o,
    //    string expectedString,
    //    Maybe<int> expectedInt,
    //    Maybe<double> expectedDouble,
    //    Maybe<bool> expectedBool,
    //    Maybe<EncodingEnum> expectedEnumeration,
    //    Maybe<DateTime> expectedDateTime,
    //    Maybe<Entity> expectedEntity,
    //    Maybe<IReadOnlyList<string>> expectedList)
    //{
    //    ISCLObject entityValue = ISCLObject.CreateFromObject(o);

    //    void TestActual<T>(Maybe<T> expected)
    //    {
    //        var actual = entityValue.TryGetValue<T>().ToMaybe();
    //        actual.Should().Be(expected);
    //    }

    //    var testConversion = !(entityValue is IArray or Entity);

    //    void TestConvert<T>(SchemaProperty schemaProperty, Maybe<T> maybe)
    //    {
    //        if (!testConversion)
    //            return;

    //        const string myProp = "MyProp";
    //        var          e      = Entity.Create((myProp, entityValue));

    //        var schema = new Schema()
    //        {
    //            Properties = ImmutableSortedDictionary<string, SchemaProperty>.Empty.Add(
    //                myProp,
    //                schemaProperty
    //            )
    //        };

    //        var convertResult = entityValue.TryConvert(
    //            schema,
    //            myProp,
    //            schemaProperty,
    //            e
    //        );

    //        if (maybe.HasNoValue)
    //            convertResult.ShouldBeFailure();
    //        else
    //        {
    //            convertResult.ShouldBeSuccessful();
    //            var expectedISCLObject = ISCLObject.CreateFromObject(maybe.GetValueOrThrow());

    //            if (convertResult.Value.value.ObjectValue is Enumeration(var type, var value))
    //            {
    //                var real = type + "." + value;
    //                real.Should().Be(expectedISCLObject.ObjectValue!.ToString());
    //            }
    //            else
    //            {
    //                convertResult.Value.value.ObjectValue.Should()
    //                    .Be(expectedISCLObject.ObjectValue);
    //            }
    //        }
    //    }

    //    var actualString = entityValue.TryGetValue<string>().Value;
    //    actualString.Should().Be(expectedString);

    //    TestConvert(
    //        new SchemaProperty() { Type = SCLType.String, Multiplicity = Multiplicity.ExactlyOne },
    //        Maybe<string>.From(expectedString)
    //    );

    //    TestActual(expectedInt);

    //    TestConvert(
    //        new SchemaProperty() { Type = SCLType.Integer, Multiplicity = Multiplicity.ExactlyOne },
    //        expectedInt
    //    );

    //    TestActual(expectedDouble);

    //    TestConvert(
    //        new SchemaProperty() { Type = SCLType.Double, Multiplicity = Multiplicity.ExactlyOne },
    //        expectedDouble
    //    );

    //    TestActual(expectedBool);

    //    TestConvert(
    //        new SchemaProperty() { Type = SCLType.Bool, Multiplicity = Multiplicity.ExactlyOne },
    //        expectedBool
    //    );

    //    var actualEnumeration = entityValue.TryGetValue<EncodingEnum>().ToMaybe();
    //    actualEnumeration.Should().Be(expectedEnumeration);

    //    TestConvert(
    //        new SchemaProperty()
    //        {
    //            Type         = SCLType.Enum,
    //            EnumType     = nameof(EncodingEnum),
    //            Values       = Enum.GetNames<EncodingEnum>(),
    //            Multiplicity = Multiplicity.ExactlyOne
    //        },
    //        expectedEnumeration
    //    );

    //    TestActual(expectedDateTime);

    //    TestConvert(
    //        new SchemaProperty() { Type = SCLType.Date, Multiplicity = Multiplicity.ExactlyOne },
    //        expectedDateTime
    //    );

    //    TestActual(expectedEntity);

    //    TestConvert(
    //        new SchemaProperty() { Type = SCLType.Entity, Multiplicity = Multiplicity.ExactlyOne },
    //        expectedEntity
    //    );

    //    var actualList = entityValue.TryGetValue<Array<StringStream>>().ToMaybe();
    //    actualList.HasValue.Should().Be(expectedList.HasValue);

    //    if (actualList.HasValue)
    //    {
    //        actualList.GetValueOrThrow()
    //            .GetElementsAsync(CancellationToken.None)
    //            .Result
    //            .Value.Select(x => x.GetString())
    //            .Should()
    //            .BeEquivalentTo(expectedList.GetValueOrThrow());
    //    }
    //}

    [Fact]
    public void TestGetDefaultValue()
    {
        ISCLObject.GetDefaultValue<Entity>().Should().Equal(Entity.Empty);
        ISCLObject.GetDefaultValue<StringStream>().Should().Be(StringStream.Empty);
        ISCLObject.GetDefaultValue<SCLInt>().Should().Be(0.ConvertToSCLObject());
        ISCLObject.GetDefaultValue<SCLDouble>().Should().Be(0.0.ConvertToSCLObject());
        ISCLObject.GetDefaultValue<SCLBool>().Should().Be(false.ConvertToSCLObject());
        ISCLObject.GetDefaultValue<Array<SCLInt>>().Should().Be(Array<SCLInt>.Empty);
        ISCLObject.GetDefaultValue<Array<SCLDouble>>().Should().Be(Array<SCLDateTime>.Empty);
    }
}
