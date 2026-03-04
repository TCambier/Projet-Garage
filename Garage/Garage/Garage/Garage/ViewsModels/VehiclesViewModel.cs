using Garage.Data;
using Garage.Models;
using GarageApp;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Garage.ViewModels
{
    public class VehiclesViewModel : ViewModelBase
    {
        private Vehicle _selectedVehicle;
        private Vehicle _newVehicle;
        private bool _isAddDialogOpen;
        private User _selectedUser;

        private Vehicle _editVehicle;
        private Vehicle _vehicleBeingEdited;
        private string _editOriginalImmatriculation;
        private bool _isEditDialogOpen;

        public ObservableCollection<Vehicle> Vehicles { get; } = new();
        public ObservableCollection<FuelType> AvailableFuels { get; } = new();
        public ObservableCollection<User> AvailableUsers { get; } = new();
        public ObservableCollection<VehicleStatus> AvailableStatuses { get; } = new();

        public Vehicle SelectedVehicle
        {
            get => _selectedVehicle;
            set
            {
                if (_selectedVehicle != value)
                {
                    _selectedVehicle = value;
                    Raise();
                }
            }
        }

        public Vehicle NewVehicle
        {
            get => _newVehicle;
            set
            {
                if (_newVehicle != value)
                {
                    _newVehicle = value;
                    Raise();
                }
            }
        }

        public Vehicle EditVehicle
        {
            get => _editVehicle;
            set
            {
                if (_editVehicle != value)
                {
                    _editVehicle = value;
                    Raise();
                }
            }
        }

        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (_selectedUser != value)
                {
                    _selectedUser = value;

                    // Le binding peut modifier SelectedUser alors que le dialog d'ajout n'est pas ouvert
                    // (donc NewVehicle peut être null). On protège pour éviter un NullReferenceException.
                    if (NewVehicle != null)
                    {
                        NewVehicle.Id_Utilisateur = value?.Id ?? 0;
                    }

                    Raise();
                }
            }
        }

        public bool IsAddDialogOpen
        {
            get => _isAddDialogOpen;
            set
            {
                if (_isAddDialogOpen != value)
                {
                    _isAddDialogOpen = value;
                    Raise();
                }
            }
        }

        public bool IsEditDialogOpen
        {
            get => _isEditDialogOpen;
            set
            {
                if (_isEditDialogOpen != value)
                {
                    _isEditDialogOpen = value;
                    Raise();
                }
            }
        }

        public ICommand AddVehicleCommand { get; }
        public ICommand SaveNewVehicleCommand { get; }
        public ICommand CancelNewVehicleCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand LoadUsersCommand { get; }
        public ICommand NavigateToDashboardCommand { get; }
        public ICommand NavigateToEntretiensCommand { get; }
        public ICommand NavigateToStatistiquesCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }
        public ICommand NavigateToPiecesCommand { get; }
        public ICommand DeleteVehicleCommand { get; }
        public ICommand EditVehicleCommand { get; }
        public ICommand SaveEditVehicleCommand { get; }
        public ICommand CancelEditVehicleCommand { get; }

        public VehiclesViewModel()
        {
            // Initialise carburants
            AvailableFuels.Add(FuelType.Essence);
            AvailableFuels.Add(FuelType.Diesel);
            AvailableFuels.Add(FuelType.Electrique);
            AvailableFuels.Add(FuelType.Hybride);

            // Initialise statuts
            AvailableStatuses.Add(VehicleStatus.EnService);
            AvailableStatuses.Add(VehicleStatus.HorsService);
            AvailableStatuses.Add(VehicleStatus.EnReparation);

            LoadVehicles();
            LoadUsers();

            AddVehicleCommand = new RelayCommand(_ => OpenAddDialog());
            SaveNewVehicleCommand = new RelayCommand(_ => SaveNewVehicle(), _ => CanSaveNewVehicle());
            CancelNewVehicleCommand = new RelayCommand(_ => CancelNewVehicle());
            ReloadCommand = new RelayCommand(_ => LoadVehicles());
            LoadUsersCommand = new RelayCommand(_ => LoadUsers());
            NavigateToDashboardCommand = new RelayCommand(_ => App.Nav.NavigateTo("Dashboard"));
            NavigateToEntretiensCommand = new RelayCommand(_ => App.Nav.NavigateTo("Entretiens"));
            NavigateToStatistiquesCommand = new RelayCommand(_ => App.Nav.NavigateTo("Statistiques"));
            NavigateToSettingsCommand = new RelayCommand(_ => App.Nav.NavigateTo("Settings"));
            NavigateToPiecesCommand = new RelayCommand(_ => App.Nav.NavigateTo("Pieces"));
            DeleteVehicleCommand = new RelayCommand(v => DeleteVehicle(v as Vehicle), v => v is Vehicle);
            EditVehicleCommand = new RelayCommand(v => OpenEditDialog(v as Vehicle), v => v is Vehicle);
            SaveEditVehicleCommand = new RelayCommand(_ => SaveEditedVehicle(), _ => CanSaveEditedVehicle());
            CancelEditVehicleCommand = new RelayCommand(_ => CancelEditVehicle());
        }

        private void LoadVehicles()
        {
            try
            {
                using (var ctx = new GarageDbContext(App.ConnectionString))
                {
                    var vehicles = ctx.Vehicles.ToList();

                    Vehicles.Clear();
                    foreach (var v in vehicles)
                    {
                        Vehicles.Add(v);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"❌ ERREUR BD :\n{ex.Message}");
            }
        }

        private void LoadUsers()
        {
            try
            {
                using (var ctx = new GarageDbContext(App.ConnectionString))
                {
                    var users = ctx.Users.ToList();
                    AvailableUsers.Clear();
                    foreach (var user in users)
                    {
                        AvailableUsers.Add(user);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"❌ Erreur chargement utilisateurs :\n{ex.Message}");
            }
        }

        private void OpenAddDialog()
        {
            // NOTE: certains schémas MySQL ont des colonnes NOT NULL (ex: Model).
            // Si l'utilisateur ne touche pas le champ, WPF peut laisser la string à null.
            // On initialise donc à string.Empty pour éviter un INSERT avec NULL.
            NewVehicle = new Vehicle
            {
                Immatriculation = string.Empty,
                Marque = string.Empty,
                Modele = string.Empty,
                Statut = VehicleStatus.EnService,
                Carburant = FuelType.Essence,
                Annee = DateTime.Now.Year,
                Kilometrage = 0
            };
            // SelectedUser reste null → utilisateur choisit dans ComboBox
            IsAddDialogOpen = true;
        }

        private bool CanSaveNewVehicle()
        {
            return NewVehicle != null
                && !string.IsNullOrWhiteSpace(NewVehicle.Immatriculation)
                && !string.IsNullOrWhiteSpace(NewVehicle.Marque)
                && NewVehicle.Annee > 1900 && NewVehicle.Annee <= 2100
                && NewVehicle.Kilometrage >= 0
                && NewVehicle.Id_Utilisateur > 0;  // 👈 Utilisateur sélectionné obligatoire
        }

        private void SaveNewVehicle()
        {
            try
            {
                if (NewVehicle.Id_Utilisateur == 0)
                    throw new Exception("Veuillez sélectionner un utilisateur");

                // Éviter les inserts avec NULL sur des colonnes potentiellement NOT NULL.
                NewVehicle.Immatriculation = (NewVehicle.Immatriculation ?? string.Empty).Trim();
                NewVehicle.Marque = (NewVehicle.Marque ?? string.Empty).Trim();
                NewVehicle.Modele = (NewVehicle.Modele ?? string.Empty).Trim();

                using (var ctx = new GarageDbContext(App.ConnectionString))
                {
                    if (ctx.Vehicles.Any(v => v.Immatriculation == NewVehicle.Immatriculation))
                        throw new Exception("Cette immatriculation existe déjà");

                    ctx.Vehicles.Add(NewVehicle);
                    ctx.SaveChanges();
                }

                Vehicles.Add(NewVehicle);
                IsAddDialogOpen = false;
                NewVehicle = null;
                SelectedUser = null;

                System.Windows.MessageBox.Show("✅ Véhicule ajouté avec succès !");
            }
            catch (DbUpdateException dbEx)
            {
                var details = dbEx.InnerException?.Message ?? dbEx.Message;
                System.Windows.MessageBox.Show($"❌ Erreur sauvegarde (base de données) :\n{details}");
            }
            catch (Exception ex)
            {
                var details = ex.InnerException?.Message ?? ex.Message;
                System.Windows.MessageBox.Show($"❌ Erreur sauvegarde :\n{details}");
            }
        }

        private void CancelNewVehicle()
        {
            IsAddDialogOpen = false;
            NewVehicle = null;
            SelectedUser = null;
        }

        private void OpenEditDialog(Vehicle vehicle)
        {
            if (vehicle == null)
                return;

            _vehicleBeingEdited = vehicle;
            _editOriginalImmatriculation = vehicle.Immatriculation;

            EditVehicle = new Vehicle
            {
                Immatriculation = vehicle.Immatriculation,
                Marque = vehicle.Marque,
                Modele = vehicle.Modele,
                Annee = vehicle.Annee,
                Kilometrage = vehicle.Kilometrage,
                Statut = vehicle.Statut,
                Carburant = vehicle.Carburant,
                Id_Utilisateur = vehicle.Id_Utilisateur
            };

            IsEditDialogOpen = true;
        }

        private bool CanSaveEditedVehicle()
        {
            return EditVehicle != null
                && !string.IsNullOrWhiteSpace(EditVehicle.Immatriculation)
                && EditVehicle.Kilometrage >= 0;
        }

        private void SaveEditedVehicle()
        {
            try
            {
                using (var ctx = new GarageDbContext(App.ConnectionString))
                {
                    var entity = ctx.Vehicles.SingleOrDefault(v => v.Immatriculation == _editOriginalImmatriculation);
                    if (entity == null)
                        throw new Exception("Véhicule introuvable en base");

                    // Vérifier la nouvelle immatriculation si elle a changé
                    if (!string.Equals(EditVehicle.Immatriculation, _editOriginalImmatriculation, StringComparison.OrdinalIgnoreCase))
                    {
                        if (ctx.Vehicles.Any(v => v.Immatriculation == EditVehicle.Immatriculation))
                            throw new Exception("Cette immatriculation existe déjà");
                    }

                    entity.Immatriculation = EditVehicle.Immatriculation;
                    entity.Kilometrage = EditVehicle.Kilometrage;
                    entity.Statut = EditVehicle.Statut;
                    entity.Carburant = EditVehicle.Carburant;

                    ctx.SaveChanges();
                }

                if (_vehicleBeingEdited != null)
                {
                    _vehicleBeingEdited.Immatriculation = EditVehicle.Immatriculation;
                    _vehicleBeingEdited.Kilometrage = EditVehicle.Kilometrage;
                    _vehicleBeingEdited.Statut = EditVehicle.Statut;
                    _vehicleBeingEdited.Carburant = EditVehicle.Carburant;

                    // Forcer la mise à jour dans la vue
                    var index = Vehicles.IndexOf(_vehicleBeingEdited);
                    if (index >= 0)
                    {
                        Vehicles[index] = _vehicleBeingEdited;
                    }
                }

                IsEditDialogOpen = false;
                EditVehicle = null;
                _vehicleBeingEdited = null;
                _editOriginalImmatriculation = null;

                System.Windows.MessageBox.Show("✅ Véhicule modifié avec succès !");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"❌ Erreur modification :\n{ex.Message}");
            }
        }

        private void CancelEditVehicle()
        {
            IsEditDialogOpen = false;
            EditVehicle = null;
            _vehicleBeingEdited = null;
            _editOriginalImmatriculation = null;
        }

        private void DeleteVehicle(Vehicle vehicle)
        {
            if (vehicle == null)
                return;

            try
            {
                using (var ctx = new GarageDbContext(App.ConnectionString))
                {
                    var entity = ctx.Vehicles.SingleOrDefault(v => v.Immatriculation == vehicle.Immatriculation);
                    if (entity != null)
                    {
                        ctx.Vehicles.Remove(entity);
                        ctx.SaveChanges();
                    }
                }

                Vehicles.Remove(vehicle);

                if (SelectedVehicle == vehicle)
                {
                    SelectedVehicle = null;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"❌ Erreur suppression :\n{ex.Message}");
            }
        }
    }
}
