using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Garage.Data
{
    public class GarageDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<MaintenanceRecord> Maintenances { get; set; }
        public DbSet<Piece> Pieces { get; set; }
        public DbSet<Utilise> Utilisations { get; set; }

        private readonly string _connectionString;

        public GarageDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseMySql(_connectionString, ServerVersion.AutoDetect(_connectionString));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Faire correspondre l'entité User à la table MySQL existante "utilisateur"
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("utilisateur");

                // Le champ Statut est stocké en VARCHAR dans MySQL, mais mappé sur l'énumération Role (int).
                // On configure donc une conversion enum <-> string pour éviter l'erreur "Can't convert VarChar to Int32".
                entity.Property(e => e.Statut)
                      .HasConversion<string>();
            });

            // Mapping de l'entité Vehicle sur la table MySQL existante "voiture"
            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.ToTable("voiture");
                entity.HasKey(v => v.Immatriculation);

                entity.Property(v => v.Statut).HasConversion<string>();
                entity.Property(v => v.Carburant).HasConversion<string>();
            });

            // Mapping de l'entité MaintenanceRecord sur la table "entretien"
            modelBuilder.Entity<MaintenanceRecord>(entity =>
            {
                entity.ToTable("entretien");
                entity.HasKey(m => m.Id);

                entity.HasMany(m => m.Utilisations)
                      .WithOne(u => u.Entretien)
                      .HasForeignKey(u => u.Id_Entretien);
            });

            // Mapping pièce
            modelBuilder.Entity<Piece>(entity =>
            {
                entity.ToTable("piece");
                entity.HasKey(p => p.Id);
            });

            // Mapping table de liaison utilise
            modelBuilder.Entity<Utilise>(entity =>
            {
                entity.ToTable("utilise");
                entity.HasKey(u => new { u.Id_Entretien, u.Id_Piece });

                entity.HasOne(u => u.Piece)
                      .WithMany()
                      .HasForeignKey(u => u.Id_Piece);
            });
        }
    }
}
