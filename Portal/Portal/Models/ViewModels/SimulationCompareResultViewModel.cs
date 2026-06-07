using System;
using System.Linq;
using Portal.Models;

namespace Portal.Models.ViewModels
{
    public class SimulationCompareResultViewModel
    {
        public SimulationCompareRequest Request { get; set; } = new();
        public AmortizationSimulationResponse? ResultA { get; set; }
        public AmortizationSimulationResponse? ResultB { get; set; }

        public decimal TotalPagoA => (decimal)(ResultA?.Schedule?.Sum(x => x.TotalPago) ?? 0) + (decimal)(ResultA?.ImpostoSeloInicial ?? 0);
        public decimal TotalPagoB => (decimal)(ResultB?.Schedule?.Sum(x => x.TotalPago) ?? 0) + (decimal)(ResultB?.ImpostoSeloInicial ?? 0);

        public decimal TotalAmortA => (decimal)(ResultA?.Schedule?.Sum(x => x.Amortization) ?? 0);
        public decimal TotalAmortB => (decimal)(ResultB?.Schedule?.Sum(x => x.Amortization) ?? 0);

        public decimal TotalJurosA => (decimal)(ResultA?.Schedule?.Sum(x => x.Interest) ?? 0);
        public decimal TotalJurosB => (decimal)(ResultB?.Schedule?.Sum(x => x.Interest) ?? 0);

        public decimal TotalImpostosComissoesA => TotalPagoA - TotalAmortA - TotalJurosA;
        public decimal TotalImpostosComissoesB => TotalPagoB - TotalAmortB - TotalJurosB;

        public decimal PrestacaoA => (decimal)(ResultA?.Schedule?.FirstOrDefault(x => x.Period == 1)?.TotalPago ?? 0);
        public decimal PrestacaoB => (decimal)(ResultB?.Schedule?.FirstOrDefault(x => x.Period == 1)?.TotalPago ?? 0);

        // Retorna "A" se A for mais barato, "B" se B for mais barato, ou "Empate"
        public string Winner => TotalPagoA < TotalPagoB ? "A" : (TotalPagoB < TotalPagoA ? "B" : "Empate");
        
        public decimal Difference => Math.Abs(TotalPagoA - TotalPagoB);
    }
}
