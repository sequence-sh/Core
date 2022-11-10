namespace Sequence.Core.Steps;

/// <summary>
/// Gets information about connectors
/// </summary>
[Alias("GetConnectorVersion")]
[TypeReferenceSchema(
    @"{
""type"": ""array"",
""items"": {
    ""type"": ""object"",
    ""required"": [ ""Id"", ""Version"" ],
   ""properties"": {
    ""Id"": {
      ""type"": ""string""
    },
    ""Version"": {
      ""type"": ""string""
    }
}
}}"
)]
public sealed class GetConnectorInformation : CompoundStep<Array<Entity>>
{
    /// <inheritdoc />
    #pragma warning disable CS1998
    protected override async ValueTask<Result<Array<Entity>, IError>> Run(
        #pragma warning restore CS1998
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var entities = new List<Entity>();

        foreach (var (connectorSettings, _) in stateMonad.StepFactoryStore.ConnectorData)
        {
            var entity = Entity.Create(
                (nameof(ConnectorSettings.Id), new StringStream(connectorSettings.Id)),
                (nameof(ConnectorSettings.Version), new StringStream(connectorSettings.Version))
            );

            entities.Add(entity);
        }

        return entities.ToSCLArray();
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<GetConnectorInformation, Array<Entity>>();
}
