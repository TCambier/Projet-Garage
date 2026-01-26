using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;

public enum VehicleStatus { EnService, HorsService, EnReparation }
public enum FuelType { Essence, Diesel, Electrique, Hybride }

[Table("voiture")]
public class Vehicle
{
    // Id non mappé car la clé primaire dans MySQL est l'immatriculation
    [NotMapped]
    public int Id { get; set; }

    [Key]
    [Required]
    [Column("Immatriculation")]
    public string Immatriculation { get; set; }

    [Column("Marque")]
    public string Marque { get; set; }

    // La colonne dans MySQL s'appelle "Model"
    [Column("Model")]
    public string Modele { get; set; }

    // Dans MySQL, Annee est un INT
    [Column("Annee")]
    public int Annee { get; set; } 


    [Column("Kilometrage")]
    public int Kilometrage { get; set; }

    // Pas de colonne dédiée dans la table voiture : non mappé
    [NotMapped]
    public DateTime? DateDernierEntretien { get; set; }

    [Column("Statut")]
    public VehicleStatus Statut { get; set; }

    [Column("Type_Carburant")]
    public FuelType Carburant { get; set; }

    [Column("Id_Utilisateur")]
    public int Id_Utilisateur { get; set; }

    [NotMapped]
    public ICollection<MaintenanceRecord> Maintenances { get; set; } = new List<MaintenanceRecord>();
}
