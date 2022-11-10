using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace Sequence.Core.Internal.Parser;

public static partial class SCLParsing
{
    /// <summary>
    /// Visitor for parsing SCL
    /// </summary>
    public class Visitor : SCLBaseVisitor<Result<FreezableStepProperty, IError>>
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

            var sequence = FreezableFactory.CreateFreezableSequence(
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
        public override Result<FreezableStepProperty, IError> VisitUnbracketedArray(
            SCLParser.UnbracketedArrayContext context)
        {
            var members =
                context.sclObjectTerm().Select(Visit);

            var r = Aggregate(new TextLocation(context), members);
            return r;
        }

        /// <inheritdoc />
        public override Result<FreezableStepProperty, IError> VisitNullValue(
            SCLParser.NullValueContext context)
        {
            var location = new TextLocation(context);

            var member = new FreezableStepProperty.Step(
                new NullConstant(location),
                location
            );

            return member;
        }

        /// <inheritdoc />
        public override Result<FreezableStepProperty, IError> VisitBoolean(
            SCLParser.BooleanContext context)
        {
            var b = context.TRUE() != null;

            var location = new TextLocation(context);

            var member = new FreezableStepProperty.Step(
                new SCLConstantFreezable<SCLBool>(b.ConvertToSCLObject(), location),
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

            var constant = new SCLConstantFreezable<SCLDateTime>(
                dateTime.ConvertToSCLObject(),
                location
            );

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

            var step = FreezableFactory.CreateFreezableArrayAccess(
                arrayResult.Value,
                indexerResult.Value,
                new TextLocation(context)
            );

            return new FreezableStepProperty.Step(step, new TextLocation(context));
        }

        /// <inheritdoc />
        public override Result<FreezableStepProperty, IError> VisitEntityGetValue(
            SCLParser.EntityGetValueContext context)
        {
            var entityResult = Visit(context.accessedEntity).Map(x => x.ConvertToStep());

            if (entityResult.IsFailure)
                return entityResult.ConvertFailure<FreezableStepProperty>();

            //var indexerResult = Visit(context.indexer).Map(x => x.ConvertToStep());
            var indexer = new SCLConstantFreezable<StringStream>(
                context.indexer.Text,
                new TextLocation(context.indexer)
            );

            var step = FreezableFactory.CreateFreezableEntityGetValue(
                entityResult.Value,
                indexer,
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
                .WithLocationSingle(
                    pt.exception is null
                        ? new TextLocation(pt.Start)
                        : new TextLocation(pt.exception.OffendingToken)
                );
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
                _ => "Unknown Error"
                //_ => throw new ArgumentOutOfRangeException(nameof(re))
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

            var step = FreezableFactory.CreateFreezableSetVariable(
                vn,
                member.Value,
                new TextLocation(context)
            );

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
                new FreezableStepData(
                    new Dictionary<StepParameterReference, FreezableStepProperty>(),
                    location
                ),
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
                new EnumConstantFreezable(prefix, suffix, location),
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
                    new SCLConstantFreezable<SCLInt>(num.ConvertToSCLObject(), location),
                    location
                );

                return member;
            }

            if (double.TryParse(text, out var d))
            {
                var member = new FreezableStepProperty.Step(
                    new SCLConstantFreezable<SCLDouble>(d.ConvertToSCLObject(), location),
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
            else if (context.MULTILINESTRING() != null)
                s = UnescapeMultiline(context.MULTILINESTRING().GetText());
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
                new SCLConstantFreezable<StringStream>(stringStream, location),
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
                var node = context.GetChild(i);

                if (node is TerminalNodeImpl isc)
                {
                    var text      = isc.GetText();
                    var unescaped = UnescapeInterpolated(text, i == 0);

                    var cs = new SCLConstantFreezable<StringStream>(
                        unescaped,
                        new TextLocation(isc.Symbol)
                    );

                    steps.Add(cs);
                }
                else
                {
                    var r = Visit(node);

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
                FreezableFactory.CreateFreezableInterpolatedString(
                    steps,
                    new TextLocation(context)
                );

            return new FreezableStepProperty.Step(freezableStep, new TextLocation(context));
        }

        static string UnescapeMultiline(string txt) =>
            TrimStartNewLine(txt.Substring(3, txt.Length - 6));

        static string TrimStartNewLine(string txt)
        {
            if (txt.StartsWith("\r\n"))
            {
                return txt[2..];
            }

            if (txt.StartsWith("\n"))
            {
                return txt[1..];
            }

            return txt;
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
            var dict   = new Dictionary<StepParameterReference, FreezableStepProperty>();

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
            var dict   = new Dictionary<StepParameterReference, FreezableStepProperty>();

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

        private Result<IReadOnlyDictionary<EntityNestedKey, FreezableStepProperty>, IError>
            AggregateEntityProperties(
                IEnumerable<SCLParser.EntityPropertyContext> entityProperties,
                ErrorLocation location)
        {
            var l      = new List<(EntityNestedKey key, FreezableStepProperty member)>();
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
                    .Failure<IReadOnlyDictionary<EntityNestedKey, FreezableStepProperty>, IError>(
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

        private Result<(EntityNestedKey names, FreezableStepProperty value), IError>
            GetEntityProperty(SCLParser.EntityPropertyContext context)
        {
            var keyComponents = context.entityPropertyName()
                .Select(GetContextString)
                .SelectMany(x => x.Split('.'))
                .ToArray();

            var key = new EntityNestedKey(keyComponents);

            var value = Visit(context.term());

            if (value.IsFailure)
                return value
                    .ConvertFailure<(EntityNestedKey name, FreezableStepProperty value)>();

            return (key, value.Value);

            static string GetContextString(SCLParser.EntityPropertyNameContext context)
            {
                if (context.NAME() != null)
                    return context.NAME().Symbol.Text;

                return GetString(context.quotedString());
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
