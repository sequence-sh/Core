using System.Collections.Generic;
using System.Linq;
using AutoTheory;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.TestHarness;
using Xunit;

namespace Reductech.EDR.Core.Tests
{

[UseTestOutputHelper]
public partial class SCLSettingsTests
{
    [Fact]
    public void TestCreateFromIConfiguration()
    {
        var myConfiguration = new Dictionary<string, string>
        {
            { "Key1", "Value1" },
            { "Nested:Key1", "NestedValue1" },
            { "Nested:Key2:0", "alpha" },
            { "Nested:Key2:1", "beta" },
            { "Nested:Key2:2", "gamma" },
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .Build();

        var settings = SCLSettings.CreateFromIConfiguration(configuration);

        var key1 = settings.Entity.TryGetValue("Key1");
        key1.ShouldHaveValue();

        key1.Value.GetPrimitiveString().Should().Be("Value1");

        var nestedKey1 = settings.Entity.TryGetValue(new EntityPropertyKey("Nested.Key1"));

        nestedKey1.ShouldHaveValue();

        nestedKey1.Value.GetPrimitiveString().Should().Be("NestedValue1");

        var nestedKey2 = settings.Entity.TryGetValue(new EntityPropertyKey("Nested.Key2"));

        nestedKey2.ShouldHaveValue();

        nestedKey2.Value.Should().BeOfType<EntityValue.NestedList>("Value should be a list");

        (nestedKey2.Value as EntityValue.NestedList)!.Value.Select(x => x.GetPrimitiveString())
            .Should()
            .BeEquivalentTo(new object[] { "alpha", "beta", "gamma" });
    }
}

}
