//using System;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.Linq;
//using System.Text;
//using Antlr4.Runtime;
//using Antlr4.Runtime.Misc;
//using Antlr4.Runtime.Tree;
//using CSharpFunctionalExtensions;
//using OneOf;
//using Reductech.EDR.Core.Internal.Errors;
//using Reductech.EDR.Core.Util;

//namespace Reductech.EDR.Core.Internal.Parser
//{

//public static class LenientSCLParsing
//{
//    public static string GetHoverString(
//        string text,
//        int position,
//        StepFactoryStore stepFactoryStore)
//    {
//        var inputStream       = new AntlrInputStream(text);
//        var lexer             = new SCLLexer(inputStream);
//        var commonTokenStream = new CommonTokenStream(lexer);
//        var parser            = new SCLParser(commonTokenStream);

//        var interval = new Interval(position, position);
//        var visitor  = new HoverVisitor(interval, stepFactoryStore);

//        var result = visitor.Visit(parser.fullSequence());

//        return result;
//    }

//    //public record SCLDocument
//    //{
//    //    public SCLDocument(string text)
//    //    {
//    //        Text = text;

//    //        ASTNode = new Lazy<IASTNode>(
//    //            () => Reductech.EDR.Core.Internal.Parser.IASTNode.Create(Text)
//    //        );
//    //    }

//    //    public string Text { get; }

//    //    public Lazy<ASTNode> ASTNode { get; }
//    //}

//    //internal class Visitor : SCLBaseVisitor<IASTNode>
//    //{
//    //    /// <inheritdoc />
//    //    public override ASTNode VisitFullSequence(SCLParser.FullSequenceContext context)
//    //    {
//    //        context.

//    //    if (context.step() != null)
//    //            return Visit(context.step());

//    //        if (context.stepSequence() != null)
//    //            return VisitStepSequence(context.stepSequence());

//    //        return ParseError(context, SCLClass.Sequence.Instance);
//    //    }

//    //    /// <inheritdoc />
//    //    public override ASTNode VisitStepSequence(SCLParser.StepSequenceContext context)
//    //    {
//    //        var nodes = context.step().Select(Visit);

//    //        return new ASTNode.Parent(
//    //            new TextLocation(context),
//    //            SCLClass.Sequence.Instance,
//    //            nodes.ToList()
//    //        );
//    //    }

//    //    /// <inheritdoc />
//    //    public override ASTNode VisitArray(SCLParser.ArrayContext context)
//    //    {
//    //        var nodes =
//    //            context.term().Select(Visit).ToList();

//    //        return new ASTNode.Parent(
//    //            new TextLocation(context),
//    //            SCLClass.Array.Instance,
//    //            nodes.ToList()
//    //        );
//    //    }

//    //    /// <inheritdoc />
//    //    public override ASTNode VisitBoolean(SCLParser.BooleanContext context)
//    //    {
//    //        var b = context.TRUE() != null;

//    //        var location = new TextLocation(context);

//    //        return new ASTNode.Terminal(location, SCLClass.Boolean.Instance);

//    //        //var member = new FreezableStepProperty(
//    //        //    new BoolConstantFreezable(b, location),
//    //        //    location
//    //        //);

//    //        //return new List<ParsedToken>() { new ParsedToken.Step(member) };
//    //    }

//    //    /// <inheritdoc />
//    //    public override ASTNode VisitDateTime(SCLParser.DateTimeContext context)
//    //    {
//    //        var location = new TextLocation(context);

//    //        if (DateTime.TryParse(context.GetText(), out var _))
//    //        {
//    //            return new ASTNode.Terminal(location, SCLClass.DateTime.Instance);

//    //            //var constant = new DateTimeConstantFreezable(dateTime, location);

//    //            //var member = new FreezableStepProperty(constant, location);
//    //            //yield return new ParsedToken.Step(member);
//    //        }
//    //        else
//    //        {
//    //            var message = context.GetText();

//    //            return new ASTNode.ErrorNode(
//    //                location,
//    //                SCLClass.DateTime.Instance,
//    //                ErrorCode.CouldNotParse.ToErrorBuilder(message, nameof(DateTime))
//    //            );
//    //        }
//    //    }

