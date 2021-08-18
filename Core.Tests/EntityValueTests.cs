using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AutoTheory;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit;

namespace Reductech.EDR.Core.Tests
{

[UseTestOutputHelper]
public partial class EntityValueTests
{
    private class GetStringData : TheoryData<object, string, Maybe<int>, Maybe<double>, Maybe<bool>,
        Maybe<EncodingEnum>, Maybe<DateTime>, Maybe<Entity>, Maybe<IReadOnlyList<string>>>
    {
        public GetStringData()
        {
            Add(
                null!,
                "\"\"",
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

    [Theory]
    [ClassData(typeof(GetStringData))]
    public void TestGetExpected(
        object o,
        string expectedString,
        Maybe<int> expectedInt,
        Maybe<double> expectedDouble,
        Maybe<bool> expectedBool,
        Maybe<EncodingEnum> expectedEnumeration,
        Maybe<DateTime> expectedDateTime,
        Maybe<Entity> expectedEntity,
        Maybe<IReadOnlyList<string>> expectedList)
    {
        EntityValue entityValue = EntityValue.CreateFromObject(o);

        void TestActual<T>(Maybe<T> expected)
        {
            var actual = entityValue.TryGetValue<T>().ToMaybe();
            actual.Should().Be(expected);
        }

        var testConversion = !(entityValue is EntityValue.NestedList or EntityValue.NestedEntity);

        void TestConvert<T>(SchemaProperty schemaProperty, Maybe<T> maybe)
        {
            if (!testConversion)
                return;

            const string myProp = "MyProp";
            var          e      = Entity.Create((myProp, entityValue));

            var schema = new Schema()
            {
                Properties = new Dictionary<string, SchemaProperty>()
                {
                    { myProp, schemaProperty }
                }
            };

            var convertResult = entityValue.TryConvert(
                schema,
                myProp,
                schemaProperty,
                e
            );

            if (maybe.HasNoValue)
                convertResult.ShouldBeFailure();
            else
            {
                convertResult.ShouldBeSuccessful();
                var expectedEntityValue = EntityValue.CreateFromObject(maybe.Value);

                if (convertResult.Value.value.ObjectValue is Enumeration enumeration)
                {
                    var real = enumeration.Type + "." + enumeration.Value;
                    real.Should().Be(expectedEntityValue.ObjectValue!.ToString());
                }
                else
                {
                    convertResult.Value.value.ObjectValue.Should()
                        .Be(expectedEntityValue.ObjectValue);
                }
            }
        }

        var actualString = entityValue.TryGetValue<string>().Value;
        actualString.Should().Be(expectedString);

        TestConvert(
            new SchemaProperty() { Type = SCLType.String, Multiplicity = Multiplicity.ExactlyOne },
            Maybe<string>.From(expectedString)
        );

        TestActual(expectedInt);

        TestConvert(
            new SchemaProperty() { Type = SCLType.Integer, Multiplicity = Multiplicity.ExactlyOne },
            expectedInt
        );

        TestActual(expectedDouble);

        TestConvert(
            new SchemaProperty() { Type = SCLType.Double, Multiplicity = Multiplicity.ExactlyOne },
            expectedDouble
        );

        TestActual(expectedBool);

        TestConvert(
            new SchemaProperty() { Type = SCLType.Bool, Multiplicity = Multiplicity.ExactlyOne },
            expectedBool
        );

        var actualEnumeration = entityValue.TryGetValue<EncodingEnum>().ToMaybe();
        actualEnumeration.Should().Be(expectedEnumeration);

        TestConvert(
            new SchemaProperty()
            {
                Type         = SCLType.Enum,
                EnumType     = nameof(EncodingEnum),
                Values       = Enum.GetNames<EncodingEnum>(),
                Multiplicity = Multiplicity.ExactlyOne
            },
            expectedEnumeration
        );

        TestActual(expectedDateTime);

        TestConvert(
            new SchemaProperty() { Type = SCLType.Date, Multiplicity = Multiplicity.ExactlyOne },
            expectedDateTime
        );

        TestActual(expectedEntity);

        TestConvert(
            new SchemaProperty() { Type = SCLType.Entity, Multiplicity = Multiplicity.ExactlyOne },
            expectedEntity
        );

        var actualList = entityValue.TryGetValue<Array<StringStream>>().ToMaybe();
        actualList.HasValue.Should().Be(expectedList.HasValue);

        if (actualList.HasValue)
        {
            actualList.Value.GetElementsAsync(CancellationToken.None)
                .Result
                .Value.Select(x => x.GetString())
                .Should()
                .BeEquivalentTo(expectedList.Value);
        }
    }

    [Fact]
    public void TestGetDefaultValue()
    {
        EntityValue.GetDefaultValue<Entity>().Should().Equal(Entity.Empty);
        EntityValue.GetDefaultValue<StringStream>().Should().Be(StringStream.Empty);
        EntityValue.GetDefaultValue<string>().Should().Be(string.Empty);
        EntityValue.GetDefaultValue<int>().Should().Be(0);
        EntityValue.GetDefaultValue<double>().Should().Be(0.0);
        EntityValue.GetDefaultValue<bool>().Should().Be(false);
        EntityValue.GetDefaultValue<Array<int>>().Should().Be(Array<int>.Empty);
        EntityValue.GetDefaultValue<Array<double>>().Should().Be(Array<double>.Empty);
    }
}

}
