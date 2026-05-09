namespace ClubService.DTOs
{
    public class CreateClubDto
    {
        public string Name { get; set; } = string.Empty;
        public int CaptainId { get; set; }
        public string CaptainUsername { get; set; } = string.Empty;
        public string? Bike { get; set; }
    }
}