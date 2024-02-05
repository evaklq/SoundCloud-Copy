namespace Semwork.Models;

public class User
{
    public int? Id { get; set; }
    public string FullName { get; set; }
    public string Nick { get; set; }
    public string Email { get; set; }
    public string Number { get; set; }
    public string Password { get; set; }
    public int Popularity { get; set; } = 0;
    public string? IconUrl { get; set; }
    public List<Comment> Comments { get; set; }
    public List<Song> FavouriteSongs { get; set; }
    public List<Song> PersonalSongs { get; set; }
    public List<Playlist> Playlists { get; set; }

    public User()
    {
        
    }
    public User(int id, string fullName, string nick, string email, string number, string password, string iconUrl, 
        List<Comment> comments, List<Song> favouriteSongs, List<Song> personalSongs, List<Playlist> playlists)
    {
        Id = id;
        FullName = fullName;
        Nick = nick;
        Email = email;
        Number = number;
        Password = password;
        IconUrl = iconUrl;
        Comments = comments;
        FavouriteSongs = favouriteSongs;
        PersonalSongs = personalSongs;
        Playlists = playlists;
        foreach (var song in personalSongs)
        {
            Popularity += song.Popularity;
        }
    }

    public User(RegUser user)
    {
        var hasher = new Hasher();
        Id = null;
        FullName = user.FullName;
        Nick = user.Nick;
        Email = user.Email;
        Number = user.Number;
        Password =  hasher.GetPasswordHash(user.Password);
        IconUrl = user.IconUrl;
        Comments = new List<Comment>();
        FavouriteSongs = new List<Song>();
        PersonalSongs = new List<Song>();
        Playlists = new List<Playlist>();
        Popularity = 0;
    }
}

public class RegUser
{
    public string FullName { get; set; }
    public string Nick { get; set; }
    public string Email { get; set; }
    public string Number { get; set; }
    public string Password { get; set; }
    public string? IconUrl { get; set; }
}

public class AuthoUser
{
    public string Login { get; set; }
    public string Password { get; set; }
}