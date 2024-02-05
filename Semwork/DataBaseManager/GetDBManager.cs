using Npgsql;
using Semwork.Models;

namespace Semwork.DBManager;

public partial class DataBaseManager
{
    public async Task<User?> GetUserFromDbByLoginAsync(string login, CancellationToken ctx = default, bool needConnection = true)
    {
        var id = -1;
        var commandStringToUsers = "";
        var isId = false;
        if (int.TryParse(login, out id))
        {
            isId = true;
            commandStringToUsers = @"select id, full_name, nick, email, phone_number, password_hash, icon from users 
                                    where id = @id";
        }
        else
        {
            commandStringToUsers = @"select id, full_name, nick, email, phone_number, password_hash, icon from users 
                                    where nick = @login
                                    or email = @login
                                    or phone_number = @login";
        }

        if (needConnection)
        {
            await _connection.OpenAsync(ctx);
        }
        try
        {
            User user = null;
            var userId = 0;
            using (var command = new NpgsqlCommand(commandStringToUsers, _connection))
            {
                if (isId)
                {
                    command.Parameters.AddWithValue("@id", id);
                }
                else
                {
                    command.Parameters.AddWithValue("@login", login);
                }
                
                var reader = await command.ExecuteReaderAsync(ctx);
                if (await reader.ReadAsync(ctx))
                {
                    var photoUrl = reader.GetValue(6).ToString() ??
                                   "https://cdn.onlinewebfonts.com/svg/img_568657.png";
                    if (photoUrl.Length == 0)
                    {
                        photoUrl = "https://cdn.onlinewebfonts.com/svg/img_568657.png";
                    }

                    if (int.TryParse(reader.GetValue(0).ToString(), out userId))
                    {
                        Console.WriteLine("id done");
                    }

                    user = new User(id: userId,
                        fullName: reader.GetValue(1).ToString() ?? string.Empty,
                        nick: reader.GetValue(2).ToString() ?? string.Empty,
                        email: reader.GetValue(3).ToString() ?? string.Empty,
                        number: reader.GetValue(4).ToString() ?? string.Empty,
                        password: reader.GetValue(5).ToString() ?? string.Empty,
                        iconUrl: photoUrl,
                        comments: new List<Comment>(),
                        favouriteSongs: new List<Song>(),
                        personalSongs: new List<Song>(),
                        playlists: new List<Playlist>());
                }
                
                await reader.CloseAsync();

                var favoriteSongs = await GetDifferentUserSongsAsync("favorite_songs", 
                            false, userId: userId, ctx: ctx);
                var personalSongs = await GetDifferentUserSongsAsync("personal_songs", 
                            false, userId: userId, ctx: ctx);
                var playlists = await GetUserPlaylistsAsync(userId, false, ctx);
                var comments = await GetUserCommentsAsync(userId, false, ctx);

                user.FavouriteSongs = favoriteSongs;
                user.PersonalSongs = personalSongs;
                user.Playlists = playlists;
                user.Comments = comments;
                
                return user;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
        finally
        {
            if (needConnection)
            {
                await _connection.CloseAsync();
            }
        }
    }

    public async Task<bool> CheckLike(int userId, int songId)
    {
        await _connection.OpenAsync();
        const string commandStringToFavorites = @"select * from favorite_songs 
                                                  where user_id = @user_id and song_id = @song_id;";
        try
        {
            using (var command = new NpgsqlCommand(commandStringToFavorites, _connection))
            {
                command.Parameters.AddWithValue("@user_id", userId);
                command.Parameters.AddWithValue("@song_id", songId);
                var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    if (!int.TryParse(reader.GetValue(0).ToString(), out var id)) continue;
                    Console.WriteLine("id done");
                    return true;
                }

                await reader.CloseAsync();

                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }
    
    public async Task<List<Comment>> GetUserCommentsAsync(int userId, bool needConnection = true,
        CancellationToken ctx = default)
    {
        var commandStringToComments = @"select id, message_context from general_comments where user_id = @user_id;";
        var resultComments = new List<Comment>();
        
        if (needConnection)
        {
            await _connection.OpenAsync(ctx);
        }

        try
        {
            using (var command = new NpgsqlCommand(commandStringToComments, _connection))
            {
                command.Parameters.AddWithValue("@user_id", userId);
                var reader = await command.ExecuteReaderAsync(ctx);
                while (await reader.ReadAsync(ctx))
                {
                    if (int.TryParse(reader.GetValue(0).ToString(), out var id))
                    {
                        Console.WriteLine("id done");
                    }
                    
                    var comment = new Comment(id, reader.GetValue(1).ToString() ?? string.Empty, userId);
                    resultComments.Add(comment);
                }

                await reader.CloseAsync();

                return resultComments;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return new List<Comment>();
        }
        finally
        {
            if (needConnection)
            {
                await _connection.CloseAsync();
            }
        }
    }
    
    public async Task<List<Comment>> GetCommentsAsync(bool needConnection = true,
        CancellationToken ctx = default)
    {
        const string commandStringToComments = @"select id, message_context, user_id from general_comments;";
        var resultComments = new List<Comment>();
        
        if (needConnection)
        {
            await _connection.OpenAsync(ctx);
        }
        
        try
        {
            using (var command = new NpgsqlCommand(commandStringToComments, _connection))
            {
                var reader = await command.ExecuteReaderAsync(ctx);
                while (await reader.ReadAsync(ctx))
                {
                    if (int.TryParse(reader.GetValue(0).ToString(), out var id))
                    {
                        Console.WriteLine("id done");
                    }
                    if (int.TryParse(reader.GetValue(2).ToString(), out var userId))
                    {
                        Console.WriteLine("id done");
                    }
                    
                    
                    var comment = new Comment(id, reader.GetValue(1).ToString() ?? string.Empty, userId);
                    resultComments.Add(comment);
                }
                
                await reader.CloseAsync();

                foreach (var comment in resultComments)
                {
                    var user = await GetUserFromDbByLoginAsync(comment.Author.Id.ToString() ?? "", ctx, false);
                    comment.Author = user ?? new User();
                }

                return resultComments;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return new List<Comment>();
        }
        finally
        {
            if (needConnection)
            {
                await _connection.CloseAsync();
            }
        }
    }

    public async Task<List<Playlist>> GetUserPlaylistsAsync(int userId, bool needConnection = true,
        CancellationToken ctx = default)
    {
        const string commandStringToPlaylists = @"select id, title from playlists where user_id = @user_id;";
        var resultPlaylists = new List<Playlist>();
        var playlistInfo = new Dictionary<int, string>();

        if (needConnection)
        {
            await _connection.OpenAsync(ctx);
        }

        try
        {
            using (var command = new NpgsqlCommand(commandStringToPlaylists, _connection))
            {
                command.Parameters.AddWithValue("@user_id", userId);

                var reader = await command.ExecuteReaderAsync(ctx);
                while (await reader.ReadAsync(ctx))
                {
                    if (!int.TryParse(reader.GetValue(0).ToString(), out var playlistId)) continue;
                    var title = reader.GetValue(1).ToString() ?? "";
                    playlistInfo.Add(playlistId, title);
                }

                await reader.CloseAsync();

                foreach (var playData in playlistInfo)
                {
                    var playlistSongs = await GetDifferentUserSongsAsync("playlist_songs", 
                        false, playlistId: playData.Key, ctx: ctx);
                    var playlist = new Playlist(playData.Key, playData.Value, playlistSongs);
                    resultPlaylists.Add(playlist);
                }
                
                return resultPlaylists;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return resultPlaylists;
        }
        finally
        {
            if (needConnection)
            {
                await _connection.CloseAsync();
            }
        }
    }

    public async Task<List<Song>> GetDifferentUserSongsAsync(string tableName, bool needConnection = true, 
                                                             int userId = -1, int playlistId = -1,
                                                             CancellationToken ctx = default)
    {
        var commandString = $"SELECT song_id FROM {tableName} WHERE ";
        var resultSongs = new List<Song>();
        var songIds = new List<int>();
        if (userId != -1)
        {
            commandString += "user_id = @user_id;";
        }
        else 
        {
            commandString += "playlist_id = @playlist_id;";
        }
        
        if (needConnection)
        {
            await _connection.OpenAsync(ctx);
        }

        try
        {
            using (var command = new NpgsqlCommand(commandString, _connection))
            {
                if (userId != -1)
                {
                    command.Parameters.AddWithValue("@user_id", userId);
                }
                else
                {
                    command.Parameters.AddWithValue("@playlist_id", playlistId);
                }

                var reader = await command.ExecuteReaderAsync(ctx);
                while (await reader.ReadAsync(ctx))
                {
                    if (!int.TryParse(reader.GetValue(0).ToString(), out var id)) continue;
                    songIds.Add(id);
                }
                
                await reader.CloseAsync();

                foreach (var id in songIds)
                {
                    var song = await GetSongByIdAsync(id, false, ctx);
                    if (song != null) resultSongs.Add(song);
                }

                return resultSongs;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return resultSongs;
        }
        finally
        {
            if (needConnection)
            {
                await _connection.CloseAsync();
            }
        }
    }

    public async Task<Song?> GetSongByIdAsync(int songId, bool needConnection = true, CancellationToken ctx = default)
    {
        const string commandStringToSongs = @"select * from songs where id = @song_id";
        Song resultSong = null;

        if (needConnection)
        {
            await _connection.OpenAsync(ctx);
        }

        try
        {
            using (var command = new NpgsqlCommand(commandStringToSongs, _connection))
            {
                command.Parameters.AddWithValue("@song_id", songId);
                var reader = await command.ExecuteReaderAsync(ctx);
                while (await reader.ReadAsync(ctx))
                {
                    var iconUrl = reader.GetValue(3).ToString() ?? 
                                        "https://cdn.onlinewebfonts.com/svg/img_41170.png";
                    
                    if (int.TryParse(reader.GetValue(0).ToString(), out var id))
                    {
                        Console.WriteLine("id done");
                    }
                    if (int.TryParse(reader.GetValue(6).ToString(), out var popularity))
                    {
                        Console.WriteLine("popularity done");
                    }

                    resultSong = new Song(id,
                        reader.GetValue(1).ToString() ?? string.Empty,
                        reader.GetValue(2).ToString() ?? string.Empty,
                        iconUrl,
                        reader.GetValue(4).ToString() ?? string.Empty,
                        reader.GetValue(5).ToString() ?? string.Empty,
                        popularity);
                }

                await reader.CloseAsync();

                return resultSong;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return resultSong;
        }
        finally
        {
            if (needConnection)
            {
                await _connection.CloseAsync();
            }
        }
    }
    
    public async Task<List<Song>> GetSongsAsync(CancellationToken ctx = default)
    {
        const string commandStringToSongs = @"select * from songs";
        var resultSongs = new List<Song>();
        await _connection.OpenAsync(ctx);

        try
        {
            using (var command = new NpgsqlCommand(commandStringToSongs, _connection))
            {
                var reader = await command.ExecuteReaderAsync(ctx);
                while (await reader.ReadAsync(ctx))
                {
                    var iconUrl = reader.GetValue(3).ToString() ?? 
                                        "https://cdn.onlinewebfonts.com/svg/img_41170.png";
                    
                    if (int.TryParse(reader.GetValue(0).ToString(), out var id))
                    {
                        Console.WriteLine("id done");
                    }
                    if (int.TryParse(reader.GetValue(6).ToString(), out var popularity))
                    {
                        Console.WriteLine("popularity done");
                    }

                    var resultSong = new Song(id,
                        reader.GetValue(1).ToString() ?? string.Empty,
                        reader.GetValue(2).ToString() ?? string.Empty,
                        iconUrl,
                        reader.GetValue(4).ToString() ?? string.Empty,
                        reader.GetValue(5).ToString() ?? string.Empty,
                        popularity);
                    
                    resultSongs.Add(resultSong);
                }

                await reader.CloseAsync();

                return resultSongs;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return resultSongs;
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }
    
    public async Task<List<Song>?> GetSongsFromSearchAsync(string searchText, CancellationToken ctx = default)
    {
        const string commandStringToSongs = @"select * from songs where title = @searchText or artist = @searchText";
        var resultSongs = new List<Song>();
        
        await _connection.OpenAsync(ctx);

        try
        {
            using (var command = new NpgsqlCommand(commandStringToSongs, _connection))
            {
                command.Parameters.AddWithValue("@searchText", searchText);
                var reader = await command.ExecuteReaderAsync(ctx);
                while (await reader.ReadAsync(ctx))
                {
                    var iconUrl = reader.GetValue(3).ToString() ?? 
                                        "https://cdn.onlinewebfonts.com/svg/img_41170.png";
                    
                    if (int.TryParse(reader.GetValue(0).ToString(), out var id))
                    {
                        Console.WriteLine("id done");
                    }
                    if (int.TryParse(reader.GetValue(6).ToString(), out var popularity))
                    {
                        Console.WriteLine("popularity done");
                    }

                    var resultSong = new Song(id,
                        reader.GetValue(1).ToString() ?? string.Empty,
                        reader.GetValue(2).ToString() ?? string.Empty,
                        iconUrl,
                        reader.GetValue(4).ToString() ?? string.Empty,
                        reader.GetValue(5).ToString() ?? string.Empty,
                        popularity);
                    resultSongs.Add(resultSong);
                }

                await reader.CloseAsync();

                return resultSongs;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return resultSongs;
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }
}