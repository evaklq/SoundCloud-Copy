namespace Semwork.Models;

public class Session
{
    public Guid Id { get; set; }
    public string Role { get; set; }
    public User User { get; set; }
    public Session(User user, string role)
    {
        Id = Guid.NewGuid();
        Role = role;
        User = user;
    }
}