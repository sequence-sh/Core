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
    /// Parses strings as process members
    /// </summary>
    public class ProcessMemberParser
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


            .Match(Character.EqualTo('='), ProcessToken.Assignment, false)
            .Match(QuotedString.SqlStyle, ProcessToken.StringLiteral)
            .Match(QuotedString.CStyle, ProcessToken.StringLiteral)

            .Match(Span.EqualToIgnoreCase("true").Or(Span.EqualToIgnoreCase("false")), ProcessToken.Boolean, true)
            .Match(Span.EqualToIgnoreCase("not"), ProcessToken.NotOperator, true)
            .Match(Span.Regex(@"[0-9]+", RegexOptions.Compiled), ProcessToken.Number)
            .Match(Span.Regex(@"[a-z0-9-_]+\.[a-z0-9-_]+", RegexOptions.Compiled | RegexOptions.IgnoreCase), ProcessToken.Enum, true)

            .Match(Span.Regex("[a-z0-9-_]+", RegexOptions.Compiled | RegexOptions.IgnoreCase), ProcessToken.FuncOrArgumentName, true)
            .Build(); //TODO add wildcard



        private TokenListParser<ProcessToken, ProcessMember> Parser { get; }

        /// <summary>
        /// Tries to parse a string as a process member.
        /// </summary>
        public CSharpFunctionalExtensions. Result<ProcessMember> TryParse(string s)
        {
            var tokensResult = Tokenizer.TryTokenize(s);

            if (!tokensResult.HasValue)
                return Result.Failure<ProcessMember>(tokensResult.FormatErrorMessageFragment());

            if (!tokensResult.Remainder.IsAtEnd)
                return Result.Failure<ProcessMember>(tokensResult.FormatErrorMessageFragment());

            var parseResult = Parser.TryParse(tokensResult.Value);

            if(!parseResult.HasValue)
                return Result.Failure<ProcessMember>(parseResult.FormatErrorMessageFragment());

            if(!parseResult.Remainder.IsAtEnd)
                return Result.Failure<ProcessMember>(parseResult.FormatErrorMessageFragment());

            return parseResult.Value;
        }

        /// <summary>
        /// The process factory store
        /// </summary>
        public ProcessFactoryStore ProcessFactoryStore { get; }


        /// <summary>
        /// Create a new ProcessMemberParser
        /// </summary>
        [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
        public ProcessMemberParser(ProcessFactoryStore processFactoryStore)
        {
            ProcessFactoryStore = processFactoryStore;

            Lazy<TokenListParser<ProcessToken, ProcessMember>> processMember = null!;
            Lazy<TokenListParser<ProcessToken, IFreezableProcess>> freezableProcess = null!;

            TokenListParser<ProcessToken, IFreezableProcess> enumConstant = CreateEnumParser(processFactoryStore);

            var singleTermParser = NumberParser
                .Or(BoolParser)
                .Or(enumConstant)
                .Or(StringConstantParser)
                .Or(GetVariableParser);

            TokenListParser<ProcessToken, IFreezableProcess> setVariable =
                (from vnToken in Token.EqualTo(ProcessToken.VariableName)
                 from _ in Token.EqualTo(ProcessToken.Assignment)
                 from value in Parse.Ref(() => freezableProcess.Value)
                 select SetVariableProcessFactory.CreateFreezable(
                         new VariableName(vnToken.ToStringValue().TrimStart('<').TrimEnd('>')),
                         value)).Try();

            TokenListParser<ProcessToken, ProcessMember> array =

                (from _1 in Token.EqualTo(ProcessToken.OpenArray)
                    from _2 in Token.EqualTo(ProcessToken.CloseArray)
                    select new ProcessMember(new List<IFreezableProcess>())
                    ).Try()
                .Or
                (from _1 in Token.EqualTo(ProcessToken.OpenArray)
                from elements in Parse.Chain(Token.EqualTo(ProcessToken.Delimiter),
                    Parse.Ref(() => freezableProcess.Value).Select(x => new[]{x} as IEnumerable<IFreezableProcess>),
                    (_2,a,b)=> a.Concat(b))
                from _3 in Token.EqualTo(ProcessToken.CloseArray)
                select new ProcessMember(elements.ToList()));


            TokenListParser<ProcessToken, IFreezableProcess> notOperation =

                (from o in Token.EqualTo(ProcessToken.NotOperator)
                    from f1 in singleTermParser
                    select new CompoundFreezableProcess(NotProcessFactory.Instance,
                        new FreezableProcessData(new Dictionary<string, ProcessMember>
                        {
                            {nameof(Not.Boolean), new ProcessMember(f1)}
                        }), null) as IFreezableProcess).Try();

            TokenListParser<ProcessToken, IFreezableProcess> mathOperation =

                (from f1 in singleTermParser
                 from o in Token.EqualTo(ProcessToken.MathOperator)
                 from f2 in singleTermParser
                 select new CompoundFreezableProcess(ApplyMathOperatorProcessFactory.Instance,
                     new FreezableProcessData(new Dictionary<string, ProcessMember>()
                     {
                        {nameof(ApplyMathOperator.Left), new ProcessMember(f1)},
                        {nameof(ApplyMathOperator.Operator),
                            new ProcessMember(new ConstantFreezableProcess(Extensions.TryParseValue<MathOperator>(o.ToStringValue()).Value))}
                        ,
                        {nameof(ApplyMathOperator.Right), new ProcessMember(f2)}
                     }), null) as IFreezableProcess).Try();


            TokenListParser<ProcessToken, IFreezableProcess> booleanOperation =
                (from f1 in singleTermParser
                 from o in Token.EqualTo(ProcessToken.BooleanOperator)
                 from f2 in singleTermParser
                 select new CompoundFreezableProcess(ApplyBooleanProcessFactory.Instance,
                     new FreezableProcessData(new Dictionary<string, ProcessMember>
                     {
                        {nameof(ApplyBooleanOperator.Left), new ProcessMember(f1)},
                        {nameof(ApplyBooleanOperator.Operator),
                            new ProcessMember(new ConstantFreezableProcess(Extensions.TryParseValue<BooleanOperator>(o.ToStringValue()).Value))}
                        ,
                        {nameof(ApplyBooleanOperator.Right), new ProcessMember(f2)}
                     }), null) as IFreezableProcess).Try();

            TokenListParser<ProcessToken, IFreezableProcess> compareOperation =
                (from f1 in singleTermParser
                 from o in Token.EqualTo(ProcessToken.Comparator)
                 from f2 in singleTermParser
                 select new CompoundFreezableProcess(CompareProcessFactory.Instance,
                     new FreezableProcessData(new Dictionary<string, ProcessMember>()
                     {
                        {nameof(Compare<int>.Left), new ProcessMember(f1)},
                        {nameof(Compare<int>.Operator),
                            new ProcessMember(new ConstantFreezableProcess(Extensions.TryParseValue<CompareOperator>(o.ToStringValue()).Value))}
                        ,
                        {nameof(Compare<int>.Right), new ProcessMember(f2)}
                     }), null) as IFreezableProcess).Try();

            TokenListParser<ProcessToken, (string argumentName, ProcessMember processMember)> functionMember =
                (from a in Token.EqualTo(ProcessToken.FuncOrArgumentName)
                 from _ in Token.EqualTo(ProcessToken.Assignment)
                 from pm in Parse.Ref(() => processMember.Value)
                 select (a.ToStringValue(), pm)).Try();


            var functionArguments =

                from c in Parse.Chain(Token.EqualTo(ProcessToken.Delimiter),
                    functionMember.Select(p => new[] {p}),
                    (_2, a, b) => a.Concat(b).ToArray())
                select c;


            Lazy<TokenListParser<ProcessToken, IFreezableProcess>> function =
                new Lazy<TokenListParser<ProcessToken, IFreezableProcess>>(()=>
                    (from x in
                    (from fName in Token.EqualTo(ProcessToken.FuncOrArgumentName)
                     from _1 in Token.EqualTo(ProcessToken.OpenBracket)
                     from args in functionArguments
                     from _3 in Token.EqualTo(ProcessToken.CloseBracket)

                     select TryCreateProcess(fName.ToStringValue(), processFactoryStore, args))
                     where x.IsSuccess
                     select x.Value).Try()
                    );


            freezableProcess = new Lazy<TokenListParser<ProcessToken, IFreezableProcess>>(()=>

                        mathOperation
                    .Or(booleanOperation)
                    .Or(compareOperation)
                    .Or(notOperation)
                    .Or(setVariable)
                    .Or(singleTermParser) //Must come after setVariable
                .Or(Parse.Ref(()=>function.Value))
                    //.Or(Parse.Ref(()=>array.Value))
                    );

            processMember = new Lazy<TokenListParser<ProcessToken, ProcessMember>>(()=>

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
                    .Select(x=> new ProcessMember(x))
                    .Or(VariableNameParser.Select(x=> new ProcessMember(x)))
                    .Or(array)
                );


            Parser = processMember.Value;
        }


        private static CSharpFunctionalExtensions.Result<IFreezableProcess> TryCreateProcess(string funcName, ProcessFactoryStore factoryStore, (string argumentName, ProcessMember processMember)[] functionArguments)
        {
            if (!factoryStore.Dictionary.TryGetValue(funcName, out var runnableProcessFactory))
                return Result.Failure<IFreezableProcess>($"Could not find process '{funcName}'");


            var dictionary = new Dictionary<string, ProcessMember>();

            foreach (var (argumentName, processMember) in functionArguments)
            {
                var memberType = runnableProcessFactory.GetExpectedMemberType(argumentName);

                var convertResult = processMember.TryConvert(memberType);

                if (convertResult.IsFailure)
                    return convertResult.ConvertFailure<IFreezableProcess>();

                dictionary.Add(argumentName, convertResult.Value);
            }

            var process = new CompoundFreezableProcess(runnableProcessFactory,
                new FreezableProcessData(dictionary), null);


            return process;
        }


        private static readonly TokenListParser<ProcessToken, VariableName> VariableNameParser =
            from token in Token.EqualTo(ProcessToken.VariableName)
            select new VariableName(token.ToStringValue().TrimStart('<').TrimEnd('>'));

        private static readonly TokenListParser<ProcessToken, IFreezableProcess> GetVariableParser =
            from variableName in VariableNameParser
            select GetVariableProcessFactory.CreateFreezable(variableName);

        private static readonly TokenListParser<ProcessToken, IFreezableProcess> StringConstantParser =
            from token in Token.EqualTo(ProcessToken.StringLiteral)
            select new ConstantFreezableProcess(token.ToStringValue()[1..^1] ) as IFreezableProcess;

        private static readonly TokenListParser<ProcessToken, IFreezableProcess> NumberParser =
            from token in Token.EqualTo(ProcessToken.Number)
            select new ConstantFreezableProcess(int.Parse(token.ToStringValue())) as IFreezableProcess;

        private static readonly TokenListParser<ProcessToken, IFreezableProcess> BoolParser =
            from token in Token.EqualTo(ProcessToken.Boolean)
            select new ConstantFreezableProcess(bool.Parse(token.ToStringValue())) as IFreezableProcess;


        private static readonly Regex EnumRegex =
            new Regex(@"\A(?<enum>[a-z0-9-_]+)\.(?<value>[a-z0-9-_]+)\Z",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);


        private static TokenListParser<ProcessToken, IFreezableProcess> CreateEnumParser(ProcessFactoryStore processFactoryStore)
        {
            return
                Token.EqualTo(ProcessToken.Enum)
                    .Select(token => EnumRegex.Match(token.ToStringValue()))
                    .Where(x => x.Success)
                    .Select(x => TryGetEnumValue(x.Groups["enum"].Value, x.Groups["value"].Value, processFactoryStore))
                    .Where(x => x.IsSuccess)
                    .Select(x => x.Value);
        }


        private static CSharpFunctionalExtensions.Result<IFreezableProcess> TryGetEnumValue(string enumName, string value,
            ProcessFactoryStore processFactoryStore)
        {
            if (processFactoryStore.EnumTypesDictionary.TryGetValue(enumName, out var type))
                if (Enum.TryParse(type, value, true, out var enumValue))
                    return new ConstantFreezableProcess(enumValue!);
            return Result.Failure<IFreezableProcess>("Could not parse enum");
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
