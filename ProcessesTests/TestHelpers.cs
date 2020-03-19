using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using NUnit.Framework;

namespace Reductech.EDR.Utilities.Processes.Tests
{
    public static class TestHelpers
    {

        public static T AssertSuccess<T>(this Result<T> r)
        {
            if (r.IsSuccess)
                return r.Value;
            Assert.Fail(r.Error);

            throw new Exception();
        }

        public static async Task<IReadOnlyCollection<string>> AssertNoErrors(IAsyncEnumerable<Result<string>> lines)
        {
            var errors = new List<string>();
            var results = new List<string>();

            await foreach (var (_, isFailure, data, error) in lines)
            {
                if (isFailure)
                    errors.Add(error);
                else
                {
                    results.Add(data);
                }
            }
            
            CollectionAssert.IsEmpty(errors);

            return results;
        }
    }
}
