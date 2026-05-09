using EventService.Data;
using Microsoft.EntityFrameworkCore;

namespace EventService.Services
{
    public interface IEventService
    {
        Task CreateEventAsync(EventDto eventDto, int userId);
        Task<List<Event>> GetUpcomingEventsAsync(int userId);
        Task<List<Event>> GetAttendedEventsAsync(int userId);
    }

    public class EventService : IEventService
    {
        private readonly EventDbContext _context;

        public EventService(EventDbContext context)
        {
            _context = context;
        }

        public async Task CreateEventAsync(EventDto eventDto, int userId)
        {
            var newEvent = new Event
            {
                Name = string.IsNullOrWhiteSpace(eventDto.Name) ? "Unnamed Event" : eventDto.Name,
                Date = eventDto.Date,
                Location = string.IsNullOrWhiteSpace(eventDto.Location) ? "TBD" : eventDto.Location,
                UserId = userId
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Event>> GetUpcomingEventsAsync(int userId)
        {
            return await _context.Events
                .Where(e => e.Date >= DateTime.UtcNow && e.UserId == userId)
                .Include(e => e.Attendees)
                .ToListAsync();
        }

        public async Task<List<Event>> GetAttendedEventsAsync(int userId)
        {
            return await _context.Events
                .Where(e => e.Date < DateTime.UtcNow && e.UserId == userId)
                .Include(e => e.Attendees)
                .ToListAsync();
        }
    }
}