using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Garage.ViewModels
{
    public class MaintenancePieceLine : INotifyPropertyChanged
    {
        private int _quantite;

        public Piece Piece { get; }

        public int Quantite
        {
            get => _quantite;
            set
            {
                if (_quantite != value)
                {
                    _quantite = value;
                    Raise();
                    Raise(nameof(Total));
                }
            }
        }

        public decimal PrixUnite => Piece?.Prix_Unite ?? 0m;

        public decimal Total => PrixUnite * Quantite;

        public MaintenancePieceLine(Piece piece, int quantite)
        {
            Piece = piece;
            _quantite = quantite;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void Raise([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
