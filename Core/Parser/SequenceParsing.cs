using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using CSharpFunctionalExtensions;
using OneOf;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using static Reductech.EDR.Core.Internal.FreezableFactory;
using StepPropertyDict = System.Collections.Generic.Dictionary<OneOf.OneOf<string, int>, Reductech.EDR.Core.Internal.FreezableStepProperty>;

namespace Reductech.EDR.Core.Parser
{
    /// <summary>
    /// Contains methods for parsing sequences
    /// </summary>
    public static class SequenceParsing
    {
        /// <summary>
        /// Deserialize this yaml into a step.
        /// </summary>
        public static Result<IFreezableStep, IError> ParseSequence(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new SingleError("Sequence is empty.", ErrorCode.EmptySequence, EntireSequenceLocation.Instance);

            var r = TryParse(text).Map(x=> x.ConvertToStep());


            return r;
        }



        /// <summary>
        /// Try to parse this text
        /// </summary>
        public static Result<FreezableStepProperty, IError> TryParse(string text)
        {
            var inputStream = new AntlrInputStream(text);


            var lexer = new SequenceLexer(inputStream);
            var commonTokenStream = new CommonTokenStream(lexer);
            var parser = new SequenceParser(commonTokenStream);

            var visitor = new Visitor();

            var result = visitor.Visit(parser.fullSequence());

            return result;
        }



        private class Visitor : SequenceBaseVisitor<Result<FreezableStepProperty, IError>>
        {
            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitFullSequence(SequenceParser.FullSequenceContext context)
            {
                if (context.step() != null)
                    return VisitStep(context.step());

                return VisitStepSequence(context.stepSequence());
            }


            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitStepSequence(SequenceParser.StepSequenceContext context)
            {
                var results = new List<Result<IFreezableStep, IError>>();

                foreach (var stepContext in context.step())
                {
                    results.Add(VisitStep(stepContext).Map(x => x.ConvertToStep()));
                }

                var result = results.Combine(ErrorList.Combine).Map(x=>x.ToList());

                if (result.IsFailure) return result.ConvertFailure<FreezableStepProperty>();


                if (result.Value.Count == 0)
                    return new SingleError("Sequence contained no members", ErrorCode.CouldNotParse, new TextPosition(context));

                var sequence = CreateFreezableSequence(
                    result.Value.SkipLast(1).ToList(),
                    result.Value.Last(),
                    null, new TextPosition(context));

                return new FreezableStepProperty(sequence, new TextPosition(context));
            }


            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitArray(SequenceParser.ArrayContext context)
            {
                var members =
                    context.term().Select(VisitTerm);

                var r = Aggregate(new TextPosition(context),members);
                return r;
            }

            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitBoolean(SequenceParser.BooleanContext context)
            {
                var b = context.TRUE() != null;

                var member = new FreezableStepProperty(new ConstantFreezableStep(b), new TextPosition(context));
                return member;

            }

            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitBracketedStep(SequenceParser.BracketedStepContext context) => VisitStep(context.step());

            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitInfixOperation(SequenceParser.InfixOperationContext context)
            {

                var left = VisitTerm(context.term(0));
                var right = VisitTerm(context.term(1));
                var operatorSymbol = context.infixOperator().GetText();

                var result = InfixHelper.TryCreateStep(new TextPosition(context), left, right, operatorSymbol);

                return result;
            }



            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitErrorNode(IErrorNode node) =>
                new SingleError($"Could not parse '{node.GetText()}'", ErrorCode.CouldNotParse,new TextPosition(node.Symbol));

            private static SingleError ParseError(ParserRuleContext pt)
            {

                return new SingleError($"Could not parse '{pt.GetText()}'", ErrorCode.CouldNotParse, new TextPosition(pt));
            }

            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitSetVariable(SequenceParser.SetVariableContext context)
            {
                var member = VisitTerm(context.term());

                if (member.IsFailure) return member;

                var vn = GetVariableName(context.VARIABLENAME());

                var step = CreateFreezableSetVariable(vn, member.Value, new TextPosition(context));

                return new FreezableStepProperty(step, new TextPosition(context) );
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

                var member = new FreezableStepProperty(new ConstantFreezableStep(new Enumeration(prefix, suffix)), new TextPosition(context));

                return member;
            }

            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitNumber(SequenceParser.NumberContext context)
            {
                if (int.TryParse(context.NUMBER().GetText(), out var num))
                {
                    var member = new FreezableStepProperty(new ConstantFreezableStep(num), new TextPosition(context));

                    return member;
                }
                return new SingleError($"Could not parse '{context.GetText()}' as a number",
                    ErrorCode.CouldNotParse,new TextPosition(context));
            }

            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitQuotedString(SequenceParser.QuotedStringContext context)
            {
                string s = context.DOUBLEQUOTEDSTRING() != null ?
                    UnescapeDoubleQuoted(context.DOUBLEQUOTEDSTRING().GetText()):
                    UnescapeSingleQuoted(context.SINGLEQUOTEDSTRING().GetText());

                var member = new FreezableStepProperty(new ConstantFreezableStep(s), new TextPosition(context) );

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
            public override Result<FreezableStepProperty, IError> VisitFunction(SequenceParser.FunctionContext context)
            {
                var name = context.NAME().Symbol.Text;

                var errors = new List<IError>();
                var dict = new StepPropertyDict();

                var numberedArguments = context.term().Select((term, i) =>
                    (term: VisitTerm(term), number: OneOf<string, int>.FromT1(i + 1)));



                foreach (var numberedArgument in numberedArguments)
                {
                    if(numberedArgument.term.IsFailure) errors.Add(numberedArgument.term.Error);
                    else dict.Add(numberedArgument.number, numberedArgument.term.Value);
                }

                var members = AggregateNamedArguments(context.namedArgument());

                if (members.IsFailure) errors.Add(members.Error);
                else
                    foreach (var (key, value) in members.Value)
                        dict.Add(key, value);

                if(errors.Any())
                    return Result.Failure<FreezableStepProperty, IError>(ErrorList.Combine(errors));



                var fsd = new FreezableStepData(dict, new TextPosition(context) );

                var cfs = new CompoundFreezableStep(name, fsd, null);

                return new FreezableStepProperty(cfs,new TextPosition(context) );
            }

            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitEntity(SequenceParser.EntityContext context)
            {
                var members = AggregateNamedArguments(context.namedArgument());

                if (members.IsFailure) return members.ConvertFailure<FreezableStepProperty>();

                var step = new CreateEntityFreezableStep(new FreezableEntityData(members.Value, new TextPosition(context)));

                return new FreezableStepProperty(step, new TextPosition(context));
            }

            private Result<IReadOnlyDictionary<string, FreezableStepProperty>, IError>
                AggregateNamedArguments(IEnumerable<SequenceParser.NamedArgumentContext> namedArguments)
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
                            new TextPosition(duplicateKeys.Key,
                                (duplicateKeys.First().member.Location as TextPosition)!.StartIndex,
                                (duplicateKeys.Last().member.Location as TextPosition)!.StopIndex
                            )));
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

                return new FreezableStepProperty(vn, new TextPosition(node.Symbol));
            }

            private static Result<FreezableStepProperty, IError> Aggregate(TextPosition textPosition, IEnumerable<Result<FreezableStepProperty, IError>> nodes)
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

                return new FreezableStepProperty(l.ToImmutable(), textPosition);
            }

        }



    }

}
