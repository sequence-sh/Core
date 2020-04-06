using System;
using System.Collections.Generic;
using System.Text;
using CSharpFunctionalExtensions;
using NUnit.Framework;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.mutable;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Utilities.Processes.Tests
{
    public class AliasDeserializationTest
    {
        public static readonly IReadOnlyCollection<string> TestYamls = new List<string>()
        {

            @"!TestProcess
                DataTwo: Hello World",

            @"!TestProcessTwo
                DataTwo: Hello World",
        };



        [Test]
        [TestCaseSource(nameof(TestYamls))]
        public void TestDeserialize(string yamlString)
        {
            var result = YamlHelper.TryMakeFromYaml(yamlString);

            var process = result.AssertSuccess();

            Assert.IsInstanceOf(typeof(TestProcess), process);

            Assert.AreEqual("Hello World", ((TestProcess) process).Data);
        }

    }

    [YamlProcess(Alias = "TestProcessTwo")]
    public class TestProcess : Process
    {
        
        [YamlMember(Alias = "DataTwo")]
        public string Data { get; set; }

        /// <inheritdoc />
        public override string GetReturnTypeInfo()
        {
            return typeof(Unit).ToString();
        }

        /// <inheritdoc />
        public override string GetName()
        {
            return nameof(TestProcess);
        }

        /// <inheritdoc />
        public override Result<ImmutableProcess, ErrorList> TryFreeze(IProcessSettings processSettings)
        {
            throw new NotImplementedException();
        }
    }
}
