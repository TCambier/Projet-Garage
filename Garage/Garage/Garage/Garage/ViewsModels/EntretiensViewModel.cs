using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Garage.Data;
using GarageApp;

namespace Garage.ViewModels
{
    public class EntretiensViewModel : ViewModelBase
    {
        private MaintenanceRecord _selectedMaintenance;
        private MaintenanceRecord _newMaintenance;
        private MaintenanceRecord _editMaintenance;
        private MaintenanceRecord _maintenanceBeingEdited;
        private bool _isAddDialogOpen;
        private bool _isEditDialogOpen;

        private Vehicle _selectedVehicleForNewMaintenance;

        public ObservableCollection<MaintenanceRecord> Maintenances { get; } = new();
        public ObservableCollection<Vehicle> AvailableVehicles { get; } = new();
        public ObservableCollection<Piece> AvailablePieces { get; } = new();

        // Sélection pièces pour le nouvel entretien
        public ObservableCollection<MaintenancePieceLine> NewMaintenancePieces { get; } = new();

        private Piece _selectedPieceForNewMaintenance;
        public Piece SelectedPieceForNewMaintenance
        {
            get => _selectedPieceForNewMaintenance;
            set
            {
                if (_selectedPieceForNewMaintenance != value)
                {
                    _selectedPieceForNewMaintenance = value;
                    Raise();
                }
            }
        }

        private int _quantityForNewPiece = 1;
        public int QuantityForNewPiece
        {
            get => _quantityForNewPiece;
            set
            {
                if (_quantityForNewPiece != value)
                {
                    _quantityForNewPiece = value;
                    Raise();
                }
            }
        }

        public decimal NewPiecesTotal => NewMaintenancePieces.Sum(l => l.Total);

        public MaintenanceRecord SelectedMaintenance
        {
            get => _selectedMaintenance;
            set
            {
                if (_selectedMaintenance != value)
                {
                    _selectedMaintenance = value;
                    Raise();
                }
            }
        }

        public MaintenanceRecord NewMaintenance
        {
            get => _newMaintenance;
            set
            {
                if (_newMaintenance != value)
                {
                    _newMaintenance = value;
                    Raise();
                }
            }
        }

        public Vehicle SelectedVehicleForNewMaintenance
        {
            get => _selectedVehicleForNewMaintenance;
            set
            {
                if (_selectedVehicleForNewMaintenance != value)
                {
                    _selectedVehicleForNewMaintenance = value;
                    if (NewMaintenance != null && value != null)
                    {
                        NewMaintenance.Immatriculation = value.Immatriculation;
                        NewMaintenance.Id_Utilisateur = value.Id_Utilisateur;
                        Raise(nameof(NewMaintenance));
                    }
                    Raise();
                }
            }
        }

