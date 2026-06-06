using Microsoft.AspNetCore.Mvc;
using Portal.Models;
using Portal.Models.ViewModels;
using Portal.Services;
using System;
using System.Threading.Tasks;

namespace Portal.Controllers
{
    public class SimulatorController : Controller
    {
        private readonly SimulationService _simulationService;

        public SimulatorController(SimulationService simulationService)
        {
            _simulationService = simulationService;
        }

        [HttpGet]
        public IActionResult Index() => View(new SimulationRequest());

        [HttpPost]
        public async Task<IActionResult> Calculate(SimulationRequest request)
        {
            if (!ModelState.IsValid) return View("Index", request);

            try
            {
                return View("Results", await _simulationService.CalculateAsync(request));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Erro ao comunicar com o microserviço: " + ex.Message);
                return View("Index", request);
            }
        }
    }
}
