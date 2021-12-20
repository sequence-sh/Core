using System.IO;
using System.Text;

namespace Reductech.Sequence.Core.Tests;

/// <summary>
/// Tests equals and get hash code for various objects
/// </summary>
[UseTestOutputHelper]
public partial class EqualityTests
{
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

    [GenerateTheory("StringStreams")]
    public IEnumerable<EqualityTestInstance<StringStream>> StringStreamTestInstances
    {
        get
        {
            static StringStream CreateLazyStringStream(string s)
            {
                var memoryStream = new MemoryStream(Encoding.Default.GetBytes(s));

                return new StringStream(memoryStream, EncodingEnum.Default);
            }

            var l1 = CreateLazyStringStream("lazy");

            yield return new EqualityTestInstance<StringStream>(l1, l1, true);

            yield return new EqualityTestInstance<StringStream>("abc1", "abc1", true);
            yield return new EqualityTestInstance<StringStream>("abc2", "123",  false);

            yield return new EqualityTestInstance<StringStream>(
                "abc3",
                CreateLazyStringStream("abc3"),
                true
            );

            yield return new EqualityTestInstance<StringStream>(
                "abc4",
                CreateLazyStringStream("123"),
                false
            );

            yield return new EqualityTestInstance<StringStream>("abc5", null, false);

            yield return new EqualityTestInstance<StringStream>(
                CreateLazyStringStream("abc6"),
                null,
                false
            );
        }
    }

    [GenerateTheory("Errors")]
    public IEnumerable<EqualityTestInstance<IError>> SingleErrorTestInstances
    {
        get
        {
            yield return new EqualityTestInstance<IError>(
                ErrorCode.Test.ToErrorBuilder("abc").WithLocation(ErrorLocation.EmptyLocation),
                ErrorCode.Test.ToErrorBuilder("abc").WithLocation(ErrorLocation.EmptyLocation),
                true
            );

            yield return new EqualityTestInstance<IError>(
                ErrorCode.Test.ToErrorBuilder("abc").WithLocation(ErrorLocation.EmptyLocation),
                ErrorCode.Test.ToErrorBuilder("123").WithLocation(ErrorLocation.EmptyLocation),
                false
            );

            yield return new EqualityTestInstance<IError>(
                ErrorCode.Test.ToErrorBuilder("123").WithLocation(ErrorLocation.EmptyLocation),
                new ErrorList(
                    new[]
                    {
                        ErrorCode.Test.ToErrorBuilder("123")
                            .WithLocationSingle(ErrorLocation.EmptyLocation)
                    }
                ),
                true
            );

            yield return new EqualityTestInstance<IError>(
                ErrorCode.Test.ToErrorBuilder("234").WithLocation(ErrorLocation.EmptyLocation),
                new ErrorList(
                    new[]
                    {
                        ErrorCode.Test.ToErrorBuilder("234")
                            .WithLocationSingle(ErrorLocation.EmptyLocation),
                        ErrorCode.Test.ToErrorBuilder("567")
                            .WithLocationSingle(ErrorLocation.EmptyLocation)
                    }
                ),
                false
            );
        }
    }

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
                false
            );
        }
    }

    [GenerateTheory("Arrays")]
    public IEnumerable<EqualityTestInstance<Array<SCLInt>>> ArrayTestInstances
    {
        get
        {
            //Array
            static Array<SCLInt> ErrorArray(string error)
            {
                async IAsyncEnumerable<SCLInt> GetStuff()
                {
                    await Task.CompletedTask;
                    yield return 1.ConvertToSCLObject();

                    throw new ErrorException(
                        new SingleError(
                            ErrorLocation.EmptyLocation,
                            ErrorCode.Test.ToErrorBuilder(error)
                        )
                    );
                }

                var ea = new LazyArray<SCLInt>(GetStuff());

                return ea;
            }

            static Array<SCLInt> GetLazyArray(params int[] numbers)
            {
                var ea = new LazyArray<SCLInt>(
                    numbers.Select(x => x.ConvertToSCLObject()).ToAsyncEnumerable()
                );

                return ea;
            }

            yield return new EqualityTestInstance<Array<SCLInt>>(
                Array<SCLInt>.Empty,
                Array<SCLInt>.Empty,
                true
            );

            yield return new EqualityTestInstance<Array<SCLInt>>(
                Array<SCLInt>.Empty,
                null,
                false
            );

            yield return new EqualityTestInstance<Array<SCLInt>>(
                new EagerArray<SCLInt>(
                    new[] { 1, 2, 3 }.Select(x => x.ConvertToSCLObject()).ToArray()
                ),
                new EagerArray<SCLInt>(
                    new[] { 1, 2, 3 }.Select(x => x.ConvertToSCLObject()).ToArray()
                ),
                true
            );

            yield return new EqualityTestInstance<Array<SCLInt>>(
                new EagerArray<SCLInt>(
                    new[] { 1, 2, 3 }.Select(x => x.ConvertToSCLObject()).ToArray()
                ),
                Array<SCLInt>.Empty,
                false
            );

            yield return new EqualityTestInstance<Array<SCLInt>>(
                new EagerArray<SCLInt>(
                    new[] { 1, 2, 3 }.Select(x => x.ConvertToSCLObject()).ToArray()
                ),
                new EagerArray<SCLInt>(
                    new[] { 3, 2, 1 }.Select(x => x.ConvertToSCLObject()).ToArray()
                ),
                false
            );

            yield return new EqualityTestInstance<Array<SCLInt>>(
                ErrorArray("Error 1"),
                ErrorArray("Error 1"),
                true
            );

            yield return new EqualityTestInstance<Array<SCLInt>>(
                ErrorArray("Error 1"),
                ErrorArray("Error 2"),
                false
            );

            yield return new EqualityTestInstance<Array<SCLInt>>(
                ErrorArray("Error 1"),
                new EagerArray<SCLInt>(
                    new[] { 1, 2, 3 }.Select(x => x.ConvertToSCLObject()).ToArray()
                ),
                false
            );

            yield return new EqualityTestInstance<Array<SCLInt>>(
                GetLazyArray(1, 2, 3),
                new EagerArray<SCLInt>(
                    new[] { 1, 2, 3 }.Select(x => x.ConvertToSCLObject()).ToArray()
                ),
                true
            );
        }
    }
}