//    //    /// <inheritdoc />
//    //    public override ASTNode VisitArrayAccess(SCLParser.ArrayAccessContext context)
//    //    {
//    //        var arrayResult = Visit(context.arrayOrEntity);
//    //        var indexerResult = Visit(context.indexer);

//    //        return new ASTNode.Parent(
//    //            new TextLocation(context),
//    //            SCLClass.Step.Instance,
//    //            new List<ASTNode>() { arrayResult, indexerResult }
//    //        );
//    //    }

//    //    /// <inheritdoc />
//    //    public override ASTNode VisitBracketedStep(SCLParser.BracketedStepContext context) =>
//    //        new ASTNode.Parent(
//    //            new TextLocation(context),
//    //            SCLClass.Parenthetic.Instance,
//    //            new List<ASTNode>() { Visit(context.step()) }
//    //        );

//    //    /// <inheritdoc />
//    //    public override ASTNode VisitInfixOperation(SCLParser.InfixOperationContext context)
//    //    {
//    //        var operatorSymbols =
//    //            context.infixOperator().Select(x => x.GetText()).Distinct().ToList();

//    //        if (operatorSymbols.Count != 1)
//    //        {
//    //            var builder = ErrorCode.SCLSyntaxError.ToErrorBuilder("Invalid mix of operators");

//    //            return new ASTNode.ErrorNode(
//    //                new TextLocation(context),
//    //                SCLClass.Step.Instance,
//    //                builder
//    //            );
//    //        }

//    //        var nodes = context.children.Select(Visit).ToList();

//    //        return new ASTNode.Parent(new TextLocation(context), SCLClass.Step.Instance, nodes);
//    //    }

//    //    /// <inheritdoc />
//    //    public override ASTNode VisitErrorNode(IErrorNode node)
//    //    {
//    //        return new ASTNode.ErrorNode(
//    //            new TextLocation(node.Symbol),
//    //            null,
//    //            ErrorCode
//    //                .SCLSyntaxError.ToErrorBuilder(node.GetText())
//    //        );
//    //    }

//    //    private static ASTNode.ErrorNode ParseError(ParserRuleContext pt, SCLClass expectedClass)
//    //    {
//    //        var eb = ErrorCode.SCLSyntaxError.ToErrorBuilder(GetMessage(pt.exception));

//    //        return new ASTNode.ErrorNode(new TextLocation(pt), expectedClass, eb);
//    //    }

//    //    private static string GetMessage(RecognitionException re)
//    //    {
//    //        return re switch
//    //        {
//    //            FailedPredicateException fpe => fpe.Message,
//    //            InputMismatchException ime => ime.Message,
//    //            LexerNoViableAltException nve1 =>
//    //                $"No Viable Alternative - '{nve1.OffendingToken.Text}' not recognized.",
//    //            NoViableAltException nve2 =>
//    //                $"No Viable Alternative - '{nve2.OffendingToken.Text}' was unexpected.",
//    //            _ => throw new ArgumentOutOfRangeException(nameof(re))
//    //        };
//    //    }

//    //    /// <inheritdoc />
//    //    public override ASTNode VisitSetVariable(SCLParser.SetVariableContext context)
//    //    {
//    //        var vn = GetVariableName(context.VARIABLENAME());
//    //        var member = Visit(context.step());

//    //        return new ASTNode.Parent(
//    //            new TextLocation(context),
//    //            SCLClass.Step.Instance,
//    //            new List<ASTNode>() { vn, member }
//    //        );
//    //    }

//    //    /// <inheritdoc />
//    //    public override ASTNode VisitGetVariable(SCLParser.GetVariableContext context)
//    //    {
//    //        var vn = GetVariableName(context.VARIABLENAME());
//    //        return vn;
//    //    }

//    //    /// <inheritdoc />
//    //    public override ASTNode VisitEnumeration(SCLParser.EnumerationContext context)
//    //    {
//    //        if (context.children.Count == 3 && context.NAME().Length == 2)
//    //        {
//    //            var prefix = context.NAME(0).GetText();
//    //            var suffix = context.NAME(1).GetText();

//    //            var location = new TextLocation(context);

//    //            var member = new FreezableStepProperty(
//    //                new EnumConstantFreezable(new Enumeration(prefix, suffix), location),
//    //                location
//    //            );

