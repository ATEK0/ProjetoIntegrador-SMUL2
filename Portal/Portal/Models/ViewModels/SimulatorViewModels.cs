using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;

namespace Portal.Models.ViewModels
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
        public double? Principal { get; set; }

        [Required(ErrorMessage = "A taxa de juro é obrigatória.")]
        [Range(0.0, 100.0, ErrorMessage = "A taxa de juro deve estar entre 0 e 100%.")]
        public double? RatePercentage { get; set; }

        public double Rate => (RatePercentage ?? 0) / 100.0;

        [Required(ErrorMessage = "Indique o tempo/prazo.")]
        [Range(0.1, int.MaxValue, ErrorMessage = "O tempo/prazo deve ser maior que 0.")]
        public double? Time { get; set; }

        public string Periodicity { get; set; } = "monthly";

        [Range(0.0, double.MaxValue, ErrorMessage = "A comissão não pode ser negativa.")]
        public double? MonthlyCommission { get; set; }

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

    public class AmortizationSimulationResponse
    {
        [JsonPropertyName("schedule")]
        public List<AmortizationPeriod>? Schedule { get; set; }

        [JsonPropertyName("taeg")]
        public double TAEG { get; set; }

        [JsonPropertyName("is_inicial")]
        public double ImpostoSeloInicial { get; set; }
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

        [JsonPropertyName("imposto_selo")]
        public double ImpostoSelo { get; set; }

        [JsonPropertyName("comissao")]
        public double Comissao { get; set; }

        [JsonPropertyName("total_pago")]
        public double TotalPago { get; set; }

        [JsonPropertyName("balance")]
        public double Balance { get; set; }
    }

    public class SimulationCompareRequest
    {
        public SimulationRequest ProposalA { get; set; } = new() { SimulationMode = "amortization" };
        public SimulationRequest ProposalB { get; set; } = new() { SimulationMode = "amortization" };
    }
}
