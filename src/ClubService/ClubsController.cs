using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ClubService.Data;
using ClubService.Models;
using ClubService.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ClubService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClubsController : ControllerBase
    {
        private readonly ClubDbContext _context;

        public ClubsController(ClubDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateClub([FromBody] CreateClubDto clubDto)
        {
            if (string.IsNullOrWhiteSpace(clubDto.Name))
            {
                return BadRequest("Club name is required.");
            }

            var club = new Club
            {
                Name = clubDto.Name,
                CaptainId = clubDto.CaptainId,
                CaptainUsername = clubDto.CaptainUsername
            };

            _context.Clubs.Add(club);
            await _context.SaveChangesAsync();

            var captainMember = new ClubMember
            {
                ClubId = club.Id,
                Username = club.CaptainUsername,
                Bike = !string.IsNullOrWhiteSpace(clubDto.Bike) ? clubDto.Bike : "No bikes",
                IsSafe = true
            };
            _context.ClubMembers.Add(captainMember);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetClub), new { id = club.Id }, club);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyClubs([FromHeader(Name = "x-username")] string username)
        {
            if (string.IsNullOrEmpty(username)) 
                return BadRequest("x-username header is required.");

            var memberRecords = await _context.ClubMembers
                .Where(m => m.Username == username)
                .ToListAsync();

            var memberClubIds = memberRecords.Select(m => m.ClubId).ToList();

            var clubsQuery = await _context.Clubs
                .Where(c => c.CaptainUsername == username || memberClubIds.Contains(c.Id))
                .ToListAsync();

            var clubs = clubsQuery.Select(c => new
            {
                id = c.Id,
                name = c.Name,
                captainId = c.CaptainId,
                captainUsername = c.CaptainUsername,
                isPending = memberRecords.Any(m => m.ClubId == c.Id && m.Bike == "user yet to accept invite")
            }).ToList();

            return Ok(clubs);
        }

        [HttpGet("members")]
        public async Task<IActionResult> GetClubMembers([FromHeader(Name = "x-username")] string captainUsername)
        {
            if (string.IsNullOrEmpty(captainUsername)) 
                return BadRequest("x-username header is required.");

            var club = await _context.Clubs.FirstOrDefaultAsync(c => c.CaptainUsername == captainUsername);
            if (club == null) return Ok(new object[] { });

            var members = await _context.ClubMembers
                .Where(m => m.ClubId == club.Id)
                .Select(m => new { id = m.Id, name = m.Username, bike = m.Bike, is_safe = m.IsSafe })
                .ToListAsync();

            return Ok(members);
        }

        [HttpPost("members")]
        public async Task<IActionResult> InviteMember([FromHeader(Name = "x-username")] string captainUsername, [FromBody] InviteMemberDto dto)
        {
            var club = await _context.Clubs.FirstOrDefaultAsync(c => c.CaptainUsername == captainUsername);
            if (club == null) return NotFound("You do not manage any club.");

            var existingMember = await _context.ClubMembers.FirstOrDefaultAsync(m => m.ClubId == club.Id && m.Username == dto.Username);
            if (existingMember != null) return BadRequest("This user is already a member of the club.");

            var newMember = new ClubMember
            {
                ClubId = club.Id,
                Username = dto.Username,
                Bike = "user yet to accept invite",
                IsSafe = true
            };

            _context.ClubMembers.Add(newMember);
            await _context.SaveChangesAsync();
            return Ok(newMember);
        }

        [HttpPut("{clubId}/accept")]
        public async Task<IActionResult> AcceptInvite([FromHeader(Name = "x-username")] string username, int clubId, [FromBody] UpdateSafetyDto dto)
        {
            var member = await _context.ClubMembers.FirstOrDefaultAsync(m => m.Username == username && m.ClubId == clubId);
            if (member == null) return NotFound("Member not found.");

            member.IsSafe = dto.IsSafe;
            member.Bike = dto.Bike ?? "[]";

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{clubId}/leave")]
        public async Task<IActionResult> LeaveClub([FromHeader(Name = "x-username")] string username, int clubId)
        {
            var member = await _context.ClubMembers.FirstOrDefaultAsync(m => m.Username == username && m.ClubId == clubId);
            if (member == null) return NotFound("Member not found.");

            _context.ClubMembers.Remove(member);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("members/{id}")]
        public async Task<IActionResult> RemoveMember([FromHeader(Name = "x-username")] string captainUsername, int id)
        {
            var club = await _context.Clubs.FirstOrDefaultAsync(c => c.CaptainUsername == captainUsername);
            if (club == null) return NotFound("You do not manage any club.");

            var member = await _context.ClubMembers.FirstOrDefaultAsync(m => m.Id == id && m.ClubId == club.Id);
            if (member == null) return NotFound("Member not found in your club.");

            if (member.Username == captainUsername) return BadRequest("Captain cannot be removed from the club.");

            _context.ClubMembers.Remove(member);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("members/safety")]
        public async Task<IActionResult> UpdateSafetyStatus([FromHeader(Name = "x-username")] string username, [FromBody] UpdateSafetyDto dto)
        {
            if (string.IsNullOrEmpty(username)) 
                return BadRequest("x-username header is required.");

            var members = await _context.ClubMembers.Where(m => m.Username == username).ToListAsync();
            if (!members.Any()) return Ok();

            foreach (var member in members)
            {
                if (member.Bike != "user yet to accept invite") // Do not overwrite pending invites automatically
                {
                    member.IsSafe = dto.IsSafe;
                    if (dto.Bike != null)
                    {
                        member.Bike = dto.Bike;
                    }
                }
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClub(int id)
        {
            var club = await _context.Clubs.FindAsync(id);
            if (club == null) return NotFound();
            
            return Ok(club);
        }
    }
}