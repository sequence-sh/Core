using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;
using Superpower;
using Superpower.Display;
using Superpower.Model;
using Superpower.Parsers;
using Superpower.Tokenizers;
using Result = CSharpFunctionalExtensions.Result;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal.Errors;
using YamlDotNet.Core;
using Unit = Reductech.EDR.Core.Util.Unit;

namespace Reductech.EDR.Core.Serialization
{
    /// <summary>
    /// Parses strings as step members
    /// </summary>
    public class StepMemberParser
    {
        private enum ProcessToken
        {
            /// <summary>
            /// Sentinel Value.
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            None,
            [Token(Example = "<VariableName>")]
            VariableName,
            [Token(Example = "(")]
            OpenBracket,
            [Token(Example = ")")]
            CloseBracket,
            [Token(Example = "[")]
            OpenArray,
            [Token(Example = "]")]
            CloseArray,
            [Token(Example = ",", Description = "Delimiter for arrays and function calls")]
            Delimiter,
            [Token(Example = "=")]
            Assignment,
            [Token(Example = "+")]
            MathOperator,
            [Token(Example = "&&")]
            BooleanOperator,
            [Token(Example = "==")]
            Comparator,

            [Token(Example = "\"Double-Quoted\"")]
            DoubleQuotedStringLiteral,

            [Token(Example = "'Single-Quoted'")]
            SingleQuotedStringLiteral,
            [Token(Example = "123")]
            Number,
            [Token(Example = "true")]
            Boolean,
            [Token(Example = "MathOperator.And")]
            Enum,
            [Token(Example = "FuncName")]
            FuncOrArgumentName,
            [Token(Example = "Not")]
            NotOperator
        }



        private static SingleError CreateError(Superpower.Model.Result<TokenList<ProcessToken>> result, Mark start, Mark end) =>
            new SingleError(result.ErrorMessage??result.FormatErrorMessageFragment(),
                ErrorCode.CouldNotTokenize,
                new YamlRegionErrorLocation(start, end,result.ErrorPosition));

        private static SingleError CreateError(TokenListParserResult<ProcessToken, StepMember> result, Mark start, Mark end) =>
            new SingleError(result.ErrorMessage??result.FormatErrorMessageFragment(),
                ErrorCode.CouldNotParse,
                new YamlRegionErrorLocation(start, end, result.ErrorPosition));

        private static readonly Tokenizer<ProcessToken> Tokenizer = new TokenizerBuilder<ProcessToken>()
            .Ignore(Span.WhiteSpace)


            .Match(Character.EqualTo('('), ProcessToken.OpenBracket)
            .Match(Character.EqualTo(')'), ProcessToken.CloseBracket)
            .Match(Character.EqualTo('['), ProcessToken.OpenArray)
            .Match(Character.EqualTo(']'), ProcessToken.CloseArray)
            .Match(Character.EqualTo(','), ProcessToken.Delimiter)

            //VariableName must be before comparator
            .Match(Span.Regex("<[a-z0-9-_]+>", RegexOptions.Compiled | RegexOptions.IgnoreCase), ProcessToken.VariableName)
            .Match(Span.Regex(@"-?[0-9]+", RegexOptions.Compiled), ProcessToken.Number) //Number must come before MathOperator


            .Match(GetSpan(MathOperator.None), ProcessToken.MathOperator)
            .Match(GetSpan(BooleanOperator.None), ProcessToken.BooleanOperator, true)
            .Match(GetSpan(CompareOperator.None), ProcessToken.Comparator)


            .Match(Character.EqualTo('='), ProcessToken.Assignment)
            .Match(QuotedString.SqlStyle, ProcessToken.SingleQuotedStringLiteral)
            .Match(QuotedString.CStyle, ProcessToken.DoubleQuotedStringLiteral)

            .Match(Span.EqualToIgnoreCase(true.ToString()).Or(Span.EqualToIgnoreCase(false.ToString())), ProcessToken.Boolean, true)
            .Match(Span.EqualToIgnoreCase("not"), ProcessToken.NotOperator, true)

