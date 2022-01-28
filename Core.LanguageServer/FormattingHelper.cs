namespace Reductech.Sequence.Core.LanguageServer;

/// <summary>
/// Contains methods for formatting SCL
/// </summary>
public static class FormattingHelper
{
    /// <summary>
    /// Format an SCL document, even if it contains errors
    /// </summary>
    public static List<SCLTextEdit> FormatSCL(string scl, StepFactoryStore stepFactoryStore)
    {
        //Split the SCL into commands
        //Try to freeze each command
        //Format each command that freezes

        var commands = Helpers.SplitIntoCommands(scl);
        var typeResolver = QuickInfoVisitor.CreateLazyTypeResolver(scl, stepFactoryStore).Value;
        var textEdits = new List<SCLTextEdit>();
        var commandCallerMetadata = new CallerMetadata("Command", "", TypeReference.Any.Instance);

        foreach (var (command, offset) in commands)
        {
            var stepParseResult = SCLParsing.TryParseStep(command);

            if (stepParseResult.IsSuccess)
            {
                Result<IStep, IError> freezeResult;

                if (typeResolver.IsSuccess)
                {
                    freezeResult = stepParseResult.Value.TryFreeze(
                        commandCallerMetadata,
                        typeResolver.Value
                    );
                }
                else
                {
                    freezeResult = stepParseResult.Value.TryFreeze(
                        commandCallerMetadata,
                        stepFactoryStore
                    );
                }

                if (freezeResult.IsSuccess)
                {
                    var text = Format(freezeResult.Value).Trim();

                    var range = freezeResult.Value.TextLocation?.GetRange(
                        offset.Line,
                        offset.Character
                    )!;

                    var realRange = new TextRange(
                        offset,
                        new LinePosition(
                            range.EndLineNumber,
                            range.EndColumn + 1
                        ) //End one character later
                    );

                    textEdits.Add(new SCLTextEdit(text, realRange));
                }
            }
        }

        return textEdits;
    }

    /// <summary>
    /// Format a step
    /// </summary>
    public static string Format(IStep step)
    {
        var sb = new IndentationStringBuilder();
        //extract all comments from the step
        //Format the step, inserting comments where necessary

        var remainingComments =
            new Stack<Comment>(
                ReadComments(step)
                    .Select(
                        x =>
                            new Comment(
                                x.Text,
                                x.Type == SCLLexer.SINGLELINECOMMENT,
                                new TextPosition(x)
                            )
                    )
                    .OrderByDescending(x => x.Position)
            );

        step.Format(sb, new FormattingOptions(), remainingComments);

        foreach (var remainingComment in remainingComments)
            remainingComment.Append(sb);

        return sb.ToString();

        //Format1(sb, step, true, remainingComments);

        //static void Format1(
        //    IndentationStringBuilder sb,
        //    IStep step,
        //    bool topLevel,
        //    Stack<Comment> remainingComments)
        //{
        //    PrintComments(sb, remainingComments, step);

        //    step.Serialize()

        //    if (step is ICompoundStep compoundStep)
        //    {
        //        if (compoundStep is ISequenceStep sequenceStep)
        //        {
        //            foreach (var seqStep in sequenceStep.AllSteps)
        //            {
        //                PrintComments(sb, remainingComments, seqStep);
        //                sb.AppendLine();
        //                sb.Append("- ");
        //                Format1(sb, seqStep, true, remainingComments);
        //            }

        //            return;
        //        }

        //        if (compoundStep.StepFactory.Serializer is FunctionSerializer)
        //        {
        //            var allProperties =
        //                ((IEnumerable<StepProperty>)compoundStep.GetType()
        //                    .GetProperty("AllProperties")!
        //                    .GetValue(compoundStep)!).ToList();

        //            if (!topLevel && compoundStep.ShouldBracketWhenSerialized)
        //                sb.Append("(");

        //            if (allProperties.Count > 1)
        //            {
        //                sb.AppendLine(compoundStep.Name);
        //                sb.Indent();
        //            }
        //            else
        //                sb.Append(compoundStep.Name + " ");

        //            foreach (var stepProperty in allProperties)
        //            {
        //                PrintComments(sb, remainingComments, stepProperty);

        //                sb.Append(stepProperty.Name);
        //                sb.Append(": ");

        //                if (stepProperty is StepProperty.SingleStepProperty ssp)
        //                {
        //                    Format1(sb, ssp.Step, false, remainingComments);
        //                }
        //                else if (stepProperty is StepProperty.LambdaFunctionProperty lfp)
        //                {
        //                    sb.Append("(");

        //                    if (lfp.LambdaFunction.Variable is null)
        //                        sb.Append("<>");
        //                    else
        //                        sb.Append(
        //                            lfp.LambdaFunction.Variable.Value.Serialize(
        //                                SerializeOptions.Serialize
        //                            )
        //                        );

        //                    sb.Append(" => ");
        //                    Format1(sb, lfp.LambdaFunction.Step, false, remainingComments);
        //                    sb.Append(")");
        //                }
        //                else
        //                {
        //                    sb.Append(stepProperty.Serialize(SerializeOptions.Serialize));
        //                }

        //                if (allProperties.Count > 1)
        //                    sb.AppendLine();
        //            }

        //            if (!topLevel && compoundStep.ShouldBracketWhenSerialized)
        //                sb.Append(")");

        //            if (allProperties.Count > 1)
        //                sb.UnIndent();

        //            return;
        //        }
        //    }

        //    //TODO entity constant
        //    //Default to basic serialization

        //    sb.Append(step.Serialize(SerializeOptions.Serialize));
        //}

        //foreach (var remainingComment in remainingComments)
        //{
        //    remainingComment.Append(sb);
        //}
    }

    private static IEnumerable<IToken> ReadComments(IStep step)
    {
        if (step.TextLocation is null)
            yield break;

        var inputStream = new AntlrInputStream(step.TextLocation.Text);
        var lexer       = new SCLLexer(inputStream, TextWriter.Null, TextWriter.Null);

        foreach (var token in lexer.GetAllTokens())
        {
            if (token.Type is SCLLexer.DELIMITEDCOMMENT or SCLLexer.SINGLELINECOMMENT)
            {
                yield return token;
            }
        }
    }
}
