namespace Semwork.Models;

public class Playlist
{
    public int? Id { get; set; }
    public string Title { get; set; }
    public List<Song> Songs { get; set; }

    public Playlist(int id,string title, List<Song> songs)
    {
        Title = title;
        Songs = songs;
    }
}