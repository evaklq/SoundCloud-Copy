using System.Net;
using System.Runtime;
using Semwork.Models;
using Semwork.ApiManager;
using Semwork.DBManager;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace Semwork;

public static class Server
{
    private static readonly HttpListener Listener = new();
    private static readonly DataBaseManager Manager= new();
    private static readonly Hasher Hasher = new();
    private static readonly IMemoryCache Cache = new MemoryCache(new MemoryCacheOptions());
    private static Guid? _id = null;

    static async Task Main(string[] args)
    {
        Listener.Prefixes.Add("http://localhost:2400/");
        Listener.Start();

        while (Listener.IsListening)
        {
            var context = await Listener.GetContextAsync();
            var request = context.Request;
            var response = context.Response;
            var localPath = request.Url?.LocalPath ?? "";
            localPath = GetPathWithoutHtml(localPath);
            var file = new byte[]{};
            response.StatusCode = 200;
            response.ContentType = "text/html";
            
            switch (localPath)
            {
                case "/mainPage":
                    file = await File.ReadAllBytesAsync("../../../Front/html/mainPage.html");
                    break;
                case "/authoritation":
                    file = await File.ReadAllBytesAsync("../../../Front/html/authoritation.html");
                    break;
                case "/chat":
                    file = await File.ReadAllBytesAsync("../../../Front/html/chat.html");
                    break;
                case "/playlist":
                    file = await File.ReadAllBytesAsync("../../../Front/html/playlist.html");
                    break;
                case "/profile":
                    file = await File.ReadAllBytesAsync("../../../Front/html/profile.html");
                    response.ContentType = "text/html";
                    break;
                case "/registration":
                    response.AddHeader("Access-Control-Allow-Origin", "*");
                    file = await File.ReadAllBytesAsync("../../../Front/html/registration.html");
                    break;
                case "/search":
                    file = await File.ReadAllBytesAsync("../../../Front/html/search.html");
                    break;
                case "/song":
                    file = await File.ReadAllBytesAsync("../../../Front/html/song.html");
                    break;
                case "/songStudio":
                    file = await File.ReadAllBytesAsync("../../../Front/html/songStudio.html");
                    break;
                case "/autho-user":
                    response.ContentType = "application/json";
                    response.AddHeader("Access-Control-Allow-Origin", "*");
                    var authoErrors = await UserAuthoritation(request, response);
                    file = Encoding.Default.GetBytes(JsonSerializer.Serialize(authoErrors.ToArray()));
                    break;
                case "/reg-user":
                    response.ContentType = "application/json";
                    response.AddHeader("Access-Control-Allow-Origin", "*");
                    var errors = await UserRegistration(request, response);
                    file = Encoding.Default.GetBytes(JsonSerializer.Serialize(errors));
                    break;
                case "/get-personal-song":
                    response.ContentType = "application/json";
                    response.AddHeader("Access-Control-Allow-Origin", "*");
                    var userPersonal = GetUserFromCookies(request);
                    var personalSongs = await Manager.GetDifferentUserSongsAsync("personal_songs", userId: userPersonal.Id ?? -1);
                    if (personalSongs.Count >= 1)
                    {
                        var songsJson = JsonSerializer.Serialize(personalSongs.ToArray());
                        file = Encoding.UTF8.GetBytes(songsJson);
                    }
                    break;
                case "/save-song":
                    var songReader = new StreamReader(request.InputStream, request.ContentEncoding);
                    var songReadData = await songReader.ReadToEndAsync();
                    var exampleSong = JsonSerializer.Deserialize<ExampleSong>(songReadData) ?? new ExampleSong();
                    var readySong = new Song(exampleSong);
                    var userr = GetUserFromCookies(request);
                    readySong.Artist = userr.Nick;
                    readySong.IsLike = false;

                    var songId = await Manager.AddSongToMainTableAsync(readySong);
                    await Manager.AddSongsToDifferentTables(userr.Id ?? -1, new List<int>(){songId}, "personal_songs", needConnection: true);
                    Console.WriteLine("добавилось");
                    file = Encoding.Default.GetBytes(JsonSerializer.Serialize(userr));
                    break;
                case "/save-comment":
                    var commentReader = new StreamReader(request.InputStream, request.ContentEncoding);
                    var userReadData = await commentReader.ReadToEndAsync();
                    var addComment = JsonSerializer.Deserialize<AddComment>(userReadData) ?? new AddComment();
                    var comment = new Comment(-1, addComment.MessageContext, addComment.Author);
                    
                    await Manager.AddCommentToDbAsync(comment, comment.Author);
                    Console.WriteLine("добавилось");
                    file = Encoding.Default.GetBytes("good");
                    break;
                case "/get-comments":
                    response.ContentType = "application/json";
                    response.AddHeader("Access-Control-Allow-Origin", "*");
                    var comments = await Manager.GetCommentsAsync();
                    if (comments.Count >= 1)
                    {
                        var songsJson = JsonSerializer.Serialize(comments.ToArray());
                        file = Encoding.UTF8.GetBytes(songsJson);
                    }
                    break;
                case "/get-song-by-search":
                    response.ContentType = "application/json";
                    response.AddHeader("Access-Control-Allow-Origin", "*");

                    var readerSearch = new StreamReader(request.InputStream, request.ContentEncoding);
                    var searchJson = await readerSearch.ReadToEndAsync();
                    searchJson = searchJson.Trim('"');

                    var songsFromSearch = await Manager.GetSongsFromSearchAsync(searchJson);
                    var userSearch = GetUserFromCookies(request);
                    if (userSearch != null)
                    {
                        if (songsFromSearch != null)
                            foreach (var song in songsFromSearch)
                            {
                                var isLike = await Manager.CheckLike(userSearch.Id ?? -1, song.Id ?? -1);
                                song.IsLike = isLike;
                            }
                    }
                    var songsFromSearchJson = JsonSerializer.Serialize(songsFromSearch);
                    file = Encoding.UTF8.GetBytes(songsFromSearchJson);
                    break;
                case "/get-user":
                    response.ContentType = "application/json";
                    response.AddHeader("Access-Control-Allow-Origin", "*");
                    var authUser = GetUserFromCookies(request);
                    if (authUser != null)
                    {
                        var userJson = JsonSerializer.Serialize(authUser);
                        file = Encoding.UTF8.GetBytes(userJson);
                    }
                    break;
                case "/change-like":
                    response.ContentType = "application/json";
                    response.AddHeader("Access-Control-Allow-Origin", "*");

                    var readerStream = new StreamReader(request.InputStream, request.ContentEncoding);
                    var idSong = await readerStream.ReadToEndAsync();
                    idSong = idSong.Trim('"');
                    int.TryParse(idSong, out var idInt);

                    var songDb = await Manager.GetSongByIdAsync(idInt);
                    var user = GetUserFromCookies(request);
                    if (user != null)
                    {
                        var isLike = await Manager.CheckLike(user.Id ?? -1, songDb.Id ?? -1);
                        if (isLike)
                        {
                            await Manager.DeleteLikeAsync(user.Id ?? -1, songDb.Id ?? -1);
                        }
                        else
                        {
                            var songList = new List<int>() {songDb.Id ?? -1};
                            await Manager.AddSongsToDifferentTables(user.Id ?? -1, songList , "favorite_songs", needConnection: true);
                        }
                        songDb.IsLike = isLike;
                    }
                    break;
                case "/get-main-songs":
                    response.ContentType = "application/json";
                    response.AddHeader("Access-Control-Allow-Origin", "*");
                    var songs = await Manager.GetSongsAsync();
                    user = GetUserFromCookies(request);
                    if (user != null)
                    {
                        foreach (var song in songs)
                        {
                            var isLike = await Manager.CheckLike(user.Id ?? -1, song.Id ?? -1);
                            song.IsLike = isLike;
                        }
                    }
                    if (songs.Count >= 12)
                    {
                        var songsJson = JsonSerializer.Serialize(songs.ToArray());
                        file = Encoding.UTF8.GetBytes(songsJson);
                    }
                    break;
                case "/get-song-by-id":
                    response.ContentType = "application/json";
                    response.AddHeader("Access-Control-Allow-Origin", "*");

                    var reader = new StreamReader(request.InputStream, request.ContentEncoding);
                    var idJson = await reader.ReadToEndAsync();
                    idJson = idJson.Trim('"');
                    int.TryParse(idJson, out var id);

                    var songFromDb = await Manager.GetSongByIdAsync(id);
                    user = GetUserFromCookies(request);
                    if (user != null)
                    {
                        var isLike = await Manager.CheckLike(user.Id ?? -1, songFromDb.Id ?? -1);
                        songFromDb.IsLike = isLike;
                    }
                    var songJson = JsonSerializer.Serialize(songFromDb);
                    file = Encoding.UTF8.GetBytes(songJson);
                    break;
                case "/saveToApi":
                    response.AddHeader("Access-Control-Allow-Origin", "*");
                    response.ContentType = "application/json";
                    
                    var streamReader = new StreamReader(request.InputStream, request.ContentEncoding);
                    var body = await streamReader.ReadToEndAsync();
                    var data = JsonSerializer.Deserialize<string>(body) ?? "";
                    
                    var fileLink = await ApiClient.SaveFile(data, request.ContentType ?? "");
                    file = Encoding.Default.GetBytes(fileLink);
                    break;
                default:
                    var ext = Path.GetExtension(localPath);
                    var path = "../../../Front";
                    response.ContentType = ext switch
                    {
                        ".css" => "text/css",
                        ".js" => "text/javascript",
                        ".html" => "text/html",
                        ".svg" => "image/svg+xml",
                        ".jpeg" => "image/jpeg",
                        ".mp3" => "audio/mpeg",
                        _ => "text/plain"
                    };
                    path += ext switch
                    {
                        ".html" => "/html" + localPath,
                        _ => localPath
                    };
                    file = await File.ReadAllBytesAsync(path);
                    break;
            }
            await response.OutputStream.WriteAsync(file);
            response.OutputStream.Close();
        }
        Listener.Stop();
        Listener.Close();
    }
    
