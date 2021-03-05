using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using JetBrains.Annotations;
using OneOf;
using OneOf.Types;
using Reductech.EDR.Core.Internal.Errors;
using ITerminalNode = Antlr4.Runtime.Tree.ITerminalNode;

namespace Reductech.EDR.Core.Internal.Parser
{

public record ErrorNode(IErrorBuilder ErrorBuilder, Interval Interval) { }

//public class Node : OneOfBase<ISyntaxTreeNode, ITerminalNode, ErrorNode>
//{
//    public Node(ISyntaxTreeNode node) : base(
//        OneOf<ISyntaxTreeNode, ITerminalNode, ErrorNode>.FromT0(node)
//    ) { }

//    public Node(ITerminalNode node) : base(
//        OneOf<ISyntaxTreeNode, ITerminalNode, ErrorNode>.FromT1(node)
//    ) { }

//    public Node(ErrorNode node) : base(
//        OneOf<ISyntaxTreeNode, ITerminalNode, ErrorNode>.FromT2(node)
//    ) { }

//    /// <inheritdoc />
//    protected Node(OneOf<ISyntaxTreeNode, ITerminalNode, ErrorNode> input) : base(input) { }

//    public Interval Interval =>
//        Match(x => x.SourceInterval, x => x.SourceInterval, x => x.Interval);

//    public Node? GetNodeAtLocation(Interval interval)
//    {
//        if (Interval.ProperlyContains(interval))
//        {
//            if (TryPickT0(out var stn, out _) && stn is IParentSyntaxTreeNode parent)
//            {
//                foreach (var child in parent.GetChildren())
//                {
//                    var n = child.GetNodeAtLocation(Interval);

//                    if (n is not null)
//                        return n;
//                }
//            }

//            return this;
//        }

//        return null;
//    }
//}

//public interface ISyntaxTreeNode : ISyntaxTree
//{
//    IEnumerable<ErrorNode> GetAllErrors();

//    //string GetHoverInformation(StepFactoryStore stepFactoryStore);

//    //IEnumerable<string> GetAutoCompleteOptions(StepFactoryStore stepFactoryStore);
//}

//public interface IParentSyntaxTreeNode : ISyntaxTreeNode
//{
//    public IEnumerable<Node> GetChildren();
//}

//public static class ASTNodeHelpers
//{
//    public static IEnumerable<ErrorNode> GetErrorsFromChildren(this IParentSyntaxTreeNode parent)
//    {
//        foreach (var child in parent.GetChildren())
//        {
//            var errors = child.Match(
//                x => x.GetAllErrors(),
//                _ => ImmutableList<ErrorNode>.Empty,
//                x => new[] { x }
//            );

//            foreach (var errorNode in errors)
//            {
//                yield return errorNode;
//            }
//        }
//    }
//}

//public partial class SCLParser
//{
//    public partial class SetVariableContext : IParentSyntaxTreeNode
//    {
//        /// <inheritdoc />
//        public IEnumerable<Node> GetChildren()
//        {
//            yield return new Node(VARIABLENAME());
//            yield return new Node(step());
//        }

//        IEnumerable<ErrorNode> ISyntaxTreeNode.GetAllErrors() => this.GetErrorsFromChildren();
//    }

//    public partial class GetVariableContext : IParentSyntaxTreeNode
//    {
//        IEnumerable<ErrorNode> ISyntaxTreeNode.GetAllErrors() => this.GetErrorsFromChildren();

//        /// <inheritdoc />
//        public IEnumerable<Node> GetChildren()
//        {
//            yield return new Node(VARIABLENAME());
//        }
//    }

//    public partial class ArrayContext : IParentSyntaxTreeNode
//    {
//        /// <inheritdoc />
//        IEnumerable<ErrorNode> ISyntaxTreeNode.GetAllErrors() => this.GetErrorsFromChildren();

//        /// <inheritdoc />
//        public IEnumerable<Node> GetChildren()
//        {
//            return term().Select(x => new Node(x));
//        }
//    }

//    public partial class InfixOperatorContext : ISyntaxTreeNode
//    {
//        /// <inheritdoc />
//        public IEnumerable<ErrorNode> GetAllErrors()
//        {
//            yield break;
//        }
//    }

//    public partial class InfixOperationContext : IParentSyntaxTreeNode
//    {
//        /// <inheritdoc />
//        IEnumerable<ErrorNode> ISyntaxTreeNode.GetAllErrors() => this.GetErrorsFromChildren();
//        //TODO return error if  mixed operators

//        /// <inheritdoc />
//        public IEnumerable<Node> GetChildren()
//        {
//            return term().Select(x => new Node(x));
//        }
//    }

//    public partial class EntityPropertyNameContext : ISyntaxTreeNode
//    {
//        /// <inheritdoc />
//        public IEnumerable<ErrorNode> GetAllErrors()
//        {
//            if (quotedString() is not null)
//                return quotedString().GetAllErrors();

//            return null;
//        }
//    }

//    public partial class EntityPropertyContext : IParentSyntaxTreeNode
//    {
//        /// <inheritdoc />
//        IEnumerable<ErrorNode> ISyntaxTreeNode.GetAllErrors() => this.GetErrorsFromChildren();

//        /// <inheritdoc />
//        public IEnumerable<Node> GetChildren()
//        {
//            foreach (var entityPropertyNameContext in entityPropertyName())
//            {
//                yield return new Node(entityPropertyNameContext);
//            }

//            yield return new Node(term());
//        }
//    }

//    public partial class NamedArgumentContext : IParentSyntaxTreeNode
//    {
//        /// <inheritdoc />
//        public IEnumerable<ErrorNode> GetAllErrors() => this.GetErrorsFromChildren();

//        /// <inheritdoc />
//        public IEnumerable<Node> GetChildren()
//        {
//            yield return new Node(NAME());
//            yield return new Node(term());
//        }
//    }

//    public partial class FunctionContext : IParentSyntaxTreeNode
//    {
//        /// <inheritdoc />
//        public IEnumerable<ErrorNode> GetAllErrors() => this.GetErrorsFromChildren();

//        /// <inheritdoc />
//        public IEnumerable<Node> GetChildren()
//        {
//            yield return new Node(NAME());

//            foreach (var termContext in term())
//            {
//                yield return new Node(termContext);
//            }

//            foreach (var namedArgumentContext in namedArgument())
//            {
//                yield return new Node(namedArgumentContext);
//            }
//        }
//    }

//    public partial class EntityContext : IParentSyntaxTreeNode
//    {
//        /// <inheritdoc />
//        public IEnumerable<ErrorNode> GetAllErrors() => this.GetErrorsFromChildren();

//        /// <inheritdoc />
//        public IEnumerable<Node> GetChildren()
//        {
//            foreach (var entityPropertyContext in entityProperty())
//            {
//                yield return new Node(entityPropertyContext);
//            }
//        }
//    }

//    public partial class BracketedStepContext : IParentSyntaxTreeNode
//    {
//        /// <inheritdoc />
//        public IEnumerable<ErrorNode> GetAllErrors() => this.GetErrorsFromChildren();

//        /// <inheritdoc />
//        public IEnumerable<Node> GetChildren()
//        {
//            yield return new Node(step());
//        }
//    }

//    public partial class BooleanContext : ISyntaxTreeNode
//    {
//        /// <inheritdoc />
//        public IEnumerable<ErrorNode> GetAllErrors()
//        {
//            yield break;
//        }
//    }

//    public partial class DateTimeContext : ISyntaxTreeNode
//    {
//        /// <inheritdoc />
//        public IEnumerable<ErrorNode> GetAllErrors()
//        {
//            yield break; //TODO tryparse
//        }
//    }

//    public partial class QuotedStringContext : ISyntaxTreeNode
//    {
//        /// <inheritdoc />
//        public IEnumerable<ErrorNode> GetAllErrors()
//        {
//            yield break; //TODO tryparse
//        }
//    }

//    public partial class NumberContext : ISyntaxTreeNode
//    {
//        /// <inheritdoc />
//        public IEnumerable<ErrorNode> GetAllErrors()
//        {
//            yield break; //TODO tryparse
//        }
//    }

//    public partial class EnumerationContext : ISyntaxTreeNode
//    {
//        /// <inheritdoc />
//        public IEnumerable<ErrorNode> GetAllErrors()
//        {
//            yield break; //TODO tryparse
//        }
//    }

//    public partial class TermContext : IParentSyntaxTreeNode
//    {
//        /// <inheritdoc />
//        public IEnumerable<ErrorNode> GetAllErrors() => this.GetErrorsFromChildren();

//        /// <inheritdoc />
//        public virtual IEnumerable<Node> GetChildren()
//        {
//            throw new NotImplementedException($"Term Context of type {GetType().Name}");
//        }
//    }

//    public partial class ArrayAccessContext
//    {
//        /// <inheritdoc />
//        public override IEnumerable<Node> GetChildren()
//        {
//            foreach (var termContext in term())
//            {
//                yield return new Node(termContext);
//            }
//        }
//    }

//    public partial class SimpleTerm1Context
//    {
//        /// <inheritdoc />
//        public override IEnumerable<Node> GetChildren()
//        {
//            yield return new Node(simpleTerm());
//        }
//    }

//    public partial class BracketedStep1Context
//    {
//        /// <inheritdoc />
//        public override IEnumerable<Node> GetChildren()
//        {
//            yield return new Node(bracketedStep());
//        }
//    }

//    public partial class StepContext : IParentSyntaxTreeNode
//    {
//        /// <inheritdoc />
//        public IEnumerable<ErrorNode> GetAllErrors() => this.GetErrorsFromChildren();

//        /// <inheritdoc />
//        public virtual IEnumerable<Node> GetChildren()
//        {
//            throw new NotImplementedException($"Step Context of type {GetType().Name}");
//        }
//    }

//    public partial class SetVariable1Context
//    {
//        /// <inheritdoc />
//        public override IEnumerable<Node> GetChildren()
//        {
//            yield return new Node(setVariable());
//        }
//    }

//    public partial class StepSequence1Context
//    {
//        /// <inheritdoc />
//        public override IEnumerable<Node> GetChildren()
//        {
//            yield return new Node(stepSequence());
//        }
//    }

//    public partial class Function1Context
//    {
//        /// <inheritdoc />
//        public override IEnumerable<Node> GetChildren()
//        {
//            yield return new Node(function());
//        }
//    }

//    public partial class Term1Context
//    {
//        /// <inheritdoc />
//        public override IEnumerable<Node> GetChildren()
//        {
//            yield return new Node(term());
//        }
//    }

//    public partial class PipeFunctionContext
//    {
//        /// <inheritdoc />
//        public override IEnumerable<Node> GetChildren()
//        {
//            yield return new Node(step());
//            yield return new Node(function());
//        }
//    }

//    public partial class InfixOperation1Context
//    {
//        /// <inheritdoc />
//        public override IEnumerable<Node> GetChildren()
//        {
//            yield return new Node(infixOperation());
//        }
//    }

//    public partial class SimpleTermContext: IParentSyntaxTreeNode
//    {
//        /// <inheritdoc />
//        public IEnumerable<ErrorNode> GetAllErrors() => this.GetErrorsFromChildren();

//        /// <inheritdoc />
//        public IEnumerable<Node> GetChildren()
//        {
//            return children.Cast<ISyntaxTreeNode>().Select(x => new Node(x));
//        }
//    }

//public class AllErrorVisitor : AbstractParseTreeVisitor<IEnumerable<ErrorNode>>
//{
//    /// <inheritdoc />
//    protected override IEnumerable<ErrorNode> DefaultResult
//    {
//        get
//        {
//            yield break;
//        }
//    }

//    /// <inheritdoc />
//    protected override IEnumerable<ErrorNode> AggregateResult(
//        IEnumerable<ErrorNode> aggregate,
//        IEnumerable<ErrorNode> nextResult)
//    {
//        return aggregate.Concat(nextResult);
//    }
//}

}

