using System;
using System.Collections.Generic;
using System.Linq;
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
[UseTestOutputHelper]
public partial class EqualityTests
{
    [GenerateTheory("ErrorBuilders")]
    public IEnumerable<EqualityTestInstance<IErrorBuilder>> ErrorBuilderTestInstances
    {
        get
        {
            yield return new EqualityTestInstance<IErrorBuilder>(
                ErrorCode.Test.ToErrorBuilder("abc"),
                ErrorCode.Test.ToErrorBuilder("abc"),
                true
            );

            yield return new EqualityTestInstance<IErrorBuilder>(
                ErrorCode.Test.ToErrorBuilder("abc"),
                ErrorCode.Test.ToErrorBuilder("123"),
                false
            );

            yield return new EqualityTestInstance<IErrorBuilder>(
                ErrorCode.Test.ToErrorBuilder("123"),
                new ErrorBuilderList(new[] { ErrorCode.Test.ToErrorBuilder("123") }),
                true
            );

            yield return new EqualityTestInstance<IErrorBuilder>(
                ErrorCode.Test.ToErrorBuilder("234"),
                new ErrorBuilderList(
                    new[]
                    {
                        ErrorCode.Test.ToErrorBuilder("234"),
                        ErrorCode.Test.ToErrorBuilder("567")
                    }
                ),
                true
            );
        }
    }

    [GenerateTheory("Arrays")]
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

            static Array<int> GetLazyArray(params int[] numbers)
            {
                var ea = new LazyArray<int>(numbers.ToAsyncEnumerable());
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

            yield return new EqualityTestInstance<Array<int>>(
                GetLazyArray(1, 2, 3),
                new EagerArray<int>(new[] { 1, 2, 3 }),
                true
            );
        }
    }

    public record EqualityTestInstance<T>(T Left, T? Right, bool ShouldBeEqual) : ITestInstance

    {
        /// <inheritdoc />
        public void Run(ITestOutputHelper testOutputHelper)
        {
            bool lrEquals;
            bool rlEquals;

            if (Left is IEquatable<T> eL && Right is IEquatable<T> eR)
            {
                lrEquals = eL.Equals(Right);
                rlEquals = eR.Equals(Left);
            }
            else
            {
                lrEquals = Left!.Equals(Right);
                rlEquals = Right is not null && Right.Equals(Left);
            }

            var leftHash       = Left.GetHashCode();
            var rightHash      = Right?.GetHashCode() ?? -1;
            var hashCodesEqual = leftHash == rightHash;

            if (ShouldBeEqual)
            {
                lrEquals.Should().BeTrue("left should equal right");
                rlEquals.Should().BeTrue("right should equal left");
                hashCodesEqual.Should().BeTrue("Hash codes should be equal");
            }
            else
            {
                lrEquals.Should().BeFalse("left should not equal right");
                rlEquals.Should().BeFalse("right should not equal left");
                hashCodesEqual.Should().BeFalse("Hash codes should not be equal");
            }
        }

        /// <inheritdoc />
        public string Name => $"{Left} {(ShouldBeEqual ? "==" : "!=")} {Right}";
    }
}

}
