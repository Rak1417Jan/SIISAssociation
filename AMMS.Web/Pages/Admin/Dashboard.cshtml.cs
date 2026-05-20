using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AMMS.Web.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        public int TotalUsers { get; set; } = 25;
        public int ActiveUsers { get; set; } = 20;
        public int InactiveUsers { get; set; } = 5;
        public int AdminLoginAttempts { get; set; } = 112;

        public List<DashboardUserRow> Users { get; set; } = new();
        public List<DashboardActivityItem> ActivityLogs { get; set; } = new();

        public void OnGet()
        {
            Users = new List<DashboardUserRow>
            {
                new() { Name = "Mukesh Shah", Email = "mukesh.shah@email.com", Role = "Admin", IsActive = true, LastLogin = "2 minutes ago" },
                new() { Name = "Priya Desai", Email = "priya.desai@email.com", Role = "Editor", IsActive = true, LastLogin = "5 hours ago" },
                new() { Name = "Rajesh Kumar", Email = "rajesh.kumar@email.com", Role = "Editor", IsActive = true, LastLogin = "1 day ago" },
                new() { Name = "Anjali Patel", Email = "anjali.patel@email.com", Role = "Editor", IsActive = true, LastLogin = "2 days ago" },
                new() { Name = "Vijay Gupta", Email = "vijay.gupta@email.com", Role = "Editor", IsActive = false, LastLogin = "3 days ago" }
            };

            ActivityLogs = new List<DashboardActivityItem>
            {
                new() { UserName = "Mukesh Shah", Action = "Logged in", TimeAgo = "2 minutes ago" },
                new() { UserName = "Priya Desai", Action = "Deactivated by Admin", TimeAgo = "5 hours ago" },
                new() { UserName = "Rajesh Kumar", Action = "Password reset by Admin", TimeAgo = "1 day ago" },
                new() { UserName = "Anjali Patel", Action = "Reset her password", TimeAgo = "2 days ago" }
            };
        }
    }

    public class DashboardUserRow
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public bool IsActive { get; set; }
        public string LastLogin { get; set; } = "";
    }

    public class DashboardActivityItem
    {
        public string UserName { get; set; } = "";
        public string Action { get; set; } = "";
        public string TimeAgo { get; set; } = "";
    }
}
