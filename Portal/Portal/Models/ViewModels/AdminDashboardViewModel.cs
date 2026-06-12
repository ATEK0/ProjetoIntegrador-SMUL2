namespace Portal.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public List<SchoolClass> Classes { get; set; } = new();
        public List<Challenge> Challenges { get; set; } = new();
        public List<User> Users { get; set; } = new();
    }
}
