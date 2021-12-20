namespace Reductech.Sequence.Core.Steps.REST;

/// <summary>
/// Contains methods to help with Flurl Requests
/// </summary>
public static class RestHelpers
{
    /// <summary>
    /// Add entity properties as headers to this request
    /// </summary>
    public static IRestRequest AddHeaders(this IRestRequest request, Entity entity)
    {
        foreach (var (name, sclObject, _) in entity.Dictionary.Values)
        {
            request = request.AddHeader(
                name,
                sclObject.Serialize(SerializeOptions.Primitive)
            );
        }

        return request;
    }

    /// <summary>
    /// Execute this request. Returns an error if it fails
    /// </summary>
    public static async Task<Result<string, IErrorBuilder>> TryRun(
        this IRestRequest request,
        IRestClient client,
        CancellationToken cancellationToken)
    {
        IRestResponse response;

        try
        {
            response = await client.ExecuteAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            return ErrorCode.Unknown.ToErrorBuilder(ex);
        }

        if (response.IsSuccessful)
            return response.Content;

        return ErrorCode.RequestFailed
            .ToErrorBuilder(response.StatusCode, response.StatusDescription, response.ErrorMessage);
    }
}
