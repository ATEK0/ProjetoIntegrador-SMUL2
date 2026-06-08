using System.Collections.Generic;
using System.Linq;
using Portal.Models;

namespace Portal.Models.ViewModels
{
    public class SimulationResultViewModel
    {
        public string SimulationMode { get; set; } = "interest";
        public SimulationRequest Request { get; set; } = new();
        public InterestSimulationResponse? InterestResult { get; set; }
        public List<AmortizationPeriod>? AmortizationResult { get; set; }

        public string RegimeLabel => Request.Type?.ToLower() switch
        {
            "simple" => "Juros simples",
            "compound" => "Juros compostos",
            "french" => "Sistema Francês",
            "sac" => "Sistema SAC",
            "american" => "Sistema Americano",
            _ => Request.Type ?? "—"
        };

        public bool HasVariableRates =>
            Request.HasVariableRates
            || (InterestResult?.Breakdown != null && InterestResult.Breakdown.Count > 0);

        public string RateSummary
        {
            get
            {
                var tiers = Request.GetEffectiveRateTiers();
                if (tiers.Count > 0)
                    return string.Join(" · ", tiers.Select(t => $"desde P{t.FromPeriod}: {t.RatePercentage:N2}%"));

                if (InterestResult?.Breakdown != null && InterestResult.Breakdown.Count > 0)
                    return string.Join(" · ", InterestResult.Breakdown
                        .GroupBy(b => b.Rate)
                        .Select(g => $"{(g.Key * 100):N2}% (P{string.Join(", P", g.Select(b => b.Period))})"));

                return $"{Request.RatePercentage:N2} %";
            }
        }
    }
}
