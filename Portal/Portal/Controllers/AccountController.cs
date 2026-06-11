using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Portal.Models.ViewModels;
using Portal.Services;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Portal.Controllers
{
    public class AccountController : BaseController
    {
        private readonly AccountService _accountService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(AccountService accountService, ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectByRole(User.FindFirst(ClaimTypes.Role)?.Value);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Por favor, preencha todos os campos obrigatórios.");
                return View();
            }

            try
            {
                var result = await _accountService.TryLoginAsync(email, password);
                if (result == null)
                {
                    ModelState.AddModelError(string.Empty, "E-mail ou password incorretos.");
                    return View();
                }

                Response.Cookies.Append("JWT_Token", result.Value.token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Strict,
                    Expires = rememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddHours(2)
                });

                return RedirectByRole(result.Value.user.Role?.RoleName);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no login para {Email}", email);
                ModelState.AddModelError(string.Empty, "Ocorreu um erro ao tentar iniciar sessão. Tente novamente.");
                return View();
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectByRole(User.FindFirst(ClaimTypes.Role)?.Value);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string name, string email, string password, int? genderId, DateTime? birthDate, string role = "aluno")
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Por favor, preencha todos os campos obrigatórios.");
                return View();
            }

            var error = await _accountService.RegisterAsync(name, email, password, genderId, birthDate, role);
            if (error != null)
            {
                ModelState.AddModelError(string.Empty, error);
                return View();
            }

            TempData["Success"] = "Conta criada com sucesso! Faça login abaixo.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet, HttpPost]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("JWT_Token");
            return RedirectToAction(nameof(Login));
        }

        [Authorize, HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction(nameof(Login));

            var vm = await _accountService.GetProfileAsync(userId.Value);
            return vm == null ? RedirectToAction(nameof(Login)) : View(vm);
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> Profile(ProfileViewModel vm)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction(nameof(Login));

            if (!ModelState.IsValid)
                return View(await RepopulateProfile(vm, userId.Value));

            var error = await _accountService.UpdateProfileAsync(userId.Value, vm);
            if (error != null)
            {
                ModelState.AddModelError(string.Empty, error);
                return View(await RepopulateProfile(vm, userId.Value));
            }

            Response.Cookies.Append("JWT_Token", await _accountService.GenerateTokenForUserAsync(userId.Value), new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            TempData["Success"] = "Perfil atualizado com sucesso!";
            return RedirectToAction(nameof(Profile));
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction(nameof(Login));

            if (!ModelState.IsValid)
            {
                var profile = await _accountService.GetProfileAsync(userId.Value);
                return profile == null ? RedirectToAction(nameof(Login)) : View("Profile", profile);
            }

            var error = await _accountService.ChangePasswordAsync(userId.Value, vm);
            TempData[error == null ? "Success" : "Error"] =
                error ?? "Password alterada com sucesso!";
            return RedirectToAction(nameof(Profile));
        }



        private async Task<ProfileViewModel> RepopulateProfile(ProfileViewModel vm, int userId)
        {
            var current = await _accountService.GetProfileAsync(userId);
            if (current != null) { vm.Role = current.Role; vm.CreatedAt = current.CreatedAt; }
            return vm;
        }

        private IActionResult RedirectByRole(string? role)
        {
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Admin", "Dashboard");
            if (string.Equals(role, "Professor", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Professor", "Dashboard");
            return RedirectToAction("Aluno", "Dashboard");
        }
    }
}
