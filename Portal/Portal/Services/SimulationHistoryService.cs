using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Models;
using Portal.Models.ViewModels;

namespace Portal.Services
{
    public class SimulationHistoryService
    {
        private readonly ApplicationDbContext _context;

        public SimulationHistoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SimulationHistory>> GetHistoryAsync(int userId)
        {
            return await _context.SimulationHistories
                .Include(sh => sh.User)
                .Where(sh => sh.UserId == userId)
                .OrderByDescending(sh => sh.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<SimulationHistory>> GetAllHistoryAsync()
        {
            return await _context.SimulationHistories
                .Include(sh => sh.User)
                .OrderByDescending(sh => sh.CreatedAt)
                .ToListAsync();
        }

        public async Task SaveSimulationAsync(int userId, SimulationRequest request, SimulationResultViewModel result)
        {
            try
            {
                var parametersJson = System.Text.Json.JsonSerializer.Serialize(request);
                var resultJson = "";
                if (request.SimulationMode == "interest" && result.InterestResult != null)
                {
                    resultJson = System.Text.Json.JsonSerializer.Serialize(result.InterestResult);
                }
                else if (request.SimulationMode == "amortization" && result.AmortizationResult != null)
                {
                    resultJson = System.Text.Json.JsonSerializer.Serialize(result.AmortizationResult);
                }

                if (!string.IsNullOrEmpty(resultJson))
                {
                    var history = new SimulationHistory(
                        userId,
                        request.SimulationMode,
                        request.Principal ?? 0.0,
                        request.RatePercentage ?? 0.0,
                        request.Time ?? 0.0,
                        request.Type,
                        parametersJson,
                        resultJson
                    )
                    {
                        Periodicity = request.Periodicity,
                        MonthlyCommission = request.MonthlyCommission
                    };

                    _context.SimulationHistories.Add(history);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao guardar histórico de simulação: " + ex.Message);
            }
        }

        public async Task SaveCompareSimulationsAsync(int userId, SimulationCompareRequest request, SimulationCompareResultViewModel result)
        {
            try
            {
                if (result.ResultA != null)
                {
                    var paramsA = System.Text.Json.JsonSerializer.Serialize(request.ProposalA);
                    var resA = System.Text.Json.JsonSerializer.Serialize(result.ResultA);
                    var historyA = new SimulationHistory(
                        userId,
                        "amortization",
                        request.ProposalA.Principal ?? 0.0,
                        request.ProposalA.RatePercentage ?? 0.0,
                        request.ProposalA.Time ?? 0.0,
                        request.ProposalA.Type,
                        paramsA,
                        resA
                    )
                    {
                        Periodicity = request.ProposalA.Periodicity,
                        MonthlyCommission = request.ProposalA.MonthlyCommission
                    };
                    _context.SimulationHistories.Add(historyA);
                }

                if (result.ResultB != null)
                {
                    var paramsB = System.Text.Json.JsonSerializer.Serialize(request.ProposalB);
                    var resB = System.Text.Json.JsonSerializer.Serialize(result.ResultB);
                    var historyB = new SimulationHistory(
                        userId,
                        "amortization",
                        request.ProposalB.Principal ?? 0.0,
                        request.ProposalB.RatePercentage ?? 0.0,
                        request.ProposalB.Time ?? 0.0,
                        request.ProposalB.Type,
                        paramsB,
                        resB
                    )
                    {
                        Periodicity = request.ProposalB.Periodicity,
                        MonthlyCommission = request.ProposalB.MonthlyCommission
                    };
                    _context.SimulationHistories.Add(historyB);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao guardar histórico de comparação: " + ex.Message);
            }
        }

        public async Task<SimulationResultViewModel?> GetHistoryDetailsAsync(int id, int userId, bool isAdmin = false)
        {
            var query = _context.SimulationHistories.AsQueryable();
            if (!isAdmin)
            {
                query = query.Where(sh => sh.UserId == userId);
            }

            var simulation = await query.FirstOrDefaultAsync(sh => sh.Id == id);

            if (simulation == null)
            {
                return null;
            }

            var request = System.Text.Json.JsonSerializer.Deserialize<SimulationRequest>(simulation.ParametersJson);
            var result = new SimulationResultViewModel
            {
                SimulationMode = simulation.SimulationMode,
                Request = request ?? new SimulationRequest()
            };

            if (simulation.SimulationMode == "interest")
            {
                result.InterestResult = System.Text.Json.JsonSerializer.Deserialize<InterestSimulationResponse>(simulation.ResultJson);
            }
            else if (simulation.SimulationMode == "amortization")
            {
                result.AmortizationResult = System.Text.Json.JsonSerializer.Deserialize<AmortizationSimulationResponse>(simulation.ResultJson);
            }

            result.UserScenarios = await _context.Scenarios
                .Where(s => s.StudentId == simulation.UserId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return result;
        }

        public async Task<bool> DeleteHistoryAsync(int id, int userId, bool isAdmin = false)
        {
            var query = _context.SimulationHistories.AsQueryable();
            if (!isAdmin)
            {
                query = query.Where(sh => sh.UserId == userId);
            }

            var simulation = await query.FirstOrDefaultAsync(sh => sh.Id == id);

            if (simulation == null)
            {
                return false;
            }

            simulation.MarkAsDeleted();
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