//    //            return new
//    //                yield return new ParsedToken.Step(member);
//    //        }
//    //        else
//    //            return ParseError(context, SCLClass.Enum.Instance);
//    //    }

//    //    /// <inheritdoc />
//    //    public override ASTNode VisitNumber(SCLParser.NumberContext context)
//    //    {
//    //        var text = context.GetText();
//    //        var location = new TextLocation(context);

//    //        if (int.TryParse(text, out var num))
//    //        {
//    //            var member = new FreezableStepProperty(
//    //                new IntConstantFreezable(num, location),
//    //                location
//    //            );

//    //            yield return new ParsedToken.Step(member);
//    //        }

//    //        else if (double.TryParse(text, out var d))
//    //        {
//    //            var member = new FreezableStepProperty(
//    //                new DoubleConstantFreezable(d, location),
//    //                new TextLocation(context)
//    //            );

//    //            yield return new ParsedToken.Step(member);
//    //        }
//    //        else
//    //        {
//    //            yield return new ParsedToken.Error(
//    //                ErrorCode.CouldNotParse.ToErrorBuilder(context.GetText(), "number"),
//    //                location
//    //            );
//    //        }
//    //    }

//    //    private static string GetString(SCLParser.QuotedStringContext context)
//    //    {
//    //        string s = context.DOUBLEQUOTEDSTRING() != null
//    //            ? UnescapeDoubleQuoted(context.DOUBLEQUOTEDSTRING().GetText())
//    //            : UnescapeSingleQuoted(context.SINGLEQUOTEDSTRING().GetText());

//    //        return s;

//    //        static string UnescapeDoubleQuoted(string txt)
//    //        {
//    //            txt = txt[1..^1]; //Remove quotes

//    //            if (string.IsNullOrEmpty(txt)) { return txt; }

//    //            var sb = new StringBuilder(txt.Length);

//    //            for (var ix = 0; ix < txt.Length;)
//    //            {
//    //                var jx = txt.IndexOf('\\', ix);

//    //                if (jx < 0 || jx == txt.Length - 1)
//    //                    jx = txt.Length;

//    //                sb.Append(txt, ix, jx - ix);

//    //                if (jx >= txt.Length)
//    //                    break;

//    //                var escapedChar = GetEscapedChar(txt, jx);

//    //                if (escapedChar.HasValue)
//    //                    sb.Append(escapedChar.Value);
//    //                else
//    //                    sb.Append('\\').Append(txt[jx + 1]);

//    //                ix = jx + 2;
//    //            }

//    //            return sb.ToString();
//    //        }

//    //        static string UnescapeSingleQuoted(string s)
//    //        {
//    //            s = s[1..^1]; //Remove quotes

//    //            s = s
//    //                .Replace("''", "'");

//    //            return s;
//    //        }

//    //        static char? GetEscapedChar(string txt, int jx)
//    //        {
//    //            return txt[jx + 1] switch
//    //            {
//    //                '"' => '"',
//    //                'n' => '\n',
//    //                'r' => '\r',
//    //                't' => '\t',
//    //                '\\' => '\\',
//    //                _ => null
//    //            };
//    //        }
//    //    }

//    //    /// <inheritdoc />
//    //    public override ASTNode VisitQuotedString(SCLParser.QuotedStringContext context)
//    //    {
//    //        string s = GetString(context);

//    //        var stringStream = new StringStream(s);
//    //        var location = new TextLocation(context);

//    //        var member = new FreezableStepProperty(
//    //            new StringConstantFreezable(stringStream, location),
//    //            location
//    //        );

//    //        yield return new ParsedToken.Step(member);
//    //    }

//    //    /// <inheritdoc />
//    //    public override ASTNode VisitPipeFunction(SCLParser.PipeFunctionContext context)
//    //    {
//    //        var name = context.NAME().Symbol.Text;

//    //        var errors = new List<IError>();
//    //        var dict = new Dictionary<StepParameterReference, FreezableStepProperty>();

//    //        var firstStep = Visit(context.step());

//    //        foreach (var parsedToken in firstStep)
//    //        {
//    //            yield return parsedToken;
//    //        }

//    //        dict.Add(new StepParameterReference(1), firstStep.Value);

