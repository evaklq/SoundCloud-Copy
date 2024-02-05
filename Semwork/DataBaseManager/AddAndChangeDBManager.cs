using System.Net;
using Npgsql;
using Semwork.Models;
using NpgsqlTypes;

namespace Semwork.DBManager;

public partial class DataBaseManager
{
    private readonly NpgsqlConnection _connection;
    public DataBaseManager()
    {
        const string connectionString = "Host=localhost:5432;" +
                                        "Database=solumfy;" +
                                        "Username=postgres;" +
                                        "Password=evaklq";
        _connection = new NpgsqlConnection(connectionString);
    }
    
    /// <summary>
    /// Add user to data base with user.
    /// Firstly add user to "users". Secondly add user favorite and personal songs to "favorite_songs"
    /// and "personal_songs". Thirdly add user playlists to "playlists".
    /// </summary>
    /// <returns> Return user with id from data base after additing user. </returns>;
    public async Task<User?> AddUserToDbAsync(User user, CancellationToken ctx = default)
    {
        if (user.Id != null)
        {
            var oldUser = await GetUserFromDbByLoginAsync(user.Nick, ctx);
            if (oldUser != null)
            {
                return oldUser;
            }
        }
        const string commandStringToUsers = @"insert into users (full_name, nick, email, phone_number, password_hash, icon)
                                values (@name, @nick, @email, @number, @password, @icon)
                                on conflict do nothing
                                returning id;";
        
        await _connection.OpenAsync(ctx);

        try
        {
            using (var command = new NpgsqlCommand(commandStringToUsers, _connection))
            {
                command.Parameters.AddWithValue("@name", NpgsqlDbType.Text, user.FullName);
                command.Parameters.AddWithValue("@nick", NpgsqlDbType.Text, user.Nick);
                command.Parameters.AddWithValue("@email", NpgsqlDbType.Text, user.Email);
                command.Parameters.AddWithValue("@number", NpgsqlDbType.Text, user.Number);
                command.Parameters.AddWithValue("@password", NpgsqlDbType.Text, user.Password);
                command.Parameters.AddWithValue("@icon", NpgsqlDbType.Text, user.IconUrl ?? "");

                var insertedId = await command.ExecuteScalarAsync(ctx);
                var userId = 0;
                if (insertedId != null && int.TryParse(insertedId.ToString(), out userId))
                {
                    user.Id = userId;
                }
                
                var favoriteSongsId = new List<int>();
                foreach (var song in user.FavouriteSongs)
                {
                    var id = await GetOrCreateSongAsync(song, ctx);
                    song.Id = id;
                    favoriteSongsId.Add(id);
                }
                var personalSongsId = new List<int>();
                foreach (var song in user.PersonalSongs)
                {
                    var id = await GetOrCreateSongAsync(song, ctx);
                    song.Id = id;
                    personalSongsId.Add(id);
                }
                
                await AddSongsToDifferentTables(userId, favoriteSongsId, "favorite_songs", ctx);
                await AddSongsToDifferentTables(userId, personalSongsId, "personal_songs", ctx);
                
                foreach (var playlist in user.Playlists)
                {
                    var playlistSongsId = new List<int>();

                    var playlistId = await GetOrCreatePlaylistAsync(playlist, user, ctx);
                    playlist.Id = playlistId;
                    foreach (var song in playlist.Songs)
                    {
                        var id = await GetOrCreateSongAsync(song, ctx);
                        song.Id = id;
                        playlistSongsId.Add(id);
                    }

                    await AddSongsToDifferentTables(playlistId, playlistSongsId, "playlist_songs", ctx);
                }

                foreach (var comment in user.Comments)
                {
                    var id = await GetOrCreateCommentAsync(comment, user, ctx);
                    comment.Id = id;
                }

                await UpdateUserAsync(user, false, ctx);
                return user;
            }
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine($"Error executing SQL query: {ex.Message}");
            return null;
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }

    public async Task UpdateUserAsync(User user, bool needToConnection = true, CancellationToken ctx = default)
    {
        const string commandStringToUsers = @"UPDATE users
                                     SET full_name = @name, nick = @nick, email = @email, 
                                     phone_number = @number, password_hash = @password, icon = @icon
                                     WHERE id = @user_id";
        if (needToConnection)
        {
            await _connection.OpenAsync(ctx);
        }

        try
        {
            await using (var command = new NpgsqlCommand(commandStringToUsers, _connection))
            {
                command.Parameters.AddWithValue("@user_id", NpgsqlDbType.Integer, user.Id ?? -1);
                command.Parameters.AddWithValue("@name", NpgsqlDbType.Text, user.FullName);
                command.Parameters.AddWithValue("@nick", NpgsqlDbType.Text, user.Nick);
                command.Parameters.AddWithValue("@email", NpgsqlDbType.Text, user.Email);
                command.Parameters.AddWithValue("@number", NpgsqlDbType.Text, user.Number);
                command.Parameters.AddWithValue("@password", NpgsqlDbType.Text, user.Password);
                command.Parameters.AddWithValue("@icon", NpgsqlDbType.Text, user.IconUrl ?? "");

                await command.ExecuteNonQueryAsync(ctx);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            if (needToConnection)
            {
                await _connection.CloseAsync();
            }
        }
    }
    
    /// <summary>
    /// Support func to add song to data base.
    /// Doesn't have open connection cause using in function with open connection.
    /// </summary>
    /// <returns> Can returns less 0 value when mistake to don't have exeptions. </returns>
    public async Task<int> AddSongToMainTableAsync(Song song, bool needConnection = true, 
                                                   CancellationToken ctx = default)
    {
        const string commandStringToSongs = @"insert into songs (title, artist, icon, song, album, popularity)
                                        values (@title, @artist, @iconUrl, @songUrl, @album, @popularity)
                                        on conflict do nothing
                                        returning id;";
        if (needConnection)
        {
            await _connection.OpenAsync(ctx);
        }

        try
        {
            using (var command = new NpgsqlCommand(commandStringToSongs, _connection))
            {
                command.Parameters.AddWithValue("@title", NpgsqlDbType.Text, song.Title);
                command.Parameters.AddWithValue("@artist", NpgsqlDbType.Text, song.Artist);
                command.Parameters.AddWithValue("@iconUrl", NpgsqlDbType.Text, song.IconUrl);
                command.Parameters.AddWithValue("@songUrl", NpgsqlDbType.Text, song.SongUrl);
                command.Parameters.AddWithValue("@album", NpgsqlDbType.Text, song.Album);
                command.Parameters.AddWithValue("@popularity", NpgsqlDbType.Integer, song.Popularity);

                var insertedId = await command.ExecuteScalarAsync(ctx);
                if (int.TryParse(insertedId?.ToString(), out var songId))
                {
                    Console.WriteLine("song id get back");
                    return songId;
                }

                return -1;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return -1;
        }
        finally
        {
            if (needConnection)
            {
                await _connection.CloseAsync();
            }
        }
    }

    public async Task<int> AddCommentToDbAsync(Comment comment, User user, bool needConnection = true,
        CancellationToken ctx = default)
    {
        const string commandStringToComments = @"insert into general_comments (user_id, message_context)
                                values (@userId, @message)
                                on conflict do nothing
                                returning id;";
        if (needConnection)
        {
            await _connection.OpenAsync(ctx);
        }

        try
        {
            using (var command = new NpgsqlCommand(commandStringToComments, _connection))
            {
                command.Parameters.AddWithValue("@userId", user.Id ?? -1);
                command.Parameters.AddWithValue("@message", comment.MessageContext);
            
                var insertedId = await command.ExecuteScalarAsync(ctx);
                if (insertedId != null && int.TryParse(insertedId.ToString(), out var commentId))
                {
                    return commentId;
                }

                return -1;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return -1;
        }
        finally
        {
            if (needConnection)
            {
                await _connection.CloseAsync();
            }
        }
    }

    /// <summary>
    /// Support func to add playlist to data base with playlist and user to "playlists".
    /// Doesn't have open connection cause using in function with open connection.
    /// </summary>
    /// <returns> Can return less 0 value when mistake to don't have exeptions. </returns>
    public async Task<int> AddPlaylistToDbAsync(Playlist playlist, User user, bool needConnection = true, 
                                                CancellationToken ctx = default)
    {
        const string commandStringToUsers = @"insert into playlists (user_id, title)
                                values (@user_id, @title)
                                on conflict do nothing
                                returning id;";
        if (needConnection)
        {
            await _connection.OpenAsync(ctx);
        }

        try
        {
            using (var command = new NpgsqlCommand(commandStringToUsers, _connection))
            {
                command.Parameters.AddWithValue("@user_id", NpgsqlDbType.Integer, user.Id ?? -1);
                command.Parameters.AddWithValue("@title", NpgsqlDbType.Text, playlist.Title);

                var insertedId = await command.ExecuteScalarAsync(ctx);
                if (insertedId != null && int.TryParse(insertedId.ToString(), out var playlistId))
                {
                    return playlistId;
                }

                return -1;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return -1;
        }
        finally
        {
            if (needConnection)
            {
                await _connection.CloseAsync();
            }
        }
    }
    
    /// <summary>
    /// Support func to add songs or playlist with user id or playlist id and song ids into different tables.
    /// You can add songs in "personal_songs" or "favorite_songs".
    /// And add playlist songs in "playlist_songs".
    /// Doesn't have open connection cause using in function with open connection.
    /// </summary>
    public async Task AddSongsToDifferentTables(int userId, List<int> songsId, string tableName, 
        CancellationToken ctx = default, bool needConnection = false)
    {
        string commandString;
        if (tableName == "playlist_songs")
        {
            commandString = $"insert into {tableName} (song_id, playlist_id) values (@user_id, @song_id)";
        }
        else
        {
            commandString = $"insert into {tableName} (user_id, song_id) values (@user_id, @song_id)";
        }

        if (needConnection)
        {
            await _connection.OpenAsync(ctx);
        }

        try
        {
            using (var command = new NpgsqlCommand(commandString, _connection))
            {
                command.Parameters.Add("@user_id", NpgsqlDbType.Integer);
                command.Parameters.Add("@song_id", NpgsqlDbType.Integer);

                foreach (var id in songsId)
                {
                    if (tableName == "playlist_songs")
                    {
                        command.Parameters["@user_id"].Value = id;
                        command.Parameters["@song_id"].Value = userId;
                    }
                    else
                    {
                        command.Parameters["@user_id"].Value = userId;
                        command.Parameters["@song_id"].Value = id;
                    }

                    await command.ExecuteNonQueryAsync(ctx);

                    command.Parameters.Clear();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            if (needConnection)
            {
                await _connection.CloseAsync();
            }
        }
    }
    
    /// <summary>
    /// Support func to check song id in data base or add new song.
    /// Doesn't have open connection cause using in function with open connection.
    /// </summary>
    private async Task<int> GetOrCreateCommentAsync(Comment comment, User user, CancellationToken ctx = default)
    {
        int commentId;
        if (comment.Id == null)
        {
            commentId = await AddCommentToDbAsync(comment, user, false, ctx);
        }
        else
        {
            commentId = await CheckCommentInDbAsyncById(comment, user, ctx);
        }

        if (commentId < 0)
        {
            throw new Exception("Id was < 0, it's bad");
        }
        return commentId;
    }
    
    /// <summary>
    /// Support func to check song id in data base or add new song.
    /// Doesn't have open connection cause using in function with open connection.
    /// </summary>
    private async Task<int> GetOrCreatePlaylistAsync(Playlist playlist, User user, CancellationToken ctx = default)
    {
        int playlistId;
        if (playlist.Id == null)
        {
            playlistId = await AddPlaylistToDbAsync(playlist, user, false, ctx);
        }
        else
        {
            playlistId = await CheckPlaylistInDbAsyncById(playlist, user, ctx);
        }

        if (playlistId < 0)
        {
            throw new Exception("Id was < 0, it's bad");
        }
        return playlistId;
    }
    
    /// <summary>
    /// Support func to check song id in data base or add new song.
    /// Doesn't have open connection cause using in function with open connection.
    /// </summary>
    private async Task<int> GetOrCreateSongAsync(Song song, CancellationToken ctx = default)
    {
        int songId;
        if (song.Id == null)
        {
            songId = await AddSongToMainTableAsync(song, false, ctx);
        }
        else
        {
            songId = await CheckSongInDbAsyncById(song, ctx);
        }

        if (songId < 0)
        {
            throw new Exception("Id was < 0, it's bad");
        }
        return songId;
    }
    
    /// <summary>
    /// Support func to check that data base has current song id.
    /// Doesn't have open connection cause using in function with open connection.
    /// </summary>
    /// <returns> Can returns less 0 value when mistake to don't have exeptions. </returns>
    private async Task<int> CheckCommentInDbAsyncById(Comment comment, User user, CancellationToken ctx = default)
    {
        const string commandStringToSongs = @"SELECT id FROM general_comments 
                                              WHERE message_context = @message and user_id = @user_id;";
        try
        {
            using (var command = new NpgsqlCommand(commandStringToSongs, _connection))
            {
                command.Parameters.AddWithValue("@message", comment.MessageContext);
                command.Parameters.AddWithValue("@user_id", user.Id ?? -1);

                using (var reader = await command.ExecuteReaderAsync(ctx))
                {
                    while (await reader.ReadAsync(ctx))
                    {
                        if (!int.TryParse(reader.GetValue(0).ToString(), out var id)) continue;
                        Console.WriteLine("Comment found. Id: " + id);
                        return id;
                    }
                    
                    await reader.CloseAsync();
                }

                var newCommentId = await AddCommentToDbAsync(comment, user, false, ctx);
                Console.WriteLine("New comment added. Id: " + newCommentId);
                return newCommentId;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return -1;
        }
    }
    
    /// <summary>
    /// Support func to check that data base has current song id.
    /// Doesn't have open connection cause using in function with open connection.
    /// </summary>
    /// <returns> Can returns less 0 value when mistake to don't have exeptions. </returns>
    private async Task<int> CheckSongInDbAsyncById(Song song, CancellationToken ctx = default)
    {
        const string commandStringToSongs = @"SELECT id FROM songs WHERE song = @song;";
        try
        {
            using (var command = new NpgsqlCommand(commandStringToSongs, _connection))
            {
                command.Parameters.AddWithValue("@song", song.SongUrl);

                using (var reader = await command.ExecuteReaderAsync(ctx))
                {
                    while (await reader.ReadAsync(ctx))
                    {
                        if (!int.TryParse(reader.GetValue(0).ToString(), out var id)) continue;
                        Console.WriteLine("Song found. Id: " + id);
                        return id;
                    }
                    
                    await reader.CloseAsync();
                }

                var newSongId = await AddSongToMainTableAsync(song, false, ctx);
                Console.WriteLine("New song added. Id: " + newSongId);
                return newSongId;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return -1;
        }
    }
    
    /// <summary>
    /// Support func to check that data base has current song id.
    /// Doesn't have open connection cause using in function with open connection.
    /// </summary>
    /// <returns> Can returns less 0 value when mistake to don't have exeptions. </returns>
    private async Task<int> CheckPlaylistInDbAsyncById(Playlist playlist, User user, CancellationToken ctx = default)
    {
        const string commandStringToSongs = @"SELECT id, user_id FROM playlists WHERE title = @title;";
        var playlistId = 0;
        try
        {
            using (var command = new NpgsqlCommand(commandStringToSongs, _connection))
            {
                command.Parameters.AddWithValue("@title", playlist.Title);

                using (var reader = await command.ExecuteReaderAsync(ctx))
                {
                    while (await reader.ReadAsync(ctx))
                    {
                        if (!int.TryParse(reader.GetValue(0).ToString(), out playlistId)) continue;
                        Console.WriteLine("Playlist found. Id: " + playlistId);
                        
                        if (!int.TryParse(reader.GetValue(1).ToString(), out var userId)) continue;
                        Console.WriteLine("User found. Id: " + userId);
                    }
                    
                    await reader.CloseAsync();
                    
                    var playlistsSongs = await GetDifferentUserSongsAsync("playlist_songs", false, 
                        playlistId: playlistId, ctx: ctx);

                    if (playlistsSongs == playlist.Songs)
                    {
                        return playlistId;
                    }
                }

                var newPlaylistId = await AddPlaylistToDbAsync(playlist, user, false, ctx);
                Console.WriteLine("New song added. Id: " + newPlaylistId);
                return newPlaylistId;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return -1;
        }
    }
}