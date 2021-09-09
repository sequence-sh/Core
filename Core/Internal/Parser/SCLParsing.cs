using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
using static Reductech.EDR.Core.Internal.FreezableFactory;
using StepParameterDict =
    System.Collections.Generic.Dictionary<Reductech.EDR.Core.Internal.StepParameterReference,
        Reductech.EDR.Core.Internal.FreezableStepProperty>;

namespace Reductech.EDR.Core.Internal.Parser
{

/// <summary>
/// Contains methods for parsing sequences
/// </summary>
public static class SCLParsing
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

    private static Result<FreezableStepProperty, IError> TryParse(string text)
    {
        var inputStream       = new AntlrInputStream(text);
        var lexer             = new SCLLexer(inputStream);
        var commonTokenStream = new CommonTokenStream(lexer);
        var parser            = new SCLParser(commonTokenStream);

        var syntaxErrorListener = new SyntaxErrorListener();
        parser.AddErrorListener(syntaxErrorListener);

        var visitor = new Visitor();

        var result = visitor.Visit(parser.fullSequence());

        if (result.IsFailure)
            return result;

        if (syntaxErrorListener.Errors.Any())
        {
            return Result.Failure<FreezableStepProperty, IError>(
                ErrorList.Combine(syntaxErrorListener.Errors)
            );
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
        public readonly List<IError> Errors = new();

        /// <inheritdoc />
        public override void SyntaxError(
            TextWriter output,
            IRecognizer recognizer,
            IToken offendingSymbol,
            int line,
            int charPositionInLine,
            string msg,
            RecognitionException e)
        {
            var error =
                ErrorCode.SCLSyntaxError.ToErrorBuilder(msg)
                    .WithLocation(new TextLocation(offendingSymbol));

            Errors.Add(error);
        }
    }

    private class Visitor : SCLBaseVisitor<Result<FreezableStepProperty, IError>>
    {
        /// <inheritdoc />
        public override Result<FreezableStepProperty, IError> VisitFullSequence(
            SCLParser.FullSequenceContext context)
        {
            if (context.step() != null)
                return Visit(context.step());

            if (context.stepSequence() != null)
                return VisitStepSequence(context.stepSequence());

            return ParseError(context);
        }

        /// <inheritdoc />
        public override Result<FreezableStepProperty, IError> VisitStepSequence(
            SCLParser.StepSequenceContext context)
        {
            var results = new List<Result<IFreezableStep, IError>>();

            foreach (var stepContext in context.step())
            {
                results.Add(Visit(stepContext).Map(x => x.ConvertToStep()));
            }

            var result = results.Combine(ErrorList.Combine).Map(x => x.ToList());

            if (result.IsFailure)
                return result.ConvertFailure<FreezableStepProperty>();

            if (result.Value.Count == 0)
                return new SingleError(
                    new TextLocation(context),
                    ErrorCode.EmptySequence
                );

            var sequence = CreateFreezableSequence(
                result.Value.SkipLast(1).ToList(),
                result.Value.Last(),
                new TextLocation(context)
            );

            return new FreezableStepProperty.Step(
                sequence,
                new TextLocation(context)
            );
        }

        /// <inheritdoc />
        public override Result<FreezableStepProperty, IError> VisitArray(
            SCLParser.ArrayContext context)
        {
            var members =
                context.term().Select(Visit);

            var r = Aggregate(new TextLocation(context), members);
            return r;
        }

        /// <inheritdoc />
        public override Result<FreezableStepProperty, IError> VisitBoolean(
            SCLParser.BooleanContext context)
        {
            var b = context.TRUE() != null;

            var location = new TextLocation(context);

            var member = new FreezableStepProperty.Step(
                new BoolConstantFreezable(b, location),
                location
            );

            return member;
        }

        /// <inheritdoc />
        public override Result<FreezableStepProperty, IError> VisitDateTime(
            SCLParser.DateTimeContext context)
        {
            var location = new TextLocation(context);

            if (!DateTime.TryParse(context.GetText(), out var dateTime))
            {
                var message = context.GetText();

                return new SingleError(
                    location,
                    ErrorCode.CouldNotParse,
                    message,
                    nameof(DateTime)
                );
            }

            var constant = new DateTimeConstantFreezable(dateTime, location);

            var member = new FreezableStepProperty.Step(constant, location);
            return member;
        }

        /// <inheritdoc />
        public override Result<FreezableStepProperty, IError> VisitArrayAccess(
            SCLParser.ArrayAccessContext context)
        {
            var arrayResult = Visit(context.arrayOrEntity).Map(x => x.ConvertToStep());

            if (arrayResult.IsFailure)
                return arrayResult.ConvertFailure<FreezableStepProperty>();

            var indexerResult = Visit(context.indexer).Map(x => x.ConvertToStep());

            if (indexerResult.IsFailure)
                return indexerResult.ConvertFailure<FreezableStepProperty>();

            var step = CreateFreezableArrayAccess(
                arrayResult.Value,
                indexerResult.Value,
                new TextLocation(context)
            );

            return new FreezableStepProperty.Step(step, new TextLocation(context));
        }

        /// <inheritdoc />
        public override Result<FreezableStepProperty, IError> VisitBracketedStep(
            SCLParser.BracketedStepContext context) => Visit(context.step())
            .Map(x => x with { StepMetadata = x.StepMetadata with { Bracketed = true } });

        /// <inheritdoc />
        public override Result<FreezableStepProperty, IError> VisitInfixOperation(
            SCLParser.InfixOperationContext context)
        {
            var operatorSymbols =
                context.infixOperator().Select(x => x.GetText()).Distinct().ToList();

            if (operatorSymbols.Count != 1)
            {
                return Result.Failure<FreezableStepProperty, IError>(
                    ErrorCode.SCLSyntaxError.ToErrorBuilder("Invalid mix of operators")
                        .WithLocation(new TextLocation(context))
                );
            }

            var operatorSymbol = operatorSymbols.Single();

            var terms = context.infixableTerm()
                .Select(Visit)
                .Where(x => x.IsFailure || x.Value != null)
                .ToList();

            var result = InfixHelper.TryCreateStep(
                new TextLocation(context),
                operatorSymbol,
                terms
            );

            return result;
        }

        /// <inheritdoc />
        public override Result<FreezableStepProperty, IError> VisitErrorNode(IErrorNode node) =>
            ErrorCode.SCLSyntaxError.ToErrorBuilder(node.GetText())
                .WithLocationSingle(new TextLocation(node.Symbol));

        private static SingleError ParseError(ParserRuleContext pt)
        {
            return ErrorCode.SCLSyntaxError.ToErrorBuilder(GetMessage(pt.exception))
                .WithLocationSingle(new TextLocation(pt.exception.OffendingToken));
        }

        private static string GetMessage(RecognitionException re)
        {
            return re switch
            {
                FailedPredicateException fpe => fpe.Message,
                InputMismatchException ime   => ime.Message,
                LexerNoViableAltException nve1 =>
                    $"No Viable Alternative - '{nve1.OffendingToken.Text}' not recognized.",
                NoViableAltException nve2 =>
                    $"No Viable Alternative - '{nve2.OffendingToken.Text}' was unexpected.",
                _ => throw new ArgumentOutOfRangeException(nameof(re))
            };
        }

        /// <inheritdoc />
        public override Result<FreezableStepProperty, IError> VisitSetVariable(
            SCLParser.SetVariableContext context)
        {
            var member = Visit(context.step());

            if (member.IsFailure)
                return member;

            var vn = GetVariableName(context.VARIABLENAME());

            var step = CreateFreezableSetVariable(vn, member.Value, new TextLocation(context));

            return new FreezableStepProperty.Step(step, new TextLocation(context));
        }

        /// <inheritdoc />
        public override Result<FreezableStepProperty, IError> VisitGetVariable(
            SCLParser.GetVariableContext context)
        {
            var vn = GetVariableName(context.VARIABLENAME());
            return vn;
        }

        /// <inheritdoc />
        public override Result<FreezableStepProperty, IError> VisitGetAutomaticVariable(
            SCLParser.GetAutomaticVariableContext context)
        {
            var location = new TextLocation(context);

            var cfs = new CompoundFreezableStep(
                "GetAutomaticVariable",
                new FreezableStepData(new StepParameterDict(), location),
                location
            );

            return new FreezableStepProperty.Step(cfs, location);
        }

        /// <inheritdoc />
        public override Result<FreezableStepProperty, IError> VisitEnumeration(
            SCLParser.EnumerationContext context)
        {
            if (context.children.Count != 3 || context.NAME().Length != 2)
                return ParseError(context);

            var prefix = context.NAME(0).GetText();
            var suffix = context.NAME(1).GetText();

            var location = new TextLocation(context);

            var member = new FreezableStepProperty.Step(
                new EnumConstantFreezable(new Enumeration(prefix, suffix), location),
                location
            );

            return member;
        }

        /// <inheritdoc />
        public override Result<FreezableStepProperty, IError> VisitNumber(
            SCLParser.NumberContext context)
        {
            var text     = context.GetText();
            var location = new TextLocation(context);

            if (int.TryParse(text, out var num))
            {
                var member = new FreezableStepProperty.Step(
                    new IntConstantFreezable(num, location),
                    location
                );

                return member;
            }

            if (double.TryParse(text, out var d))
            {
                var member = new FreezableStepProperty.Step(
                    new DoubleConstantFreezable(d, location),
                    new TextLocation(context)
                );

                return member;
            }

            return new SingleError(
                location,
                ErrorCode.CouldNotParse,
                context.GetText(),
                "Number"
            );
        }

        private static string GetString(SCLParser.QuotedStringContext context)
        {
            string s;

            if (context.DOUBLEQUOTEDSTRING() != null)
                s = UnescapeDoubleQuoted(context.DOUBLEQUOTEDSTRING().GetText());
            else if (context.SINGLEQUOTEDSTRING() != null)
                s = UnescapeSingleQuoted(context.SINGLEQUOTEDSTRING().GetText());
            else if (context.SIMPLEISTRING() != null)
                s = UnescapeInterpolated(context.SIMPLEISTRING().GetText(), true);
            else
                throw new Exception($"Could not parse {context}");

            return s;
        }

        /// <inheritdoc />
        public override Result<FreezableStepProperty, IError> VisitQuotedString(
            SCLParser.QuotedStringContext context)
        {
            string s = GetString(context);

            StringStream stringStream = s;
            var          location     = new TextLocation(context);

            var member = new FreezableStepProperty.Step(
                new StringConstantFreezable(stringStream, location),
                location
            );

            return member;
        }

        /// <inheritdoc />
        public override Result<FreezableStepProperty, IError> VisitInterpolatedString(
            SCLParser.InterpolatedStringContext context)
        {
            var steps  = new List<IFreezableStep>();
            var errors = new List<IError>();

            for (var i = 0; i < context.ChildCount; i++)
            {
                var index = i / 2;

                if (i % 2 == 0)
                {
                    var s         = (ITerminalNode)context.GetChild(i);
                    var text      = s.GetText();
                    var unescaped = UnescapeInterpolated(text, i == 0);

                    var cs = new StringConstantFreezable(
                        unescaped,
                        new TextLocation(s.Symbol)
                    );

                    steps.Add(cs);
                }
                else
                {
                    var s = context.step(index);
                    var r = Visit(s);

                    if (r.IsSuccess) { steps.Add(r.Value.ConvertToStep()); }
                    else { errors.Add(r.Error); }
                }
            }

            if (errors.Any())
            {
                var e = ErrorList.Combine(errors);
                return Result.Failure<FreezableStepProperty, IError>(e);
            }

            var freezableStep =
                CreateFreezableInterpolatedString(
                    steps,
                    new TextLocation(context)
                );

            return new FreezableStepProperty.Step(freezableStep, new TextLocation(context));
        }

        static string UnescapeInterpolated(string txt, bool removeDollar)
        {
            if (removeDollar)
                txt = txt[1..];

            txt = txt[1..^1]; //Remove quotes and dollar

            if (string.IsNullOrEmpty(txt)) { return txt; }

            txt = txt.Replace("{{", "{");

            var sb = new StringBuilder(txt.Length);

            for (var ix = 0; ix < txt.Length;)
            {
                var jx = txt.IndexOf('\\', ix);

                if (jx < 0 || jx == txt.Length - 1)
                    jx = txt.Length;

                sb.Append(txt, ix, jx - ix);

                if (jx >= txt.Length)
                    break;

                var escapedChar = GetEscapedChar(txt, jx);

                if (escapedChar.HasValue)
                    sb.Append(escapedChar.Value);
                else
                    sb.Append('\\').Append(txt[jx + 1]);

                ix = jx + 2;
            }

            return sb.ToString();
        }

        static string UnescapeDoubleQuoted(string txt)
        {
            txt = txt[1..^1]; //Remove quotes

            if (string.IsNullOrEmpty(txt)) { return txt; }

            var sb = new StringBuilder(txt.Length);

            for (var ix = 0; ix < txt.Length;)
            {
                var jx = txt.IndexOf('\\', ix);

                if (jx < 0 || jx == txt.Length - 1)
                    jx = txt.Length;

                sb.Append(txt, ix, jx - ix);

                if (jx >= txt.Length)
                    break;

                var escapedChar = GetEscapedChar(txt, jx);

                if (escapedChar.HasValue)
                    sb.Append(escapedChar.Value);
                else
                    sb.Append('\\').Append(txt[jx + 1]);

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

        static char? GetEscapedChar(string txt, int jx)
        {
            return txt[jx + 1] switch
            {
                '"'  => '"',
                'n'  => '\n',
                'r'  => '\r',
                't'  => '\t',
                '\\' => '\\',
                _    => null
            };
        }

        /// <inheritdoc />
        public override Result<FreezableStepProperty, IError> VisitLambda(
            SCLParser.LambdaContext context)
        {
            var vnText = context.VARIABLENAME()?.Symbol.Text;

            VariableName? vn = vnText is null
                ? null
                : new VariableName(vnText.TrimStart('<').TrimEnd('>'));

            var step = Visit(context.step());

            if (step.IsFailure)
                return step;

            var location = new TextLocation(context);

            var lambda = new FreezableStepProperty.Lambda(vn, step.Value.ConvertToStep(), location);

            return lambda;
        }

        /// <inheritdoc />
        public override Result<FreezableStepProperty, IError> VisitPipeFunction(
            SCLParser.PipeFunctionContext context)
        {
            var name = context.function().NAME().Symbol.Text;

            var errors = new List<IError>();
            var dict   = new StepParameterDict();

            var firstStep = Visit(context.step());

            if (firstStep.IsFailure)
                errors.Add(firstStep.Error);
            else
                dict.Add(
                    new StepParameterReference.Index(1),
                    firstStep.Value with
                    {
                        StepMetadata = firstStep.Value.StepMetadata with
                        {
                            PassedAsInfix = true
                        }
                    }
                );

            var numberedArguments = context.function()
                .term()
                .Select(
                    (term, i) =>
                        (term: Visit(term), number: i + 2)
                );

            foreach (var (term, number) in numberedArguments)
            {
                if (term.IsFailure)
                    errors.Add(term.Error);
                else
                    dict.Add(new StepParameterReference.Index(number), term.Value);
            }

            var location = new TextLocation(context);

            var members = AggregateNamedArguments(context.function().namedArgument(), location);

            if (members.IsFailure)
                errors.Add(members.Error);
            else
                foreach (var (key, value) in members.Value)
                    dict.Add(new StepParameterReference.Named(key), value);

            if (errors.Any())
                return Result.Failure<FreezableStepProperty, IError>(ErrorList.Combine(errors));

            var fsd = new FreezableStepData(dict, new TextLocation(context));

            var cfs = new CompoundFreezableStep(name, fsd, location);

            return new FreezableStepProperty.Step(cfs, new TextLocation(context));
        }

        /// <inheritdoc />
        public override Result<FreezableStepProperty, IError> VisitFunction(
            SCLParser.FunctionContext context)
        {
            var name = context.NAME().Symbol.Text;

            var errors = new List<IError>();
            var dict   = new StepParameterDict();

            var numberedArguments = context.term()
                .Select(
                    (term, i) =>
                        (term: Visit(term), number: i + 1)
                );

            foreach (var (term, number) in numberedArguments)
            {
                if (term.IsFailure)
                    errors.Add(term.Error);
                else
                    dict.Add(new StepParameterReference.Index(number), term.Value);
            }

            var location = new TextLocation(context);

            var members = AggregateNamedArguments(context.namedArgument(), location);

            if (members.IsFailure)
                errors.Add(members.Error);
            else
                foreach (var (key, value) in members.Value)
                    dict.Add(new StepParameterReference.Named(key), value);

            if (errors.Any())
                return Result.Failure<FreezableStepProperty, IError>(ErrorList.Combine(errors));

            var fsd = new FreezableStepData(dict, new TextLocation(context));

            var cfs = new CompoundFreezableStep(name, fsd, location);

            return new FreezableStepProperty.Step(cfs, new TextLocation(context));
        }

        /// <inheritdoc />
        public override Result<FreezableStepProperty, IError> VisitEntity(
            SCLParser.EntityContext context)
        {
            var members = AggregateEntityProperties(
                context.entityProperty(),
                new TextLocation(context)
            );

            if (members.IsFailure)
                return members.ConvertFailure<FreezableStepProperty>();

            var step = new CreateEntityFreezableStep(
                new FreezableEntityData(members.Value, new TextLocation(context))
            );

            return new FreezableStepProperty.Step(step, new TextLocation(context));
        }

        private Result<IReadOnlyDictionary<EntityPropertyKey, FreezableStepProperty>, IError>
            AggregateEntityProperties(
                IEnumerable<SCLParser.EntityPropertyContext> entityProperties,
                ErrorLocation location)
        {
            var l      = new List<(EntityPropertyKey key, FreezableStepProperty member)>();
            var errors = new List<IError>();

            foreach (var r in entityProperties.Select(GetEntityProperty))
            {
                if (r.IsFailure)
                    errors.Add(r.Error);
                else
                    l.Add(r.Value);
            }

            foreach (var duplicateKeys in l.GroupBy(x => x.key).Where(x => x.Count() > 1))
            {
                errors.Add(
                    new SingleError(
                        location,
                        ErrorCode.DuplicateParameter,
                        duplicateKeys.Key
                    )
                );
            }

            if (errors.Any())
                return Result
                    .Failure<IReadOnlyDictionary<EntityPropertyKey, FreezableStepProperty>, IError>(
                        ErrorList.Combine(errors)
                    );

            var dict = l.ToDictionary(x => x.key, x => x.member);

            return dict;
        }

        private Result<IReadOnlyDictionary<string, FreezableStepProperty>, IError>
            AggregateNamedArguments(
                IEnumerable<SCLParser.NamedArgumentContext> namedArguments,
                ErrorLocation location)
        {
            var l      = new List<(string key, FreezableStepProperty member)>();
            var errors = new List<IError>();

            foreach (var r in namedArguments.Select(GetNamedArgument))
            {
                if (r.IsFailure)
                    errors.Add(r.Error);
                else
                    l.Add(r.Value);
            }

            foreach (var duplicateKeys in l.GroupBy(x => x.key).Where(x => x.Count() > 1))
            {
                errors.Add(
                    new SingleError(
                        location,
                        ErrorCode.DuplicateParameter,
                        duplicateKeys.Key
                    )
                );
            }

            if (errors.Any())
                return Result.Failure<IReadOnlyDictionary<string, FreezableStepProperty>, IError>(
                    ErrorList.Combine(errors)
                );

            var dict = l.ToDictionary(x => x.key, x => x.member);

            return dict;
        }

        private Result<(EntityPropertyKey names, FreezableStepProperty value), IError>
            GetEntityProperty(SCLParser.EntityPropertyContext context)
        {
            var keyComponents = context.entityPropertyName()
                .Select(GetContextString)
                .SelectMany(x => x.Split('.'))
                .ToList();

            var key = new EntityPropertyKey(keyComponents);

            var value = Visit(context.term());

            if (value.IsFailure)
                return value
                    .ConvertFailure<(EntityPropertyKey name, FreezableStepProperty value)>();

            return (key, value.Value);

            static string GetContextString(SCLParser.EntityPropertyNameContext context)
            {
                if (context.NAME() != null)
                    return context.NAME().Symbol.Text;

                return Visitor.GetString(context.quotedString());
            }
        }

        private Result<(string name, FreezableStepProperty value), IError> GetNamedArgument(
            SCLParser.NamedArgumentContext context)
        {
            var key = context.NAME().Symbol.Text;

            var value = Visit(context.term());

            if (value.IsFailure)
                return value.ConvertFailure<(string name, FreezableStepProperty value)>();

            return (key, value.Value);
        }

        private static FreezableStepProperty GetVariableName(ITerminalNode node)
        {
            var text = node.Symbol.Text;

            if (text == null || !text.StartsWith('<') || !text.EndsWith('>'))
                throw new Exception(
                    $"Expected variable name to be in angle brackets but was '{text}'"
                );

            var vn = new VariableName(text.TrimStart('<').TrimEnd('>'));

            return new FreezableStepProperty.Variable(vn, new TextLocation(node.Symbol));
        }

        private static Result<FreezableStepProperty, IError> Aggregate(
            TextLocation textLocation,
            IEnumerable<Result<FreezableStepProperty, IError>> nodes)
        {
            var l      = ImmutableList<IFreezableStep>.Empty.ToBuilder();
            var errors = new List<IError>();

            foreach (var node in nodes)
            {
                var result = node.Map(x => x.ConvertToStep());

                if (result.IsSuccess)
                    l.Add(result.Value);
                else
                    errors.Add(result.Error);
            }

            if (errors.Any())
                return Result.Failure<FreezableStepProperty, IError>(ErrorList.Combine(errors));

            return new FreezableStepProperty.StepList(l.ToImmutable(), textLocation);
        }
    }
}

}
