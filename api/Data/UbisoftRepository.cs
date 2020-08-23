using System;
using System.Net;
using api.Models.Ubisoft;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using RestSharp;

namespace api.Data
{
    public class UbisoftRepository
    {
        private readonly UbisoftConfig _config;
        private readonly ILogger<UbisoftRepository> _log;

        public UbisoftRepository(IOptions<UbisoftConfig> config, ILogger<UbisoftRepository> log)
        {
            _log = log;
            _config = config.Value;
        }

        private string[] Statistics => new[]
        {
            "generalpvp_timeplayed",
            "generalpvp_matchplayed",
            "generalpvp_matchwon",
            "generalpvp_matchlost",
            "generalpvp_kills",
            "generalpvp_death"
        };

        private string StatisticsString => string.Join(",", Statistics);

        public T Execute<T>(IRestRequest request) where T : class
        {
            var client = new RestClient(_config.BaseUrl);

            var response = Policy
                .HandleResult<IRestResponse<T>>(m => m.StatusCode != HttpStatusCode.OK)
                .WaitAndRetry(
                    3,
                    i => TimeSpan.FromSeconds(2),
                    (result, timeSpan, retryCount, context) =>
                    {
                        _log.LogError($"Request failed to /{result.Result.Request.Resource}. Error: {result.Result.StatusCode}");
                    })
                .Execute(() => client.Execute<T>(request));

            return response.Data;
        }

        public ProfileResponse GetProfile(string platform, string username)
        {
            var request = new RestRequest("v2/profiles", Method.GET);

            request.AddQueryParameter("platformType", platform);
            request.AddQueryParameter("nameOnPlatform", username);

            return Execute<ProfileResponse>(request);
        }

        public GeneralStatsResponse GetStats(string users, string platform = "uplay")
        {
            var request = new RestRequest(_config.PlatformUrl(platform));

            request.AddQueryParameter("populations", users);
            request.AddQueryParameter("statistics", StatisticsString);

            return Execute<GeneralStatsResponse>(request);

        }
    }
}
