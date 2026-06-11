using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Models.ViewModels;
using Portal.Services;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Portal.Controllers
{
    public class SimulatorController : Controller
    {
        private readonly SimulationService _simulationService;
        private readonly ApplicationDbContext _context;

        public SimulatorController(SimulationService simulationService, ApplicationDbContext context)
        {
            _simulationService = simulationService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index() => View(new SimulationRequest());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Calculate(SimulationRequest request)
        {
            if (!ModelState.IsValid) return View("Index", request);

            try
            {
                var result = await _simulationService.CalculateAsync(request);

                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    var claim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (claim != null && int.TryParse(claim.Value, out int studentId))
                    {
                        result.UserScenarios = await _context.Scenarios
                            .Where(s => s.StudentId == studentId)
                            .OrderByDescending(s => s.CreatedAt)
                            .ToListAsync();
                    }
                }

                return View("Results", result);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Erro ao comunicar com o microserviço: " + ex.Message);
                return View("Index", request);
            }
        }

        [HttpGet]
        public IActionResult Compare()
        {
            var request = new SimulationCompareRequest();
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Compare(SimulationCompareRequest request)
        {
            request.ProposalA.SimulationMode = "amortization";
            request.ProposalB.SimulationMode = "amortization";

            if (!ModelState.IsValid)
            {
                return View(request);
            }

            try
            {
                var vm = new SimulationCompareResultViewModel { Request = request };

                var taskA = _simulationService.CalculateAsync(request.ProposalA);
                var taskB = _simulationService.CalculateAsync(request.ProposalB);

                await Task.WhenAll(taskA, taskB);

                vm.ResultA = taskA.Result.AmortizationResult;
                vm.ResultB = taskB.Result.AmortizationResult;

                return View("CompareResults", vm);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Erro ao simular a comparação: " + ex.Message);
                return View(request);
            }
        }
    }
}
