using Garage.Data;
using GarageApp;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Garage.ViewModels
{
    public class PiecesViewModel : ViewModelBase
    {
        private Piece _selectedPiece;
        private Piece _newPiece;
        private Piece _editPiece;
        private Piece _pieceBeingEdited;
        private bool _isAddDialogOpen;
        private bool _isEditDialogOpen;

        public ObservableCollection<Piece> Pieces { get; } = new();

        public Piece SelectedPiece
        {
            get => _selectedPiece;
            set
            {
                if (_selectedPiece != value)
                {
                    _selectedPiece = value;
                    Raise();
                }
            }
        }

        public Piece NewPiece
        {
            get => _newPiece;
            set
            {
                if (_newPiece != value)
                {
                    _newPiece = value;
                    Raise();
                }
            }
        }

        public Piece EditPiece
        {
            get => _editPiece;
            set
            {
                if (_editPiece != value)
                {
                    _editPiece = value;
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

        // Profil utilisateur (sidebar)
        public string UserFullName { get; }
        public string UserStatus { get; }
        public string UserInitials { get; }

        // Commands CRUD
        public ICommand AddPieceCommand { get; }
        public ICommand SaveNewPieceCommand { get; }
        public ICommand CancelNewPieceCommand { get; }
        public ICommand EditPieceCommand { get; }
        public ICommand SaveEditPieceCommand { get; }
        public ICommand CancelEditPieceCommand { get; }
        public ICommand DeletePieceCommand { get; }

        // Commands navigation
        public ICommand NavigateToDashboardCommand { get; }
        public ICommand NavigateToVehiculesCommand { get; }
        public ICommand NavigateToEntretiensCommand { get; }
        public ICommand NavigateToStatistiquesCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }

        public PiecesViewModel()
        {
            // Profil utilisateur
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

            LoadPieces();

            AddPieceCommand = new RelayCommand(_ => OpenAddDialog());
            SaveNewPieceCommand = new RelayCommand(_ => SaveNewPiece(), _ => CanSaveNewPiece());
            CancelNewPieceCommand = new RelayCommand(_ => CancelNewPiece());
            EditPieceCommand = new RelayCommand(p => OpenEditDialog(p as Piece), p => p is Piece);
            SaveEditPieceCommand = new RelayCommand(_ => SaveEditedPiece(), _ => CanSaveEditedPiece());
            CancelEditPieceCommand = new RelayCommand(_ => CancelEditPiece());
            DeletePieceCommand = new RelayCommand(p => DeletePiece(p as Piece), p => p is Piece);

            NavigateToDashboardCommand = new RelayCommand(_ => App.Nav.NavigateTo("Dashboard"));
            NavigateToVehiculesCommand = new RelayCommand(_ => App.Nav.NavigateTo("Vehicules"));
            NavigateToEntretiensCommand = new RelayCommand(_ => App.Nav.NavigateTo("Entretiens"));
            NavigateToStatistiquesCommand = new RelayCommand(_ => App.Nav.NavigateTo("Statistiques"));
            NavigateToSettingsCommand = new RelayCommand(_ => App.Nav.NavigateTo("Settings"));
        }

        private void LoadPieces()
        {
            try
            {
                using (var ctx = new GarageDbContext(App.ConnectionString))
                {
                    var pieces = ctx.Pieces.ToList();
                    Pieces.Clear();
                    foreach (var p in pieces)
                    {
                        Pieces.Add(p);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"❌ ERREUR BD :\n{ex.Message}");
            }
        }

        private void OpenAddDialog()
        {
            NewPiece = new Piece
            {
                Libelle = string.Empty,
                Prix_Unite = 0,
                Nb_Piece = 0
            };
            IsAddDialogOpen = true;
        }

        private bool CanSaveNewPiece()
        {
            return NewPiece != null
                && !string.IsNullOrWhiteSpace(NewPiece.Libelle)
                && NewPiece.Prix_Unite >= 0
                && NewPiece.Nb_Piece >= 0;
        }

        private void SaveNewPiece()
        {
            try
            {
                NewPiece.Libelle = (NewPiece.Libelle ?? string.Empty).Trim();

                using (var ctx = new GarageDbContext(App.ConnectionString))
                {
                    ctx.Pieces.Add(NewPiece);
                    ctx.SaveChanges();
                }

                Pieces.Add(NewPiece);
                IsAddDialogOpen = false;
                NewPiece = null;

                System.Windows.MessageBox.Show("✅ Pièce ajoutée avec succès !");
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

        private void CancelNewPiece()
        {
            IsAddDialogOpen = false;
            NewPiece = null;
        }

        private void OpenEditDialog(Piece piece)
        {
            if (piece == null)
                return;

            _pieceBeingEdited = piece;

            EditPiece = new Piece
            {
                Id = piece.Id,
                Libelle = piece.Libelle,
                Prix_Unite = piece.Prix_Unite,
                Nb_Piece = piece.Nb_Piece
            };

            IsEditDialogOpen = true;
        }

        private bool CanSaveEditedPiece()
        {
            return EditPiece != null
                && !string.IsNullOrWhiteSpace(EditPiece.Libelle)
                && EditPiece.Prix_Unite >= 0
                && EditPiece.Nb_Piece >= 0;
        }

        private void SaveEditedPiece()
        {
            try
            {
                using (var ctx = new GarageDbContext(App.ConnectionString))
                {
                    var entity = ctx.Pieces.SingleOrDefault(p => p.Id == _pieceBeingEdited.Id);
                    if (entity == null)
                        throw new Exception("Pièce introuvable en base");

                    entity.Libelle = (EditPiece.Libelle ?? string.Empty).Trim();
                    entity.Prix_Unite = EditPiece.Prix_Unite;
                    entity.Nb_Piece = EditPiece.Nb_Piece;

                    ctx.SaveChanges();
                }

                if (_pieceBeingEdited != null)
                {
                    _pieceBeingEdited.Libelle = EditPiece.Libelle;
                    _pieceBeingEdited.Prix_Unite = EditPiece.Prix_Unite;
                    _pieceBeingEdited.Nb_Piece = EditPiece.Nb_Piece;

                    // Forcer la mise à jour dans la vue
                    var index = Pieces.IndexOf(_pieceBeingEdited);
                    if (index >= 0)
                    {
                        Pieces[index] = _pieceBeingEdited;
                    }
                }

                IsEditDialogOpen = false;
                EditPiece = null;
                _pieceBeingEdited = null;

                System.Windows.MessageBox.Show("✅ Pièce modifiée avec succès !");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"❌ Erreur modification :\n{ex.Message}");
            }
        }

        private void CancelEditPiece()
        {
            IsEditDialogOpen = false;
            EditPiece = null;
            _pieceBeingEdited = null;
        }

        private void DeletePiece(Piece piece)
        {
            if (piece == null)
                return;

            try
            {
                using (var ctx = new GarageDbContext(App.ConnectionString))
                {
                    var entity = ctx.Pieces.SingleOrDefault(p => p.Id == piece.Id);
                    if (entity != null)
                    {
                        ctx.Pieces.Remove(entity);
                        ctx.SaveChanges();
                    }
                }

                Pieces.Remove(piece);

                if (SelectedPiece == piece)
                {
                    SelectedPiece = null;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"❌ Erreur suppression :\n{ex.Message}");
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
