using System;
using System.Collections.Generic;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Garage.Services
{
    /// <summary>
    /// Données d'une ligne de pièce pour la facture.
    /// </summary>
    public class InvoicePieceLine
    {
        public string Libelle { get; set; }
        public int Quantite { get; set; }
        public decimal PrixUnitaire { get; set; }
        public decimal Total => PrixUnitaire * Quantite;
    }

    /// <summary>
    /// Données complètes nécessaires à la génération d'une facture PDF.
    /// </summary>
    public class InvoiceData
    {
        public int EntretienId { get; set; }
        public string Type { get; set; }
        public DateTime DateIntervention { get; set; }
        public string Immatriculation { get; set; }
        public string Marque { get; set; }
        public string Modele { get; set; }
        public int Kilometrage { get; set; }
        public string ClientNom { get; set; }
        public decimal CoutTotal { get; set; }
        public decimal PrixFacture { get; set; }
        public List<InvoicePieceLine> Pieces { get; set; } = new();
    }

    public static class InvoicePdfService
    {
        /// <summary>
        /// Génère un fichier PDF de facture au chemin spécifié.
        /// </summary>
        public static void Generate(InvoiceData data, string outputPath)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.MarginHorizontal(40);
                    page.MarginVertical(50);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Element(c => ComposeHeader(c, data));
                    page.Content().Element(c => ComposeContent(c, data));
                    page.Footer().Element(ComposeFooter);
                });
            });

            document.GeneratePdf(outputPath);
        }

        private static void ComposeHeader(IContainer container, InvoiceData data)
        {
            container.Column(col =>
            {
                // Nom du garage
                col.Item().Text("AutoMaint Pro – Garage")
                    .FontSize(22).Bold().FontColor("#1e3a5f");

                col.Item().PaddingTop(4).Text("Facture d'entretien")
                    .FontSize(14).FontColor("#6B7280");

                col.Item().PaddingTop(16).LineHorizontal(1).LineColor("#D1D5DB");
            });
        }

        private static void ComposeContent(IContainer container, InvoiceData data)
        {
            container.PaddingTop(20).Column(col =>
            {
                // --- Bloc infos facture + client ---
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(left =>
                    {
                        left.Item().Text("Informations facture").Bold().FontSize(12);
                        left.Item().PaddingTop(6).Text($"N° Facture : ENT-{data.EntretienId:D5}");
                        left.Item().Text($"Date : {data.DateIntervention:dd/MM/yyyy}");
                        left.Item().Text($"Type : {data.Type}");
                    });

                    row.RelativeItem().Column(right =>
                    {
                        right.Item().Text("Client").Bold().FontSize(12);
                        right.Item().PaddingTop(6).Text(data.ClientNom ?? "—");
                    });
                });

                col.Item().PaddingVertical(12).LineHorizontal(0.5f).LineColor("#E5E7EB");

                // --- Bloc véhicule ---
                col.Item().Text("Véhicule").Bold().FontSize(12);
                col.Item().PaddingTop(4).Text($"Immatriculation : {data.Immatriculation}");
                col.Item().Text($"Marque / Modèle : {data.Marque ?? "—"} {data.Modele ?? ""}");
                col.Item().Text($"Kilométrage : {data.Kilometrage} km");

                col.Item().PaddingVertical(12).LineHorizontal(0.5f).LineColor("#E5E7EB");

                // --- Tableau des pièces ---
                if (data.Pieces.Count > 0)
                {
                    col.Item().Text("Détail des pièces").Bold().FontSize(12);
                    col.Item().PaddingTop(8).Element(c => ComposePiecesTable(c, data.Pieces));
                    col.Item().PaddingVertical(12).LineHorizontal(0.5f).LineColor("#E5E7EB");
                }

                // --- Totaux ---
                col.Item().PaddingTop(4).AlignRight().Column(totals =>
                {
                    decimal totalPieces = 0;
                    foreach (var p in data.Pieces)
                        totalPieces += p.Total;

                    if (data.Pieces.Count > 0)
                    {
                        totals.Item().Text($"Total pièces : {totalPieces:0.00} €").FontSize(11);
                    }

                    totals.Item().Text($"Coût entretien : {data.CoutTotal:0.00} €").FontSize(11);

                    totals.Item().PaddingTop(6)
                        .Text($"TOTAL À PAYER : {data.PrixFacture:0.00} €")
                        .Bold().FontSize(14).FontColor("#1e3a5f");
                });
            });
        }

        private static void ComposePiecesTable(IContainer container, List<InvoicePieceLine> pieces)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4); // Libellé
                    columns.RelativeColumn(1); // Qté
                    columns.RelativeColumn(2); // PU
                    columns.RelativeColumn(2); // Total
                });

                // En-tête
                table.Header(header =>
                {
                    header.Cell().Background("#1e3a5f").Padding(6)
                        .Text("Pièce").FontColor(Colors.White).Bold();
                    header.Cell().Background("#1e3a5f").Padding(6)
                        .Text("Qté").FontColor(Colors.White).Bold();
                    header.Cell().Background("#1e3a5f").Padding(6)
                        .Text("P.U.").FontColor(Colors.White).Bold();
                    header.Cell().Background("#1e3a5f").Padding(6)
                        .Text("Total").FontColor(Colors.White).Bold();
                });

                // Lignes
                bool alt = false;
                foreach (var piece in pieces)
                {
                    var bg = alt ? "#F3F4F6" : "#FFFFFF";
                    table.Cell().Background(bg).Padding(6).Text(piece.Libelle);
                    table.Cell().Background(bg).Padding(6).Text(piece.Quantite.ToString());
                    table.Cell().Background(bg).Padding(6).Text($"{piece.PrixUnitaire:0.00} €");
                    table.Cell().Background(bg).Padding(6).Text($"{piece.Total:0.00} €");
                    alt = !alt;
                }
            });
        }

        private static void ComposeFooter(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().LineHorizontal(1).LineColor("#D1D5DB");
                col.Item().PaddingTop(8).AlignCenter()
                    .Text("AutoMaint Pro – Merci de votre confiance")
                    .FontSize(9).FontColor("#9CA3AF");
            });
        }
    }
}
