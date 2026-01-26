using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using Garage.Services;

namespace Garage
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static AuthService Auth { get; private set; }
        public static NavigationService Nav { get; private set; }
        public static string ConnectionString { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Applique le thème sauvegardé (clair/sombre)
            ThemeService.LoadAndApply();

            // Configuration de la connexion à la base MySQL (phpMyAdmin)
            // Priorité à la variable d'environnement GARAGE_CONNECTION_STRING pour éviter de hardcoder des identifiants.
            // Exemple (PowerShell):
            //   $env:GARAGE_CONNECTION_STRING = "server=localhost;database=garage;user=root;password=...;"
            var envConn = Environment.GetEnvironmentVariable("GARAGE_CONNECTION_STRING");
            ConnectionString = !string.IsNullOrWhiteSpace(envConn)
                ? envConn
                : "server=localhost;database=garage;user=root;password=;";

            // Initialisation du service d'authentification
            Auth = new AuthService(ConnectionString);

            // Création et configuration manuelle de la fenêtre principale
            var mainWindow = new MainWindow();

            // Configuration du service de navigation sur la fenêtre principale
            Nav = new NavigationService(mainWindow.MainFrame);
            Nav.Register("Login", () => new Views.LoginView());
            Nav.Register("Dashboard", () => new Views.DashboardView());
            Nav.Register("Register", () => new Views.RegisterView());
            Nav.Register("Vehicules", () => new Views.VehiclesView());
            Nav.Register("Entretiens", () => new Views.EntretiensView());
            Nav.Register("Statistiques", () => new Views.StatisticsView());
            Nav.Register("Settings", () => new Views.SettingsView());

            // Page initiale : écran de connexion
            Nav.NavigateTo("Login");

            // Afficher la fenêtre principale
            MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}
