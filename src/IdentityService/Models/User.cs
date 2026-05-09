namespace IdentityService.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Rider"; // Default role is Rider

    public ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>();
    public ICollection<Group> OwnedGroups { get; set; } = new List<Group>();
}

public class GroupMember
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int GroupId { get; set; }
}

public class UserDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Rider";
}

public class LoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RoleUpdateDto
{
    public string Role { get; set; } = string.Empty;
}

public class Group
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<User> Members { get; set; } = new List<User>();
}
