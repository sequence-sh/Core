//using Antlr4.Runtime.Misc;
//using Antlr4.Runtime.Tree;

//namespace Reductech.EDR.Core.Internal.Parser
//{

//public class CodeCompletionVisitor : AbstractParseTreeVisitor<string>
//{
//    public CodeCompletionVisitor(Interval interval, StepFactoryStore stepFactoryStore)
//    {
//        Interval         = interval;
//        StepFactoryStore = stepFactoryStore;
//    }

//    public Interval Interval { get; }

//    public StepFactoryStore StepFactoryStore { get; }

//    /// <inheritdoc />
//    public override string Visit(IParseTree tree)
//    {
//        if (tree.SourceInterval.ProperlyContains(Interval))
//            return base.Visit(tree);

//        return DefaultResult;
//    }
//}

//}


