using Microsoft.EntityFrameworkCore;
using ClubService.Models;

namespace ClubService.Data
{
    public class ClubDbContext : DbContext
    {
        public ClubDbContext(DbContextOptions<ClubDbContext> options) : base(options) { }

        public DbSet<Club> Clubs { get; set; }
        public DbSet<ClubMember> ClubMembers { get; set; }
    }
}