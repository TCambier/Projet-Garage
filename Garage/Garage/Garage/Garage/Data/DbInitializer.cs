using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Garage.Data;

public static class DbInitializer
{
    public static void Initialize(string connectionString, string adminUser, string adminPassword)
    {
        using var ctx = new GarageDbContext(connectionString);
        ctx.Database.EnsureCreated();

        if (!ctx.Users.Any())
        {
            if (string.IsNullOrWhiteSpace(adminUser) || string.IsNullOrWhiteSpace(adminPassword))
            {
                System.Diagnostics.Debug.WriteLine(
                    "[DbInitializer] Aucun compte admin créé : identifiants non configurés. " +
                    "Renseignez AdminAccount:Identifiant et AdminAccount:Password dans appsettings.json.");
                return;
            }

            var admin = new User
            {
                Identifiant = adminUser,
                Mdp = HashHelper.Sha256(adminPassword),
                Statut = Role.Patron
            };
            ctx.Users.Add(admin);
            ctx.SaveChanges();
        }
    }
}
