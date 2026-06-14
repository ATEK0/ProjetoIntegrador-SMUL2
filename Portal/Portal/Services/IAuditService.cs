using System.Threading.Tasks;
using Portal.Models;

namespace Portal.Services
{
    public interface IAuditService
    {
        Task LogAsync(
            AuditAction action,
            string entityType,
            string? entityId = null,
            int? userId = null,
            string? userEmail = null);
    }
}
