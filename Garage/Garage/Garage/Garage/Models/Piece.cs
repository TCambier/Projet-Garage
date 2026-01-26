using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("piece")]
public class Piece
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("Libelle")]
    public string Libelle { get; set; }

    [Column("Prix_Unite")]
    public decimal Prix_Unite { get; set; }

    [Column("Nb_Piece")]
    public int Nb_Piece { get; set; }

    [NotMapped]
    public string Display
        => $"{Libelle} - {Prix_Unite:0.00} € (Stock: {Nb_Piece})";
}
