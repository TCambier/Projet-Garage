using Garage.Data;
using GarageApp;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Garage.ViewModels
{
    public class StatisticsViewModel : ViewModelBase
    {
        public ObservableCollection<MonthStatItem> MonthlyStats { get; } = new();
        public ObservableCollection<VehicleCostItem> TopVehicleCosts { get; } = new();

        public string CostLinePoints { get; private set; }
        public string ProfitLinePoints { get; private set; }

        public decimal TotalYearCost { get; private set; }
        public decimal TotalYearProfit { get; private set; }

        // Axe Y (mêmes ordres de grandeur que ceux utilisés pour normaliser les barres/lignes)
        public decimal AxisMaxValue { get; private set; }
        public decimal AxisMidValue { get; private set; }

        // Navigation
        public ICommand NavigateToDashboardCommand { get; }
        public ICommand NavigateToVehiculesCommand { get; }
        public ICommand NavigateToEntretiensCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }
        public ICommand NavigateToPiecesCommand { get; }

        public StatisticsViewModel()
        {
            LoadData();

            NavigateToDashboardCommand = new RelayCommand(_ => App.Nav.NavigateTo("Dashboard"));
            NavigateToVehiculesCommand = new RelayCommand(_ => App.Nav.NavigateTo("Vehicules"));
            NavigateToEntretiensCommand = new RelayCommand(_ => App.Nav.NavigateTo("Entretiens"));
            NavigateToSettingsCommand = new RelayCommand(_ => App.Nav.NavigateTo("Settings"));
            NavigateToPiecesCommand = new RelayCommand(_ => App.Nav.NavigateTo("Pieces"));
        }

        private void LoadData()
        {
            try
            {
                using (var ctx = new GarageDbContext(App.ConnectionString))
                {
                    const string InProgressStatus = "En Cours";

                    var today = DateTime.Today;
                    var start = new DateTime(today.Year, today.Month, 1).AddMonths(-11);

                    // NOTE: on exclut les entretiens "En Cours" des statistiques.
                    var maints = ctx.Maintenances
                        .Where(m => m.DateIntervention >= start && m.Statut != InProgressStatus)
                        .ToList();

                    MonthlyStats.Clear();

                    for (int i = 0; i < 12; i++)
                    {
                        var monthStart = start.AddMonths(i);
                        var monthEnd = monthStart.AddMonths(1);

                        var monthMaints = maints
                            .Where(m => m.DateIntervention >= monthStart && m.DateIntervention < monthEnd)
                            .ToList();

                        decimal cost = monthMaints.Sum(m => m.Cout);
                        decimal revenue = monthMaints.Sum(m => m.Prix);
                        decimal profit = revenue - cost;

                        var item = new MonthStatItem
                        {
                            Label = monthStart.ToString("MMM"),
                            Cost = cost,
                            Profit = profit
                        };

                        MonthlyStats.Add(item);
                    }

                    TotalYearCost = MonthlyStats.Sum(m => m.Cost);
                    TotalYearProfit = MonthlyStats.Sum(m => m.Profit);

                    NormalizeMonthlyHeightsAndLines();

                    // Top 5 véhicules les plus coûteux (somme des coûts d'entretien)
                    var vehicleCosts = maints
                        .GroupBy(m => m.Immatriculation)
                        .Select(g => new
                        {
                            Immatriculation = g.Key,
                            TotalCost = g.Sum(x => x.Cout)
                        })
                        .Where(x => x.TotalCost > 0)
                        .OrderByDescending(x => x.TotalCost)
                        .Take(5)
                        .ToList();

                    // Optionnel : essayer de récupérer la marque / modèle pour un label plus parlant
                    var immats = vehicleCosts.Select(v => v.Immatriculation).ToList();
                    var vehicles = ctx.Vehicles
                        .Where(v => immats.Contains(v.Immatriculation))
                        .ToList();

                    TopVehicleCosts.Clear();
                    foreach (var vc in vehicleCosts)
                    {
                        var veh = vehicles.FirstOrDefault(v => v.Immatriculation == vc.Immatriculation);
                        string label = veh != null
                            ? $"{veh.Immatriculation} - {veh.Marque} {veh.Modele}"
                            : vc.Immatriculation;

                        TopVehicleCosts.Add(new VehicleCostItem
                        {
                            Label = label,
                            TotalCost = vc.TotalCost
                        });
                    }

                    NormalizeVehicleWidths();
                }

                Raise(nameof(TotalYearCost));
                Raise(nameof(TotalYearProfit));
                Raise(nameof(CostLinePoints));
                Raise(nameof(ProfitLinePoints));
                Raise(nameof(AxisMaxValue));
                Raise(nameof(AxisMidValue));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"❌ Erreur chargement statistiques :\n{ex.Message}");
            }
        }

        private void NormalizeMonthlyHeightsAndLines()
        {
            if (MonthlyStats.Count == 0)
                return;

            // On réutilise la même valeur maximale pour :
            // - la hauteur des barres (coûts)
            // - la courbe coûts / bénéfices
            var maxDecimal = Math.Max(
                MonthlyStats.Max(m => m.Cost),
                MonthlyStats.Max(m => m.Profit));

            if (maxDecimal <= 0)
                maxDecimal = 1; // éviter division par zéro

            AxisMaxValue = maxDecimal;
            AxisMidValue = Math.Round(maxDecimal / 2m, 0);

            double maxValue = (double)maxDecimal;

            const double maxBarHeight = 120.0;   // pixels
            const double chartHeight = 120.0;    // même base pour les lignes
            const double chartBottom = 140.0;    // Y de la ligne de base
            const double chartLeft = 10.0;
            const double chartRight = 330.0;

            double stepX = (MonthlyStats.Count > 1)
                ? (chartRight - chartLeft) / (MonthlyStats.Count - 1)
                : 0;

            var costPoints = new System.Text.StringBuilder();
            var profitPoints = new System.Text.StringBuilder();

            for (int i = 0; i < MonthlyStats.Count; i++)
            {
                var m = MonthlyStats[i];
                m.CostBarHeight = (double)m.Cost / maxValue * maxBarHeight;

                double x = chartLeft + stepX * i;
                double yCost = chartBottom - ((double)m.Cost / maxValue * chartHeight);
                double yProfit = chartBottom - ((double)m.Profit / maxValue * chartHeight);

                if (i > 0)
                {
                    costPoints.Append(' ');
                    profitPoints.Append(' ');
                }

                costPoints.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "{0:F0},{1:F0}", x, yCost);
                profitPoints.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "{0:F0},{1:F0}", x, yProfit);
            }

            CostLinePoints = costPoints.ToString();
            ProfitLinePoints = profitPoints.ToString();
        }

        private void NormalizeVehicleWidths()
        {
            if (TopVehicleCosts.Count == 0)
                return;

            double maxCost = (double)TopVehicleCosts.Max(v => v.TotalCost);
            if (maxCost <= 0)
                maxCost = 1;

            const double maxWidth = 220.0;

            foreach (var v in TopVehicleCosts)
            {
                v.BarWidth = (double)v.TotalCost / maxCost * maxWidth;
            }
        }
    }

    public class MonthStatItem
    {
        public string Label { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit { get; set; }
        public double CostBarHeight { get; set; }
    }

    public class VehicleCostItem
    {
        public string Label { get; set; }
        public decimal TotalCost { get; set; }
        public double BarWidth { get; set; }
    }
}
