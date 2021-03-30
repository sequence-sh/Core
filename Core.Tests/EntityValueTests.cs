using System;
using System.Collections.Generic;
using System.Linq;
using AutoTheory;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Enums;
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
                "(a: 1 b: 2)",
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
        var entityValue = EntityValue.CreateFromObject(o);

        var actualString = entityValue.TryGetValue<string>().Value;
        actualString.Should().Be(expectedString);

        var actualInt = entityValue.TryGetValue<int>().ToMaybe();
        actualInt.Should().Be(expectedInt);

        var actualDouble = entityValue.TryGetValue<double>().ToMaybe();
        actualDouble.Should().Be(expectedDouble);

        var actualBool = entityValue.TryGetValue<bool>().ToMaybe();
        actualBool.Should().Be(expectedBool);

        var actualEnumeration = entityValue.TryGetValue<EncodingEnum>().ToMaybe();

        actualEnumeration.Should().Be(expectedEnumeration);

        var actualDateTime = entityValue.TryGetValue<DateTime>().ToMaybe();
        actualDateTime.Should().Be(expectedDateTime);

        var actualEntity = entityValue.TryGetValue<Entity>().ToMaybe();
        actualEntity.Should().Be(expectedEntity);

        var actualList = entityValue.TryGetValue<Array<StringStream>>().ToMaybe();
        actualList.HasValue.Should().Be(expectedList.HasValue);

        if (actualList.HasValue)
        {
            actualList.Value.GetElements()
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
        EntityValue.GetDefaultValue<Array<int>>().Should().Be(new Array<int>());
        EntityValue.GetDefaultValue<Array<double>>().Should().Be(new Array<double>());
    }
}

}
