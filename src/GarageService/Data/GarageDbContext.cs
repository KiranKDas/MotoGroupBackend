using Microsoft.EntityFrameworkCore;

namespace GarageService.Data
{
    public class GarageDbContext : DbContext
    {
        public GarageDbContext(DbContextOptions<GarageDbContext> options) : base(options) { }

        public DbSet<Motorcycle> Motorcycles { get; set; }
        public DbSet<MaintenanceLog> MaintenanceLogs { get; set; }
        public DbSet<GlobalCatalog> GlobalCatalogs { get; set; }
    }

    public class Motorcycle
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public int Mileage { get; set; }
        public List<MaintenanceLog> MaintenanceLogs { get; set; } = new();
    }

    public class MaintenanceLog
    {
        public int Id { get; set; }
        public int MotorcycleId { get; set; }
        public Motorcycle Motorcycle { get; set; }
        public DateTime ServiceDate { get; set; }
        public string ServiceType { get; set; }
        public decimal Cost { get; set; }
    }

    public class GlobalCatalog
    {
        public int Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
    }
}