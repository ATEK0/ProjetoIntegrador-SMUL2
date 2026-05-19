using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Portal.Data;
using Portal.Models;
using Portal.Models.ViewModels;
using System;
using System.Linq;

using Microsoft.Extensions.Logging;

namespace Portal.Controllers
{
    [Authorize]
    public class ClassesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClassesController> _logger;

        public ClassesController(ApplicationDbContext context, ILogger<ClassesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 1;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return Redirect("/admin/classes");
        }

        public IActionResult Index()
        {
            int currentTeacherId = GetUserId();

            var myClasses = _context.Classes
                .Where(c => c.TeacherId == currentTeacherId)
                .Select(c => new TeacherClassViewModel
                {
                    ClassId = c.Id,
                    ClassName = c.Name,
                    MembershipCode = c.MembershipCode,
                    StudentNames = _context.ClassEnrollments
                        .Where(ce => ce.ClassId == c.Id)
                        .Select(ce => ce.Student.Name)
                        .ToList()
                })
                .ToList();

            return View(myClasses);
        }

        [HttpPost]
        public IActionResult Create(CreateClassViewModel model)
        {
            int teacherId = GetUserId();

            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage
                                 ?? "O nome da turma é obrigatório.";
                _logger.LogWarning("Professor ID {TeacherId} tentou criar turma com dados inválidos. Erro: {Error}", teacherId, firstError);
                TempData["Error"] = firstError;
                return Redirect("/admin/classes");
            }

            _logger.LogInformation("Professor ID {TeacherId} está a tentar criar a turma '{ClassName}'", teacherId, model.Name);

            try
            {
                string code = GenerateMembershipCode();

                var newClass = new SchoolClass(teacherId, model.Name, code);
                _context.Classes.Add(newClass);
                _context.SaveChanges();

                _logger.LogInformation("Turma '{ClassName}' criada com sucesso por Professor ID {TeacherId}. Código gerado: {Code}", model.Name, teacherId, code);

                TempData["Success"] = "Turma criada com sucesso!";
                return Redirect("/admin/classes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro grave ao criar turma '{ClassName}' por Professor ID {TeacherId}", model.Name, teacherId);
                TempData["Error"] = $"Erro ao criar turma: {ex.Message}";
                return Redirect("/admin/classes");
            }
        }

        private string GenerateMembershipCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();

            string code;
            do
            {
                code = new string(Enumerable.Repeat(chars, 6)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            }
            while (_context.Classes.Any(c => c.MembershipCode == code));

            return code;
        }

        public IActionResult Join()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Join(string membershipCode)
        {
            int studentId = GetUserId();

            if (string.IsNullOrWhiteSpace(membershipCode))
            {
                _logger.LogWarning("Aluno ID {StudentId} submeteu código de adesão em branco.", studentId);
                ModelState.AddModelError("MembershipCode", "O código de adesão é obrigatório.");
                return View();
            }

            _logger.LogInformation("Aluno ID {StudentId} a tentar aderir com o código: '{Code}'", studentId, membershipCode);

            var targetClass = _context.Classes
                .FirstOrDefault(c => c.MembershipCode == membershipCode.Trim().ToUpper());

            if (targetClass == null)
            {
                _logger.LogWarning("Aluno ID {StudentId} falhou ao aderir: código '{Code}' não existe na BD.", studentId, membershipCode);
                ModelState.AddModelError("", "Código inválido. Não foi encontrada nenhuma turma ou desafio com este código.");
                return View();
            }

            bool alreadyJoined = _context.ClassEnrollments
                .Any(ce => ce.ClassId == targetClass.Id && ce.StudentId == studentId);

            if (alreadyJoined)
            {
                _logger.LogWarning("Aluno ID {StudentId} tentou aderir à turma '{ClassName}' (ID: {ClassId}), mas já estava inscrito.", studentId, targetClass.Name, targetClass.Id);
                ModelState.AddModelError("", "Já estás inscrito nesta turma / desafio!");
                return View();
            }

            try
            {
                var newEnrollment = new ClassEnrollment(targetClass.Id, studentId);

                _context.ClassEnrollments.Add(newEnrollment);
                _context.SaveChanges();

                _logger.LogInformation("Aluno ID {StudentId} aderiu com sucesso à turma '{ClassName}' (ID: {ClassId}) usando o código '{Code}'", studentId, targetClass.Name, targetClass.Id, membershipCode);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro grave ao inscrever Aluno ID {StudentId} na turma '{ClassName}' (ID: {ClassId})", studentId, targetClass.Name, targetClass.Id);
                ModelState.AddModelError("", $"Erro inesperado ao aderir à turma: {ex.Message}");
                return View();
            }
        }
    }
}
