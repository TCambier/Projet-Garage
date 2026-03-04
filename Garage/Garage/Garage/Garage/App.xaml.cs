using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using Garage.Services;
using Microsoft.Extensions.Configuration;

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

            // Chargement de la configuration (appsettings.json + variables d'environnement)
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            // Connexion BDD : variable d'env > appsettings.json
            var connStr = Environment.GetEnvironmentVariable("GARAGE_CONNECTION_STRING")
                       ?? configuration.GetConnectionString("GarageDb");

            if (string.IsNullOrWhiteSpace(connStr))
            {
                MessageBox.Show(
                    "Chaîne de connexion introuvable.\n\n" +
                    "Configurez 'ConnectionStrings:GarageDb' dans appsettings.json " +
                    "ou définissez la variable d'environnement GARAGE_CONNECTION_STRING.",
                    "Erreur de configuration", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            ConnectionString = connStr;

            // Initialisation du compte administrateur par défaut
            var adminUser = configuration["AdminAccount:Identifiant"] ?? "";
            var adminPassword = configuration["AdminAccount:Password"] ?? "";
            DbInitializer.Initialize(ConnectionString, adminUser, adminPassword);

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
            Nav.Register("Pieces", () => new Views.PiecesView());
            Nav.Register("Statistiques", () => new Views.StatisticsView());
            Nav.Register("Settings", () => new Views.SettingsView());
            Nav.Register("ClientDashboard", () => new Views.ClientDashboardView());

            // Page initiale : écran de connexion
            Nav.NavigateTo("Login");

            // Afficher la fenêtre principale
            MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}