            .Match(Span.Regex(@"[a-z0-9-_]+\.[a-z0-9-_]+", RegexOptions.Compiled | RegexOptions.IgnoreCase), ProcessToken.Enum, true)

            .Match(Span.Regex("[a-z0-9-_]+", RegexOptions.Compiled | RegexOptions.IgnoreCase), ProcessToken.FuncOrArgumentName, true)
            .Build();



        private TokenListParser<ProcessToken, StepMember> Parser { get; }

        /// <summary>
        /// Tries to parse a string as a step member.
        /// </summary>
        public  Result<StepMember, IError> TryParse(string s, Mark start, Mark end)
        {
            var tokensResult = Tokenizer.TryTokenize(s);

            if (!tokensResult.HasValue)
                return CreateError(tokensResult, start, end);

            if (!tokensResult.Remainder.IsAtEnd)
                return CreateError(tokensResult, start, end);

            if(ParseAsConstantString(tokensResult.Value))
                return new StepMember(new ConstantFreezableStep(s));

            var parseResult = Parser.TryParse(tokensResult.Value);

            if (!parseResult.HasValue)
                return CreateError(parseResult, start, end);

            if (!parseResult.Remainder.IsAtEnd)
                return CreateError(parseResult, start, end);

            var checkResult = CheckForErrors(parseResult.Value)
                .MapError(x=>x.WithLocation(start,end));

            return checkResult;

        }

        /// <summary>
        /// Recursively checks the StepMember for errors.
        /// </summary>
        private static Result<StepMember, IErrorBuilder> CheckForErrors(StepMember stepMember)
        {
            var r = stepMember.Match(x => Unit.Default,
                CheckForErrors2,
                x=> x
                    .Select(CheckForErrors2)
                    .Combine(ErrorBuilderList.Combine)
                    .Map(_=> Unit.Default))
                    .Map(_=> stepMember);

            return r;



            static Result<Unit, IErrorBuilder> CheckForErrors2(IFreezableStep step)
            {
                if (step is ParseError parseError)
                    return Result.Failure<Unit, IErrorBuilder>(parseError.ErrorBuilder);

                else
                {
                    if (step is CompoundFreezableStep compoundStep)
                    {
                        var r1 =
                            compoundStep.FreezableStepData.StepMembersDictionary.Values
                                .Select(CheckForErrors)
                                .Combine(ErrorBuilderList.Combine)
                                .Map(_ => Unit.Default);
                        return r1;
                    }
                    return Unit.Default;
                }
            }
        }


        private static bool ParseAsConstantString(TokenList<ProcessToken> tokenList) =>
            tokenList.All(x => x.Kind == ProcessToken.FuncOrArgumentName);

        /// <summary>
        /// The step factory store
        /// </summary>
        public StepFactoryStore StepFactoryStore { get; }


