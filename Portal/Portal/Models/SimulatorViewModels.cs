using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;

namespace Portal.Models
{
    public class InterestRateTier
    {
        [Range(1, int.MaxValue, ErrorMessage = "O período inicial deve ser pelo menos 1.")]
        public int FromPeriod { get; set; } = 1;

        [Range(0.0, 100.0, ErrorMessage = "A taxa deve estar entre 0 e 100%.")]
        public double RatePercentage { get; set; }
    }

    public class SimulationRequest
    {
        [Required(ErrorMessage = "Selecione o tipo de simulação.")]
        public string SimulationMode { get; set; } = "interest";

        [Required(ErrorMessage = "Indique o capital.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O capital deve ser maior que 0.")]
        public double Principal { get; set; }

        [Range(0.0, 100.0, ErrorMessage = "A taxa de juro deve estar entre 0 e 100%.")]
        public double RatePercentage { get; set; }

        public double Rate => RatePercentage / 100.0;

        [Required(ErrorMessage = "Indique o número de períodos.")]
        [Range(1, int.MaxValue, ErrorMessage = "O tempo/período deve ser maior que 0.")]
        public double Time { get; set; }

        [Required(ErrorMessage = "Selecione o regime.")]
        public string Type { get; set; } = "simple";

        public bool UseVariableRates { get; set; }

        public List<InterestRateTier> RateTiers { get; set; } = new();

        public List<InterestRateTier> GetEffectiveRateTiers()
        {
            if (!UseVariableRates || RateTiers == null || RateTiers.Count == 0)
                return new();

            return RateTiers
                .Where(t => t.FromPeriod >= 1 && t.RatePercentage >= 0)
                .OrderBy(t => t.FromPeriod)
                .ToList();
        }

        public bool HasVariableRates => GetEffectiveRateTiers().Count > 0;
    }

    public class InterestSimulationResponse
    {
        [JsonPropertyName("interest")]
        public double Interest { get; set; }

        [JsonPropertyName("total_amount")]
        public double TotalAmount { get; set; }

        [JsonPropertyName("breakdown")]
        public List<InterestPeriodBreakdown>? Breakdown { get; set; }
    }

    public class InterestPeriodBreakdown
    {
        [JsonPropertyName("period")]
        public int Period { get; set; }

        [JsonPropertyName("rate")]
        public double Rate { get; set; }

        [JsonPropertyName("interest")]
        public double Interest { get; set; }

        [JsonPropertyName("amount")]
        public double Amount { get; set; }

        [JsonPropertyName("partial")]
        public bool Partial { get; set; }
    }

    public class AmortizationPeriod
    {
        [JsonPropertyName("period")]
        public int Period { get; set; }

        [JsonPropertyName("installment")]
        public double Installment { get; set; }

        [JsonPropertyName("interest")]
        public double Interest { get; set; }

        [JsonPropertyName("amortization")]
        public double Amortization { get; set; }

        [JsonPropertyName("balance")]
        public double Balance { get; set; }
    }
}
