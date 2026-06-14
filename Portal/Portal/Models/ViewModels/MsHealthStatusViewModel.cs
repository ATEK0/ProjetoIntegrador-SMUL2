using System;

namespace Portal.Models.ViewModels
{
    public class MsHealthStatusViewModel
    {
        public bool IsOnline { get; set; }
        public string? Service { get; set; }
        public string? Status { get; set; }
        public string? Version { get; set; }
        public string? Error { get; set; }
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
        public long ResponseTimeMs { get; set; }
    }
}
