using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoTheory;
using FluentAssertions;
using Reductech.EDR.Core.Internal.Errors;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests
{

/// <summary>
/// Tests equals and get hash code for various objects
/// </summary>
[AutoTheory.UseTestOutputHelper]
public partial class EqualityTests
{
    [AutoTheory.GenerateTheory("Arrays")]
    public IEnumerable<EqualityTestInstance<Array<int>>> ArrayTestInstances
    {
        get
        {
            //Array
            static Array<int> ErrorArray(string error)
            {
                async IAsyncEnumerable<int> GetStuff()
                {
                    await Task.CompletedTask;
                    yield return 1;

                    throw new ErrorException(
                        new SingleError(
                            ErrorLocation.EmptyLocation,
                            ErrorCode.Test.ToErrorBuilder(error)
                        )
                    );
                }

                var ea = new LazyArray<int>(GetStuff());

                return ea;
            }

            yield return new EqualityTestInstance<Array<int>>(
                Array<int>.Empty,
                Array<int>.Empty,
                true
            );

            yield return new EqualityTestInstance<Array<int>>(
                Array<int>.Empty,
                null,
                false
            );

            yield return new EqualityTestInstance<Array<int>>(
                new EagerArray<int>(new[] { 1, 2, 3 }),
                new EagerArray<int>(new[] { 1, 2, 3 }),
                true
            );

            yield return new EqualityTestInstance<Array<int>>(
                new EagerArray<int>(new[] { 1, 2, 3 }),
                Array<int>.Empty,
                false
            );

            yield return new EqualityTestInstance<Array<int>>(
                new EagerArray<int>(new[] { 1, 2, 3 }),
                new EagerArray<int>(new[] { 3, 2, 1 }),
                false
            );

            yield return new EqualityTestInstance<Array<int>>(
                ErrorArray("Error 1"),
                ErrorArray("Error 1"),
                true
            );

            yield return new EqualityTestInstance<Array<int>>(
                ErrorArray("Error 1"),
                ErrorArray("Error 2"),
                false
            );

            yield return new EqualityTestInstance<Array<int>>(
                ErrorArray("Error 1"),
                new EagerArray<int>(new[] { 1, 2, 3 }),
                false
            );
        }
    }

    public record EqualityTestInstance<T>(T Left, T? Right, bool ShouldBeEqual) : ITestInstance
    {
        /// <inheritdoc />
        public void Run(ITestOutputHelper testOutputHelper)
        {
            var equal          = Left!.Equals(Right);
            var leftHash       = Left.GetHashCode();
            var rightHash      = Right?.GetHashCode() ?? -1;
            var hashCodesEqual = leftHash == rightHash;

            if (ShouldBeEqual)
            {
                equal.Should().BeTrue("they should be equal");
                hashCodesEqual.Should().BeTrue("Hash codes should be equal");
            }
            else
            {
                equal.Should().BeFalse("they should not be equal");
                hashCodesEqual.Should().BeFalse("Hash codes should not be equal");
            }
        }

        /// <inheritdoc />
        public string Name => $"{Left} {(ShouldBeEqual ? "==" : "!=")} {Right}";
    }
}

}
