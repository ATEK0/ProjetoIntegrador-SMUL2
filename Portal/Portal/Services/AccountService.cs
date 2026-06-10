using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Portal.Data;
using Portal.Models;
using Portal.Models.ViewModels;

namespace Portal.Services
{
    public class AccountService
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher = new();
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountService> _logger;

        public AccountService(ApplicationDbContext context, IConfiguration configuration, ILogger<AccountService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<(User user, string token)?> TryLoginAsync(string email, string password)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.UserStatus)
                .FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower());

            if (user == null) return null;

            if (_passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Failed)
                return null;

            if (user.UserStatus?.StatusName != "Ativo")
                throw new InvalidOperationException("A sua conta não está ativa. Por favor, contacte o administrador.");

            return (user, GenerateToken(user));
        }

        public async Task<string?> RegisterAsync(string name, string email, string password, int? genderId, DateTime? birthDate, string role)
        {
            if (string.IsNullOrWhiteSpace(role)) role = "aluno";

            if (await _context.Users.AnyAsync(u => u.Email == email.Trim().ToLower()))
                return "Este e-mail já está registado na plataforma.";

            string dbRoleName = role.Equals("professor", StringComparison.OrdinalIgnoreCase) ? "Professor" : "Aluno";
            var dbRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == dbRoleName);
            if (dbRole == null)
                return $"A função '{dbRoleName}' selecionada não é válida.";

            var status = await _context.UserStatuses.FirstOrDefaultAsync(s => s.StatusName == "Ativo");
            if (status == null)
                return "Não foi possível encontrar o estado 'Ativo' na base de dados.";

            var newUser = new User(name.Trim(), status.Id, dbRole.Id) { Email = email, GenderId = genderId, BirthDate = birthDate };
            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, password);
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Registo concluído para {Email}.", email);
            return null;
        }

        public async Task<ProfileViewModel?> GetProfileAsync(int userId)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            return new ProfileViewModel
            {
                Name = user.Name,
                Email = user.Email,
                Role = user.Role?.RoleName ?? "—",
                GenderId = user.GenderId,
                BirthDate = user.BirthDate,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<string?> UpdateProfileAsync(int userId, ProfileViewModel vm)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return "Utilizador não encontrado.";

            if (await _context.Users.AnyAsync(u => u.Email == vm.Email.Trim().ToLower() && u.Id != userId))
                return "Este e-mail já está em uso por outra conta.";

            user.Name = vm.Name.Trim();
            user.Email = vm.Email.Trim().ToLower();
            user.GenderId = vm.GenderId;
            user.BirthDate = vm.BirthDate;
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<string?> ChangePasswordAsync(int userId, ChangePasswordViewModel vm)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return "Utilizador não encontrado.";

            if (_passwordHasher.VerifyHashedPassword(user, user.PasswordHash, vm.CurrentPassword) == PasswordVerificationResult.Failed)
                return "A password atual está incorreta.";

            user.PasswordHash = _passwordHasher.HashPassword(user, vm.NewPassword);
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<string> GenerateTokenForUserAsync(int userId)
        {
            var user = await _context.Users.Include(u => u.Role).FirstAsync(u => u.Id == userId);
            return GenerateToken(user);
        }

        private string GenerateToken(User user)
        {
            var jwtSecret = _configuration["Jwt:Secret"]!;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

            var token = new JwtSecurityToken(
                claims: new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "Aluno")
                },
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