        /// <summary>
        /// Create a new StepMemberParser
        /// </summary>
        public StepMemberParser(StepFactoryStore stepFactoryStore)
        {
            StepFactoryStore = stepFactoryStore;

            Lazy<TokenListParser<ProcessToken, StepMember>> stepMember = null!;
            Lazy<TokenListParser<ProcessToken, IFreezableStep>> freezableProcess = null!;

            TokenListParser<ProcessToken, IFreezableStep> enumConstant = CreateEnumParser(StepFactoryStore);

            Lazy<TokenListParser<ProcessToken, IFreezableStep>> singleTerm = null!;

            TokenListParser<ProcessToken, IFreezableStep> setVariable =
                (from vnToken in Token.EqualTo(ProcessToken.VariableName)
                 from _ in Token.EqualTo(ProcessToken.Assignment)
                 // ReSharper disable AccessToModifiedClosure
                 from value in Parse.Ref(() => freezableProcess.Value)

                 select SetVariableStepFactory.CreateFreezable(
                         new VariableName(vnToken.ToStringValue().TrimStart('<').TrimEnd('>')),
                         value)).Try();

            TokenListParser<ProcessToken, StepMember> array =

                (from _1 in Token.EqualTo(ProcessToken.OpenArray)
                    from _2 in Token.EqualTo(ProcessToken.CloseArray)
                    select new StepMember(new List<IFreezableStep>())
                    ).Try()
                .Or
                (from _1 in Token.EqualTo(ProcessToken.OpenArray)
                from elements in Parse.Chain(Token.EqualTo(ProcessToken.Delimiter),
                    Parse.Ref(() => freezableProcess.Value).Select(x => new[]{x} as IEnumerable<IFreezableStep>),
                    (_2,a,b)=> a.Concat(b))
                from _3 in Token.EqualTo(ProcessToken.CloseArray)
                select new StepMember(elements.ToList()));


            TokenListParser<ProcessToken, IFreezableStep> notOperation =

                (from o in Token.EqualTo(ProcessToken.NotOperator)
                    from _1 in Token.EqualTo(ProcessToken.OpenBracket)
                    from f1 in Parse.Ref(()=> singleTerm.Value)
                    from _2 in Token.EqualTo(ProcessToken.CloseBracket)
                    select NotStepFactory.CreateFreezable(f1)).Try();

            TokenListParser<ProcessToken, IFreezableStep> mathOperation =

                (from f1 in Parse.Ref(()=> singleTerm.Value)
                 from o in Token.EqualTo(ProcessToken.MathOperator)
                 from f2 in Parse.Ref(()=> singleTerm.Value)
                 select ApplyMathOperatorStepFactory.CreateFreezable(
                     f1,
                     new ConstantFreezableStep(Extensions.TryParseValue<MathOperator>(o.ToStringValue()).Value),
                     f2
                     )).Try();


            TokenListParser<ProcessToken, IFreezableStep> booleanOperation =
                (from f1 in Parse.Ref(()=> singleTerm.Value)
                 from o in Token.EqualTo(ProcessToken.BooleanOperator)
                 from f2 in Parse.Ref(()=> singleTerm.Value)
                 select ApplyBooleanOperatorStepFactory.CreateFreezable(f1,
                     new ConstantFreezableStep(Extensions.TryParseValue<BooleanOperator>(o.ToStringValue()).Value),
                  f2)).Try();

            TokenListParser<ProcessToken, IFreezableStep> compareOperation =
                (from f1 in Parse.Ref(() => singleTerm.Value)
                    from o in Token.EqualTo(ProcessToken.Comparator)
                    from f2 in Parse.Ref(() => singleTerm.Value)
                    select CompareStepFactory.CreateFreezable(f1,
                        new ConstantFreezableStep(Extensions.TryParseValue<CompareOperator>(o.ToStringValue()).Value),
                        f2)).Try();

            TokenListParser<ProcessToken, (string argumentName, StepMember stepMember)> functionMember =
                (from a in Token.EqualTo(ProcessToken.FuncOrArgumentName)
                 from _ in Token.EqualTo(ProcessToken.Assignment)
                 from pm in Parse.Ref(() => stepMember.Value)
                 select (a.ToStringValue(), pm)).Try();

            Lazy<TokenListParser<ProcessToken, IFreezableStep>> function =
                new Lazy<TokenListParser<ProcessToken, IFreezableStep>>(()=>
                    (from fName in Token.EqualTo(ProcessToken.FuncOrArgumentName)
                     from _1 in Token.EqualTo(ProcessToken.OpenBracket)
                     from args in functionMember
                         .ManyDelimitedBy(Token.EqualTo(ProcessToken.Delimiter),
                      Token.EqualTo(ProcessToken.CloseBracket))

                     select CreateProcess(fName.ToStringValue(), StepFactoryStore, args)).Try()
                    );


            Lazy<TokenListParser<ProcessToken, IFreezableStep>> entity =
                new Lazy<TokenListParser<ProcessToken, IFreezableStep>>(()=>
                    (from _1 in Token.EqualTo(ProcessToken.OpenBracket)
                    from args in functionMember
                        .ManyDelimitedBy(Token.EqualTo(ProcessToken.Delimiter),
                     Token.EqualTo(ProcessToken.CloseBracket))
                    select CreateEntity(args)).Try()
                   );

            var operation = mathOperation.Or(booleanOperation).Or(compareOperation);

            var bracketedOperation =
                from _1 in Token.EqualTo(ProcessToken.OpenBracket)
                from o in operation
                from _2 in Token.EqualTo(ProcessToken.CloseBracket)
                select o;


            singleTerm = new Lazy<TokenListParser<ProcessToken, IFreezableStep>>(()=>
                NumberParser
                .Or(BoolParser)
                .Or(enumConstant)
                .Or(StringConstantParser)
                .Or(GetVariableParser)
                .Or(notOperation)
                .Or(function.Value)
                .Or(entity.Value)
                .Or(bracketedOperation)
                .Or(array.Select(x=>x.ConvertToStep(false)))
                );


            freezableProcess = new Lazy<TokenListParser<ProcessToken, IFreezableStep>>(()=>
                    operation
                    .Or(setVariable)
                    .Or(singleTerm.Value) //Must come after setVariable
                    );

            stepMember = new Lazy<TokenListParser<ProcessToken, StepMember>>(()=>

                (operation
                    .Or(bracketedOperation)
                    .Or(notOperation)
                    .Or(NumberParser)
                    .Or(BoolParser)
                    .Or(enumConstant)
                    .Or(StringConstantParser)
                    .Or(setVariable)
                    //note: no getVariable here
                    .Or(Parse.Ref(() => function.Value))
                    .Or(Parse.Ref(() => entity.Value))
                    .Select(x => new StepMember(x))
                    .Or(VariableNameParser.Select(x => new StepMember(x)))
                    .Or(array)).Try()

                );


            Parser = stepMember.Value;
            // ReSharper restore AccessToModifiedClosure
        }

