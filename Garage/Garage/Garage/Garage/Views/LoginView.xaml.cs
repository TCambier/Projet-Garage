using System.Windows.Controls;
using GarageApp.ViewModels; // adapte si ton namespace VM est différent

namespace Garage.Views
{
    public partial class LoginView : Page
    {
        public LoginView()
        {
            InitializeComponent();
            DataContext = new LoginViewModel();
        }

        private void PasswordBox_OnPasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
                vm.Password = (sender as PasswordBox).Password;
        }
    }
}