//public record ErrorNode(
//    TextLocation Location,
//    IErrorBuilder ErrorBuilder) : ASTNode(Location)
//{
//    /// <inheritdoc />
//    public override IErrorBuilder? MaybeError => ErrorBuilder;

//    /// <inheritdoc />
//    public override ASTNode? GetNodeAtPosition(TextPosition position)
//    {
//        if (Location.Contains(position))
//            return this;

//        return null;
//    }
//}

//public abstract record TerminalNode(TextLocation Location) : ASTNode(Location)
//{
//    /// <inheritdoc />
//    public override IErrorBuilder? MaybeError => null;

//    /// <inheritdoc />
//    public override ASTNode? GetNodeAtPosition(TextPosition position)
//    {
//        if (Location.Contains(position))
//            return this;

//        return null;
//    }
//}

//public abstract record ParentNode(
//    TextLocation Location,
//    IEnumerable<IASTNode> Children) : ASTNode(Location)
//{
//    /// <inheritdoc />
//    public override IErrorBuilder? MaybeError =>
//        ErrorBuilderList.MaybeCombine(Children.Select(x => x.MaybeError));

//    /// <inheritdoc />
//    public override ASTNode? GetNodeAtPosition(TextPosition position)
//    {
//        if (!Location.Contains(position))
//            return null;

