using GarageService.Data;
using Microsoft.EntityFrameworkCore;

namespace GarageService.Services
{
    public interface IGarageService
    {
        Task AddMotorcycleAsync(MotorcycleDto motorcycle, int userId);
        Task<List<Motorcycle>> GetMotorcyclesAsync(int userId);
        Task<List<GlobalCatalog>> GetGlobalCatalogAsync();
    }

    public class GarageService : IGarageService
    {
        private readonly GarageDbContext _context;

        public GarageService(GarageDbContext context)
        {
            _context = context;
        }

        public async Task AddMotorcycleAsync(MotorcycleDto motorcycleDto, int userId)
        {
            var motorcycle = new Motorcycle
            {
                Make = motorcycleDto.Make,
                Model = motorcycleDto.Model,
                Year = motorcycleDto.Year,
                Mileage = motorcycleDto.Mileage,
                UserId = userId // Requires UserId property on Motorcycle model
            };

            _context.Motorcycles.Add(motorcycle);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Motorcycle>> GetMotorcyclesAsync(int userId)
        {
            return await _context.Motorcycles.Where(m => m.UserId == userId).Include(m => m.MaintenanceLogs).ToListAsync();
        }

        public async Task<List<GlobalCatalog>> GetGlobalCatalogAsync()
        {
            return await _context.GlobalCatalogs.ToListAsync();
        }
    }

    public class MotorcycleDto
    {
        public int Id { get; set; } // Added Id property
        public required string Make { get; set; }
        public required string Model { get; set; }
        public int Year { get; set; }
        public int Mileage { get; set; }
    }
}