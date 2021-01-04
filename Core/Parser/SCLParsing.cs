using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using CSharpFunctionalExtensions;
using OneOf;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using static Reductech.EDR.Core.Internal.FreezableFactory;
using StepParameterDict = System.Collections.Generic.Dictionary<Reductech.EDR.Core.Internal.StepParameterReference, Reductech.EDR.Core.Internal.FreezableStepProperty>;

namespace Reductech.EDR.Core.Parser
{
    /// <summary>
    /// Contains methods for parsing sequences
    /// </summary>
    public static class SCLParsing
    {
        /// <summary>
        /// Deserialize this SCL into a step.
        /// </summary>
        public static Result<IFreezableStep, IError> ParseSequence(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new SingleError("Sequence is empty.", ErrorCode.EmptySequence, EntireSequenceLocation.Instance);

            var r = TryParse(text).Map(x=> x.ConvertToStep());

            return r;
        }

        /// <summary>
        /// Try to parse this SCL text
        /// </summary>
        public static Result<FreezableStepProperty, IError> TryParse(string text)
        {
            var inputStream = new AntlrInputStream(text);
            var lexer = new SequenceLexer(inputStream);
            var commonTokenStream = new CommonTokenStream(lexer);
            var parser = new SequenceParser(commonTokenStream);

            var syntaxErrorListener = new SyntaxErrorListener();
            parser.AddErrorListener(syntaxErrorListener);

            var visitor = new Visitor();

            var result = visitor.Visit(parser.fullSequence());

            if (syntaxErrorListener.Errors.Any())
            {

                return Result.Failure<FreezableStepProperty, IError>(ErrorList.Combine(syntaxErrorListener.Errors));
            }

            return result;
        }

        /// <summary>
        /// Error listener that looks for syntax errors
        /// </summary>
        public class SyntaxErrorListener : BaseErrorListener
        {
            /// <summary>
            /// Errors found by this error listener.
            /// </summary>
            public readonly List<IError> Errors = new List<IError>();

            /// <inheritdoc />
            public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine,
                string msg, RecognitionException e)
            {
                var error = new SingleError(msg, ErrorCode.CouldNotParse, new TextLocation(offendingSymbol));
                Errors.Add(error);
            }
        }

        private class Visitor : SequenceBaseVisitor<Result<FreezableStepProperty, IError>>
        {
            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitFullSequence(SequenceParser.FullSequenceContext context)
            {
                if (context.step() != null)
                    return Visit(context.step());

                if(context.stepSequence() != null)
                    return VisitStepSequence(context.stepSequence());

                return ParseError(context);
            }


            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitStepSequence(SequenceParser.StepSequenceContext context)
            {
                var results = new List<Result<IFreezableStep, IError>>();

                foreach (var stepContext in context.step())
                {
                    results.Add(Visit(stepContext).Map(x => x.ConvertToStep()));
                }

                var result = results.Combine(ErrorList.Combine).Map(x=>x.ToList());

                if (result.IsFailure) return result.ConvertFailure<FreezableStepProperty>();


                if (result.Value.Count == 0)
                    return new SingleError("Sequence contained no members", ErrorCode.CouldNotParse, new TextLocation(context));

                var sequence = CreateFreezableSequence(
                    result.Value.SkipLast(1).ToList(),
                    result.Value.Last(),
                    null, new TextLocation(context));

                return new FreezableStepProperty(sequence, new TextLocation(context));
            }


            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitArray(SequenceParser.ArrayContext context)
            {
                var members =
                    context.term().Select(VisitTerm);

                var r = Aggregate(new TextLocation(context),members);
                return r;
            }

            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitBoolean(SequenceParser.BooleanContext context)
            {
                var b = context.TRUE() != null;

                var member = new FreezableStepProperty(new BoolConstantFreezable(b), new TextLocation(context));
                return member;
            }

            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitDateTime(SequenceParser.DateTimeContext context)
            {
                if (!DateTime.TryParse(context.GetText(), out var dateTime))
                    return new SingleError($"Could not parse '{context.GetText()}'", ErrorCode.CouldNotParse,
                        new TextLocation(context));

                var constant = new DateTimeConstantFreezable(dateTime);

                var member = new FreezableStepProperty(constant, new TextLocation(context));
                return member;
            }

            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitBracketedStep(SequenceParser.BracketedStepContext context) => Visit(context.step());

            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitInfixOperation(SequenceParser.InfixOperationContext context)
            {

                var left = VisitTerm(context.term(0));
                var right = VisitTerm(context.term(1));
                var operatorSymbol = context.infixOperator().GetText();

                var result = InfixHelper.TryCreateStep(new TextLocation(context), left, right, operatorSymbol);

                return result;
            }

            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitErrorNode(IErrorNode node) =>
                new SingleError($"Could not parse '{node.GetText()}'", ErrorCode.CouldNotParse,new TextLocation(node.Symbol));

