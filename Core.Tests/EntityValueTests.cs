using System;
using System.Collections.Generic;
using System.Linq;
using AutoTheory;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal;
using Xunit;

namespace Reductech.EDR.Core.Tests
{

[UseTestOutputHelper]
public partial class EntityValueTests
{
    private class GetStringData : TheoryData<object, string, Maybe<int>, Maybe<double>, Maybe<bool>,
        Maybe<Enumeration>, Maybe<DateTime>, Maybe<Entity>, Maybe<IReadOnlyList<string>>>
    {
        public GetStringData()
        {
            Add(
                null!,
                "",
                Maybe<int>.None,
                Maybe<double>.None,
                Maybe<bool>.None,
                Maybe<Enumeration>.None,
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
                Maybe<Enumeration>.None,
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
                Maybe<Enumeration>.None,
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
                Maybe<Enumeration>.None,
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
                Maybe<Enumeration>.None,
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
                Maybe<Enumeration>.None,
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
                Maybe<Enumeration>.None,
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
                new Enumeration("EncodingEnum", "Unicode"),
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
                Maybe<Enumeration>.None,
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
                Maybe<Enumeration>.None,
                Maybe<DateTime>.None,
                Entity.Create(("a", 1), ("b", 2)),
                Maybe<IReadOnlyList<string>>.None
            );

            Add(
                new List<object>() { "a", 1 },
                "a,1",
                Maybe<int>.None,
                Maybe<double>.None,
                Maybe<bool>.None,
                Maybe<Enumeration>.None,
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
        Maybe<Enumeration> expectedEnumeration,
        Maybe<DateTime> expectedDateTime,
        Maybe<Entity> expectedEntity,
        Maybe<IReadOnlyList<string>> expectedList)
    {
        var entityValue = EntityValue.CreateFromObject(o);

        var actualString = entityValue.GetString();
        actualString.Should().Be(expectedString);

        var actualInt = entityValue.TryGetInt();
        actualInt.Should().Be(expectedInt);

        var actualDouble = entityValue.TryGetDouble();
        actualDouble.Should().Be(expectedDouble);

        var actualBool = entityValue.TryGetBool();
        actualBool.Should().Be(expectedBool);

        var actualEnumeration = entityValue.TryGetEnumeration(
            "EncodingEnum",
            new List<string>() { "Unicode" }
        );

        actualEnumeration.Should().Be(expectedEnumeration);

        var actualDateTime = entityValue.TryGetDateTime();
        actualDateTime.Should().Be(expectedDateTime);

        var actualEntity = entityValue.TryGetEntity();
        actualEntity.Should().Be(expectedEntity);

        var actualList = entityValue.TryGetEntityValueList();
        actualList.HasValue.Should().Be(expectedList.HasValue);

        if (actualList.HasValue)
        {
            actualList.Value.Select(x => x.GetString())
                .ToList()
                .Should()
                .BeEquivalentTo(expectedList.Value);
        }
    }
}

}
