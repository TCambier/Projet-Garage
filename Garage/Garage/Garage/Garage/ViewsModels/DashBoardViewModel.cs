using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Garage.Data;
using Garage.Models;
using GarageApp;
using Microsoft.EntityFrameworkCore;

namespace Garage.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private DashboardModel _stats;
        public DashboardModel Stats
        {
            get => _stats;
            set
            {
                if (!ReferenceEquals(_stats, value))
                {
                    _stats = value;
                    Raise();
                }
            }
        }

        public ObservableCollection<UrgentAlert> UrgentAlerts { get; }
        public ObservableCollection<int> InterventionsByMonth { get; }
        public ObservableCollection<int> YAxisLabels { get; }

        public string UserFullName { get; }
        public string UserStatus { get; }
        public string UserInitials { get; }

        // 👉 Commandes de navigation
        public ICommand NavigateToVehiculesCommand { get; }
        public ICommand NavigateToEntretiensCommand { get; }
        public ICommand NavigateToStatistiquesCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }

        public DashboardViewModel()
        {
            // valeurs par défaut (évite les bindings null)
            Stats = new DashboardModel();
            UrgentAlerts = new ObservableCollection<UrgentAlert>();
            InterventionsByMonth = new ObservableCollection<int>(Enumerable.Repeat(0, 12));
            YAxisLabels = new ObservableCollection<int>(new[] { 4, 3, 2, 1, 0 });

            // Récupération utilisateur
            var user = App.Auth?.CurrentUser;
            if (user != null)
            {
                var prenom = user.Prenom ?? string.Empty;
                var nom = user.Nom ?? string.Empty;

                UserFullName = ($"{prenom} {nom}").Trim();
                UserStatus = user.Statut.ToString();
                UserInitials = BuildInitials(prenom, nom);
            }
            else
            {
                UserFullName = "Utilisateur";
                UserStatus = string.Empty;
                UserInitials = "?";
            }

            NavigateToVehiculesCommand = new RelayCommand(_ => App.Nav.NavigateTo("Vehicules"));
            NavigateToEntretiensCommand = new RelayCommand(_ => App.Nav.NavigateTo("Entretiens"));
            NavigateToStatistiquesCommand = new RelayCommand(_ => App.Nav.NavigateTo("Statistiques"));
            NavigateToSettingsCommand = new RelayCommand(_ => App.Nav.NavigateTo("Settings"));

            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            try
            {
                using var ctx = new GarageDbContext(App.ConnectionString);

                // --- Stats du haut ---
                var now = DateTime.Today;

                var vehiclesInService = ctx.Vehicles.AsNoTracking().Count(v => v.Statut == VehicleStatus.EnService);
                var interventionsThisMonth = ctx.Maintenances.AsNoTracking().Count(m => m.DateIntervention.Month == now.Month && m.DateIntervention.Year == now.Year);

                // Le projet utilise "En Cours" (voir EntretiensViewModel). On accepte aussi "En cours".
                var pendingAlerts = ctx.Maintenances.AsNoTracking().Count(m => m.Statut == "En Cours" || m.Statut == "En cours");

                Stats = new DashboardModel
                {
                    VehiclesInService = vehiclesInService,
                    InterventionsThisMonth = interventionsThisMonth,
                    PendingAlerts = pendingAlerts
                };

                // --- Données du graphique (Nb d'interventions par mois) ---
                // Demande : prendre le mois dans Date_Intervention (XXXX-XX-XX) et compter par mois.
                // Ici : agrégation sur tous les enregistrements, sans distinguer l'année.
                var grouped = ctx.Maintenances.AsNoTracking()
                    .GroupBy(m => m.DateIntervention.Month)
                    .Select(g => new { Month = g.Key, Count = g.Count() })
                    .ToList();

                var counts = new int[12];
                foreach (var g in grouped)
                {
                    if (g.Month >= 1 && g.Month <= 12)
                        counts[g.Month - 1] = g.Count;
                }

                for (int i = 0; i < 12; i++)
                {
                    InterventionsByMonth[i] = counts[i];
                }

                UpdateYAxisLabels(counts);

                // --- Alertes urgentes : entretiens encore à faire (Statut = En Cours) ---
                var urgent = ctx.Maintenances.AsNoTracking()
                    .Where(m => m.Statut == "En Cours" || m.Statut == "En cours")
                    .OrderBy(m => m.DateIntervention)
                    .Take(6)
                    .Select(m => new { m.Type, m.Immatriculation })
                    .ToList();

                UrgentAlerts.Clear();
                foreach (var u in urgent)
                {
                    UrgentAlerts.Add(new UrgentAlert
                    {
                        Title = u.Type,
                        Vehicle = u.Immatriculation
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Erreur chargement dashboard :\n{ex.Message}");
            }
        }

        private void UpdateYAxisLabels(IReadOnlyList<int> data)
        {
            var max = data.Count == 0 ? 0 : data.Max();

            // 5 labels (haut -> bas) : top, 3/4, 1/2, 1/4, 0
            var step = max <= 0 ? 1 : (int)Math.Ceiling(max / 4.0);
            var top = step * 4;

            YAxisLabels.Clear();
            YAxisLabels.Add(top);
            YAxisLabels.Add(top - step);
            YAxisLabels.Add(top - (2 * step));
            YAxisLabels.Add(top - (3 * step));
            YAxisLabels.Add(0);
        }

        private static string BuildInitials(string prenom, string nom)
        {
            char first = !string.IsNullOrWhiteSpace(prenom) ? char.ToUpper(prenom[0]) : '?';
            char second = !string.IsNullOrWhiteSpace(nom) ? char.ToUpper(nom[0]) : '\0';
            return second == '\0' ? first.ToString() : $"{first}{second}";
        }
    }
}
