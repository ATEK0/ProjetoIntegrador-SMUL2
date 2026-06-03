using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System;
using System.Linq;
using Portal.Data;
using Portal.Models;

namespace Portal.Controllers
{
    // [Authorize] 
    public class ScenariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ScenariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string familyName, decimal initialBalance)
        {
            if (string.IsNullOrWhiteSpace(familyName))
            {
                return Content("Erro de validação: O nome de família é obrigatório.");
            }

            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                int studentId;
                if (int.TryParse(userIdClaim, out int parsedId))
                {
                    studentId = parsedId;
                }
                else
                {
                    studentId = 1; // Fallback
                }

                // Verificar se o estudante existe de forma síncrona
                var student = _context.Users.Find(studentId);
                if (student == null)
                {
                    return Content($"Erro: O estudante com ID {studentId} não existe na base de dados. Por favor, verifique se a conta está ativa e se o ID é válido.");
                }

                var scenario = new Scenario(studentId, familyName, initialBalance);
                
                _context.Add(scenario);
                _context.SaveChanges();

                return RedirectToAction("Aluno", "Dashboard");
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException != null ? $"\nDetalhe Interno: {ex.InnerException.Message}" : "";
                return Content($"Erro interno no servidor ao criar o cenário: {ex.Message}{innerMsg}");
            }
        }
    }
}