//        foreach (var astNode in Children)
//        {
//            var n = astNode.GetNodeAtPosition(position);

//            if (n != null)
//                return n;
//        }

//        return null;
//    }
//}

//public interface ITermNode : IASTNode { }

//public record NumberNode
//    (TextLocation Location, double Number) : TerminalNode(Location), ITermNode { }

//public record BooleanNode(TextLocation Location, bool Value) : TerminalNode(Location), ITermNode { }

//public record StringNode
//    (TextLocation Location, string String) : TerminalNode(Location), ITermNode { }

//public record DateTimeNode(TextLocation Location, DateTime DateTime) : TerminalNode(Location),
//    ITermNode { }

//public record VariableNameNode
//    (TextLocation Location, VariableName Name) : TerminalNode(Location), ITermNode { }

//public record EnumNameNode(TextLocation Location, string String) : TerminalNode(Location) { }
//public record EnumValueNode(TextLocation Location, string String) : TerminalNode(Location) { }

//public record EnumerationNodeNode(
//    TextLocation Location,
//    NodeOrError<EnumNameNode> Name,
//    NodeOrError<EnumValueNode> Value) : ParentNode(Location, new IASTNode[] { Name, Value }),
//                                        ITermNode { }

//public record ArrayNode
//    (TextLocation Location, IReadOnlyCollection<NodeOrError<ITermNode>> Elements) : ParentNode(
//        Location,
//        Elements
//    ), ITermNode;

