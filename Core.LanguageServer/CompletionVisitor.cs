using Reductech.Sequence.Core.Entities.Schema;

namespace Reductech.Sequence.Core.LanguageServer;

/// <summary>
/// Visits SCL for completion
/// </summary>
public class CompletionVisitor : SCLBaseVisitor<CompletionResponse?>
{
    /// <summary>
    /// Creates a new Completion Visitor
    /// </summary>
    public CompletionVisitor(
        LinePosition position,
        StepFactoryStore stepFactoryStore,
        Lazy<TypeResolver> lazyTypeResolver)
    {
        Position         = position;
        StepFactoryStore = stepFactoryStore;
        LazyTypeResolver = lazyTypeResolver;
    }

    /// <summary>
    /// The position
    /// </summary>
    public LinePosition Position { get; }

    /// <summary>
    /// The Step Factory Store
    /// </summary>
    public StepFactoryStore StepFactoryStore { get; }

    /// <summary>
    /// A Lazy Type Resolver
    /// </summary>
    public Lazy<TypeResolver> LazyTypeResolver { get; }

    /// <inheritdoc />
    public override CompletionResponse? VisitChildren(IRuleNode node)
    {
        var i = 0;

        while (i < node.ChildCount)
        {
            var child = node.GetChild(i);

            if (child is TerminalNodeImpl tni && tni.GetText() == "<EOF>")
            {
                break;
            }

            if (child is ParserRuleContext prc)
            {
                if (prc.StartsAfter(Position))
                    break;

                if (prc.ContainsPosition(Position))
                {
                    var result = Visit(child);

                    if (result is not null)
                        return result;
                }
            }

            i++;
        }

        if (i >= 1) //Go back to the last function and use that
        {
            var lastChild = node.GetChild(i - 1);

            var r = Visit(lastChild);
            return r;
        }

        return null;
    }

    /// <inheritdoc />
    public override CompletionResponse? VisitEntityGetValue(SCLParser.EntityGetValueContext context)
    {
        if (context.indexer.ContainsPosition(Position))
        {
            var callerMetadata = new CallerMetadata(
                nameof(EntityGetValue<ISCLObject>),
                nameof(EntityGetValue<ISCLObject>.Entity),
                TypeReference.Entity.NoSchema
            );

            var visitor = new SCLParsing.Visitor();

            var typeReference = visitor.Visit(context.accessedEntity)
                .Map(x => x.ConvertToStep())
                .Bind(x => x.TryGetOutputTypeReference(callerMetadata, LazyTypeResolver.Value));

            if (typeReference.IsSuccess && typeReference.Value is TypeReference.Entity
                {
                    Schema.HasValue: true
                } entityTypeReference)
            {
                var completionItems = entityTypeReference.Schema.Value.GetKeyNodePairs();

                var response = EntityPropertiesCompletionResponse(
                    completionItems,
                    context.indexer.GetRange()
                );

                return response;
            }
        }

        return base.VisitEntityGetValue(context);
    }

    /// <inheritdoc />
    public override CompletionResponse? VisitErrorNode(IErrorNode node)
    {
        if (node.Symbol.ContainsPosition(Position))
        {
            return base.VisitErrorNode(node);
        }

        return base.VisitErrorNode(node);
    }

    /// <inheritdoc />
    public override CompletionResponse? VisitFunction1(SCLParser.Function1Context context)
    {
        var func = context.function();

        var result = VisitFunction(func);

        return result;
    }

