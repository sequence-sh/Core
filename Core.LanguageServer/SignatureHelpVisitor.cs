namespace Sequence.Core.LanguageServer;

/// <summary>
/// Visits SCL to get Signature Help
/// </summary>
public class SignatureHelpVisitor : SCLBaseVisitor<SignatureHelpResponse?>
{
    /// <inheritdoc />
    public SignatureHelpVisitor(
        LinePosition position,
        StepFactoryStore stepFactoryStore,
        DocumentationOptions documentationOptions)
    {
        Position             = position;
        StepFactoryStore     = stepFactoryStore;
        DocumentationOptions = documentationOptions;
    }

    /// <summary>
    /// The position to get Signature Help at
    /// </summary>
    public LinePosition Position { get; }

    /// <summary>
    /// The Step Factory Store
    /// </summary>
    public StepFactoryStore StepFactoryStore { get; }

    /// <summary>
    /// The Documentation Options to use
    /// </summary>
    public DocumentationOptions DocumentationOptions { get; }

    /// <inheritdoc />
    public override SignatureHelpResponse? Visit(IParseTree tree)
    {
        if (tree is ParserRuleContext context)
        {
            if (context.ContainsPosition(Position))
            {
                return base.Visit(tree);
            }
            else if (context.EndsBefore(Position) && !context.HasSiblingsAfter(Position))
            {
                //This position is at the end of this line - enter anyway
                var result = base.Visit(tree);
                return result;
            }
        }

        return DefaultResult;
    }

    /// <inheritdoc />
    protected override bool ShouldVisitNextChild(
        IRuleNode node,
        SignatureHelpResponse? currentResult)
    {
        if (currentResult is not null)
            return false;

        return true;
    }

    /// <inheritdoc />
    public override SignatureHelpResponse? VisitFunction(SCLParser.FunctionContext context)
    {
        var name = context.NAME().GetText();

        if (!context.ContainsPosition(Position))
        {
            if (context.EndsBefore(Position) &&
                context.Stop.IsSameLineAs(
                    Position
                )) //This position is on the line after the step definition
            {
                if (!StepFactoryStore.Dictionary.TryGetValue(name, out var stepFactory))
                    return null; //No clue what name to use

                var result = StepParametersSignatureHelp(stepFactory, DocumentationOptions);
                return result;
            }

            return null;
        }

        if (context.NAME().Symbol.ContainsPosition(Position))
        {
            return null;
        }

        var positionalTerms = context.term();

        for (var index = 0; index < positionalTerms.Length; index++)
        {
            var term = positionalTerms[index];

            if (term.ContainsPosition(Position))
            {
                return Visit(term);
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

                    //var range = namedArgumentContext.NAME().Symbol.GetRange();

                    return StepParametersSignatureHelp(stepFactory, DocumentationOptions);
                }

                return Visit(namedArgumentContext);
            }
        }

        {
            if (!StepFactoryStore.Dictionary.TryGetValue(name, out var stepFactory))
                return null; //No clue what name to use

            return StepParametersSignatureHelp(stepFactory, DocumentationOptions);
        }
    }

    private static SignatureHelpResponse StepParametersSignatureHelp(
        IStepFactory stepFactory,
        DocumentationOptions options)
    {
        var documentation = Helpers.GetMarkDownDocumentation(
            stepFactory,
            options
        );

        var parameters =
            stepFactory.ParameterDictionary.Keys
                .OfType<StepParameterReference.Named>()
                .Select(CreateSignatureHelpParameter)
                .ToList();

        static SignatureHelpParameter CreateSignatureHelpParameter(
            StepParameterReference.Named stepParameterReference)
        {
            return new(
                stepParameterReference.Name,
                stepParameterReference.Name,
                ""
            );
        }

        var signatureHelp = new SignatureHelpResponse(
            0,
            0,
            new List<SignatureHelpItem>()
            {
                new(stepFactory.TypeName, stepFactory.TypeName, documentation, parameters)
            }
        );

        return signatureHelp;
    }
}
