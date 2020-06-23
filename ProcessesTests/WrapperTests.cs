using NUnit.Framework;
using Reductech.EDR.Utilities.Processes.mutable;

namespace Reductech.EDR.Utilities.Processes.Tests
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