//public record EntityNode(
//    TextLocation Location,
//    IReadOnlyCollection<NodeOrError<EntityPropertyNode>> Properties) : ParentNode(
//    Location,
//    Properties
//), ITermNode;

//public record EntityPropertyNode(
//        TextLocation Location,
//        NodeOrError<EntityPropertyNameNode> Name,
//        NodeOrError<ITermNode> Value)
//    : ParentNode(Location, new IASTNode[] { Name, Value });

//public record EntityPropertyNameNode
//    (TextLocation Location, EntityPropertyKey PropertyKey) : TerminalNode(Location);

//public record StepParameterNameNode(TextLocation Location, string Name) : TerminalNode(Location);
//public record StepNameNode(TextLocation Location, string Name) : TerminalNode(Location);

//public interface IStepParameterNode : IASTNode{}

//public record StepNode(NodeOrError<StepNameNode> StepName, )
//{

//}

//public record NamedStepParameterNode(
//        TextLocation Location,
//        NodeOrError<StepParameterNameNode> Name,
//        NodeOrError<ITermNode> Value)
//    : ParentNode(Location, new IASTNode[] { Name, Value }), IStepParameterNode;

//public record PositionalStepParameterNode
//    (TextLocation Location, NodeOrError<ITermNode> Value)
//    : WrapperNode<ITermNode>(Location, Value), IStepParameterNode;

//public record SequenceNode(
//    TextLocation Location,
//    IReadOnlyCollection<NodeOrError<CommandNode>> Steps) : ParentNode(
//                                                               Location,
//                                                               Steps
//                                                           ),
//                                                           ITermNode { }

//public record CommandNode
//    (TextLocation Location, NodeOrError<ITermNode> Child) : WrapperNode<ITermNode>(
//        Location,
//        Child
//    ) { }

//public record ParentheticalNode
//    (TextLocation Location, NodeOrError<ITermNode> Child) : WrapperNode<ITermNode>(Location, Child),
//                                                            ITermNode;

//public record WrapperNode<TChild>(TextLocation Location, NodeOrError<TChild> Child) : ParentNode(
//    Location,
//    new[] { Child }
//) where TChild : IASTNode { }

//public class NodeOrError<TNode> : OneOfBase<TNode, ErrorNode>, IASTNode
//    where TNode : IASTNode
//{
//    /// <inheritdoc />
//    public NodeOrError(OneOf<TNode, ErrorNode> input) : base(input) { }

//    private IASTNode Node => Match(x => x, x => x as IASTNode);

//    /// <inheritdoc />
//    public IErrorBuilder? MaybeError => Node.MaybeError;

//    /// <inheritdoc />
//    public ASTNode? GetNodeAtPosition(TextPosition position)
//    {
//        return Node.GetNodeAtPosition(position);
//    }

//    /// <inheritdoc />
//    public TextLocation Location => Node.Location;
//}

//public abstract record ASTNode(TextLocation Location) : IASTNode
//{
//    public static ASTNode Create(string text)
//    {
//        var inputStream       = new AntlrInputStream(text);
//        var lexer             = new SCLLexer(inputStream);
//        var commonTokenStream = new CommonTokenStream(lexer);
//        var parser            = new SCLParser(commonTokenStream);

//        //TODO syntax listener
//        //TODO lexer error listener
//        //TODO parser error listener

//        var visitor = new LenientSCLParsing.Visitor();

//        var result = visitor.Visit(parser.fullSequence());

//        if (result is null)
//            return new ErrorNode(
//                new TextLocation(text, new TextPosition(0, 0, 0), new TextPosition(0, 0, 0)),
//                ErrorCode.EmptySequence.ToErrorBuilder()
//            );

//        return result;
//    }

//    public abstract IErrorBuilder? MaybeError { get; }

//    public abstract ASTNode? GetNodeAtPosition(TextPosition position);
//}