        /// <summary>
        /// A temporary object created when there has been some problem in parsing.
        /// </summary>
        private sealed class ParseError : IFreezableStep
        {
            public ParseError(IErrorBuilder errorBuilder) => ErrorBuilder = errorBuilder;

            public IErrorBuilder ErrorBuilder { get; }

            /// <inheritdoc />
            public Result<IStep, IError> TryFreeze(StepContext stepContext) => Result.Failure<IStep, IError>(ErrorBuilder.WithLocation(EntireSequenceLocation.Instance));

            /// <inheritdoc />
            public Result<IReadOnlyCollection<(VariableName VariableName, ITypeReference typeReference)>, IError> TryGetVariablesSet(TypeResolver typeResolver) => Result.Failure<IReadOnlyCollection<(VariableName VariableName, ITypeReference typeReference)>, IError>(ErrorBuilder.WithLocation(EntireSequenceLocation.Instance));

            /// <inheritdoc />
            public string StepName => ErrorBuilder.AsString;

            /// <inheritdoc />
            public Result<ITypeReference, IError> TryGetOutputTypeReference(TypeResolver typeResolver) => Result.Failure<ITypeReference, IError>(ErrorBuilder.WithLocation(EntireSequenceLocation.Instance));

            public bool Equals(IFreezableStep? other)
            {
                return other is ParseError pe && ErrorBuilder.Equals(pe.ErrorBuilder);
            }

            /// <inheritdoc />
            public override string ToString() => ErrorBuilder.AsString;

            /// <inheritdoc />
            public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is IFreezableStep other && Equals(other);


            /// <inheritdoc />
            public override int GetHashCode() => ErrorBuilder.GetHashCode();
        }


