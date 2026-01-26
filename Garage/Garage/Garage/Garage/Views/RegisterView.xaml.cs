using System.Windows.Controls;
using System.Windows;
using GarageApp.ViewModels;

namespace Garage.Views
{
    public partial class RegisterView : Page
    {
        public RegisterView()
        {
            InitializeComponent();
            DataContext = new RegisterViewModel();
        }

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel vm)
                vm.Password = (sender as PasswordBox)?.Password;
        }
    }
}
