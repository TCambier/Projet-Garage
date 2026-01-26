using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Garage.Data;

public static class DbInitializer
{
    public static void Initialize(string connectionString)
    {
        using var ctx = new GarageDbContext(connectionString);
        ctx.Database.EnsureCreated();

        if (!ctx.Users.Any())
        {
            var admin = new User
            {
                Identifiant = "J.Durand",
                Mdp = HashHelper.Sha256("admin123"), // change password
                Statut = Role.Patron
            };
            ctx.Users.Add(admin);
            ctx.SaveChanges();
        }

        
    }
}
