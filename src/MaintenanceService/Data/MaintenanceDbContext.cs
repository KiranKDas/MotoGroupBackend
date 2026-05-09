using Microsoft.EntityFrameworkCore;

namespace MaintenanceService.Data
{
    public class MaintenanceDbContext : DbContext
    {
        public MaintenanceDbContext(DbContextOptions<MaintenanceDbContext> options) : base(options) { }

        public DbSet<MaintenanceLog> MaintenanceLogs { get; set; }
    }

    public class MaintenanceLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MotorcycleId { get; set; }
        public DateTime ServiceDate { get; set; }
        public required string ServiceType { get; set; }
        public int Mileage { get; set; }
        public decimal Cost { get; set; }
    }

    public class MaintenanceLogDto
    {
        public int MotorcycleId { get; set; }
        public DateTime ServiceDate { get; set; }
        public required string ServiceType { get; set; }
        public int Mileage { get; set; }
        public decimal Cost { get; set; }
    }
}