using Reductech.Sequence.Core.LanguageServer;

namespace Reductech.Sequence.Core.Tests.LanguageServer;

public class DiagnosticsTest
{
    [Theory]
    [InlineData("Print 123")]
    [InlineData("Pront 123", "The step 'Pront' does not exist[start: (0, 0), end: (0, 8)]")]
    [InlineData("a b", "The step 'a' does not exist[start: (0, 0), end: (0, 2)]")]
    [InlineData("- print 1\r\n- a b", "The step 'a' does not exist[start: (1, 2), end: (1, 4)]")]
    [InlineData(
        "- print 1\r\n- a b\r\n- print 2",
        "The step 'a' does not exist[start: (1, 2), end: (1, 4)]"
    )]
    [InlineData(
        "- print 1\r\n- a b\r\n- c d\r\n- print 3",
        "The step 'a' does not exist[start: (1, 2), end: (1, 4)]",
        "The step 'c' does not exist[start: (2, 2), end: (2, 4)]"
    )]
    [InlineData(
        "- print .\r\n- print 123",
        "Syntax Error: no viable alternative at input '- print .'[start: (0, 0), end: (0, 9)]"
    )]
    public void TestGetDiagnostics(string text, params string[] expectedErrors)
    {
        var sfs         = StepFactoryStore.Create();
        var diagnostics = DiagnosticsHelper.GetDiagnostics(text, sfs);

        diagnostics.Select(x => x.Message + new TextRange(x.Start, x.End))
            .Should()
            .BeEquivalentTo(expectedErrors);
    }
}
