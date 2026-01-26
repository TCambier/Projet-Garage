using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Garage.Data;
using GarageApp;
using Garage;

namespace GarageApp.ViewModels
{
    public class RegisterViewModel : ViewModelBase
    {
        private string username;
        public string Username
        {
            get => username;
            set { username = value; Raise(); }
        }

        private string password;
        public string Password
        {
            get => password;
            set { password = value; Raise(); }
        }

        private string firstName;
        public string FirstName
        {
            get => firstName;
            set { firstName = value; Raise(); }
        }

        private string lastName;
        public string LastName
        {
            get => lastName;
            set { lastName = value; Raise(); }
        }

        private bool hasError;
        public bool HasError
        {
            get => hasError;
            set { hasError = value; Raise(); }
        }

        private string errorMessage;
        public string ErrorMessage
        {
            get => errorMessage;
            set { errorMessage = value; Raise(); }
        }

        private string successMessage;
        public string SuccessMessage
        {
            get => successMessage;
            set { successMessage = value; Raise(); }
        }

        public ICommand RegisterCommand { get; }
        public ICommand CancelCommand { get; }

        public RegisterViewModel()
        {
            RegisterCommand = new RelayCommand(Register);
            CancelCommand = new RelayCommand(_ => App.Nav.NavigateTo("Login"));
        }

        private void Register(object obj)
        {
            HasError = false;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(LastName) ||
                string.IsNullOrWhiteSpace(FirstName) ||
                string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Password))
            {
                ShowError("Veuillez remplir tous les champs.");
                return;
            }

            try
            {
                // Connexion à la base MySQL (phpMyAdmin)
                string connectionString = App.ConnectionString;

                using (var ctx = new GarageDbContext(connectionString))
                {
                    if (ctx.Users.Any(u => u.Identifiant == Username))
                    {
                        ShowError("Ce nom d'utilisateur existe déjà.");
                        return;
                    }

                    var user = new User
                    {
                        Identifiant = Username,
                        Mdp = HashHelper.Sha256(Password),
                        Statut = Role.Client,      // Statut client
                        Prenom = FirstName,
                        Nom = LastName
                    };

                    ctx.Users.Add(user);
                    ctx.SaveChanges();
                }

                SuccessMessage = "Compte créé avec succès. Vous pouvez maintenant vous connecter.";

                // Affiche un message de succès puis redirige vers la page de connexion
                MessageBox.Show(SuccessMessage, "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                App.Nav.NavigateTo("Login");
            }
            catch (Exception ex)
            {
                ShowError($"Erreur lors de la création du compte : {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            HasError = true;
            ErrorMessage = message;
        }
    }
}
