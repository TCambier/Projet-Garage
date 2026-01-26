using System.ComponentModel.DataAnnotations.Schema;

[Table("utilise")]
public class Utilise
{
    [Column("Id_Entretien")]
    public int Id_Entretien { get; set; }

    [Column("Id_Piece")]
    public int Id_Piece { get; set; }

    [Column("Prix_Unite_Applique")]
    public decimal Prix_Unite_Applique { get; set; }

    [Column("Quantite")]
    public int Quantite { get; set; }

    public Piece Piece { get; set; }
    public MaintenanceRecord Entretien { get; set; }
}
