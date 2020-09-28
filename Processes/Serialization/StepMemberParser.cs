using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using Reductech.EDR.Processes.General;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;
using Superpower;
using Superpower.Display;
using Superpower.Model;
using Superpower.Parsers;
using Superpower.Tokenizers;
using Result = CSharpFunctionalExtensions.Result;

namespace Reductech.EDR.Processes.Serialization
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
            [Token(Example = "<Path>")]
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
            //[Token(Example = ":", Description = "Separates an argument name and an argument value")]
            //ArgumentSeparator,
            [Token(Example = "=")]
            Assignment,
            [Token(Example = "+")]
            MathOperator,
            [Token(Example = "&&")]
            BooleanOperator,
            [Token(Example = "==")]
            Comparator,
            [Token(Example = "'Hello World'")]
            StringLiteral,
            [Token(Example = "123")]
            Number,
            [Token(Example = "true")]
            Boolean,
            [Token(Example = "MathOperator.And")]
            Enum,
            [Token(Example = "WriteFile")]
            FuncOrArgumentName,
            [Token(Example = "Not")]
            NotOperator
        }


        private static readonly Tokenizer<ProcessToken> Tokenizer = new TokenizerBuilder<ProcessToken>()
            .Ignore(Span.WhiteSpace)


            .Match(Character.EqualTo('('), ProcessToken.OpenBracket)
            .Match(Character.EqualTo(')'), ProcessToken.CloseBracket)
            .Match(Character.EqualTo('['), ProcessToken.OpenArray)
            .Match(Character.EqualTo(']'), ProcessToken.CloseArray)
            .Match(Character.EqualTo(','), ProcessToken.Delimiter)

            //VariableName must be before comparator
            .Match(Span.Regex("<[a-z0-9-_]+>", RegexOptions.Compiled | RegexOptions.IgnoreCase), ProcessToken.VariableName)

            .Match(GetSpan(MathOperator.None), ProcessToken.MathOperator, true)
            .Match(GetSpan(BooleanOperator.None), ProcessToken.BooleanOperator, true)
            .Match(GetSpan(CompareOperator.None), ProcessToken.Comparator, true)


            .Match(Character.EqualTo('='), ProcessToken.Assignment)
            .Match(QuotedString.SqlStyle, ProcessToken.StringLiteral)
            .Match(QuotedString.CStyle, ProcessToken.StringLiteral)

            .Match(Span.EqualToIgnoreCase("true").Or(Span.EqualToIgnoreCase("false")), ProcessToken.Boolean, true)
            .Match(Span.EqualToIgnoreCase("not"), ProcessToken.NotOperator, true)
            .Match(Span.Regex(@"[0-9]+", RegexOptions.Compiled), ProcessToken.Number)
            .Match(Span.Regex(@"[a-z0-9-_]+\.[a-z0-9-_]+", RegexOptions.Compiled | RegexOptions.IgnoreCase), ProcessToken.Enum, true)

            .Match(Span.Regex("[a-z0-9-_]+", RegexOptions.Compiled | RegexOptions.IgnoreCase), ProcessToken.FuncOrArgumentName, true)
            .Build(); //TODO add wildcard



        private TokenListParser<ProcessToken, StepMember> Parser { get; }

        /// <summary>
        /// Tries to parse a string as a step member.
        /// </summary>
        public CSharpFunctionalExtensions. Result<StepMember> TryParse(string s)
        {
            var tokensResult = Tokenizer.TryTokenize(s);

            if (!tokensResult.HasValue)
                return Result.Failure<StepMember>(tokensResult.FormatErrorMessageFragment());

            if (!tokensResult.Remainder.IsAtEnd)
                return Result.Failure<StepMember>(tokensResult.FormatErrorMessageFragment());

            var parseResult = Parser.TryParse(tokensResult.Value);

            if(!parseResult.HasValue)
                return Result.Failure<StepMember>(parseResult.FormatErrorMessageFragment());

            if(!parseResult.Remainder.IsAtEnd)
                return Result.Failure<StepMember>(parseResult.FormatErrorMessageFragment());

            return parseResult.Value;
        }

        /// <summary>
        /// The step factory store
        /// </summary>
        public StepFactoryStore StepFactoryStore { get; }


        /// <summary>
        /// Create a new StepMemberParser
        /// </summary>
        [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
        public StepMemberParser(StepFactoryStore stepFactoryStore)
        {
            StepFactoryStore = stepFactoryStore;

            Lazy<TokenListParser<ProcessToken, StepMember>> processMember = null!;
            Lazy<TokenListParser<ProcessToken, IFreezableStep>> freezableProcess = null!;

            TokenListParser<ProcessToken, IFreezableStep> enumConstant = CreateEnumParser(StepFactoryStore);

            var singleTermParser = NumberParser
                .Or(BoolParser)
                .Or(enumConstant)
                .Or(StringConstantParser)
                .Or(GetVariableParser);


            Lazy<TokenListParser<ProcessToken, IFreezableStep>> stp = null!;

            TokenListParser<ProcessToken, IFreezableStep> setVariable =
                (from vnToken in Token.EqualTo(ProcessToken.VariableName)
                 from _ in Token.EqualTo(ProcessToken.Assignment)
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
                    from f1 in Parse.Ref(()=> stp.Value)
                    from _2 in Token.EqualTo(ProcessToken.CloseBracket)
                    select new CompoundFreezableStep(NotStepFactory.Instance,
                        new FreezableStepData(new Dictionary<string, StepMember>
                        {
                            {nameof(Not.Boolean), new StepMember(f1)}
                        }), null) as IFreezableStep).Try();

            TokenListParser<ProcessToken, IFreezableStep> mathOperation =

                (from f1 in Parse.Ref(()=> stp.Value)
                 from o in Token.EqualTo(ProcessToken.MathOperator)
                 from f2 in Parse.Ref(()=> stp.Value)
                 select new CompoundFreezableStep(ApplyMathOperatorStepFactory.Instance,
                     new FreezableStepData(new Dictionary<string, StepMember>()
                     {
                        {nameof(ApplyMathOperator.Left), new StepMember(f1)},
                        {nameof(ApplyMathOperator.Operator),
                            new StepMember(new ConstantFreezableStep(Extensions.TryParseValue<MathOperator>(o.ToStringValue()).Value))}
                        ,
                        {nameof(ApplyMathOperator.Right), new StepMember(f2)}
                     }), null) as IFreezableStep).Try();


            TokenListParser<ProcessToken, IFreezableStep> booleanOperation =
                (from f1 in Parse.Ref(()=> stp.Value)
                 from o in Token.EqualTo(ProcessToken.BooleanOperator)
                 from f2 in Parse.Ref(()=> stp.Value)
                 select new CompoundFreezableStep(ApplyBooleanStepFactory.Instance,
                     new FreezableStepData(new Dictionary<string, StepMember>
                     {
                        {nameof(ApplyBooleanOperator.Left), new StepMember(f1)},
                        {nameof(ApplyBooleanOperator.Operator),
                            new StepMember(new ConstantFreezableStep(Extensions.TryParseValue<BooleanOperator>(o.ToStringValue()).Value))}
                        ,
                        {nameof(ApplyBooleanOperator.Right), new StepMember(f2)}
                     }), null) as IFreezableStep).Try();

            TokenListParser<ProcessToken, IFreezableStep> compareOperation =
                (from f1 in Parse.Ref(()=> stp.Value)
                 from o in Token.EqualTo(ProcessToken.Comparator)
                 from f2 in Parse.Ref(()=> stp.Value)
                 select new CompoundFreezableStep(CompareStepFactory.Instance,
                     new FreezableStepData(new Dictionary<string, StepMember>()
                     {
                        {nameof(Compare<int>.Left), new StepMember(f1)},
                        {nameof(Compare<int>.Operator),
                            new StepMember(new ConstantFreezableStep(Extensions.TryParseValue<CompareOperator>(o.ToStringValue()).Value))}
                        ,
                        {nameof(Compare<int>.Right), new StepMember(f2)}
                     }), null) as IFreezableStep).Try();

            TokenListParser<ProcessToken, (string argumentName, StepMember processMember)> functionMember =
                (from a in Token.EqualTo(ProcessToken.FuncOrArgumentName)
                 from _ in Token.EqualTo(ProcessToken.Assignment)
                 from pm in Parse.Ref(() => processMember.Value)
                 select (a.ToStringValue(), pm)).Try();

            Lazy<TokenListParser<ProcessToken, IFreezableStep>> function =
                new Lazy<TokenListParser<ProcessToken, IFreezableStep>>(()=>
                    (from x in
                    (from fName in Token.EqualTo(ProcessToken.FuncOrArgumentName)
                     from _1 in Token.EqualTo(ProcessToken.OpenBracket)
                     from args in functionMember
                         .ManyDelimitedBy(Token.EqualTo(ProcessToken.Delimiter),
                      Token.EqualTo(ProcessToken.CloseBracket))

                     select TryCreateProcess(fName.ToStringValue(), StepFactoryStore, args))
                     where x.IsSuccess
                     select x.Value).Try()
                    );


            stp = new Lazy<TokenListParser<ProcessToken, IFreezableStep>>(()=>
                singleTermParser
                    .Or(notOperation)
                .Or(Parse.Ref(()=>function.Value))
                );


            freezableProcess = new Lazy<TokenListParser<ProcessToken, IFreezableStep>>(()=>

                        mathOperation
                    .Or(booleanOperation)
                    .Or(compareOperation)
                    .Or(notOperation)
                    .Or(setVariable)
                    .Or(singleTermParser) //Must come after setVariable
                .Or(Parse.Ref(()=>function.Value))
                    //.Or(Parse.Ref(()=>array.Value))
                    );

            processMember = new Lazy<TokenListParser<ProcessToken, StepMember>>(()=>

                            mathOperation
                    .Or(booleanOperation)
                    .Or(compareOperation)
                    .Or(notOperation)
                    .Or(NumberParser)
                    .Or(BoolParser)
                    .Or(enumConstant)
                    .Or(StringConstantParser)
                    .Or(setVariable)
                    //note: no getVariable here
                    .Or(Parse.Ref(() => function.Value))
                    .Select(x=> new StepMember(x))
                    .Or(VariableNameParser.Select(x=> new StepMember(x)))
                    .Or(array)
                );


            Parser = processMember.Value;
        }


        private static CSharpFunctionalExtensions.Result<IFreezableStep> TryCreateProcess(string funcName, StepFactoryStore factoryStore, (string argumentName, StepMember processMember)[] functionArguments)
        {
            if (!factoryStore.Dictionary.TryGetValue(funcName, out var runnableStepFactory))
                return Result.Failure<IFreezableStep>($"Could not find step '{funcName}'");


            var dictionary = new Dictionary<string, StepMember>();

            foreach (var (argumentName, processMember) in functionArguments)
            {
                var memberType = runnableStepFactory.GetExpectedMemberType(argumentName);

                var convertResult = processMember.TryConvert(memberType);

                if (convertResult.IsFailure)
                    return convertResult.ConvertFailure<IFreezableStep>();

                dictionary.Add(argumentName, convertResult.Value);
            }

            var process = new CompoundFreezableStep(runnableStepFactory,
                new FreezableStepData(dictionary), null);


            return process;
        }


        private static readonly TokenListParser<ProcessToken, VariableName> VariableNameParser =
            from token in Token.EqualTo(ProcessToken.VariableName)
            select new VariableName(token.ToStringValue().TrimStart('<').TrimEnd('>'));

        private static readonly TokenListParser<ProcessToken, IFreezableStep> GetVariableParser =
            from variableName in VariableNameParser
            select GetVariableStepFactory.CreateFreezable(variableName);

        private static readonly TokenListParser<ProcessToken, IFreezableStep> StringConstantParser =
            from token in Token.EqualTo(ProcessToken.StringLiteral)
            select new ConstantFreezableStep(token.ToStringValue()[1..^1] ) as IFreezableStep;

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
}
