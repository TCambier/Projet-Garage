namespace Garage.Models
{
    public class DashboardModel
    {
        public int VehiclesInService { get; set; }
        public int InterventionsThisMonth { get; set; }
        public int PendingAlerts { get; set; }
    }

    public class UrgentAlert
    {
        public string Title { get; set; }
        public string Vehicle { get; set; }
        public string Icon { get; set; } = "⚠️";
    }
}
