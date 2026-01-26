using System.ComponentModel;
using System.Windows.Input;
using Garage;
using GarageApp;

namespace GarageApp.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string username;
        public string Username
        {
            get => username;
            set { username = value; OnPropertyChanged(nameof(Username)); }
        }

        private string password;
        public string Password
        {
            get => password;
            set { password = value; OnPropertyChanged(nameof(Password)); }
        }

        private bool hasError;
        public bool HasError
        {
            get => hasError;
            set { hasError = value; OnPropertyChanged(nameof(HasError)); }
        }

        private string errorMessage;
        public string ErrorMessage
        {
            get => errorMessage;
            set { errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
        }

        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(Login);
            NavigateToRegisterCommand = new RelayCommand(_ => App.Nav.NavigateTo("Register"));
        }

        private void Login(object obj)
        {
            HasError = false;

            if (string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Password))
            {
                Error("Veuillez remplir tous les champs.");
                return;
            }

            // --- LOGIN AVEC AuthService ---
            if (App.Auth.Login(Username, Password))
            {
                // utilise le service de navigation par clé
                App.Nav.NavigateTo("Dashboard");
                return;
            }

            // --- ERREUR ---
            Error("Identifiants incorrects.");
        }

        private void Error(string msg)
        {
            HasError = true;
            ErrorMessage = msg;
        }

        private void OnPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