        private static IFreezableStep CreateEntity((string argumentName, StepMember stepMember)[] entityArguments)
        {
            var pairs = ImmutableList<KeyValuePair<string, EntityValue>>.Empty.ToBuilder();

            var errorBuilders = new List<IErrorBuilder>();

            foreach (var (argumentName, stepMember) in entityArguments)
            {
                static Result<EntityValue, IErrorBuilder> HandleVariableName(VariableName variableName)
                {
                    return new ErrorBuilder($"Cannot include a <{variableName.Name}> inside an entity because it is a variable.", ErrorCode.InvalidCast);
                }

                static Result<EntityValue, IErrorBuilder> HandleStep(IFreezableStep step) => GetSingleValueFromStep(step).Map(x => new EntityValue(x));

                static Result<EntitySingleValue, IErrorBuilder> GetSingleValueFromStep(IFreezableStep step)
                {
                    if (step is ConstantFreezableStep cfs)
                    {
                        return EntitySingleValue.Create(cfs.Value.ToString()!);
                    }
                    return new ErrorBuilder($"Cannot include '{step.StepName}' inside an entity because it is not a constant.", ErrorCode.InvalidCast);
                }

                static Result<EntityValue, IErrorBuilder> HandleStepList(IReadOnlyList<IFreezableStep> steps)
                {
                    var r = steps.Select(GetSingleValueFromStep)
                        .Combine(ErrorBuilderList.Combine)
                        .Map(x=> new EntityValue(x.ToList()));

                    return r;
                }


                var evResult = stepMember.Match(HandleVariableName, HandleStep, HandleStepList);
                if(evResult.IsFailure)
                    errorBuilders.Add(evResult.Error);
                else
                    pairs.Add(new KeyValuePair<string, EntityValue>(argumentName, evResult.Value));
            }

            if(errorBuilders.Any())
                return new ParseError(ErrorBuilderList.Combine(errorBuilders));

            var entity = new Entities.Entity(pairs.ToImmutable());

            return new ConstantFreezableStep(entity);

        }

        private static IFreezableStep CreateProcess(string funcName, StepFactoryStore factoryStore,
            (string argumentName, StepMember stepMember)[] functionArguments)
        {
            var (isSuccess, _, value, error) = TryCreateProcess2(funcName, factoryStore, functionArguments);
            return isSuccess ? value : new ParseError(error);
        }

        private static Result<IFreezableStep, IErrorBuilder> TryCreateProcess2(string funcName, StepFactoryStore factoryStore, (string argumentName, StepMember stepMember)[] functionArguments)
        {
            if (!factoryStore.Dictionary.TryGetValue(funcName, out var runnableStepFactory))
                return Result.Failure<IFreezableStep, IErrorBuilder>( ErrorHelper.MissingStepError(funcName));


            var dictionary = new Dictionary<string, StepMember>();
            var errors = new List<IErrorBuilder>();


            foreach (var argumentGroup in functionArguments.GroupBy(x=>x.argumentName))
            {
                if(argumentGroup.Count() > 1)
                    errors.Add(ErrorHelper.DuplicateParameterError(argumentGroup.Key));


                foreach (var sm in argumentGroup.Select(x=>x.stepMember))
                {
                    var cr = CheckForErrors(sm);
                    if(cr.IsFailure)
                        errors.Add(cr.Error);
                }

                dictionary.Add(argumentGroup.Key, argumentGroup.First().stepMember);
            }


            var fsd = FreezableStepData.TryCreate(runnableStepFactory, dictionary)
                .Map(x => new CompoundFreezableStep(runnableStepFactory, x, null) as IFreezableStep);

            if (fsd.IsSuccess)
            {
                if (!errors.Any())
                    return fsd;
                return Result.Failure<IFreezableStep, IErrorBuilder>(ErrorBuilderList.Combine(errors));
            }

            return Result.Failure<IFreezableStep, IErrorBuilder>(ErrorBuilderList.Combine(errors.Prepend(fsd.Error)));
        }


        private static readonly TokenListParser<ProcessToken, VariableName> VariableNameParser =
            from token in Token.EqualTo(ProcessToken.VariableName)
            select new VariableName(token.ToStringValue().TrimStart('<').TrimEnd('>'));

        private static readonly TokenListParser<ProcessToken, IFreezableStep> GetVariableParser =
            from variableName in VariableNameParser
            select GetVariableStepFactory.CreateFreezable(variableName);

        private static readonly TokenListParser<ProcessToken, IFreezableStep> StringConstantParser =
            (from token in Token.EqualTo(ProcessToken.SingleQuotedStringLiteral)
             select GetConstantStringSingleQuoted(token))
            .Or(from token in Token.EqualTo(ProcessToken.DoubleQuotedStringLiteral)
                select GetConstantStringDoubleQuoted(token));

