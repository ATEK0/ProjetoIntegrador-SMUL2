using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Models;
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
        private readonly SimulationHistoryService _historyService;

        public SimulatorController(SimulationService simulationService, ApplicationDbContext context, SimulationHistoryService historyService)
        {
            _simulationService = simulationService;
            _context = context;
            _historyService = historyService;
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
                    if (claim != null && int.TryParse(claim.Value, out int userId))
                    {
                        result.UserScenarios = await _context.Scenarios
                            .Where(s => s.StudentId == userId)
                            .OrderByDescending(s => s.CreatedAt)
                            .ToListAsync();

                        await _historyService.SaveSimulationAsync(userId, request, result);
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

                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    var claim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (claim != null && int.TryParse(claim.Value, out int userId))
                    {
                        await _historyService.SaveCompareSimulationsAsync(userId, request, vm);
                    }
                }

                return View("CompareResults", vm);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Erro ao simular a comparação: " + ex.Message);
                return View(request);
            }
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || !int.TryParse(claim.Value, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            bool isAdmin = User.IsInRole("Admin");
            var history = isAdmin 
                ? await _historyService.GetAllHistoryAsync()
                : await _historyService.GetHistoryAsync(userId);

            return View(history);
        }

        [HttpGet]
        public async Task<IActionResult> HistoryDetails(int id)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || !int.TryParse(claim.Value, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            bool isAdmin = User.IsInRole("Admin");
            var result = await _historyService.GetHistoryDetailsAsync(id, userId, isAdmin);
            if (result == null)
            {
                return NotFound();
            }

            ViewBag.IsFromHistory = true;
            return View("Results", result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHistory(int id)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || !int.TryParse(claim.Value, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            bool isAdmin = User.IsInRole("Admin");
            var deleted = await _historyService.DeleteHistoryAsync(id, userId, isAdmin);
            if (deleted)
            {
                TempData["Success"] = "Registo de simulação eliminado com sucesso.";
            }

            return RedirectToAction("History");
        }
    }
}
