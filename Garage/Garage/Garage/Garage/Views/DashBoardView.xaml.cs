using Garage.ViewModels;
using System.Windows.Controls;

namespace Garage.Views
{
    public partial class DashboardView : Page
    {
        public DashboardView()
        {
            InitializeComponent();
            DataContext = new DashboardViewModel();
        }
    }
}
