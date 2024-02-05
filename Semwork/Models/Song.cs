namespace Semwork.Models;

public class Song
{
    public int? Id { get; set; }
    public string Title { get; set; }
    public string Artist { get; set; }
    public string IconUrl { get; set; }
    public string SongUrl { get; set; }
    public string Album { get; set; }
    public bool IsLike { get; set; }
    public int Popularity { get; set; }
    
    public Song(int? id, string title, string artist, string iconUrl, string songUrl, string album, int popularity, bool isLike = false)
    {
        Id = id;
        Title = title;
        Artist = artist;
        IconUrl = iconUrl;
        SongUrl = songUrl;
        Album = album;
        Popularity = popularity;
        IsLike = isLike;
    }

    public Song(ExampleSong song)
    {
        Title = song.Title;
        IconUrl = song.IconUrl;
        SongUrl = song.SongUrl;
        Album = "";
        Popularity = 0;
        IsLike = false;
    }
}

public class ExampleSong {
    public string Title { get; set; }
    public string IconUrl { get; set; }
    public string SongUrl { get; set; }
}