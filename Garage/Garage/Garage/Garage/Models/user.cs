using System.ComponentModel.DataAnnotations;

public enum Role { Patron, Mecanicien, Secretaire, Client }

public class User
{
    [Key] public int Id { get; set; }
    [Required] public string Identifiant { get; set; }
    [Required] public string Mdp { get; set; }
    [Required] public Role Statut { get; set; }
    public string Nom { get; set; }
    public string Prenom { get; set; }

    // Utilisé pour la recherche (TextSearch.TextPath)
    public string NomPrenom
    {
        get
        {
            var fullName = $"{Nom ?? string.Empty} {Prenom ?? string.Empty}".Trim();
            return string.IsNullOrWhiteSpace(fullName) ? Id.ToString() : fullName;
        }
    }

    // Utilisé par le ComboBox (VehiclesView.xaml) pour afficher un libellé lisible.
    // Format demandé : "Nom Prénom - Id".
    public string PrenomNom
    {
        get
        {
            var fullName = NomPrenom;
            // Si on n'a pas de nom/prénom, on affiche juste l'id.
            return string.IsNullOrWhiteSpace(fullName) || fullName == Id.ToString()
                ? Id.ToString()
                : $"{fullName} - {Id}";
        }
    }
}
