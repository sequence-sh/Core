using Namotion.Reflection;

namespace Reductech.Sequence.Core.LanguageServer;

/// <summary>
/// Visits SCL to find quick info
/// </summary>
public class QuickInfoVisitor : SCLBaseVisitor<QuickInfoResponse?>
{
    /// <summary>
    /// Create a new QuickInfoVisitor
    /// </summary>
    public QuickInfoVisitor(
        LinePosition position,
        StepFactoryStore stepFactoryStore,
        Lazy<TypeResolver> lazyTypeResolver,
        IReadOnlyDictionary<VariableName, ISCLObject> injectedVariables)
    {
        LinePosition      = position;
        StepFactoryStore  = stepFactoryStore;
        LazyTypeResolver  = lazyTypeResolver;
        InjectedVariables = injectedVariables;
    }

    /// <summary>
    /// The position of the QuickInfo
    /// </summary>
    public LinePosition LinePosition { get; }

    /// <summary>
    /// The Step Factory Store
    /// </summary>
    public StepFactoryStore StepFactoryStore { get; }

    /// <summary>
    /// A Lazy Type Resolver
    /// </summary>
    public Lazy<TypeResolver> LazyTypeResolver { get; }

    /// <summary>
    /// Injected Variables
    /// </summary>
    public IReadOnlyDictionary<VariableName, ISCLObject> InjectedVariables { get; }

    /// <inheritdoc />
    protected override bool ShouldVisitNextChild(IRuleNode node, QuickInfoResponse? currentResult)
    {
        return currentResult == null;
    }

    /// <inheritdoc />
    public override QuickInfoResponse? Visit(IParseTree tree)
    {
        if (tree is ParserRuleContext context && context.ContainsPosition(LinePosition))
            return base.Visit(tree);

        return DefaultResult;
    }

    /// <inheritdoc />
    public override QuickInfoResponse? VisitFunction(SCLParser.FunctionContext context)
    {
        if (!context.ContainsPosition(LinePosition))
            return null;

        var name = context.NAME().GetText();

        if (StepFactoryStore.Dictionary.TryGetValue(name, out var stepFactory))
        {
            if (!context.NAME().Symbol.ContainsPosition(LinePosition))
            {
                var positionalTerms = context.term();

                for (var index = 0; index < positionalTerms.Length; index++)
                {
                    var term = positionalTerms[index];

                    if (term.ContainsPosition(LinePosition))
                    {
                        int trueIndex;

                        if (context.Parent is SCLParser.PipeFunctionContext pipeFunctionContext &&
                            pipeFunctionContext.children.Last() == context)
                        {
                            trueIndex = index + 2;
                        }
                        else
                        {
                            trueIndex = index + 1;
                        }

                        var indexReference = new StepParameterReference.Index(trueIndex);

                        if (
                            stepFactory.ParameterDictionary.TryGetValue(
                                indexReference,
                                out var stepParameter
                            ))
                        {
                            var nHover = Visit(term);

                            if (nHover is null)
                            {
                                return Description(
                                    stepParameter.Name,
                                    stepParameter.GetHumanReadableTypeDescription(),
                                    stepParameter.Summary
                                );
                            }

                            return nHover;
                        }

                        return Error($"Step '{name}' does not take an argument {index}");
                    }
                }

                foreach (var namedArgumentContext in context.namedArgument())
                {
                    if (namedArgumentContext.ContainsPosition(LinePosition))
                    {
                        var argumentName = namedArgumentContext.NAME().GetText();

                        if (stepFactory.ParameterDictionary.TryGetValue(
                                new StepParameterReference.Named(argumentName),
                                out var stepParameter
                            ))
                        {
                            var nHover = Visit(namedArgumentContext);

                            if (nHover is null)
                                return Description(
                                    stepParameter.Name,
                                    TypeReference.Create(stepParameter.ActualType),
                                    stepParameter.Summary
                                );

                            return nHover;
                        }

                        return Error($"Step '{name}' does not take an argument {argumentName}");
                    }
                }
            }

            var summary = stepFactory.Summary;

            return Description(
                stepFactory.TypeName,
                stepFactory.GetHumanReadableTypeDescription(),
                summary
            );
        }

        return Error(name);
    }

    /// <inheritdoc />
    public override QuickInfoResponse? VisitNumber(SCLParser.NumberContext context)
    {
        if (!context.ContainsPosition(LinePosition))
            return null;

        var text = context.GetText();

        var typeName = int.TryParse(text, out var _)
            ? TypeReference.Actual.Integer
            : TypeReference.Actual.Double;

        return Description(text, typeName, null);
    }

    /// <inheritdoc />
    public override QuickInfoResponse? VisitBoolean(SCLParser.BooleanContext context)
    {
        if (!context.ContainsPosition(LinePosition))
            return null;

        return Description(
            context.GetText(),
            TypeReference.Actual.Bool,
            null
        );
    }

    /// <inheritdoc />
    public override QuickInfoResponse? VisitEntity(SCLParser.EntityContext context)
    {
        if (!context.ContainsPosition(LinePosition))
            return null;

        foreach (var contextChild in context.children)
        {
            var r = Visit(contextChild);

            if (r is not null)
                return r;
        }

        return Description(
            context.GetText(),
            TypeReference.Entity.NoSchema,
            null
        );
    }

    /// <inheritdoc />
    public override QuickInfoResponse? VisitDateTime(SCLParser.DateTimeContext context)
    {
        if (!context.ContainsPosition(LinePosition))
            return null;

        return Description(
            context.GetText(),
            TypeReference.Actual.Date,
            null
        );
    }

