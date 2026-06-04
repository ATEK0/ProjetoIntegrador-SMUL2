using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Portal.Controllers
{
    public class SimulatorController : Controller
    {
        public const string MsCafinHttpClientName = "MsCafin";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly JsonSerializerOptions _jsonWriteOptions;

        public SimulatorController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _jsonWriteOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new SimulationRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Calculate(SimulationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", request);
            }

            if (request.SimulationMode == "interest")
            {
                if (request.Type is not ("simple" or "compound"))
                    request.Type = "simple";

                if (request.UseVariableRates)
                {
                    var tiers = request.GetEffectiveRateTiers().ToList();
                    if (tiers.Count == 0)
                    {
                        ModelState.AddModelError(nameof(request.RateTiers), "Adicione pelo menos uma faixa de taxa.");
                        return View("Index", request);
                    }
                    if (tiers[0].FromPeriod != 1)
                    {
                        ModelState.AddModelError(nameof(request.RateTiers), "A primeira faixa deve começar no período 1.");
                        return View("Index", request);
                    }
                    if (tiers.Any(t => t.FromPeriod > request.Time))
                    {
                        ModelState.AddModelError(nameof(request.RateTiers), "Nenhuma faixa pode começar depois do número total de períodos.");
                        return View("Index", request);
                    }
                    request.RatePercentage = tiers[0].RatePercentage;
                }
                else if (request.RatePercentage < 0)
                {
                    ModelState.AddModelError(nameof(request.RatePercentage), "Indique a taxa de juro.");
                    return View("Index", request);
                }
            }

            if (request.SimulationMode == "amortization" &&
                request.Type is not ("french" or "sac" or "american"))
            {
                request.Type = "french";
            }

            var httpClient = _httpClientFactory.CreateClient(MsCafinHttpClientName);

            try
            {
                if (request.SimulationMode == "interest")
                {
                    var result = await SimulateInterestAsync(httpClient, request);
                    ViewBag.SimulationMode = "interest";
                    ViewBag.InterestResult = result;
                }
                else if (request.SimulationMode == "amortization")
                {
                    var result = await SimulateAmortizationAsync(httpClient, request);
                    ViewBag.SimulationMode = "amortization";
                    ViewBag.AmortizationResult = result;
                }

                ViewBag.RequestData = request;
                return View("Results");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Erro ao comunicar com o microserviço de simulação: " + ex.Message);
                return View("Index", request);
            }
        }

        private async Task<InterestSimulationResponse> SimulateInterestAsync(HttpClient httpClient, SimulationRequest request)
        {
            var tiers = request.GetEffectiveRateTiers().ToList();
            var payload = new Dictionary<string, object>
            {
                ["principal"] = request.Principal,
                ["rate"] = request.Rate,
                ["time"] = request.Time,
                ["type"] = request.Type,
            };

            if (tiers.Count > 0)
            {
                payload["rate_tiers"] = tiers.Select(t => new
                {
                    from_period = t.FromPeriod,
                    rate = t.RatePercentage / 100.0
                }).ToList();
            }

            var response = await httpClient.PostAsJsonAsync("api/v1/simulate/interest/", payload, _jsonWriteOptions);
            await EnsureSuccessOrThrowAsync(response);

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<InterestSimulationResponse>>(_jsonOptions);
            return apiResponse?.Data ?? throw new Exception("Resposta vazia do microserviço.");
        }

        private async Task<List<AmortizationPeriod>> SimulateAmortizationAsync(HttpClient httpClient, SimulationRequest request)
        {
            var payload = new
            {
                principal = request.Principal,
                rate = request.Rate,
                periods = (int)request.Time,
                type = request.Type
            };

            var response = await httpClient.PostAsJsonAsync("api/v1/simulate/amortization/", payload, _jsonWriteOptions);
            await EnsureSuccessOrThrowAsync(response);

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<AmortizationPeriod>>>(_jsonOptions);
            return apiResponse?.Data ?? throw new Exception("Resposta vazia do microserviço.");
        }

        private static async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return;

            var body = await response.Content.ReadAsStringAsync();
            var message = TryExtractApiError(body) ?? response.ReasonPhrase;
            throw new Exception($"{(int)response.StatusCode} ({response.StatusCode}): {message}");
        }

        private static string TryExtractApiError(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return null;
            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                if (root.TryGetProperty("errors", out var errors) && errors.ValueKind != JsonValueKind.Null)
                {
                    if (errors.ValueKind == JsonValueKind.Array)
                        return string.Join(" ", errors.EnumerateArray().Select(e => e.ToString()));
                    if (errors.ValueKind == JsonValueKind.Object)
                        return string.Join(" ", errors.EnumerateObject().Select(p => $"{p.Name}: {p.Value}"));
                    return errors.ToString();
                }
                if (root.TryGetProperty("error", out var single))
                    return single.ToString();
            }
            catch
            {
                return body.Length > 300 ? body[..300] + "…" : body;
            }
            return null;
        }
    }
}
