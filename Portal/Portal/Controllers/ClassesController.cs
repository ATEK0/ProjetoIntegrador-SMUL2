using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Portal.Models.ViewModels;
using Portal.Services;
using Portal;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Portal.Controllers
{
    [Authorize]
    public class ClassesController : BaseController
    {
        private readonly ClassService _classService;
        private readonly ILogger<ClassesController> _logger;

        public ClassesController(ClassService classService, ILogger<ClassesController> logger)
        {
            _classService = classService;
            _logger = logger;
        }

        [HttpGet, Authorize(Roles = "Admin")]
        public IActionResult Create() => RedirectToAction("AdminClasses", "Dashboard");

        public async Task<IActionResult> Index()
        {
            var id = GetUserId();
            if (id == null) return Unauthorized();
            return View(await _classService.GetTeacherClassesAsync(id.Value));
        }

        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateClassViewModel model)
        {
            var id = GetUserId();
            if (id == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                ToastMessages.SetErrors(this);
                return RedirectToAction("AdminClasses", "Dashboard");
            }

            try
            {
                await _classService.CreateClassAsync(id.Value, model.Name);
                TempData["Success"] = "Turma criada com sucesso!";
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar turma");
                TempData["Error"] = $"Erro ao criar turma: {ex.Message}";
            }

            return RedirectToAction("AdminClasses", "Dashboard");
        }

        public IActionResult Join() => View();

        [HttpPost]
        public async Task<IActionResult> Join(string membershipCode)
        {
            var id = GetUserId();
            if (id == null) return Unauthorized();

            var error = await _classService.JoinClassAsync(id.Value, membershipCode);
            if (error != null)
            {
                ModelState.AddModelError("", error);
                return View();
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<IActionResult> EnrollUser(int userId, int classId)
        {
            var error = await _classService.EnrollUserAsync(userId, classId);
            TempData[error == null ? "Success" : "Error"] = error ?? "Utilizador inscrito com sucesso!";
            return RedirectToAction("AdminUsers", "Dashboard");
        }

        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveUserFromClass(int userId, int classId)
        {
            var error = await _classService.RemoveUserFromClassAsync(userId, classId);
            TempData[error == null ? "Success" : "Error"] = error ?? "Utilizador removido da turma!";
            return RedirectToAction("AdminUsers", "Dashboard");
        }


    }
}
