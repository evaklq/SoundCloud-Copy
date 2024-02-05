namespace Semwork.Models;

public class Comment
{
    public int? Id { get; set; }
    public string MessageContext { get; set; }
    public User Author { get; set; }

    public Comment(int id, string message, User user)
    {
        Id = id;
        MessageContext = message;
        Author = user;
    }

    public Comment(int id, string message, int userId)
    {
        Id = id;
        MessageContext = message;
        Author = new User
        {
            Id = userId
        };
    }

    public Comment()
    {
        
    }
}

public class AddComment
{
    public string MessageContext { get; set; }
    public User Author { get; set; }
}