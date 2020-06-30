using NUnit.Framework;
using Reductech.EDR.Processes.Mutable;

namespace Reductech.EDR.Processes.Tests
{
    public class WrapperTests
    {
        [Test]
        public void TestObjectCasting()
        {
            var process = new ReturnIntProcess{Value = 5};

            var immutableProcess = process.TryFreeze<object>(EmptySettings.Instance);

            immutableProcess.AssertSuccess();
        }


        [Test]
        public void TestUnitCasting()
        {
            var process = new ReturnIntProcess{Value = 5};

            var immutableProcess = process.TryFreeze<Unit>(EmptySettings.Instance);

            immutableProcess.AssertSuccess();
        }
    }
}