//    //        var numberedArguments = context.term()
//    //            .Select(
//    //                (term, i) =>
//    //                    (term: Visit(term), number: OneOf<string, int>.FromT1(i + 2))
//    //            );

//    //        foreach (var (term, number) in numberedArguments)
//    //        {
//    //            if (term.IsFailure)
//    //                errors.Add(term.ErrorNode);
//    //            else
//    //                dict.Add(new StepParameterReference(number), term.Value);
//    //        }

//    //        var location = new TextLocation(context);

//    //        var members = AggregateNamedArguments(context.namedArgument(), location);

//    //        if (members.IsFailure)
//    //            errors.Add(members.Error);
//    //        else
//    //            foreach (var (key, value) in members.Value)
//    //                dict.Add(new StepParameterReference(key), value);

//    //        if (errors.Any())
//    //            return Result.Failure<FreezableStepProperty, IError>(ErrorList.Combine(errors));

//    //        var fsd = new FreezableStepData(dict, new TextLocation(context));

//    //        var cfs = new CompoundFreezableStep(name, fsd, null, location);

//    //        return new FreezableStepProperty(cfs, new TextLocation(context));
//    //    }

//    //    /// <inheritdoc />
//    //    public override ASTNode VisitFunction(SCLParser.FunctionContext context)
//    //    {
//    //        var name = context.NAME().Symbol.Text;

//    //        var errors = new List<IError>();
//    //        var dict = new Dictionary<StepParameterReference, FreezableStepProperty>();

//    //        var numberedArguments = context.term()
//    //            .Select(
//    //                (term, i) =>
//    //                    (term: Visit(term), number: OneOf<string, int>.FromT1(i + 1))
//    //            );

//    //        foreach (var (term, number) in numberedArguments)
//    //        {
//    //            if (term.IsFailure)
//    //                errors.Add(term.ErrorNode);
//    //            else
//    //                dict.Add(new StepParameterReference(number), term.Value);
//    //        }

//    //        var location = new TextLocation(context);

//    //        var members = AggregateNamedArguments(context.namedArgument(), location);

//    //        if (members.IsFailure)
//    //            errors.Add(members.Error);
//    //        else
//    //            foreach (var (key, value) in members.Value)
//    //                dict.Add(new StepParameterReference(key), value);

//    //        if (errors.Any())
//    //            return Result.Failure<FreezableStepProperty, IError>(ErrorList.Combine(errors));

//    //        var fsd = new FreezableStepData(dict, new TextLocation(context));

//    //        var cfs = new CompoundFreezableStep(name, fsd, null, location);

//    //        return new FreezableStepProperty(cfs, new TextLocation(context));
//    //    }

//    //    /// <inheritdoc />
//    //    public override ASTNode VisitEntity(SCLParser.EntityContext context)
//    //    {
//    //        var members = AggregateEntityProperties(
//    //            context.entityProperty(),
//    //            new TextLocation(context)
//    //        );

//    //        if (members.IsFailure)
//    //            return members.ConvertFailure<FreezableStepProperty>();

//    //        var step = new CreateEntityFreezableStep(
//    //            new FreezableEntityData(members.Value, new TextLocation(context))
//    //        );

//    //        return new FreezableStepProperty(step, new TextLocation(context));
//    //    }

//    //    private Result<IReadOnlyDictionary<EntityPropertyKey, FreezableStepProperty>, IError>
//    //        AggregateEntityProperties(
//    //            IEnumerable<SCLParser.EntityPropertyContext> entityProperties,
//    //            ErrorLocation location)
//    //    {
//    //        var l = new List<(EntityPropertyKey key, FreezableStepProperty member)>();
//    //        var errors = new List<IError>();

//    //        foreach (var r in entityProperties.Select(GetEntityProperty))
//    //        {
//    //            if (r.IsFailure)
//    //                errors.Add(r.Error);
//    //            else
//    //                l.Add(r.Value);
//    //        }

//    //        foreach (var duplicateKeys in l.GroupBy(x => x.key).Where(x => x.Count() > 1))
//    //        {
//    //            errors.Add(
//    //                new SingleError(
//    //                    location,
//    //                    ErrorCode.DuplicateParameter,
//    //                    duplicateKeys.Key
//    //                )
//    //            );
//    //        }