        public MaintenanceRecord EditMaintenance
        {
            get => _editMaintenance;
            set
            {
                if (_editMaintenance != value)
                {
                    _editMaintenance = value;
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

        // Commands
        public ICommand AddMaintenanceCommand { get; }
        public ICommand SaveNewMaintenanceCommand { get; }
        public ICommand CancelNewMaintenanceCommand { get; }
        public ICommand AddPieceToNewMaintenanceCommand { get; }
        public ICommand RemovePieceFromNewMaintenanceCommand { get; }
        public ICommand EditMaintenanceCommand { get; }
        public ICommand SaveEditMaintenanceCommand { get; }
        public ICommand CancelEditMaintenanceCommand { get; }
        public ICommand DeleteMaintenanceCommand { get; }
        public ICommand MarkAsDoneTodayCommand { get; }
        public ICommand NavigateToDashboardCommand { get; }
        public ICommand NavigateToVehiculesCommand { get; }
        public ICommand NavigateToStatistiquesCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }

        public EntretiensViewModel()
        {
            LoadMaintenances();
            LoadVehicles();
            LoadPieces();

            // recalcul auto du coût mini quand la liste change
            NewMaintenancePieces.CollectionChanged += (_, __) =>
            {
                Raise(nameof(NewPiecesTotal));
                CommandManager.InvalidateRequerySuggested();
            };

            AddMaintenanceCommand = new RelayCommand(_ => OpenAddDialog());
            SaveNewMaintenanceCommand = new RelayCommand(_ => SaveNewMaintenance(), _ => CanSaveNewMaintenance());
            CancelNewMaintenanceCommand = new RelayCommand(_ => CancelNewMaintenance());

            AddPieceToNewMaintenanceCommand = new RelayCommand(_ => AddPieceToNewMaintenance(), _ => CanAddPieceToNewMaintenance());
            RemovePieceFromNewMaintenanceCommand = new RelayCommand(p => RemovePieceFromNewMaintenance(p as MaintenancePieceLine), p => p is MaintenancePieceLine);

            EditMaintenanceCommand = new RelayCommand(m => OpenEditDialog(m as MaintenanceRecord), m => m is MaintenanceRecord);
            SaveEditMaintenanceCommand = new RelayCommand(_ => SaveEditedMaintenance(), _ => CanSaveEditedMaintenance());
            CancelEditMaintenanceCommand = new RelayCommand(_ => CancelEditMaintenance());

            DeleteMaintenanceCommand = new RelayCommand(m => DeleteMaintenance(m as MaintenanceRecord), m => m is MaintenanceRecord);
            MarkAsDoneTodayCommand = new RelayCommand(m => MarkAsDoneToday(m as MaintenanceRecord), m => m is MaintenanceRecord);

            NavigateToDashboardCommand = new RelayCommand(_ => App.Nav.NavigateTo("Dashboard"));
            NavigateToVehiculesCommand = new RelayCommand(_ => App.Nav.NavigateTo("Vehicules"));
            NavigateToStatistiquesCommand = new RelayCommand(_ => App.Nav.NavigateTo("Statistiques"));
            NavigateToSettingsCommand = new RelayCommand(_ => App.Nav.NavigateTo("Settings"));
        }

        private void LoadMaintenances()
        {
            try
            {
                using (var ctx = new GarageDbContext(App.ConnectionString))
                {
                    var list = ctx.Maintenances
                        .OrderBy(m => m.DateIntervention)
                        .ToList();

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
                        {
                            m.Pieces = string.Join(", ", used.Select(u => $"{u.Piece.Libelle} x{u.Quantite}"));
                        }
                        else
                        {
                            m.Pieces = string.Empty;
                        }
                    }

                    Maintenances.Clear();
                    foreach (var m in list)
                    {
                        Maintenances.Add(m);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Erreur chargement entretiens :\n{ex.Message}");
            }
        }

        private void LoadVehicles()
        {
            try
            {
                using (var ctx = new GarageDbContext(App.ConnectionString))
                {
                    var list = ctx.Vehicles
                        .OrderBy(v => v.Immatriculation)
                        .ToList();

                    AvailableVehicles.Clear();
                    foreach (var v in list)
                    {
                        AvailableVehicles.Add(v);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Erreur chargement véhicules :\n{ex.Message}");
            }
        }

        private void LoadPieces()
        {
            try
            {
                using (var ctx = new GarageDbContext(App.ConnectionString))
                {
                    var list = ctx.Pieces
                        .Where(p => p.Nb_Piece > 0)
                        .OrderBy(p => p.Libelle)
                        .ToList();

                    AvailablePieces.Clear();
                    foreach (var p in list)
                    {
                        AvailablePieces.Add(p);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Erreur chargement pièces :\n{ex.Message}");
            }
        }

        private void OpenAddDialog()
        {
            NewMaintenance = new MaintenanceRecord
            {
                DateIntervention = DateTime.Today,
                Kilometrage = 0,
                Cout = 0,
                Prix = 0,
                Id_Utilisateur = App.Auth?.CurrentUser?.Id ?? 0,
                Statut = "En Cours"
            };

            // reset sélection véhicule + pièces
            SelectedVehicleForNewMaintenance = null;
            SelectedPieceForNewMaintenance = null;
            QuantityForNewPiece = 1;
            NewMaintenancePieces.Clear();
            Raise(nameof(NewPiecesTotal));

            IsAddDialogOpen = true;
        }

        private bool CanAddPieceToNewMaintenance()
        {
            return SelectedPieceForNewMaintenance != null && QuantityForNewPiece > 0;
        }

        private void AddPieceToNewMaintenance()
        {
            if (SelectedPieceForNewMaintenance == null)
                return;

            if (QuantityForNewPiece <= 0)
            {
                MessageBox.Show("Quantité invalide (doit être > 0)");
                return;
            }

            // additionne sur la même pièce si déjà présente dans la liste
            var existing = NewMaintenancePieces.FirstOrDefault(l => l.Piece.Id == SelectedPieceForNewMaintenance.Id);
            var alreadyUsed = existing?.Quantite ?? 0;

            if (alreadyUsed + QuantityForNewPiece > SelectedPieceForNewMaintenance.Nb_Piece)
            {
                MessageBox.Show($"Stock insuffisant pour '{SelectedPieceForNewMaintenance.Libelle}' (stock: {SelectedPieceForNewMaintenance.Nb_Piece}, demandé: {alreadyUsed + QuantityForNewPiece})");
                return;
            }

            if (existing != null)
            {
                existing.Quantite += QuantityForNewPiece;
            }
            else
            {
                NewMaintenancePieces.Add(new MaintenancePieceLine(SelectedPieceForNewMaintenance, QuantityForNewPiece));
            }

            // force un coût minimum = total des pièces
            if (NewMaintenance != null && NewMaintenance.Cout < NewPiecesTotal)
            {
                NewMaintenance.Cout = NewPiecesTotal;
                Raise(nameof(NewMaintenance));
            }

            Raise(nameof(NewPiecesTotal));
            CommandManager.InvalidateRequerySuggested();
        }

        private void RemovePieceFromNewMaintenance(MaintenancePieceLine line)
        {
            if (line == null)
                return;

            NewMaintenancePieces.Remove(line);

            if (NewMaintenance != null && NewMaintenance.Cout < NewPiecesTotal)
            {
                NewMaintenance.Cout = NewPiecesTotal;
                Raise(nameof(NewMaintenance));
            }

            Raise(nameof(NewPiecesTotal));
            CommandManager.InvalidateRequerySuggested();
        }

        private bool CanSaveNewMaintenance()
        {
            return NewMaintenance != null
                   && !string.IsNullOrWhiteSpace(NewMaintenance.Immatriculation)
                   && !string.IsNullOrWhiteSpace(NewMaintenance.Type)
                   && NewMaintenance.Kilometrage >= 0
                   && NewMaintenance.Cout >= NewPiecesTotal;
        }

        private void SaveNewMaintenance()
        {
            try
            {
                using (var ctx = new GarageDbContext(App.ConnectionString))
                using (var tx = ctx.Database.BeginTransaction())
                {
                    // On sécurise le coût mini côté base aussi (mais on force ici pour l'IHM)
                    if (NewMaintenance.Cout < NewPiecesTotal)
                        NewMaintenance.Cout = NewPiecesTotal;

                    ctx.Maintenances.Add(NewMaintenance);
                    ctx.SaveChanges(); // récupère Id

                    // Enregistrer les pièces réellement utilisées + décrémenter le stock
                    foreach (var line in NewMaintenancePieces)
                    {
                        var piece = ctx.Pieces.Single(p => p.Id == line.Piece.Id);

                        if (piece.Nb_Piece < line.Quantite)
                            throw new Exception($"Stock insuffisant pour '{piece.Libelle}' (stock: {piece.Nb_Piece}, demandé: {line.Quantite})");

                        piece.Nb_Piece -= line.Quantite;

                        ctx.Utilisations.Add(new Utilise
                        {
                            Id_Entretien = NewMaintenance.Id,
                            Id_Piece = piece.Id,
                            Prix_Unite_Applique = piece.Prix_Unite,
                            Quantite = line.Quantite,
                        });
                    }

                    ctx.SaveChanges();
                    tx.Commit();
                }

                // libellé IHM (persisté via table utilise, mais affichage via champ non-mappé)
                NewMaintenance.Pieces = string.Join(", ", NewMaintenancePieces.Select(l => $"{l.Piece.Libelle} x{l.Quantite}"));

                Maintenances.Add(NewMaintenance);
                IsAddDialogOpen = false;
                NewMaintenance = null;

                // reload pièces (stock mis à jour)
                LoadPieces();

                MessageBox.Show("✅ Entretien ajouté avec succès !");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Erreur ajout entretien :\n{ex.Message}");
            }
        }

        private void CancelNewMaintenance()
        {
            IsAddDialogOpen = false;
            NewMaintenance = null;
            NewMaintenancePieces.Clear();
            SelectedPieceForNewMaintenance = null;
            QuantityForNewPiece = 1;
            Raise(nameof(NewPiecesTotal));
        }

        private void OpenEditDialog(MaintenanceRecord maintenance)
        {
            if (maintenance == null)
                return;

            _maintenanceBeingEdited = maintenance;

            EditMaintenance = new MaintenanceRecord
            {
                Id = maintenance.Id,
                Type = maintenance.Type,
                Immatriculation = maintenance.Immatriculation,
                Pieces = maintenance.Pieces,
                DateIntervention = maintenance.DateIntervention,
                Kilometrage = maintenance.Kilometrage,
                Cout = maintenance.Cout,
                Prix = maintenance.Prix,
                Id_Utilisateur = maintenance.Id_Utilisateur,
                Statut = maintenance.Statut
            };

            IsEditDialogOpen = true;
        }

        private bool CanSaveEditedMaintenance()
        {
            return EditMaintenance != null
                   && !string.IsNullOrWhiteSpace(EditMaintenance.Immatriculation)
                   && !string.IsNullOrWhiteSpace(EditMaintenance.Type);
        }

        private void SaveEditedMaintenance()
        {
            try
            {
                using (var ctx = new GarageDbContext(App.ConnectionString))
                {
                    var entity = ctx.Maintenances.SingleOrDefault(m => m.Id == EditMaintenance.Id);
                    if (entity == null)
                        throw new Exception("Entretien introuvable en base");

                    entity.Type = EditMaintenance.Type;
                    entity.Immatriculation = EditMaintenance.Immatriculation;
                    entity.DateIntervention = EditMaintenance.DateIntervention;
                    entity.Kilometrage = EditMaintenance.Kilometrage;
                    entity.Cout = EditMaintenance.Cout;
                    entity.Prix = EditMaintenance.Prix;
                    entity.Id_Utilisateur = EditMaintenance.Id_Utilisateur;
                    entity.Statut = EditMaintenance.Statut;

                    ctx.SaveChanges();
                }

                if (_maintenanceBeingEdited != null)
                {
                    _maintenanceBeingEdited.Type = EditMaintenance.Type;
                    _maintenanceBeingEdited.Immatriculation = EditMaintenance.Immatriculation;
                    _maintenanceBeingEdited.Pieces = EditMaintenance.Pieces;
                    _maintenanceBeingEdited.DateIntervention = EditMaintenance.DateIntervention;
                    _maintenanceBeingEdited.Kilometrage = EditMaintenance.Kilometrage;
                    _maintenanceBeingEdited.Cout = EditMaintenance.Cout;
                    _maintenanceBeingEdited.Prix = EditMaintenance.Prix;
                    _maintenanceBeingEdited.Id_Utilisateur = EditMaintenance.Id_Utilisateur;
                    _maintenanceBeingEdited.Statut = EditMaintenance.Statut;

                    var index = Maintenances.IndexOf(_maintenanceBeingEdited);
                    if (index >= 0)
                    {
                        Maintenances[index] = _maintenanceBeingEdited;
                    }
                }

                IsEditDialogOpen = false;
                EditMaintenance = null;
                _maintenanceBeingEdited = null;

                System.Windows.MessageBox.Show("✅ Entretien modifié avec succès !");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"❌ Erreur modification entretien :\n{ex.Message}");
            }
        }

        private void CancelEditMaintenance()
        {
            IsEditDialogOpen = false;
            EditMaintenance = null;
            _maintenanceBeingEdited = null;
        }

        private void DeleteMaintenance(MaintenanceRecord maintenance)
        {
            if (maintenance == null)
                return;

            try
            {
                using (var ctx = new GarageDbContext(App.ConnectionString))
                {
                    var entity = ctx.Maintenances.SingleOrDefault(m => m.Id == maintenance.Id);
                    if (entity != null)
                    {
                        ctx.Maintenances.Remove(entity);
                        ctx.SaveChanges();
                    }
                }

                Maintenances.Remove(maintenance);

                if (SelectedMaintenance == maintenance)
                {
                    SelectedMaintenance = null;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"❌ Erreur suppression entretien :\n{ex.Message}");
            }
        }

        private void MarkAsDoneToday(MaintenanceRecord maintenance)
        {
            if (maintenance == null)
                return;

            try
            {
                using (var ctx = new GarageDbContext(App.ConnectionString))
                {
                    var entity = ctx.Maintenances.SingleOrDefault(m => m.Id == maintenance.Id);
                    if (entity == null)
                        throw new Exception("Entretien introuvable en base");

                    entity.DateIntervention = DateTime.Today;
                    entity.Statut = "Terminé";
                    ctx.SaveChanges();
                }

                // Mise à jour de l'objet déjà dans la liste (on ne le supprime pas)
                maintenance.DateIntervention = DateTime.Today;
                maintenance.Statut = "Terminé";

                // Force un rafraîchissement de la ligne dans la ListView pour mettre à jour les bindings/visibilités
                var index = Maintenances.IndexOf(maintenance);
                if (index >= 0)
                {
                    Maintenances[index] = maintenance;
                }

                System.Windows.MessageBox.Show("✅ Entretien marqué comme terminé pour aujourd'hui.");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"❌ Erreur mise à jour entretien :\n{ex.Message}");
            }
        }
    }
}
