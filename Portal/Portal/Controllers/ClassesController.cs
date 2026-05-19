using Microsoft.AspNetCore.Mvc;
using Portal.Data;
using Portal.Models;
using Portal.Models.ViewModels;

namespace Portal.Controllers
{
    public class ClassesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClassesController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Create()
        {
            return Redirect("/admin/classes");
        }

        [HttpPost]
        public IActionResult Create(CreateClassViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage
                                 ?? "O nome da turma é obrigatório.";
                TempData["Error"] = firstError;
                return Redirect("/admin/classes");
            }

            try
            {
                string code = GenerateMembershipCode();

                int teacherId = 1;
                var newClass = new SchoolClass(teacherId, model.Name, code);
                _context.Classes.Add(newClass);
                _context.SaveChanges();

                TempData["Success"] = "Turma criada com sucesso!";
                return Redirect("/admin/classes");
            }
            catch (Exception ex)
            {
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
            if (string.IsNullOrWhiteSpace(membershipCode))
            {
                ModelState.AddModelError("MembershipCode", "O código de adesão é obrigatório.");
                return View();
            }

            var targetClass = _context.Classes
                .FirstOrDefault(c => c.MembershipCode == membershipCode.Trim().ToUpper());

            if (targetClass == null)
            {
                ModelState.AddModelError("", "Código inválido. Não foi encontrada nenhuma turma ou desafio com este código.");
                return View();
            }
            int studentId = 2;

            bool alreadyJoined = _context.ClassEnrollments
        .Any(ce => ce.ClassId == targetClass.Id && ce.StudentId == studentId);

            if (alreadyJoined)
            {
                ModelState.AddModelError("", "Já estás inscrito nesta turma / desafio!");
                return View();
            }

            try
            {
                var newEnrollment = new ClassEnrollment(targetClass.Id, studentId);

                _context.ClassEnrollments.Add(newEnrollment);
                _context.SaveChanges();

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Erro inesperado ao aderir à turma: {ex.Message}");
                return View();
            }
        }
    }
}
