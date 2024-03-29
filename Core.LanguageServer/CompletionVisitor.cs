﻿using System.Text.RegularExpressions;
using Sequence.Core.Entities.Schema;

namespace Sequence.Core.LanguageServer;

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
        Lazy<TypeResolver> lazyTypeResolver,
        DocumentationOptions documentationOptions)
    {
        Position             = position;
        StepFactoryStore     = stepFactoryStore;
        LazyTypeResolver     = lazyTypeResolver;
        DocumentationOptions = documentationOptions;
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

    /// <summary>
    /// Options to use for rendering documentation
    /// </summary>
    public DocumentationOptions DocumentationOptions { get; }

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
            var range = node.Symbol.GetRange();

            var completionResponse = TryGetCompletionResponse(
                node.Symbol.Text,
                range
            );

            if (completionResponse is not null)
                return completionResponse;

            var previous = node.GetPrevious();

            if (previous is not null)
            {
                var combinedText = previous.GetText() + node.Symbol.Text;
                range.StartColumn  -= 1;
                completionResponse =  TryGetCompletionResponse(combinedText, range);

                if (completionResponse is not null)
                    return completionResponse;
            }
        }

        return base.VisitErrorNode(node);
    }

    private CompletionResponse? TryGetCompletionResponse(string text, TextRange range)
    {
        var variableStartMatch = VariableStartRegex.Match(text);

        if (variableStartMatch.Success)
        {
            var name = variableStartMatch.Groups["name"].Value;

            return VariableNameCompletionResponse(name, range);
        }

        return null;
    }

    private static readonly Regex VariableStartRegex = new(
        @"\A<(?<name>[a-z0-9]*)\Z",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

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
                        context,
                        DocumentationOptions
                    );
            }

            return null;
        }

        if (context.NAME().Symbol.ContainsPosition(Position))
        {
            var previous = context.GetPrevious();

            if (previous?.GetText() == "<")
            {
                var range = context.GetRange();
                range.StartColumn -= 1;
                var completionResponse = TryGetCompletionResponse("<" + context.GetText(), range);

                if (completionResponse is not null)
                    return completionResponse;
            }

            var nameText = context.NAME().GetText();

            var options =
                StepFactoryStore.Dictionary
                    .Where(x => x.Key.Contains(nameText, StringComparison.OrdinalIgnoreCase))
                    .GroupBy(x => x.Value, x => x.Key)
                    .ToList();

            return ReplaceWithSteps(
                options,
                context.NAME().Symbol.GetRange(),
                DocumentationOptions
            );
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
                        context,
                        DocumentationOptions
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

                    return StepParametersCompletionResponse(
                        stepFactory,
                        range,
                        context,
                        DocumentationOptions
                    );
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
                context,
                DocumentationOptions
            );
        }
    }

    private static CompletionResponse ReplaceWithSteps(
        IEnumerable<IGrouping<IStepFactory, string>> stepFactories,
        TextRange range,
        DocumentationOptions documentationOptions)
    {
        var items = stepFactories.SelectMany(CreateCompletionItems).ToList();

        return new CompletionResponse(false, items);

        IEnumerable<CompletionItem> CreateCompletionItems(IGrouping<IStepFactory, string> factory)
        {
            var documentation = Helpers.GetMarkDownDocumentation(
                factory,
                documentationOptions
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

    private CompletionResponse VariableNameCompletionResponse(string text, TextRange range)
    {
        var completionItems = LazyTypeResolver.Value.Dictionary
            .Where(x => x.Key.Name.StartsWith(text, StringComparison.OrdinalIgnoreCase))
            .Select(
                x => new CompletionItem(
                    x.Key.Serialize(SerializeOptions.Serialize),
                    x.Value.TypeReference.HumanReadableTypeName,
                    x.Value.GetMarkdown() ?? "",
                    //x.Value. .Serialize(SerializeOptions.Serialize),
                    x.Key.Name.Equals(text, StringComparison.OrdinalIgnoreCase),
                    new SCLTextEdit(
                        x.Key.Serialize(SerializeOptions.Serialize),
                        range
                    )
                )
            )
            .ToList();

        return new CompletionResponse(false, completionItems);
    }

    private static CompletionResponse EntityPropertiesCompletionResponse(
        IEnumerable<(EntityNestedKey, SchemaNode)> pairs,
        TextRange range)
    {
        var items = pairs.Select(x => CreateCompletionItem(x.Item1, x.Item2)).ToList();

        CompletionItem CreateCompletionItem(EntityNestedKey key, SchemaNode node)
        {
            var type = node.ToTypeReference().Match(x => x.HumanReadableTypeName, () => "Unknown");

            return new CompletionItem(
                key.AsString,
                type,
                $"`type`",
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
        SCLParser.FunctionContext functionContext,
        DocumentationOptions documentationOptions)
    {
        var usedNamedArguments = functionContext.namedArgument()
            .Select(x => x.NAME().GetText())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var documentation = Helpers.GetMarkDownDocumentation(
            stepFactory,
            documentationOptions
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
