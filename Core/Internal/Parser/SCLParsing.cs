using System.IO;
using Antlr4.Runtime;
using StepParameterDict =
    System.Collections.Generic.Dictionary<Reductech.Sequence.Core.Internal.StepParameterReference,
        Reductech.Sequence.Core.Internal.FreezableStepProperty>;

namespace Reductech.Sequence.Core.Internal.Parser;

/// <summary>
/// Contains methods for parsing sequences
/// </summary>
public static partial class SCLParsing
{
    /// <summary>
    /// Try to parse this SCL text as a step
    /// </summary>
    public static Result<IFreezableStep, IError> TryParseStep(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new SingleError(
                ErrorLocation.EmptyLocation,
                ErrorCode.EmptySequence
            );

        var r = TryParse(text).Map(x => x.ConvertToStep());

        return r;
    }

    /// <summary>
    /// Creates a type resolver lazily
    /// </summary>
    public static TypeResolver CreateTypeResolver(
        string fullSCL,
        StepFactoryStore stepFactoryStore,
        IReadOnlyDictionary<VariableName, InjectedVariable>? variablesToInject = null)
    {
        var fullResult =
            TryParseStep(fullSCL)
                .Bind(
                    x => TypeResolver.TryCreate(
                        stepFactoryStore,
                        SCLRunner.RootCallerMetadata,
                        Maybe<VariableName>.None,
                        x,
                        variablesToInject
                    )
                );

        if (fullResult.IsSuccess)
            return fullResult.Value;

        var partialResult1 =
            TypeResolver.TryCreate(
                stepFactoryStore,
                SCLRunner.RootCallerMetadata,
                Maybe<VariableName>.None,
                null,
                variablesToInject
            );

        //TODO parse individual lines to get types

        if (partialResult1.IsSuccess)
            return partialResult1.Value;

        return TypeResolver.Create(stepFactoryStore);
    }

    private static Result<FreezableStepProperty, IError> TryParse(string text)
    {
        var inputStream       = new AntlrInputStream(text);
        var lexer             = new SCLLexer(inputStream, TextWriter.Null, TextWriter.Null);
        var commonTokenStream = new CommonTokenStream(lexer);
        var parser            = new SCLParser(commonTokenStream, TextWriter.Null, TextWriter.Null);

        var syntaxErrorListener = new SyntaxErrorListener();
        parser.AddErrorListener(syntaxErrorListener);

        var visitor = new Visitor();

        var result = visitor.Visit(parser.fullSequence());

        if (syntaxErrorListener.Errors.Any())
        {
            return Result.Failure<FreezableStepProperty, IError>(
                ErrorList.Combine(syntaxErrorListener.Errors)
            );
        }

        if (result.IsFailure)
            return result;

        if (syntaxErrorListener.UncategorizedErrors.Any())
        {
            return Result.Failure<FreezableStepProperty, IError>(
                ErrorList.Combine(syntaxErrorListener.UncategorizedErrors)
            );
        }

        return result;
    }
}
