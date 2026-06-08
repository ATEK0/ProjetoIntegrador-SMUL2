using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Portal.Models;
using Portal.Models.ViewModels;

namespace Portal.Services
{
    public class SimulationService
    {
        private readonly HttpClient _httpClient;

        public SimulationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<SimulationResultViewModel> CalculateAsync(SimulationRequest request)
        {
            var result = new SimulationResultViewModel { Request = request };

            if (request.SimulationMode == "interest")
            {
                var tiers = request.GetEffectiveRateTiers();
                var type = request.Type is "simple" or "compound" ? request.Type : "simple";
                var rate = tiers.Count > 0 ? tiers[0].RatePercentage / 100.0 : request.Rate;

                string json;
                if (tiers.Count > 0)
                {
                    json = JsonSerializer.Serialize(new
                    {
                        principal = request.Principal,
                        rate,
                        time = request.Time,
                        type,
                        rate_tiers = tiers.Select(t => new { from_period = t.FromPeriod, rate = t.RatePercentage / 100.0 })
                    });
                }
                else
                {
                    json = JsonSerializer.Serialize(new
                    {
                        principal = request.Principal,
                        rate,
                        time = request.Time,
                        type
                    });
                }

                var response = await PostJsonAsync("api/v1/simulate/interest/", json);
                result.SimulationMode = "interest";
                result.InterestResult = await response.Content.ReadFromJsonAsync<InterestSimulationResponse>();
            }
            else if (request.SimulationMode == "amortization")
            {
                var type = request.Type is "french" or "sac" or "american" ? request.Type : "french";
                var json = JsonSerializer.Serialize(new
                {
                    principal = request.Principal,
                    rate = request.Rate,
                    years = request.Time,
                    periodicity = request.Periodicity,
                    commission = request.MonthlyCommission ?? 0.0,
                    type
                });

                var response = await PostJsonAsync("api/v1/simulate/amortization/", json);
                result.SimulationMode = "amortization";
                result.AmortizationResult = await response.Content.ReadFromJsonAsync<AmortizationSimulationResponse>();
            }

            return result;
        }

        private async Task<HttpResponseMessage> PostJsonAsync(string url, string json)
        {
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception(string.IsNullOrWhiteSpace(body) ? response.ReasonPhrase : body);
            }
            return response;
        }
    }
}
