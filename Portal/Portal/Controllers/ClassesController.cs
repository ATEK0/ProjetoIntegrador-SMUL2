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

        [HttpPost]
        public async Task<IActionResult> Join(string membershipCode)
        {
            var id = GetUserId();
            if (id == null) return Unauthorized();

            var error = await _classService.JoinClassAsync(id.Value, membershipCode);
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction("Aluno", "Dashboard");
            }

            TempData["Success"] = "Inscrição na turma realizada com sucesso!";
            return RedirectToAction("Aluno", "Dashboard");
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


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var viewModel = await _classService.GetClassDetailsAsync(id);
            if (viewModel == null)
            {
                TempData["Error"] = "Turma não encontrada.";
                return RedirectToAction("AdminClasses", "Dashboard");
            }
            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, string name, int teacherId)
        {
            var error = await _classService.EditClassAsync(id, name, teacherId);
            if (error != null) TempData["Error"] = error;
            else TempData["Success"] = "Detalhes da turma atualizados com sucesso.";
            return RedirectToAction("Details", new { id = id });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var error = await _classService.DeleteClassAsync(id);
            if (error != null) TempData["Error"] = error;
            else TempData["Success"] = "Turma eliminada com sucesso.";
            return RedirectToAction("AdminClasses", "Dashboard");
        }
    }
}
