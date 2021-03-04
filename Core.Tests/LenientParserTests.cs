//using FluentAssertions;
//using Reductech.EDR.Core.Internal;
//using Reductech.EDR.Core.Internal.Parser;
//using Xunit;

//namespace Reductech.EDR.Core.Tests
//{

//[AutoTheory.UseTestOutputHelper]
//public partial class LenientParserTests
//{
//    [Theory]
//    [InlineData("Print 123", 1, "la La LA")]
//    [InlineData("Print Egg", 1, "la La LA")]
//    public void ShouldGiveCorrectHover(string text, int position, string expectedHover)
//    {
//        var sfs = StepFactoryStore.CreateUsingReflection(typeof(IStep));

//        var hover = LenientSCLParsing.GetHoverString(text, position, sfs);

//        hover.Should().Be(expectedHover);
//    }
//}

//}


