//using System;
//using System.Collections.Generic;
//using AutoTheory;
//using FluentAssertions;
//using Reductech.EDR.Core.Entities;
//using Reductech.EDR.Core.Internal;
//using Xunit;

//namespace Reductech.EDR.Core.Tests
//{

//[UseTestOutputHelper]
//public partial class EntityTests
//{
//    [Theory]
//    [InlineData("Hello", "Hello")]
//    [InlineData(1,       "1")]
//    [InlineData(1.0,     "1")]
//    [InlineData(true,    "True")]
//    public void TestGetString(object value, string expectedString)
//    {
//        var ev = EntityValue.CreateFromObject(value);

//        var s = ev.GetPrimitiveString();

//        s.Should().Be(expectedString);
//    }

//    [Fact]
//    public void TestGetInt()
//    {
//        var ev = EntityValue.CreateFromObject("1");

//        var maybe = ev.TryGetInt();

//        maybe.HasValue.Should().BeTrue();

//        maybe.Value.Should().Be(1);
//    }

//    [Fact]
//    public void TestGetDouble()
//    {
//        var ev = EntityValue.CreateFromObject("1.0");

//        var maybe = ev.TryGetDouble();

//        maybe.HasValue.Should().BeTrue();

//        maybe.Value.Should().Be(1);
//    }

//    [Fact]
//    public void TestGetBool()
//    {
//        var ev = EntityValue.CreateFromObject(true.ToString());

//        var maybe = ev.TryGetBool();

//        maybe.HasValue.Should().BeTrue();

//        maybe.Value.Should().Be(true);
//    }

//    [Fact]
//    public void TestGetEnumeration()
//    {
//        var ev = EntityValue.CreateFromObject("haddock");

//        var maybe = ev.TryGetEnumeration("Fish", new List<string>() { "Salmon", "Haddock" });

//        maybe.HasValue.Should().BeTrue();

//        maybe.Value.Should().Be(new Enumeration("Fish", "haddock"));
//    }

//    [Fact]
//    public void TestGetDateTime()
//    {
//        var dt = new DateTime(1990, 1, 6);

//        var ev = EntityValue.CreateFromObject(dt.ToString("O"));

//        var maybe = ev.TryGetDateTime();

//        maybe.HasValue.Should().BeTrue();

//        maybe.Value.Should().Be(dt);
//    }
//}

//}


