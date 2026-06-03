using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Portal.Data;
using Portal.Models;
using Portal.Models.ViewModels;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Portal.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher = new();
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ApplicationDbContext context, IConfiguration configuration, ILogger<AccountController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectUserBasedOnRole(User.FindFirst(ClaimTypes.Role)?.Value);
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password, bool rememberMe)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Por favor, preencha todos os campos obrigatórios.";
                return View();
            }

            _logger.LogInformation("Tentativa de login iniciada para o e-mail: {Email}", email);

            try
            {
                var user = _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.UserStatus)
                    .FirstOrDefault(u => u.Email == email.Trim().ToLower());

                if (user == null)
                {
                    _logger.LogWarning("Falha no login: o e-mail {Email} não foi encontrado na base de dados.", email);
                    TempData["Error"] = "E-mail ou password incorretos.";
                    return View();
                }

                // Verificar a password
                var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
                if (verificationResult == PasswordVerificationResult.Failed)
                {
                    _logger.LogWarning("Falha no login para {Email}: a password introduzida está incorreta.", email);
                    TempData["Error"] = "E-mail ou password incorretos.";
                    return View();
                }

                if (user.UserStatus?.StatusName != "Ativo")
                {
                    _logger.LogWarning("Falha no login para {Email}: a conta encontra-se com o estado '{Status}', não estando Ativa.", email, user.UserStatus?.StatusName ?? "desconhecido");
                    TempData["Error"] = "A sua conta não está ativa. Por favor, contacte o administrador.";
                    return View();
                }

                // Gerar token JWT
                var token = GenerateJwtToken(user);

                // Armazenar o JWT num cookie seguro HTTP-Only
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false, // Em produção deve ser true (apenas HTTPS)
                    SameSite = SameSiteMode.Strict,
                    Expires = rememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddHours(2)
                };

                Response.Cookies.Append("JWT_Token", token, cookieOptions);

                _logger.LogInformation("Login bem-sucedido para {Email} (ID: {UserId}). Role: {Role}.", email, user.Id, user.Role?.RoleName);

                return RedirectUserBasedOnRole(user.Role?.RoleName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro grave durante a tentativa de login para o e-mail: {Email}", email);
                TempData["Error"] = $"Erro ao tentar iniciar sessão: {ex.Message}";
                return View();
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectUserBasedOnRole(User.FindFirst(ClaimTypes.Role)?.Value);
            }
            return View();
        }

        [HttpPost]
        public IActionResult Register(string name, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Por favor, preencha todos os campos.";
                return View();
            }

            _logger.LogInformation("Tentativa de registo iniciada para o e-mail: {Email}", email);

            try
            {
                // Verificar se o e-mail já existe
                var existingUser = _context.Users.Any(u => u.Email == email.Trim().ToLower());
                if (existingUser)
                {
                    _logger.LogWarning("Falha no registo: e-mail já registado: {Email}", email);
                    TempData["Error"] = "Este e-mail já está registado na plataforma.";
                    return View();
                }

                // Determinar a Role correspondente (Por defeito é Aluno)
                string dbRoleName = "Aluno";
                var dbRole = _context.Roles.FirstOrDefault(r => r.RoleName == dbRoleName);
                if (dbRole == null)
                {
                    _logger.LogError("Falha no registo para {Email}: a função '{dbRoleName}' não existe na BD.", email, dbRoleName);
                    TempData["Error"] = $"A função '{dbRoleName}' selecionada não é válida. Por favor, verifique se a base de dados está inicializada.";
                    return View();
                }

                // Determinar o Estado correspondente (Ativo por padrão)
                var status = _context.UserStatuses.FirstOrDefault(s => s.StatusName == "Ativo");
                if (status == null)
                {
                    _logger.LogError("Falha no registo para {Email}: o estado 'Ativo' não foi encontrado na BD.", email);
                    TempData["Error"] = "Não foi possível encontrar o estado 'Ativo' na base de dados. Por favor, verifique se a base de dados está inicializada.";
                    return View();
                }

                var newUser = new User(name.Trim(), status.Id, dbRole.Id)
                {
                    Email = email
                };

                newUser.PasswordHash = _passwordHasher.HashPassword(newUser, password);

                _context.Users.Add(newUser);
                _context.SaveChanges();

                _logger.LogInformation("Registo concluído com sucesso para o utilizador: {Email} (ID: {UserId}, Função: {Role}).", email, newUser.Id, dbRole.RoleName);

                TempData["Success"] = "Conta criada com sucesso! Faça login abaixo.";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro grave ao tentar registar utilizador com e-mail: {Email}", email);
                TempData["Error"] = $"Erro ao criar conta: {ex.Message}";
                return View();
            }
        }

        [HttpGet]
        [HttpPost]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("JWT_Token");
            return RedirectToAction(nameof(Login));
        }

        // ── PERFIL ───────────────────────────────────────────────────────────────

        [Authorize]
        [HttpGet]
        public IActionResult Profile()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction(nameof(Login));

            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Id == userId.Value);

            if (user == null) return RedirectToAction(nameof(Login));

            var vm = new ProfileViewModel
            {
                Name = user.Name,
                Email = user.Email,
                Role = user.Role?.RoleName ?? "—",
                CreatedAt = user.CreatedAt
            };

            return View(vm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(ProfileViewModel vm)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction(nameof(Login));

            _logger.LogInformation("Tentativa de atualização de perfil para utilizador ID: {UserId}", userId);

            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Id == userId.Value);

            if (user == null)
            {
                _logger.LogWarning("Atualização de perfil falhou: utilizador com ID {UserId} não encontrado.", userId);
                return RedirectToAction(nameof(Login));
            }

            // Repopular campos de apresentação antes de devolver a view em caso de erro
            vm.Role = user.Role?.RoleName ?? "—";
            vm.CreatedAt = user.CreatedAt;

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Atualização de perfil falhou para ID {UserId} devido a erros de validação no formulário.", userId);
                TempData["ProfileError"] = "Por favor, corrija os erros no formulário.";
                return View(vm);
            }

            // Verificar se o e-mail já está em uso por outro utilizador
            bool emailTaken = _context.Users
                .Any(u => u.Email == vm.Email.Trim().ToLower() && u.Id != userId.Value);

            if (emailTaken)
            {
                _logger.LogWarning("Atualização de perfil falhou para ID {UserId}: e-mail '{Email}' já está registado noutra conta.", userId, vm.Email);
                TempData["ProfileError"] = "Este e-mail já está em uso por outra conta.";
                return View(vm);
            }

            string oldEmail = user.Email;
            user.Name = vm.Name.Trim();
            user.Email = vm.Email.Trim().ToLower();
            _context.SaveChanges();

            _logger.LogInformation("Perfil atualizado com sucesso para ID {UserId}. Nome: '{Name}', E-mail: '{OldEmail}' -> '{NewEmail}'.", userId, user.Name, oldEmail, user.Email);

            // Re-emitir o token JWT com os dados atualizados
            var newToken = GenerateJwtToken(user);
            Response.Cookies.Append("JWT_Token", newToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            TempData["ProfileSuccess"] = "Perfil atualizado com sucesso!";
            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel vm)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction(nameof(Login));

            _logger.LogInformation("Tentativa de alteração de password para o utilizador ID: {UserId}", userId);

            var user = _context.Users
                .FirstOrDefault(u => u.Id == userId.Value);

            if (user == null)
            {
                _logger.LogWarning("Alteração de password falhou: utilizador com ID {UserId} não encontrado.", userId);
                return RedirectToAction(nameof(Login));
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Alteração de password falhou para ID {UserId} devido a erros de validação no formulário.", userId);
                TempData["PasswordError"] = "Por favor, corrija os erros no formulário.";
                return RedirectToAction(nameof(Profile));
            }

            // Verificar password atual
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, vm.CurrentPassword);
            if (result == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Alteração de password falhou para ID {UserId}: a password atual fornecida está incorreta.", userId);
                TempData["PasswordError"] = "A password atual está incorreta.";
                return RedirectToAction(nameof(Profile));
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, vm.NewPassword);
            _context.SaveChanges();

            _logger.LogInformation("Password alterada com sucesso para o utilizador ID: {UserId}.", userId);

            TempData["PasswordSuccess"] = "Password alterada com sucesso!";
            return RedirectToAction(nameof(Profile));
        }

        // ── HELPERS ───────────────────────────────────────────────────────────────

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && int.TryParse(claim.Value, out int id))
                return id;
            return null;
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSecret = _configuration["Jwt:Secret"] ?? "SuaChaveSecretaSuperProtegidaComPeloMenos32CaracteresDEC!";
            var key = Encoding.UTF8.GetBytes(jwtSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "Aluno")
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private IActionResult RedirectUserBasedOnRole(string? roleName)
        {
            if (string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Admin", "Dashboard");
            }
            else if (string.Equals(roleName, "Professor", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Professor", "Dashboard");
            }
            else
            {
                return RedirectToAction("Aluno", "Dashboard");
            }
        }
    }
}
