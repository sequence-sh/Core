using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using NUnit.Framework;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Immutable;
using Reductech.EDR.Processes.Mutable;
using Reductech.EDR.Processes.Mutable.Chain;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Processes.Tests
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
        /// <summary>
        /// Data on this test process
        /// </summary>
        [YamlMember(Alias = "DataTwo")]
        public string? Data { get; set; }

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
        public override Result<IImmutableProcess<TFinal>> TryFreeze<TFinal>(IProcessSettings processSettings)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetRequirements()
        {
            yield break;
        }

        /// <inheritdoc />
        public override Result<ChainLinkBuilder<TInput, TFinal>> TryCreateChainLinkBuilder<TInput, TFinal>()
        {
            throw new NotImplementedException();
        }
    }
}