    /// <inheritdoc />
    public override QuickInfoResponse? VisitEnumeration(SCLParser.EnumerationContext context)
    {
        if (!context.ContainsPosition(LinePosition))
            return null;

        if (context.children.Count != 3 || context.NAME().Length != 2)
            return null;

        var prefix = context.NAME(0).GetText();
        var suffix = context.NAME(1).GetText();

        if (!StepFactoryStore.EnumTypesDictionary.TryGetValue(prefix, out var enumType))
        {
            return Error($"'{prefix}' is not a valid enum type.");
        }

        if (!Enum.TryParse(enumType, suffix, true, out var value) || value is null)
        {
            return Error($"'{suffix}' is not a member of enumeration '{prefix}'");
        }

        var summary = value.GetType().GetXmlDocsSummary();

        return Description(
            value.ToString(),
            TypeNameHelper.GetHumanReadableTypeName(enumType),
            summary
        );
    }

    /// <inheritdoc />
    public override QuickInfoResponse? VisitGetAutomaticVariable(
        SCLParser.GetAutomaticVariableContext context)
    {
        if (!context.ContainsPosition(LinePosition))
            return null;

        return Description(
            "<>",
            TypeReference.AutomaticVariable.Instance,
            null
        );
    }

    /// <inheritdoc />
    public override QuickInfoResponse? VisitGetVariable(SCLParser.GetVariableContext context)
    {
        if (!context.ContainsPosition(LinePosition))
            return null;

        var vn = new VariableName(context.GetText().TrimStart('<').TrimEnd('>'));

        //TODO use injected variables here

        if (LazyTypeResolver.Value.Dictionary.TryGetValue(vn, out var tr))
        {
            return Description(
                context.GetText(),
                tr.TypeReference,
                null
            );
        }

        return Description(
            context.GetText(),
            nameof(VariableName),
            null
        );
    }

    /// <inheritdoc />
    public override QuickInfoResponse? VisitSetVariable(SCLParser.SetVariableContext context)
    {
        if (!context.ContainsPosition(LinePosition))
            return null;

        var variableHover = VisitVariable(context.VARIABLENAME());

        if (variableHover is not null)
            return variableHover;

        var h2 = Visit(context.step());

        if (h2 is not null)
            return h2;

        var setVariable = new SetVariable<SCLInt>().StepFactory;

        return Description(
            setVariable.TypeName,
            setVariable.GetHumanReadableTypeDescription(),
            setVariable.Summary
        );
    }

    private QuickInfoResponse? VisitVariable(ITerminalNode variableNameNode)
    {
        if (!variableNameNode.Symbol.ContainsPosition(LinePosition))
            return null;

        var text = variableNameNode.GetText();

        var vn = new VariableName(text.TrimStart('<').TrimEnd('>'));

        //TODO use injected variables here

        if (LazyTypeResolver.Value.Dictionary.TryGetValue(vn, out var tr))
        {
            return Description(
                text,
                tr.TypeReference,
                null
            );
        }

        return Description(
            text,
            nameof(VariableName),
            null
        );
    }

    /// <inheritdoc />
    public override QuickInfoResponse? VisitQuotedString(SCLParser.QuotedStringContext context)
    {
        if (!context.ContainsPosition(LinePosition))
            return null;

        return Description(
            context.GetText(),
            TypeReference.Actual.String,
            null
        );
    }

    /// <inheritdoc />
    public override QuickInfoResponse? VisitArray(SCLParser.ArrayContext context)
    {
        if (!context.ContainsPosition(LinePosition))
            return null;

        foreach (var contextChild in context.children)
        {
            var h1 = Visit(contextChild);

            if (h1 is not null)
                return h1;
        }

        return DescribeStep(context.GetText());
    }

    /// <inheritdoc />
    public override QuickInfoResponse? VisitInfixOperation(SCLParser.InfixOperationContext context)
    {
        if (!context.ContainsPosition(LinePosition))
            return null;

        foreach (var termContext in context.infixableTerm())
        {
            var h1 = Visit(termContext);

            if (h1 is not null)
                return h1;
        }

        var operatorSymbols =
            context.infixOperator().Select(x => x.GetText()).Distinct().ToList();

        if (operatorSymbols.Count != 1)
        {
            return Error("Invalid mix of operators");
        }

        return DescribeStep(context.GetText());
    }

    private QuickInfoResponse DescribeStep(string text)
    {
        var step = SCLParsing.TryParseStep(text);

        if (step.IsFailure)
            return Error(step.Error.AsString);

        var callerMetadata = new CallerMetadata("Step", "Parameter", TypeReference.Any.Instance);

        Result<IStep, IError> freezeResult;

        freezeResult = step.Value.TryFreeze(callerMetadata, LazyTypeResolver.Value);

        if (freezeResult.IsFailure)
            return Error(freezeResult.Error.AsString);

        return Description(freezeResult.Value);
    }

    private static QuickInfoResponse Description(IStep step)
    {
        var     name = step.Name;
        string? description;

        if (step is ICompoundStep cs)
            description = cs.StepFactory.Summary;
        else
            description = null;

        return Description(name, TypeReference.Create(step.OutputType), description);
    }

    private static QuickInfoResponse Description(
        string? name,
        string? type,
        string? summary)
    {
        var strings = new List<string>(3);

        if (name is not null)
            strings.Add($"`{name}`");

        if (type is not null)
            strings.Add($"{type}");

        if (summary is not null)
            strings.Add(summary);

        return new() { MarkdownStrings = strings };
    }

    private static QuickInfoResponse Description(
        string? name,
        TypeReference type,
        string? summary)
    {
        var tName = type.HumanReadableTypeName;
        tName = $"`{tName.Trim('`')}`";
        return Description(name, tName, summary);
    }

    private static QuickInfoResponse Error(string message)
    {
        return new() { MarkdownStrings = new List<string>() { message } };
    }
}
