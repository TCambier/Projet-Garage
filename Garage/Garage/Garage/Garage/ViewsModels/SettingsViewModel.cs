using System.Windows.Input;
using Garage.Services;
using GarageApp;

namespace Garage.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private bool _isDarkMode;

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    Raise();

                    ThemeService.Apply(_isDarkMode ? AppTheme.Dark : AppTheme.Light);
                }
            }
        }

        // Navigation
        public ICommand NavigateToDashboardCommand { get; }
        public ICommand NavigateToVehiculesCommand { get; }
        public ICommand NavigateToEntretiensCommand { get; }
        public ICommand NavigateToStatistiquesCommand { get; }

        public SettingsViewModel()
        {
            _isDarkMode = ThemeService.CurrentTheme == AppTheme.Dark;

            NavigateToDashboardCommand = new RelayCommand(_ => App.Nav.NavigateTo("Dashboard"));
            NavigateToVehiculesCommand = new RelayCommand(_ => App.Nav.NavigateTo("Vehicules"));
            NavigateToEntretiensCommand = new RelayCommand(_ => App.Nav.NavigateTo("Entretiens"));
            NavigateToStatistiquesCommand = new RelayCommand(_ => App.Nav.NavigateTo("Statistiques"));
        }
    }
}
