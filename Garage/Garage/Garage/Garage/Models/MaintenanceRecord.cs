using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("entretien")]
public class MaintenanceRecord
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Required]
    [Column("Type")]
    public string Type { get; set; }

    [Column("Date_Intervention")]
    public DateTime DateIntervention { get; set; }

    [Column("Cout")]
    public decimal Cout { get; set; }

    [Column("Prix")]
    public decimal Prix { get; set; }

    [Column("Kilometrage")]
    public int Kilometrage { get; set; }

    [Column("Immatriculation")]
    public string Immatriculation { get; set; }

    [Column("Id_Utilisateur")]
    public int Id_Utilisateur { get; set; }

    [Column("Statut")]
    public string Statut { get; set; }

    // Navigation EF (table utilise)
    public ICollection<Utilise> Utilisations { get; set; } = new List<Utilise>();

    // Champs additionnels non mappés pour l'IHM
    [NotMapped]
    public string Pieces { get; set; }
}
