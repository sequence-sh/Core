using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using NUnit.Framework;

namespace ProcessesTests
{
    public static class TestHelpers
    {
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
