using System.Text.Json;

namespace Sequence.Core.Tests;

public class JsonConvertersTests
{
    public JsonConvertersTests(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }

    public ITestOutputHelper TestOutputHelper { get; }

    [Fact]
    public void TestVersionWrite()
    {
        var version = new Version(5, 1);

        var result =
            JsonSerializer.Serialize(
                version,
                new JsonSerializerOptions() { Converters = { VersionJsonConverter.Instance } }
            );

        result.Should().Be("\"5.1\"");
    }

    [Theory]
    [InlineData("\"5.1\"",  "5.1")]
    [InlineData("\"blah\"", null)]
    public void TestVersionRead(string input, string? expected)
    {
        var result =
            JsonSerializer.Deserialize<Version>(
                input,
                new JsonSerializerOptions() { Converters = { VersionJsonConverter.Instance } }
            );

        if (expected is null)
            result.Should().BeNull();
        else
        {
            result.Should().NotBeNull();
            result.ToString().Should().Be(expected);
        }
    }

    [Theory]
    [InlineData("{\"foo\": \"abc\"}", "('foo': abc)")]
    public void TestEntityRead(string input, string? expected)
    {
        var result =
            JsonSerializer.Deserialize<Entity>(
                input,
                new JsonSerializerOptions() { Converters = { EntityJsonConverter.Instance } }
            );

        if (expected is null)
            result.Should().BeNull();
        else
        {
            result.Should().NotBeNull();
            result.ToString().Should().Be(expected);
        }
    }
}
