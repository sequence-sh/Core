﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using CSharpFunctionalExtensions;
using NUnit.Framework;
using Processes.process;
using YamlDotNet.Serialization;

namespace ProcessesTests
{
    public class ConditionalTest
    {

        [Test]
        public void TestThenBranch()
        {
            var process = new Conditional
            {
                If = new Assertion
                {
                    Success = true
                } ,
                Then = new EmitProcess
                {
                    Term = "Yes"
                },
                Else = new EmitProcess
                {
                    Term = "No"
                }
            };

            var results = process.Execute(new EmptySettings());

            var resultList = TestHelpers.AssertNoErrors(results).Result;

            CollectionAssert.Contains(resultList, "Yes");
        }

        [Test]
        public void TestElseBranch()
        {
            var process = new Conditional
            {
                If = new Assertion
                {
                    Success = false
                } ,
                Then = new EmitProcess
                {
                    Term = "Yes"
                },
                Else = new EmitProcess
                {
                    Term = "No"
                }
            };

            var results = process.Execute(new EmptySettings());

            var resultList = TestHelpers.AssertNoErrors(results).Result;

            CollectionAssert.Contains(resultList, "No");
        }


    }


    public class Assertion : Process
    {
        public override IEnumerable<string> GetArgumentErrors()
        {
            yield break;
        }

        public override IEnumerable<string> GetSettingsErrors(IProcessSettings processSettings)
        {
            yield break;
        }

        public override string GetName() => Success.ToString();

#pragma warning disable 1998
        public override async IAsyncEnumerable<Result<string>> Execute(IProcessSettings processSettings)
#pragma warning restore 1998
        {
            if (Success)
                yield return Result.Success("Succeeded");
            else yield return Result.Failure<string>("Failed");
        }

        [DataMember]
        [YamlMember]
        [Required]
        public bool Success { get; set; }
    }
}