    private static string GetPathWithoutHtml(string path)
    {
        return path.Replace("/html", "");
    }
    
    private static User? GetUserFromCookies(HttpListenerRequest request)
    {
        if (_id == null)
        {
            return null;
        }
        Cache.TryGetValue(_id.ToString() ?? "" , out Session? session);
        return session?.User;
    }

    private static async Task<string[]> UserRegistration(HttpListenerRequest request, HttpListenerResponse response)
    {
        var streamReader = new StreamReader(request.InputStream, request.ContentEncoding);
        var userReadData = await streamReader.ReadToEndAsync();
        var regUser = JsonSerializer.Deserialize<RegUser>(userReadData) ?? new RegUser();
        var user = new User(regUser);

        var isRepeatNick = await Manager.GetUserFromDbByLoginAsync(user.Nick) != null;
        var isRepeatEmail = await Manager.GetUserFromDbByLoginAsync(user.Email) != null;
        var isRepeatNumber = await Manager.GetUserFromDbByLoginAsync(user.Number) != null;
                    
        var validator = new Validator(isRepeatNick, isRepeatEmail, isRepeatNumber);
        var errors = validator.ValidateAndGetErrorMessages(regUser);

        if (errors.Length != 0) return errors;
        await Manager.AddUserToDbAsync(user);
        
        var session = new Session(user, "user");
        Cache.Set(session.Id.ToString(), session, 
            new MemoryCacheEntryOptions{AbsoluteExpiration = new DateTimeOffset(DateTime.UtcNow.AddMinutes(20))});

        var cookie = new Cookie()
        {
            Name = "SessionId",
            Value = session.Id.ToString(),
            Expires = DateTime.Now + new TimeSpan(0, 20, 0)
        };
        response.Cookies.Add(cookie);
        Console.WriteLine("добавилось");

        return errors;
    }
    
    private static async Task<List<string>> UserAuthoritation(HttpListenerRequest request, HttpListenerResponse response)
    {
        var streamReader = new StreamReader(request.InputStream, request.ContentEncoding);
        var authoUserData = await streamReader.ReadToEndAsync();
        var authoUser = JsonSerializer.Deserialize<AuthoUser>(authoUserData) ?? new AuthoUser();
        var authoErrors = new List<string>();

        var user = await Manager.GetUserFromDbByLoginAsync(authoUser.Login);
        if (user != null && user.Password == Hasher.GetPasswordHash(authoUser.Password))
        {
            var session = new Session(user, "user");
            Cache.Set(session.Id.ToString(), session, 
                new MemoryCacheEntryOptions{AbsoluteExpiration = new DateTimeOffset(DateTime.UtcNow.AddMinutes(20))});
            var cookie = new Cookie()
            {
                Name = "SessionId",
                Value = session.Id.ToString(),
                Expires = DateTime.Now + new TimeSpan(0, 20, 0)
            };
            _id = session.Id;
            response.Cookies.Add(cookie);
        }
        else
        {
            authoErrors.Add("Wrong password or login");
        }

        return authoErrors;
    }
}