            private static SingleError ParseError(ParserRuleContext pt)
            {

                return new SingleError($"Could not parse '{pt.GetText()}'", ErrorCode.CouldNotParse, new TextLocation(pt));
            }

            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitSetVariable(SequenceParser.SetVariableContext context)
            {
                var member = Visit(context.step());

                if (member.IsFailure) return member;

                var vn = GetVariableName(context.VARIABLENAME());

                var step = CreateFreezableSetVariable(vn, member.Value, new TextLocation(context));

                return new FreezableStepProperty(step, new TextLocation(context) );
            }

            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitGetVariable(SequenceParser.GetVariableContext context)
            {
                var vn = GetVariableName(context.VARIABLENAME());
                return vn;
            }

            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitEnumeration(SequenceParser.EnumerationContext context)
            {
                if (context.children.Count != 3 || context.NAME().Length != 2)
                    return ParseError(context);

                var prefix = context.NAME(0).GetText();
                var suffix = context.NAME(1).GetText();

                var member = new FreezableStepProperty(new EnumConstantFreezable(new Enumeration(prefix, suffix)), new TextLocation(context));

                return member;
            }

            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitNumber(SequenceParser.NumberContext context)
            {
                if (int.TryParse(context.NUMBER().GetText(), out var num))
                {
                    var member = new FreezableStepProperty(new IntConstantFreezable(num), new TextLocation(context));

                    return member;
                }
                return new SingleError($"Could not parse '{context.GetText()}' as a number",
                    ErrorCode.CouldNotParse,new TextLocation(context));
            }

            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitQuotedString(SequenceParser.QuotedStringContext context)
            {
                string s = context.DOUBLEQUOTEDSTRING() != null ?
                    UnescapeDoubleQuoted(context.DOUBLEQUOTEDSTRING().GetText()):
                    UnescapeSingleQuoted(context.SINGLEQUOTEDSTRING().GetText());

                var stringStream = new StringStream(s);

                var member = new FreezableStepProperty(new StringConstantFreezable(stringStream), new TextLocation(context) );

                return member;

                static string UnescapeDoubleQuoted(string txt)
                {
                    txt = txt[1..^1]; //Remove quotes

                    if (string.IsNullOrEmpty(txt)) { return txt; }
                    var sb = new StringBuilder(txt.Length);
                    for (var ix = 0; ix < txt.Length;)
                    {
                        var jx = txt.IndexOf('\\', ix);
                        if (jx < 0 || jx == txt.Length - 1) jx = txt.Length;
                        sb.Append(txt, ix, jx - ix);
                        if (jx >= txt.Length) break;
                        switch (txt[jx + 1])
                        {
                            case '"': sb.Append('"'); break;   //double quote
                            case 'n': sb.Append('\n'); break;  // Line feed
                            case 'r': sb.Append('\r'); break;  // Carriage return
                            case 't': sb.Append('\t'); break;  // Tab
                            case '\\': sb.Append('\\'); break; // Don't escape
                            default:                                 // Unrecognized, copy as-is
                                sb.Append('\\').Append(txt[jx + 1]); break;
                        }
                        ix = jx + 2;
                    }
                    return sb.ToString();
                }

                static string UnescapeSingleQuoted(string s)
                {
                    s = s[1..^1]; //Remove quotes
                    s = s
                        .Replace("''", "'");
                    return s;
                }
            }

            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitPipeFunction(SequenceParser.PipeFunctionContext context)
            {
                var name = context.NAME().Symbol.Text;

                var errors = new List<IError>();
                var dict = new StepParameterDict();

                var firstStep = Visit(context.step());
                if(firstStep.IsFailure) errors.Add(firstStep.Error);
                else dict.Add(new StepParameterReference(1), firstStep.Value);

                var numberedArguments = context.term().Select((term, i) =>
                    (term: VisitTerm(term), number: OneOf<string, int>.FromT1(i + 2)));


                foreach (var (term, number) in numberedArguments)
                {
                    if (term.IsFailure) errors.Add(term.Error);
                    else dict.Add(new StepParameterReference(number), term.Value);
                }

                var members = AggregateNamedArguments(context.namedArgument(), new TextLocation(context));

                if (members.IsFailure) errors.Add(members.Error);
                else
                    foreach (var (key, value) in members.Value)
                        dict.Add(new StepParameterReference(key), value);

                if (errors.Any())
                    return Result.Failure<FreezableStepProperty, IError>(ErrorList.Combine(errors));



                var fsd = new FreezableStepData(dict, new TextLocation(context));

                var cfs = new CompoundFreezableStep(name, fsd, null);

                return new FreezableStepProperty(cfs, new TextLocation(context));
            }

            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitFunction(SequenceParser.FunctionContext context)
            {
                var name = context.NAME().Symbol.Text;

                var errors = new List<IError>();
                var dict = new StepParameterDict();

                var numberedArguments = context.term().Select((term, i) =>
                    (term: VisitTerm(term), number: OneOf<string, int>.FromT1(i + 1)));


                foreach (var (term, number) in numberedArguments)
                {
                    if(term.IsFailure) errors.Add(term.Error);
                    else dict.Add(new StepParameterReference(number),  term.Value);
                }

                var members = AggregateNamedArguments(context.namedArgument(), new TextLocation(context));

                if (members.IsFailure) errors.Add(members.Error);
                else
                    foreach (var (key, value) in members.Value)
                        dict.Add(new StepParameterReference(key), value);

                if(errors.Any())
                    return Result.Failure<FreezableStepProperty, IError>(ErrorList.Combine(errors));



                var fsd = new FreezableStepData(dict, new TextLocation(context) );

                var cfs = new CompoundFreezableStep(name, fsd, null);

                return new FreezableStepProperty(cfs,new TextLocation(context) );
            }

            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitEntity(SequenceParser.EntityContext context)
            {
                var members = AggregateNamedArguments(context.namedArgument(), new TextLocation(context));

                if (members.IsFailure) return members.ConvertFailure<FreezableStepProperty>();

                var step = new CreateEntityFreezableStep(new FreezableEntityData(members.Value, new TextLocation(context)));

                return new FreezableStepProperty(step, new TextLocation(context));
            }

