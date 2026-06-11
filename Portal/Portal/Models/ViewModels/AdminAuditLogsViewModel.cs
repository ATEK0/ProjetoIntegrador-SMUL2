using System.Collections.Generic;

namespace Portal.Models.ViewModels
{
    public class AdminAuditLogsViewModel
    {
        public List<AuditLogItemViewModel> Logs { get; set; } = new();
        public int Page { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public string? FilterAction { get; set; }
        public string? FilterEntityType { get; set; }
        public List<string> AvailableActions { get; set; } = new();
        public List<string> AvailableEntityTypes { get; set; } = new();
        public string? LoadError { get; set; }
    }
}
