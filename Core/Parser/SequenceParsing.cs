using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using CSharpFunctionalExtensions;
using OneOf;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Util;

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

            var result = visitor.Visit(parser.sequence());

            return result;
        }


        private class Visitor : SequenceBaseVisitor<Result<FreezableStepProperty, IError>>
        {
            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitSequence(SequenceParser.SequenceContext context)
            {
                var results = new List<Result<IFreezableStep, IError>>();

                foreach (var child in context.children)
                {
                    if (child is SequenceParser.StepContext step)
                        results.Add(VisitStep(step).Map(x => x.ConvertToStep()));
                    else switch (child)
                    {
                        case null:
                            results.Add(new SingleError("Step was null", ErrorCode.CouldNotParse, new TextPosition(context)));
                            break;
                        case ErrorNodeImpl errorNodeImpl:
                            results.Add(VisitErrorNode(errorNodeImpl).Map(x=>x.ConvertToStep()));
                            break;
                        case TerminalNodeImpl _:
                            //Skip eof
                            break;
                        case ParserRuleContext prc:
                            results.Add(ParseError(prc));
                            break;
                        default:
                            throw new Exception($"Could not handle '{child}'");
                    }
                }


                var result = results.Combine(ErrorList.Combine).Map(x=>x.ToList());

                if (result.IsFailure) return result.ConvertFailure<FreezableStepProperty>();


                if (result.Value.Count == 0)
                    return new SingleError("Sequence contained no members", ErrorCode.CouldNotParse, new TextPosition(context));

                var sequence = SequenceStepFactory.CreateFreezable(
                    result.Value.SkipLast(1).ToList(),
                    result.Value.Last(),
                    null, new TextPosition(context));

                return new FreezableStepProperty(
                    OneOf<VariableName, IFreezableStep, IReadOnlyList<IFreezableStep>>.FromT1(sequence),
                    new TextPosition(context));
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
            public override Result<FreezableStepProperty, IError> VisitBool(SequenceParser.BoolContext context)
            {
                var b = context.TRUE() != null;

                var member = new FreezableStepProperty(new ConstantFreezableStep(b), new TextPosition(context));
                return member;

            }

            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitBracketedstep(SequenceParser.BracketedstepContext context) => VisitStep(context.step());

            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitInfixoperation(SequenceParser.InfixoperationContext context)
            {

                var left = VisitTerm(context.term(0));
                var right = VisitTerm(context.term(1));
                var operatorSymbol = context.infixoperator().GetText();

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
            public override Result<FreezableStepProperty, IError> VisitSetvariable(SequenceParser.SetvariableContext context)
            {
                var member = VisitTerm(context.term());

                if (member.IsFailure) return member;

                var vn = GetVariableName(context.VARIABLENAME());


                var stepData = new FreezableStepData(new Dictionary<string, FreezableStepProperty>()
                {
                    {nameof(SetVariable<object>.Variable), vn },
                    {nameof(SetVariable<object>.Value), member.Value },

                }, new TextPosition(context));


                var step = new CompoundFreezableStep(SetVariableStepFactory.Instance.TypeName, stepData, null);

                return new FreezableStepProperty(OneOf<VariableName, IFreezableStep, IReadOnlyList<IFreezableStep>>.FromT1(step), new TextPosition(context) );
            }

            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitGetvariable(SequenceParser.GetvariableContext context)
            {
                var vn = GetVariableName(context.VARIABLENAME());

                var stepData = new FreezableStepData(new Dictionary<string, FreezableStepProperty>()
                {
                    {nameof(GetVariable<object>.Variable), vn }

                }, new TextPosition(context) );


                var step = new CompoundFreezableStep(GetVariableStepFactory.Instance.TypeName, stepData, null);


                return new FreezableStepProperty(OneOf<VariableName, IFreezableStep, IReadOnlyList<IFreezableStep>>.FromT1(step), new TextPosition(context) );
            }

            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitEnum(SequenceParser.EnumContext context)
            {
                if (context.children.Count != 2 || context.TOKEN().Length != 2)
                    return ParseError(context);

                var prefix = context.TOKEN(0).GetText();
                var suffix = context.TOKEN(1).GetText();

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
            public override Result<FreezableStepProperty, IError> VisitString(SequenceParser.StringContext context)
            {
                string s = context.DOUBLEQUOTEDSTRING() != null ?
                    UnescapeDoubleQuoted(context.DOUBLEQUOTEDSTRING().GetText()):
                    UnescapeSingleQuoted(context.SINGLEQUOTEDSTRING().GetText());

                var member = new FreezableStepProperty(new ConstantFreezableStep(s), new TextPosition(context) );

                return member;

                static string UnescapeDoubleQuoted(string s)
                {
                    s = s[1..^1]; //Remove quotes
                    s = s
                        .Replace(@"\\", "\\")
                        .Replace(@"\""", "\"")
                        .Replace(@"\r", "\r")
                        .Replace(@"\n", "\n");
                    return s;
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
                var name = context.TOKEN().Symbol.Text;
                var members = AggregateFunctionMembers(context.functionmember());

                if (members.IsFailure) return members.ConvertFailure<FreezableStepProperty>();


                var fsd = new FreezableStepData(members.Value, new TextPosition(context) );

                var cfs = new CompoundFreezableStep(name, fsd, null);

                return new FreezableStepProperty(OneOf<VariableName, IFreezableStep, IReadOnlyList<IFreezableStep>>.FromT1(cfs),new TextPosition(context) );
            }

            /// <inheritdoc />
            public override Result<FreezableStepProperty, IError> VisitEntity(SequenceParser.EntityContext context)
            {
                var members = AggregateFunctionMembers(context.functionmember());

                if (members.IsFailure) return members.ConvertFailure<FreezableStepProperty>();

                var step = new CreateEntityFreezableStep(new FreezableStepData(members.Value, new TextPosition(context)));

                return new FreezableStepProperty(OneOf<VariableName, IFreezableStep, IReadOnlyList<IFreezableStep>>.FromT1(step), new TextPosition(context));
            }

            private Result<IReadOnlyDictionary<string, FreezableStepProperty>, IError>
                AggregateFunctionMembers(
                    IEnumerable<SequenceParser.FunctionmemberContext> functionMembers)
            {
                var l = new List<(string key, FreezableStepProperty member)>();
                var errors = new List<IError>();

                foreach (var r in functionMembers.Select(GetFunctionMember))
                {
                    if(r.IsFailure)errors.Add(r.Error);
                    else
                        l.Add(r.Value);
                }

                foreach (var duplicateKeys in l.GroupBy(x=>x.key).Where(x=>x.Count() > 1))
                {
                    errors.Add(new SingleError(
                        $"Duplicate Parameter '{duplicateKeys.Key}'",
                        ErrorCode.DuplicateParameter,
                            new TextPosition(duplicateKeys.Key,
                                (duplicateKeys.First().member.Location as TextPosition)!.StartIndex,
                                (duplicateKeys.Last().member.Location  as TextPosition)!.StopIndex
                            )));
                }

                if (errors.Any())
                    return Result.Failure<IReadOnlyDictionary<string, FreezableStepProperty>, IError>(ErrorList.Combine(errors));

                var dict = l.ToDictionary(x => x.key, x => x.member);

                return dict;
            }

            private Result<(string name, FreezableStepProperty value), IError> GetFunctionMember(
                SequenceParser.FunctionmemberContext context)
            {
                var key = context.TOKEN().Symbol.Text;

                var value = VisitStep(context.step());
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
                var l = new List<IFreezableStep>();
                var errors = new List<IError>();

                foreach (var node in nodes)
                {
                    var result = node.Map(x => x.ConvertToStep());

                    if (result.IsSuccess) l.Add(result.Value);
                    else errors.Add(result.Error);
                }

                if (errors.Any())
                    return Result.Failure<FreezableStepProperty, IError>(ErrorList.Combine(errors));

                return new FreezableStepProperty(l, textPosition);
            }

        }

        private static class InfixHelper
        {
            private class OperatorData
            {
                public OperatorData(string stepName, string leftName, string rightName, string operatorStepName,
                    IFreezableStep operatorStep)
                {
                    LeftName = leftName;
                    RightName = rightName;
                    OperatorStepName = operatorStepName;
                    OperatorStep = operatorStep;
                    StepName = stepName;
                }

                public string StepName { get; }

                public string LeftName { get; }

                public string RightName { get; }

                public string OperatorStepName { get; }

                public IFreezableStep OperatorStep { get; }
            }


            public static Result<FreezableStepProperty, IError> TryCreateStep(
                IErrorLocation errorLocation,
                Result<FreezableStepProperty, IError> left,
                Result<FreezableStepProperty, IError> right, string op)
            {

                List<IError> errors = new List<IError>();

                if (left.IsFailure) errors.Add(left.Error);
                if (right.IsFailure) errors.Add(right.Error);

                if (!OperatorDataDictionary.TryGetValue(op, out var opData))
                    errors.Add(new SingleError($"Operator '{op}' is not defined", ErrorCode.CouldNotParse, errorLocation));


                if (errors.Any())
                    return Result.Failure<FreezableStepProperty, IError>(ErrorList.Combine(errors));

                var data = new FreezableStepData(new Dictionary<string, FreezableStepProperty>
            {
                {opData!.OperatorStepName, new FreezableStepProperty(OneOf<VariableName, IFreezableStep, IReadOnlyList<IFreezableStep>>.FromT1(opData.OperatorStep), errorLocation )},
                {opData.LeftName, left.Value},
                {opData.RightName, right.Value},

            }, errorLocation);

                var step = new CompoundFreezableStep(opData.StepName, data, null);

                return new FreezableStepProperty(OneOf<VariableName, IFreezableStep, IReadOnlyList<IFreezableStep>>.FromT1(step), errorLocation);

            }



            private static readonly IReadOnlyDictionary<string, OperatorData> OperatorDataDictionary =
                Enum.GetValues<MathOperator>()

                    .ToDictionary(mo => mo.GetDisplayName(), mo => new OperatorData(
                        nameof(ApplyMathOperator),
                        nameof(ApplyMathOperator.Left),
                        nameof(ApplyMathOperator.Right),
                        nameof(ApplyMathOperator.Operator),
                        new ConstantFreezableStep(new Enumeration(nameof(MathOperator), mo.ToString()))
                    ))
                    .Concat(
                        Enum.GetValues<BooleanOperator>()
                    .ToDictionary(mo => mo.GetDisplayName(), mo => new OperatorData(
                        nameof(ApplyBooleanOperator),
                        nameof(ApplyBooleanOperator.Left),
                        nameof(ApplyBooleanOperator.Right),
                        nameof(ApplyBooleanOperator.Operator),
                        new ConstantFreezableStep(new Enumeration(nameof(BooleanOperator), mo.ToString()))
                    ))
                        )
                    .Concat(
                        Enum.GetValues<CompareOperator>()
                    .ToDictionary(mo => mo.GetDisplayName(), mo => new OperatorData(
                        CompareStepFactory.Instance.TypeName,
                        nameof(Compare<int>.Left),
                        nameof(Compare<int>.Right),
                        nameof(Compare<int>.Operator),
                        new ConstantFreezableStep(new Enumeration(nameof(CompareOperator), mo.ToString()))
                    ))
                        )
                    .Where(x=>x.Key != MathOperator.None.ToString())

                    .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);


        }

    }


    /*
    /// <summary>
    /// A node representing an infix operation
    /// </summary>
    public class InfixOperatorNode : INode
    {
        public InfixOperatorNode(IErrorLocation location, INode left, string @operator, INode right)
        {
            Left = left;
            Operator = @operator;
            Right = right;
            ErrorLocation = location;
        }

        /// <inheritdoc />
        public string StepName => $"{Left.StepName} {Operator} {Right.StepName}";

        /// <inheritdoc />
        public override string ToString() => StepName;

        /// <inheritdoc />
        public IErrorLocation ErrorLocation { get; }

        /// <summary>
        /// The left operand
        /// </summary>
        public INode Left { get; }

        /// <summary>
        /// The operator
        /// </summary>
        public string Operator { get; }

        /// <summary>
        /// The right operand
        /// </summary>
        public INode Right { get; }


        /// <inheritdoc />
        public Result<IStep, IError> TryFreeze(StepFactoryStore stepFactoryStore, StepContext stepContext)
        {
            var errors = new List<IError>();
            (IStepFactory factory, FreezeData freezeData)? stuff;

            if (MathOperatorDictionary.TryGetValue(Operator, out var mathOperator))
            {
                var left = Left.TryFreeze(stepFactoryStore, stepContext);
                var right = Right.TryFreeze(stepFactoryStore, stepContext);

                if (left.IsFailure) errors.Add(left.Error);
                if(right.IsFailure) errors.Add(right.Error);
                if (errors.Any())
                    stuff = null;
                else
                {
                    var freezeData = new FreezeData(ErrorLocation,
                        new Dictionary<string, OneOf<VariableName, IStep, IReadOnlyList<IStep>>>()
                        {
                            {nameof(ApplyMathOperator.Left), OneOf<VariableName, IStep, IReadOnlyList<IStep>>.FromT1(left.Value)},
                            {nameof(ApplyMathOperator.Right), OneOf<VariableName, IStep, IReadOnlyList<IStep>>.FromT1(right.Value)},
                            {nameof(ApplyMathOperator.Operator), OneOf<VariableName, IStep, IReadOnlyList<IStep>>.FromT1(new Constant<MathOperator>(mathOperator))},
                        });
                    stuff = (ApplyMathOperatorStepFactory.Instance, freezeData);
                }
            }
            else if (BoolOperatorDictionary.TryGetValue(Operator, out var booleanOperator))
            {
                var left = Left.TryFreeze(stepFactoryStore, stepContext);
                var right = Right.TryFreeze(stepFactoryStore, stepContext);

                if (left.IsFailure) errors.Add(left.Error);
                if (right.IsFailure) errors.Add(right.Error);
                if (errors.Any())
                    stuff = null;
                else
                {
                    var freezeData = new FreezeData(ErrorLocation,
                        new Dictionary<string, OneOf<VariableName, IStep, IReadOnlyList<IStep>>>()
                        {
                            {nameof(ApplyBooleanOperator.Left), OneOf<VariableName, IStep, IReadOnlyList<IStep>>.FromT1(left.Value)},
                            {nameof(ApplyBooleanOperator.Right), OneOf<VariableName, IStep, IReadOnlyList<IStep>>.FromT1(right.Value)},
                            {nameof(ApplyBooleanOperator.Operator), OneOf<VariableName, IStep, IReadOnlyList<IStep>>.FromT1(new Constant<BooleanOperator>(booleanOperator))},
                        });
                    stuff = (ApplyBooleanOperatorStepFactory.Instance, freezeData);
                }
            }

            else if (CompareOperatorDictionary.TryGetValue(Operator, out var compareOperator))
            {
                var left = Left.TryFreeze(stepFactoryStore, stepContext);
                var right = Right.TryFreeze(stepFactoryStore, stepContext);

                if (left.IsFailure) errors.Add(left.Error);
                if (right.IsFailure) errors.Add(right.Error);
                if (errors.Any())
                    stuff = null;
                else
                {
                    var freezeData = new FreezeData(ErrorLocation,
                        new Dictionary<string, OneOf<VariableName, IStep, IReadOnlyList<IStep>>>()
                        {
                            {nameof(Compare<int>.Left), OneOf<VariableName, IStep, IReadOnlyList<IStep>>.FromT1(left.Value)},
                            {nameof(Compare<int>.Right), OneOf<VariableName, IStep, IReadOnlyList<IStep>>.FromT1(right.Value)},
                            {nameof(Compare<int>.Operator), OneOf<VariableName, IStep, IReadOnlyList<IStep>>.FromT1(new Constant<CompareOperator>(compareOperator))},
                        });
                    stuff = (ApplyMathOperatorStepFactory.Instance, freezeData);
                }
            }
            else
                return new SingleError($"Operator '{Operator}' is not defined", ErrorCode.CouldNotParse, ErrorLocation);

            if(stuff == null)
                return Result.Failure<IStep, IError>(ErrorList.Combine(errors));

            var finalResult = stuff.Value.factory.TryFreeze(stepContext, stuff.Value.freezeData, null);

            return finalResult;
        }




        private static readonly IReadOnlyDictionary<string, ITypeReference> OutputTypeDictionary =
            Enum.GetValues<MathOperator>()
                .Select(mo => (mo.GetDisplayName(), new ActualTypeReference(typeof(int)) as ITypeReference))

                .Concat(
                    Enum.GetValues<BooleanOperator>()
                        .Select(mo => (mo.GetDisplayName(), new ActualTypeReference(typeof(bool)) as ITypeReference))
                ).Concat(
                    Enum.GetValues<CompareOperator>()
                        .Select(mo => (mo.GetDisplayName(), new ActualTypeReference(typeof(bool)) as ITypeReference))
                )
                .ToDictionary(x => x.Item1, x => x.Item2);




        /// <inheritdoc />
        public Result<IReadOnlyCollection<(VariableName VariableName, ITypeReference typeReference)>, IError> TryGetVariablesSet(
            StepFactoryStore stepFactoryStore, TypeResolver typeResolver)
        {
            var r1 = Left.TryGetVariablesSet(stepFactoryStore, typeResolver);
            var r2 = Right.TryGetVariablesSet(stepFactoryStore, typeResolver);

            var errors = new List<IError>();
            if(r1.IsFailure) errors.Add(r1.Error);
            if(r2.IsFailure) errors.Add(r2.Error);

            if (errors.Any())
                return Result.Failure<IReadOnlyCollection<(VariableName VariableName, ITypeReference typeReference)>, IError>(
                    ErrorList.Combine(errors));

            return r1.Value.Concat(r2.Value).ToList();
        }

        /// <inheritdoc />
        public Result<ITypeReference, IError> TryGetOutputTypeReference(StepFactoryStore stepFactoryStore, TypeResolver typeResolver)
        {
            if (OutputTypeDictionary.TryGetValue(Operator, out var typeReference))
                return Result.Success<ITypeReference, IError>(typeReference);

            return new SingleError($"Operator '{Operator}' is not defined", ErrorCode.CouldNotParse,  ErrorLocation);
        }
    }

    */

    //public class SetVariableNode : INode
    //{
    //    public SetVariableNode(IErrorLocation location, VariableName variableName, INode newValue)
    //    {
    //        VariableName = variableName;
    //        NewValue = newValue;
    //        ErrorLocation = location;
    //    }


    //    public VariableName VariableName { get; }

    //    public INode NewValue { get; }

    //    /// <inheritdoc />
    //    public string StepName => $"{VariableName} = {NewValue.StepName}";

    //    /// <inheritdoc />
    //    public IErrorLocation ErrorLocation { get; }

    //    /// <inheritdoc />
    //    public Result<IStep, IError> TryFreeze(StepFactoryStore stepFactoryStore, StepContext stepContext)
    //    {
    //        var step = NewValue.TryFreeze(stepFactoryStore, stepContext);

    //        if (step.IsFailure) return step.ConvertFailure<IStep>();

    //        var freezeData = new FreezeData(ErrorLocation,
    //            new Dictionary<string, OneOf<VariableName, IStep, IReadOnlyList<IStep>>>()
    //            {
    //                {nameof(SetVariable<object>.Variable), VariableName},
    //                {nameof(SetVariable<object>.Value), OneOf<VariableName, IStep, IReadOnlyList<IStep>>.FromT1(step.Value)},
    //            });

    //        var r = SetVariableStepFactory.Instance.TryFreeze(stepContext, freezeData, null);
    //        return r;
    //    }

    //    /// <inheritdoc />
    //    public Result<IReadOnlyCollection<(VariableName VariableName, ITypeReference typeReference)>, IError> TryGetVariablesSet(StepFactoryStore stepFactoryStore, TypeResolver typeResolver)
    //    {
    //        var r =NewValue.TryGetOutputTypeReference(stepFactoryStore, typeResolver);

    //        if (r.IsFailure) return r.ConvertFailure<IReadOnlyCollection<(VariableName VariableName, ITypeReference typeReference)>>();

    //        return  new List<(VariableName VariableName, ITypeReference typeReference)>(){(VariableName, r.Value)};
    //    }

    //    /// <inheritdoc />
    //    public Result<ITypeReference, IError> TryGetOutputTypeReference(StepFactoryStore stepFactoryStore, TypeResolver typeResolver)
    //    {
    //        return new ActualTypeReference(typeof(Unit));
    //    }
    //}

    //public class GetVariableNode : INode
    //{
    //    public GetVariableNode(IErrorLocation errorLocation, VariableName variableName)
    //    {
    //        ErrorLocation = errorLocation;
    //        VariableName = variableName;
    //    }

    //    /// <inheritdoc />
    //    public string StepName => VariableName.ToString();

    //    /// <inheritdoc />
    //    public IErrorLocation ErrorLocation { get; }

    //    /// <summary>
    //    /// The variable to get.
    //    /// </summary>
    //    public VariableName VariableName { get; }

    //    /// <inheritdoc />
    //    public Result<IStep, IError> TryFreeze(StepFactoryStore stepFactoryStore, StepContext stepContext)
    //    {
    //        var fd = new FreezeData(ErrorLocation,
    //            new Dictionary<string, OneOf<VariableName, IStep, IReadOnlyList<IStep>>>()
    //            {
    //                {nameof(GetVariable<object>.Variable), VariableName}
    //            });

    //        return GetVariableStepFactory.Instance.TryFreeze(stepContext, fd, null);
    //    }

    //    /// <inheritdoc />
    //    ///
    //    public Result<IReadOnlyCollection<(VariableName VariableName, ITypeReference typeReference)>, IError> TryGetVariablesSet(StepFactoryStore stepFactoryStore, TypeResolver typeResolver)
    //    {
    //        return new List<(VariableName VariableName, ITypeReference typeReference)>();
    //    }

    //    /// <inheritdoc />
    //    public Result<ITypeReference, IError> TryGetOutputTypeReference(StepFactoryStore stepFactoryStore, TypeResolver typeResolver) => new ActualTypeReference(typeof(Unit));
    //}

}