            private Result<IReadOnlyDictionary<string, FreezableStepProperty>, IError>
                AggregateNamedArguments(IEnumerable<SequenceParser.NamedArgumentContext> namedArguments, IErrorLocation location)
            {
                var l = new List<(string key, FreezableStepProperty member)>();
                var errors = new List<IError>();

                foreach (var r in namedArguments.Select(GetNamedArgument))
                {
                    if (r.IsFailure) errors.Add(r.Error);
                    else
                        l.Add(r.Value);
                }

                foreach (var duplicateKeys in l.GroupBy(x => x.key).Where(x => x.Count() > 1))
                {
                    errors.Add(new SingleError(
                        $"Duplicate Parameter '{duplicateKeys.Key}'",
                        ErrorCode.DuplicateParameter,
                        location));
                }

                if (errors.Any())
                    return Result.Failure<IReadOnlyDictionary<string, FreezableStepProperty>, IError>(ErrorList.Combine(errors));

                var dict = l.ToDictionary(x => x.key, x => x.member);

                return dict;
            }


            private Result<(string name, FreezableStepProperty value), IError> GetNamedArgument(SequenceParser.NamedArgumentContext context)
            {
                var key = context.NAME().Symbol.Text;

                var value = VisitTerm(context.term());
                if (value.IsFailure) return value.ConvertFailure<(string name, FreezableStepProperty value)>();

                return (key, value.Value);
            }


            private static FreezableStepProperty GetVariableName(ITerminalNode node)
            {

                var text = node.Symbol.Text;

                if (text == null || !text.StartsWith('<') || !text.EndsWith('>'))
                    throw new Exception($"Expected variable name to be in angle brackets but was '{text}'");
                var vn = new VariableName(text.TrimStart('<').TrimEnd('>'));

                return new FreezableStepProperty(vn, new TextLocation(node.Symbol));
            }

            private static Result<FreezableStepProperty, IError> Aggregate(TextLocation textLocation, IEnumerable<Result<FreezableStepProperty, IError>> nodes)
            {
                var l = ImmutableList<IFreezableStep>.Empty.ToBuilder();
                var errors = new List<IError>();

                foreach (var node in nodes)
                {
                    var result = node.Map(x => x.ConvertToStep());

                    if (result.IsSuccess) l.Add(result.Value);
                    else errors.Add(result.Error);
                }

                if (errors.Any())
                    return Result.Failure<FreezableStepProperty, IError>(ErrorList.Combine(errors));

                return new FreezableStepProperty(l.ToImmutable(), textLocation);
            }
        }
    }
}
