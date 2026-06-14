using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Portal.Data;
using Portal.Models;

namespace Portal.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(
            AuditAction action,
            string entityType,
            string? entityId = null,
            int? userId = null,
            string? userEmail = null)
        {
            var http = _httpContextAccessor.HttpContext;

            if (userId == null && http?.User?.Identity?.IsAuthenticated == true)
            {
                var claim = http.User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim != null && int.TryParse(claim.Value, out int id))
                    userId = id;
            }

            userEmail ??= http?.User?.FindFirst(ClaimTypes.Email)?.Value;

            _context.AuditLogs.Add(new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                UserId = userId,
                UserEmail = userEmail,
                ActionName = action.ToString(),
                EntityType = entityType,
                EntityId = entityId
            });

            await _context.SaveChangesAsync();
        }
    }
}