        private static IFreezableStep GetConstantStringSingleQuoted(Token<ProcessToken> token)
        {
            var r = QuotedString.SqlStyle.Invoke(token.Span);
            return new ConstantFreezableStep(r.Value);
        }

        private static IFreezableStep GetConstantStringDoubleQuoted(Token<ProcessToken> token)
        {
            var r = QuotedString.CStyle.Invoke(token.Span);
            return new ConstantFreezableStep(r.Value);
        }


        private static readonly TokenListParser<ProcessToken, IFreezableStep> NumberParser =
            from token in Token.EqualTo(ProcessToken.Number)
            select new ConstantFreezableStep(int.Parse(token.ToStringValue())) as IFreezableStep;

        private static readonly TokenListParser<ProcessToken, IFreezableStep> BoolParser =
            from token in Token.EqualTo(ProcessToken.Boolean)
            select new ConstantFreezableStep(bool.Parse(token.ToStringValue())) as IFreezableStep;


        private static readonly Regex EnumRegex =
            new Regex(@"\A(?<enum>[a-z0-9-_]+)\.(?<value>[a-z0-9-_]+)\Z",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);


        private static TokenListParser<ProcessToken, IFreezableStep> CreateEnumParser(StepFactoryStore stepFactoryStore)
        {
            return
                Token.EqualTo(ProcessToken.Enum)
                    .Select(token => EnumRegex.Match(token.ToStringValue()))
                    .Where(x => x.Success)
                    .Select(x => TryGetEnumValue(x.Groups["enum"].Value, x.Groups["value"].Value, stepFactoryStore))
                    .Where(x => x.IsSuccess)
                    .Select(x => x.Value);
        }


        private static CSharpFunctionalExtensions.Result<IFreezableStep> TryGetEnumValue(string enumName, string value,
            StepFactoryStore stepFactoryStore)
        {
            if (stepFactoryStore.EnumTypesDictionary.TryGetValue(enumName, out var type))
                if (Enum.TryParse(type, value, true, out var enumValue))
                    return new ConstantFreezableStep(enumValue!);
            return Result.Failure<IFreezableStep>("Could not parse enum");
        }

        private static TextParser<TextSpan> GetSpan<T>(params T[] excludedValues) where T : Enum =>
            Extensions.GetEnumValues<T>()
                .Except(excludedValues)
                .SelectMany(x => new []{x.ToString(), x.GetDisplayName()})
                .OrderByDescending(x=>x.Length)
                .Select(x=> Span.EqualToIgnoreCase(x).Try())
                .Aggregate((a, b) => a.Or(b));



    }

    /// <summary>
    /// An error location that contains the relative position in a yaml string where the error occured.
    /// </summary>
    public class YamlRegionErrorLocation : IErrorLocation
    {
        /// <summary>
        /// Create a new YamlRegionErrorLocation
        /// </summary>
        public YamlRegionErrorLocation(Mark start, Mark end, Position? position = null)
        {
            if (position == null)
                Start = start;
            else
            {
                var errorLine = Math.Max(position.Value.Line - 1, 0);
                var errorColumn = Math.Max(position.Value.Column - 1, 0);
                var errorAbsolute = position.Value.Absolute;


                Start = new Mark(start.Index + errorAbsolute,
                    start.Line + errorLine,
                    start.Column + errorColumn);
            }

            End = end;
        }

        /// <summary>
        /// The beginning of the region.
        /// </summary>
        public Mark Start { get; }

        /// <summary>
        /// The end of the region.
        /// </summary>
        public Mark End { get; }

        /// <inheritdoc />
        public string AsString => $"{Start} - {End}";

        /// <inheritdoc />
        public bool Equals(IErrorLocation? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return other is YamlRegionErrorLocation y && Start.Equals(y.Start) && End.Equals(y.End);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is IErrorLocation errorLocation && Equals(errorLocation);
        }

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Start, End);
    }
}
