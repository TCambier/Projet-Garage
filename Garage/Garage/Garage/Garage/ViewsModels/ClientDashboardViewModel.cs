using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Garage.Data;
using GarageApp;
using Microsoft.EntityFrameworkCore;

namespace Garage.ViewModels
{
    /// <summary>
    /// ViewModel pour la page client (consultation uniquement).
    /// Affiche les véhicules du client connecté et leurs entretiens.
    /// </summary>
    public class ClientDashboardViewModel : ViewModelBase
    {
        // ── Propriétés utilisateur ──
        public string UserFullName { get; }
        public string UserStatus { get; }
        public string UserInitials { get; }

        // ── Collections ──
        public ObservableCollection<Vehicle> Vehicles { get; } = new();
        public ObservableCollection<MaintenanceRecord> Maintenances { get; } = new();

        // ── Véhicule sélectionné ──
        private Vehicle _selectedVehicle;
        public Vehicle SelectedVehicle
        {
            get => _selectedVehicle;
            set
            {
                if (_selectedVehicle != value)
                {
                    _selectedVehicle = value;
                    Raise();
                    LoadMaintenancesForVehicle();
                }
            }
        }

        // ── Résumé ──
        private int _totalVehicles;
        public int TotalVehicles
        {
            get => _totalVehicles;
            set { _totalVehicles = value; Raise(); }
        }

        private int _entretiensEnCours;
        public int EntretiensEnCours
        {
            get => _entretiensEnCours;
            set { _entretiensEnCours = value; Raise(); }
        }

        private int _prochainEntretienJours;
        public int ProchainEntretienJours
        {
            get => _prochainEntretienJours;
            set { _prochainEntretienJours = value; Raise(); }
        }

        private string _prochainEntretienLabel;
        public string ProchainEntretienLabel
        {
            get => _prochainEntretienLabel;
            set { _prochainEntretienLabel = value; Raise(); }
        }

        // ── Commandes ──
        public ICommand LogoutCommand { get; }
        public ICommand RefreshCommand { get; }

        public ClientDashboardViewModel()
        {
            // Infos utilisateur
            var user = App.Auth?.CurrentUser;
            if (user != null)
            {
                var prenom = user.Prenom ?? string.Empty;
                var nom = user.Nom ?? string.Empty;
                UserFullName = $"{prenom} {nom}".Trim();
                UserStatus = user.Statut.ToString();
                UserInitials = BuildInitials(prenom, nom);
            }
            else
            {
                UserFullName = "Client";
                UserStatus = "Client";
                UserInitials = "?";
            }

            LogoutCommand = new RelayCommand(_ =>
            {
                App.Auth.Logout();
                App.Nav.NavigateTo("Login");
            });
            RefreshCommand = new RelayCommand(_ => LoadAll());

            LoadAll();
        }

        private void LoadAll()
        {
            LoadVehicles();
            ComputeSummary();

            // Sélectionner le premier véhicule par défaut
            if (Vehicles.Count > 0 && SelectedVehicle == null)
                SelectedVehicle = Vehicles[0];
        }

        private void LoadVehicles()
        {
            try
            {
                var userId = App.Auth?.CurrentUser?.Id ?? 0;

                using var ctx = new GarageDbContext(App.ConnectionString);

                var vehicles = ctx.Vehicles
                    .AsNoTracking()
                    .Where(v => v.Id_Utilisateur == userId)
                    .OrderBy(v => v.Immatriculation)
                    .ToList();

                // Récupérer les immatriculations qui ont au moins un entretien "En Cours"
                var immatEnCours = ctx.Maintenances
                    .AsNoTracking()
                    .Where(m => m.Statut == "En Cours" || m.Statut == "En cours")
                    .Select(m => m.Immatriculation)
                    .Distinct()
                    .ToHashSet();

                foreach (var v in vehicles)
                {
                    v.StatutAffiche = immatEnCours.Contains(v.Immatriculation)
                        ? "En Cours"
                        : v.Statut.ToString();
                }

                Vehicles.Clear();
                foreach (var v in vehicles)
                    Vehicles.Add(v);

                TotalVehicles = vehicles.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur chargement véhicules :\n{ex.Message}");
            }
        }

        private void LoadMaintenancesForVehicle()
        {
            Maintenances.Clear();

            if (SelectedVehicle == null)
                return;

            try
            {
                using var ctx = new GarageDbContext(App.ConnectionString);

                var list = ctx.Maintenances
                    .AsNoTracking()
                    .Where(m => m.Immatriculation == SelectedVehicle.Immatriculation)
                    .OrderByDescending(m => m.DateIntervention)
                    .ToList();

                // Charger les pièces utilisées pour l'affichage
                var ids = list.Select(m => m.Id).ToList();
                var utilisations = ctx.Utilisations
                    .Include(u => u.Piece)
                    .Where(u => ids.Contains(u.Id_Entretien))
                    .ToList();

                var map = utilisations
                    .GroupBy(u => u.Id_Entretien)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var m in list)
                {
                    if (map.TryGetValue(m.Id, out var used))
                        m.Pieces = string.Join(", ", used.Select(u => $"{u.Piece.Libelle} x{u.Quantite}"));
                    else
                        m.Pieces = string.Empty;

                    Maintenances.Add(m);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur chargement entretiens :\n{ex.Message}");
            }
        }

        private void ComputeSummary()
        {
            try
            {
                var userId = App.Auth?.CurrentUser?.Id ?? 0;

                using var ctx = new GarageDbContext(App.ConnectionString);

                // Récupérer les immatriculations du client
                var immatriculations = ctx.Vehicles
                    .AsNoTracking()
                    .Where(v => v.Id_Utilisateur == userId)
                    .Select(v => v.Immatriculation)
                    .ToList();

                // Entretiens en cours
                EntretiensEnCours = ctx.Maintenances
                    .AsNoTracking()
                    .Count(m => immatriculations.Contains(m.Immatriculation)
                                && (m.Statut == "En Cours" || m.Statut == "En cours"));

                // Prochain entretien planifié (date future la plus proche)
                var today = DateTime.Today;
                var prochain = ctx.Maintenances
                    .AsNoTracking()
                    .Where(m => immatriculations.Contains(m.Immatriculation)
                                && m.DateIntervention > today
                                && m.Statut != "Terminé")
                    .OrderBy(m => m.DateIntervention)
                    .FirstOrDefault();

                if (prochain != null)
                {
                    var jours = (prochain.DateIntervention - today).Days;
                    ProchainEntretienJours = jours;
                    ProchainEntretienLabel = jours == 0
                        ? "Aujourd'hui"
                        : jours == 1
                            ? "Demain"
                            : $"Dans {jours} jours";
                }
                else
                {
                    ProchainEntretienJours = -1;
                    ProchainEntretienLabel = "Aucun prévu";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur calcul résumé :\n{ex.Message}");
            }
        }

        private static string BuildInitials(string prenom, string nom)
        {
            char first = !string.IsNullOrWhiteSpace(prenom) ? char.ToUpper(prenom[0]) : '?';
            char second = !string.IsNullOrWhiteSpace(nom) ? char.ToUpper(nom[0]) : '\0';
            return second == '\0' ? first.ToString() : $"{first}{second}";
        }
    }
}
