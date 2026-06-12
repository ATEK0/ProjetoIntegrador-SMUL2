namespace Portal.Models.ViewModels
{
    public class AdminInfraViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalClasses { get; set; }
        public int TotalChallenges { get; set; }
        public int TotalAuditLogs { get; set; }
        public bool DatabaseOnline { get; set; }
    }
}
