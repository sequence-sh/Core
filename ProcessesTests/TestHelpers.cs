using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using NUnit.Framework;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Utilities.Processes.Tests
{
    public static class TestHelpers
    {

        public static T AssertSuccess<T>(this Result<T> r)
        {
            var (isSuccess, _, value, error) = r;
            if (isSuccess)
                return value;
            Assert.Fail(error);

            throw new Exception();
        }

        public static T AssertSuccess<T, TE>(this Result<T, TE> r)
        {
            var (isSuccess, _, value, error) = r;
            Assert.IsTrue(isSuccess, error?.ToString());

            return value;
        }

        public static async Task<IReadOnlyCollection<string>> AssertNoErrors(IAsyncEnumerable<IProcessOutput> output)
        {
            var errors = new List<string>();
            var results = new List<string>();

            await foreach (var o in output)
            {
                if (o.OutputType == OutputType.Error)
                    errors.Add(o.Text);
                else
                {
                    results.Add(o.Text);
                }
            }
            
            CollectionAssert.IsEmpty(errors);

            return results;
        }
    }
}