    /// <inheritdoc />
    public override CompletionResponse? VisitFunction(SCLParser.FunctionContext context)
    {
        var name = context.NAME().GetText();

        if (!context.ContainsPosition(Position))
        {
            if (context.EndsBefore(Position))
            {
                //Assume this is another parameter to this function
                if (StepFactoryStore.Dictionary.TryGetValue(name, out var stepFactory))
                    return StepParametersCompletionResponse(
                        stepFactory,
                        new TextRange(Position, Position),
                        context
                    );
            }

            return null;
        }

        if (context.NAME().Symbol.ContainsPosition(Position))
        {
            var nameText = context.NAME().GetText();

            var options =
                StepFactoryStore.Dictionary
                    .Where(x => x.Key.Contains(nameText, StringComparison.OrdinalIgnoreCase))
                    .GroupBy(x => x.Value, x => x.Key)
                    .ToList();

            return ReplaceWithSteps(options, context.NAME().Symbol.GetRange());
        }

        var positionalTerms = context.term();

        for (var index = 0; index < positionalTerms.Length; index++)
        {
            var term = positionalTerms[index];

            if (term.ContainsPosition(Position))
            {
                //If this term is a partial name, give argument names as potential options
                var text = term.GetText();

                if (string.IsNullOrWhiteSpace(text) || text.All(char.IsLetter))
                {
                    //The term could be the start of a parameter argument

                    if (!StepFactoryStore.Dictionary.TryGetValue(name, out var stepFactory))
                        return Visit(term); //No clue what name to use

                    var completionList1 = StepParametersCompletionResponse(
                        stepFactory,
                        term.GetRange(),
                        context
                    );

                    var completionList2 = Visit(term);

                    if (completionList2 is null)
                        return completionList1;

                    if (!completionList1.Items.Any())
                        return completionList2;

                    return completionList1;
                }
                else //Assume the term is structured object
                {
                    return Visit(term);
                }
            }
        }

        foreach (var namedArgumentContext in context.namedArgument())
        {
            if (namedArgumentContext.ContainsPosition(Position))
            {
                if (namedArgumentContext.NAME().Symbol.ContainsPosition(Position))
                {
                    if (!StepFactoryStore.Dictionary.TryGetValue(name, out var stepFactory))
                        return null; //Don't know what step factory to use

                    var range = namedArgumentContext.NAME().Symbol.GetRange();

                    return StepParametersCompletionResponse(stepFactory, range, context);
                }

                return Visit(namedArgumentContext);
            }
        }

        {
            if (!StepFactoryStore.Dictionary.TryGetValue(name, out var stepFactory))
                return null; //No clue what name to use

            return StepParametersCompletionResponse(
                stepFactory,
                new TextRange(Position, Position),
                context
            );
        }
    }

    private static CompletionResponse ReplaceWithSteps(
        IEnumerable<IGrouping<IStepFactory, string>> stepFactories,
        TextRange range)
    {
        var items = stepFactories.SelectMany(CreateCompletionItems).ToList();

        return new CompletionResponse(false, items);

        IEnumerable<CompletionItem> CreateCompletionItems(IGrouping<IStepFactory, string> factory)
        {
            var documentation = Helpers.GetMarkDownDocumentation(
                factory,
                Helpers.DocumentationRootUrl
            );

            var first = true;

            foreach (var key in factory)
            {
                yield return new(
                    key,
                    factory.Key.Summary,
                    documentation,
                    first,
                    new SCLTextEdit(key, range)
                );

                first = false;
            }
        }
    }

    private static CompletionResponse EntityPropertiesCompletionResponse(
        IEnumerable<(EntityPropertyKey, SchemaNode)> pairs,
        TextRange range)
    {
        var items = pairs.Select(x => CreateCompletionItem(x.Item1, x.Item2)).ToList();

        CompletionItem CreateCompletionItem(EntityPropertyKey key, SchemaNode node)
        {
            var type = node.ToTypeReference().Match(x => x.HumanReadableTypeName, () => "Unknown");

            return new CompletionItem(
                key.AsString,
                key.AsString,
                type,
                false,
                new SCLTextEdit(key.AsString, range)
            );
        }

        return new CompletionResponse(false, items);
    }

    /// <summary>
    /// Gets the step parameter completion list
    /// </summary>
    private static CompletionResponse StepParametersCompletionResponse(
        IStepFactory stepFactory,
        TextRange range,
        SCLParser.FunctionContext functionContext)
    {
        var usedNamedArguments = functionContext.namedArgument()
            .Select(x => x.NAME().GetText())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var documentation = Helpers.GetMarkDownDocumentation(
            stepFactory,
            Helpers.DocumentationRootUrl
        );

        var options =
            stepFactory.ParameterDictionary
                .Where(x => x.Key is StepParameterReference.Named)
                .Where(x => !usedNamedArguments.Contains(x.Value.Name))
                .Select((x, i) => CreateCompletionItem(x.Key, x.Value, i == 0))
                .ToList();

        CompletionItem CreateCompletionItem(
            StepParameterReference stepParameterReference,
            IStepParameter stepParameter,
            bool preselect)
        {
            return new CompletionItem(
                stepParameterReference.Name,
                stepParameter.Summary,
                documentation,
                preselect,
                new SCLTextEdit(stepParameterReference.Name + ":", range)
            );
        }

        return new CompletionResponse(false, options);
    }
}
