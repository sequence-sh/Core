using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Flurl.Http;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps.REST
{

/// <summary>
/// Contains methods to help with Flurl Requests
/// </summary>
public static class FlurlHelpers
{
    /// <summary>
    /// Add entity properties as headers to this request
    /// </summary>
    public static IFlurlRequest AddHeaders(this IFlurlRequest request, Entity entity)
    {
        foreach (var entityProperty in entity.Dictionary.Values)
        {
            request = request.WithHeader(
                entityProperty.Name,
                entityProperty.BestValue.GetPrimitiveString()
            );
        }

        return request;
    }

    /// <summary>
    /// Execute this request. Returns an error if it fails
    /// </summary>
    public static async Task<Result<T, IErrorBuilder>> TryRun<T>(
        this IFlurlRequest request,
        Func<IFlurlRequest, Task<T>> func)
    {
        T result;

        try
        {
            result = await func(request);
        }
        catch (FlurlHttpException flurlHttpException)
        {
            string responseMessage;

            try
            {
                IDictionary<string, object?> responseDict =
                    await flurlHttpException.GetResponseJsonAsync();

                if (responseDict.TryGetValue("Message", out var v))
                    responseMessage = v?.ToString() ?? "";
                else
                {
                    responseMessage = "";
                }
            }
            catch (Exception)
            {
                responseMessage = "";
            }

            return ErrorCode.RequestFailed
                .ToErrorBuilder(
                    flurlHttpException.StatusCode ?? 0,
                    flurlHttpException.Message,
                    responseMessage
                );
        }

        catch (Exception ex)
        {
            return ErrorCode.Unknown.ToErrorBuilder(ex);
        }

        return result;
    }
}

}
