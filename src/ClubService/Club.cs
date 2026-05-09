namespace ClubService.Models
{
    public class Club
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CaptainId { get; set; }
        public string CaptainUsername { get; set; } = string.Empty;
    }

    public class ClubMember
    {
        public int Id { get; set; }
        public int ClubId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Bike { get; set; } = string.Empty;
        public bool IsSafe { get; set; } = true;
    }
}