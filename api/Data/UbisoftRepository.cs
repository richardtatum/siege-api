using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using api.Interfaces;
using api.Models;
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
        private readonly INoSqlProvider _db;
        private readonly CancellationTokenSource _source;
        private readonly CancellationToken _token;

        public UbisoftRepository(IOptions<UbisoftConfig> config, ILogger<UbisoftRepository> log, INoSqlProvider db)
        {
            _log = log;
            _config = config.Value;
            _db = db;
            _source = new CancellationTokenSource();
            _token = _source.Token;
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

        public async Task<T> Execute<T>(IRestRequest request, string auth = null) where T : class
        {
            var client = new RestClient(_config.BaseUrl);

            if (string.IsNullOrEmpty(auth))
                auth = await ValidAuth();
            
            client.AddDefaultHeaders(new Dictionary<string, string>
            {
                {"Authorization", auth},
                {"Ubi-AppId", _config.AppId},
                {"Content-Type", "application/json"}
            });

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

        public async Task<ProfileResponse> GetProfile(string platform, string username)
        {
            var request = new RestRequest("v2/profiles", Method.GET);

            request.AddQueryParameter("platformType", platform);
            request.AddQueryParameter("nameOnPlatform", username);

            return await Execute<ProfileResponse>(request);
        }

        public async Task<GeneralStatsResponse> GetStats(string users, string platform = "uplay")
        {
            var request = new RestRequest(_config.PlatformUrl(platform), Method.GET);

            request.AddQueryParameter("populations", users);
            request.AddQueryParameter("statistics", StatisticsString);

            return await Execute<GeneralStatsResponse>(request);
        }

        public async Task<Ticket> GetTicket()
        {
            var request = new RestRequest("v3/profiles/sessions", Method.POST);
            return await Execute<Ticket>(request, GetKey());
        }

        private string GetKey()
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes($"{_config.Username}:{_config.Password}");
            return Convert.ToBase64String(bytes);
        }

        private async Task<string> ValidAuth()
        {
            // Try to get the ticket from the db
            var ticket = new Ticket();
            try
            {
                ticket = await _db.GetItemAsync<Ticket>(ticket.Id, _token);
            }
            catch (Exception e)
            {
                _source.Cancel();
                Console.WriteLine(e);
                throw;
            }

            if (ticket != null && ticket.Valid) return ticket.Content;

            // If not valid/present, get a new ticket
            ticket = await GetTicket();
            try
            {
                await _db.UpsertItemAsync(ticket, _token);
            }
            catch (Exception e)
            {
                _source.Cancel();
                Console.WriteLine(e);
                throw;
            }

            return ticket.Content;
        }
    }
}
