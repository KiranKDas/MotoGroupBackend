using MaintenanceService.Data;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceService.Services
{
    public interface IMaintenanceService
    {
        Task AddMaintenanceLogAsync(MaintenanceLogDto log, int userId);
        Task<List<MaintenanceLog>> GetMaintenanceLogsAsync(int motorcycleId, int userId);
        Task<bool> CalculateSafetyStatusAsync(int motorcycleId, int userId);
    }

    public class MaintenanceService : IMaintenanceService
    {
        private readonly MaintenanceDbContext _context;

        public MaintenanceService(MaintenanceDbContext context)
        {
            _context = context;
        }

        public async Task AddMaintenanceLogAsync(MaintenanceLogDto logDto, int userId)
        {
            var log = new MaintenanceLog
            {
                UserId = userId,
                MotorcycleId = logDto.MotorcycleId,
                ServiceDate = logDto.ServiceDate,
                ServiceType = logDto.ServiceType,
                Mileage = logDto.Mileage,
                Cost = logDto.Cost
            };

            _context.MaintenanceLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<List<MaintenanceLog>> GetMaintenanceLogsAsync(int motorcycleId, int userId)
        {
            return await _context.MaintenanceLogs
                .Where(log => log.MotorcycleId == motorcycleId && log.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> CalculateSafetyStatusAsync(int motorcycleId, int userId)
        {
            var logs = await GetMaintenanceLogsAsync(motorcycleId, userId);

            // Example logic: Check if there is a recent oil change within the last 5000 miles
            var recentOilChange = logs.Any(log => log.ServiceType == "Oil Change" && log.Mileage >= 5000);

            return recentOilChange;
        }
    }
}