using System.Net.Http.Headers;
using System.Text;
using eDereva.Application.Services;
using eDereva.Domain.Contracts.Responses;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace eDereva.Infrastructure.Services;

public class NidaService(ILogger<NidaService> logger) : INidaService
{
    private const string BaseUrl = "https://ors.brela.go.tz/um/load/load_nida/{0}";

    public async Task<NidaUserDataResponse> RetrieveUserData(string nin, CancellationToken cancellationToken)
    {
        try
        {
            var httpClient = new HttpClient();

            var requestUrl = string.Format(BaseUrl, nin);

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            requestMessage.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

            requestMessage.Headers.Accept.Clear();
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await httpClient.SendAsync(requestMessage, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError(
                    "Failed to retrieve user data from NIDA API. Status code: {StatusCode}, Reason: {ReasonPhrase}",
                    response.StatusCode, response.ReasonPhrase);
                return null!;
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseJson = JsonConvert.DeserializeObject<dynamic>(responseContent);

            if (responseJson?.obj?.result is not null)
            {
                var userDataJson = responseJson.obj.result.ToString();
                var userData = JsonConvert.DeserializeObject<NidaUserDataResponse>(userDataJson);

                logger.LogInformation("Successfully retrieved user data for NIN: {NIN}", nin);
                return userData;
            }

            var error = responseJson?.obj?.error ?? "Unknown error";
            logger.LogInformation("No result found in NIDA response for NIN: {NIN}. Error: {Error}", nin,
                (string)error);
            return null!;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while loading user data for NIN: {NIN}", nin);
            return null!;
        }
    }
}