//    //        if (errors.Any())
//    //            return Result
//    //                .Failure<IReadOnlyDictionary<EntityPropertyKey, FreezableStepProperty>, IError>(
//    //                    ErrorList.Combine(errors)
//    //                );

//    //        var dict = l.ToDictionary(x => x.key, x => x.member);

//    //        return dict;
//    //    }

//    //    private Result<IReadOnlyDictionary<string, FreezableStepProperty>, IError>
//    //        AggregateNamedArguments(
//    //            IEnumerable<SCLParser.NamedArgumentContext> namedArguments,
//    //            ErrorLocation location)
//    //    {
//    //        var l = new List<(string key, FreezableStepProperty member)>();
//    //        var errors = new List<IError>();

//    //        foreach (var r in namedArguments.Select(GetNamedArgument))
//    //        {
//    //            if (r.IsFailure)
//    //                errors.Add(r.Error);
//    //            else
//    //                l.Add(r.Value);
//    //        }

//    //        foreach (var duplicateKeys in l.GroupBy(x => x.key).Where(x => x.Count() > 1))
//    //        {
//    //            errors.Add(
//    //                new SingleError(
//    //                    location,
//    //                    ErrorCode.DuplicateParameter,
//    //                    duplicateKeys.Key
//    //                )
//    //            );
//    //        }

//    //        if (errors.Any())
//    //            return Result.Failure<IReadOnlyDictionary<string, FreezableStepProperty>, IError>(
//    //                ErrorList.Combine(errors)
//    //            );

//    //        var dict = l.ToDictionary(x => x.key, x => x.member);

//    //        return dict;
//    //    }

//    //    private Result<(EntityPropertyKey names, FreezableStepProperty value), IError>
//    //        GetEntityProperty(SCLParser.EntityPropertyContext context)
//    //    {
//    //        var key = new EntityPropertyKey(
//    //            context.entityPropertyName().Select(GetContextString).ToList()
//    //        );

//    //        var value = Visit(context.term());

//    //        if (value.IsFailure)
//    //            return value
//    //                .ConvertFailure<(EntityPropertyKey name, FreezableStepProperty value)>();

//    //        return (key, value.Value);

//    //        static string GetContextString(SCLParser.EntityPropertyNameContext context)
//    //        {
//    //            if (context.NAME() != null)
//    //                return context.NAME().Symbol.Text;

//    //            return Visitor.GetString(context.quotedString());
//    //        }
//    //    }

//    //    private Result<(string name, FreezableStepProperty value), IError> GetNamedArgument(
//    //        SCLParser.NamedArgumentContext context)
//    //    {
//    //        var key = context.NAME().Symbol.Text;

//    //        var value = Visit(context.term());

//    //        if (value.IsFailure)
//    //            return value.ConvertFailure<(string name, FreezableStepProperty value)>();

//    //        return (key, value.Value);
//    //    }

//    //    private static ASTNode GetVariableName(ITerminalNode node)
//    //    {
//    //        var text = node.Symbol.Text;

//    //        if (text == null || !text.StartsWith('<') || !text.EndsWith('>'))
//    //            throw new Exception(
//    //                $"Expected variable name to be in angle brackets but was '{text}'"
//    //            );

//    //        return new ASTNode.Terminal(
//    //            new TextLocation(node.Symbol),
//    //            SCLClass.VariableName.Instance
//    //        );
//    //    }

//    //    //private static IReadOnlyCollection<ParsedToken> Aggregate(
//    //    //    TextLocation textLocation,
//    //    //    IEnumerable<ASTNode> nodes)
//    //    //{
//    //    //    var l      = ImmutableList<IFreezableStep>.Empty.ToBuilder();
//    //    //    var errors = new List<IError>();

//    //    //    foreach (var node in nodes)
//    //    //    {
//    //    //        var result = node.Map(x => x.ConvertToStep());

//    //    //        if (result.IsSuccess)
//    //    //            l.Add(result.Value);
//    //    //        else
//    //    //            errors.Add(result.Error);
//    //    //    }

//    //    //    if (errors.Any())
//    //    //        return Result.Failure<FreezableStepProperty, IError>(ErrorList.Combine(errors));

//    //    //    return new FreezableStepProperty(l.ToImmutable(), textLocation);
//    //    //}
//}

//}


