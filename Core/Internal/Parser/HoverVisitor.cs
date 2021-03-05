//using Antlr4.Runtime.Misc;
//using Antlr4.Runtime.Tree;
//using Namotion.Reflection;

//namespace Reductech.EDR.Core.Internal.Parser
//{

//public class HoverVisitor : SCLBaseVisitor<string>
//{
//    public HoverVisitor(Interval interval, StepFactoryStore stepFactoryStore)
//    {
//        Interval         = interval;
//        StepFactoryStore = stepFactoryStore;
//    }

//    public Interval Interval { get; }
//    public StepFactoryStore StepFactoryStore { get; }

//    /// <inheritdoc />
//    public override string VisitFunction(SCLParser.FunctionContext context)
//    {
//        var name = context.NAME().GetText();

//        if (StepFactoryStore.Dictionary.TryGetValue(name, out var stepFactory))
//        {
//            if (!context.NAME().SourceInterval.ProperlyContains(Interval))
//            {
//                var positionalTerms = context.term();

//                for (var index = 0; index < positionalTerms.Length; index++)
//                {
//                    var term = positionalTerms[index];

//                    if (term.SourceInterval.ProperlyContains(Interval))
//                    {
//                        if (
//                            stepFactory.PropertyDictionary.TryGetValue(
//                                new StepParameterReference(index),
//                                out var pi
//                            ))
//                        {
//                            var nHover = Visit(term);

//                            if (string.IsNullOrWhiteSpace(nHover))
//                                return pi.GetXmlDocsSummary();

//                            return nHover;
//                        }

//                        return $"Step '{name}' does not take an argument {index}";
//                    }
//                }

//                foreach (var namedArgumentContext in context.namedArgument())
//                {
//                    if (namedArgumentContext.SourceInterval.ProperlyContains(Interval))
//                    {
//                        var argumentName = namedArgumentContext.NAME().GetText();

//                        if (stepFactory.PropertyDictionary.TryGetValue(
//                            new StepParameterReference(argumentName),
//                            out var pi
//                        ))
//                        {
//                            var nHover = Visit(namedArgumentContext);

//                            if (string.IsNullOrWhiteSpace(nHover))
//                                return pi.GetXmlDocsSummary();

//                            return nHover;
//                        }

//                        return $"Step '{name}' does not take an argument {argumentName}";
//                    }
//                }
//            }

//            var summary = stepFactory.StepType.GetXmlDocsSummary();

//            return summary;
//        }
//        else
//        {
//            return $"Step not recognized '{name}'";
//        }
//    }

//    /// <inheritdoc />
//    public override string Visit(IParseTree tree)
//    {
//        if (tree.SourceInterval.ProperlyContains(Interval))
//            return base.Visit(tree);

//        return DefaultResult;
//    }

//    /// <inheritdoc />
//    protected override string AggregateResult(string aggregate, string nextResult)
//    {
//        if (string.IsNullOrWhiteSpace(nextResult))
//            return aggregate;

//        return nextResult;
//    }
//}

//}


