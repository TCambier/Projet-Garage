using Garage.ViewModels;
using System.Windows.Controls;

namespace Garage.Views
{
    public partial class ClientDashboardView : Page
    {
        public ClientDashboardView()
        {
            InitializeComponent();
            DataContext = new ClientDashboardViewModel();
        }
    }
}
