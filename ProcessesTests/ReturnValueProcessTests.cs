using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Reductech.EDR.Processes.Mutable;
using Reductech.EDR.Processes.Output;

namespace Reductech.EDR.Processes.Tests
{
    public class ReturnValueProcessTests
    {

        [Test]
        [TestCase(1)]
        [TestCase("Hello")]
        public void TestReturnValue(object v)
        {
            var rv = new ReturnValue
            {
                Value = v,
                Type = v.GetType()
            };

            var freezeResult = rv.TryFreeze<object>(EmptySettings.Instance);

            freezeResult.IsSuccess.Should().BeTrue("Freezing should succeed");

            var r = freezeResult.Value.Execute();

            var output = r.ToListAsync();

            output.IsCompletedSuccessfully.Should().BeTrue("Should iterate successfully");

            output.Result.Count.Should().Be(1, "only one result should be returned");

            output.Result.Single().OutputType.Should().Be(OutputType.Success);
            output.Result.Single().Value.Should().Be(v, "Should return the given value");
        }

    }
}