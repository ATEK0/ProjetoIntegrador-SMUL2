using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Portal.Models
{
    public class InterestRateTier
    {
        [Range(1, int.MaxValue, ErrorMessage = "O período inicial deve ser pelo menos 1.")]
        [Display(Name = "A partir do período")]
        public int FromPeriod { get; set; } = 1;

        [Range(0.0, 100.0, ErrorMessage = "A taxa deve estar entre 0 e 100%.")]
        [Display(Name = "Taxa (%)")]
        public double RatePercentage { get; set; }
    }

    public class SimulationRequest
    {
        [Required(ErrorMessage = "Selecione o tipo de simulação.")]
        [Display(Name = "Tipo de Simulação")]
        public string SimulationMode { get; set; } = "interest"; // "interest" ou "amortization"

        [Required(ErrorMessage = "Indique o capital.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O capital deve ser maior que 0.")]
        [Display(Name = "Capital Inicial / Financiado (€)")]
        public double Principal { get; set; }

        [Range(0.0, 100.0, ErrorMessage = "A taxa de juro deve estar entre 0 e 100%.")]
        [Display(Name = "Taxa de Juro (em %)")]
        public double RatePercentage { get; set; }

        public double Rate => RatePercentage / 100.0;

        [Required(ErrorMessage = "Indique o número de períodos.")]
        [Range(1, int.MaxValue, ErrorMessage = "O tempo/período deve ser maior que 0.")]
        [Display(Name = "Tempo / N.º de Períodos")]
        public double Time { get; set; }

        [Required(ErrorMessage = "Selecione o regime.")]
        [Display(Name = "Regime Específico")]
        public string Type { get; set; } = "simple"; // simple/compound ou french/sac/american

        [Display(Name = "Taxas variáveis por período")]
        public bool UseVariableRates { get; set; }

        public List<InterestRateTier> RateTiers { get; set; } = new();

        public IEnumerable<InterestRateTier> GetEffectiveRateTiers()
        {
            if (!UseVariableRates || RateTiers == null)
                return Enumerable.Empty<InterestRateTier>();

            return RateTiers
                .Where(t => t.FromPeriod >= 1 && t.RatePercentage >= 0)
                .OrderBy(t => t.FromPeriod)
                .ToList();
        }
    }

    public class InterestPeriodBreakdown
    {
        public int Period { get; set; }
        public double Rate { get; set; }
        public double Interest { get; set; }
        public double Amount { get; set; }
        public bool Partial { get; set; }
    }

    public class InterestSimulationResponse
    {
        public double Interest { get; set; }
        public double Total_amount { get; set; }
        public List<InterestPeriodBreakdown> Breakdown { get; set; }
    }

    public class AmortizationPeriod
    {
        public int Period { get; set; }
        public double Installment { get; set; }
        public double Interest { get; set; }
        public double Amortization { get; set; }
        public double Balance { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public object Errors { get; set; }
    }
}
