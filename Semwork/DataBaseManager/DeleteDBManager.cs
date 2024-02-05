using Npgsql;
using Semwork.Models;

namespace Semwork.DBManager;

public partial class DataBaseManager
{
    /// <summary>
    /// Delete user and all user songs and comments
    /// </summary>
    /// <param name="user"></param>
    /// <param name="ctx"></param>
    public async Task DeleteUserByIdAsync(User user, CancellationToken ctx = default)
    {
        const string commandStringToUsers = @"delete from users where id = @user_id";

        foreach (var song in user.FavouriteSongs)
        {
            await DeleteSmthFromSTableById(song.Id ?? -1, "favorite_songs", ctx: ctx);
        }
        foreach (var song in user.PersonalSongs)
        {
            await DeleteSmthFromSTableById(song.Id ?? -1, "personal_songs", ctx: ctx);
        }
        foreach (var playlist in user.Playlists)
        {
            foreach (var song in playlist.Songs)
            {
                await DeleteSmthFromSTableById(song.Id ?? -1, "playlist_songs", ctx: ctx);
            }
            await DeleteSmthFromSTableById(playlist.Id ?? -1, "playlists", ctx: ctx);
        }
        foreach (var comment in user.Comments)
        {
            await DeleteSmthFromSTableById(comment.Id ?? -1, "general_comments", ctx: ctx);
        }
        
        await _connection.OpenAsync(ctx);
        try
        {
            using (var command = new NpgsqlCommand(commandStringToUsers, _connection))
            {
                command.Parameters.AddWithValue("@user_id", user.Id ?? 0);

                await command.ExecuteNonQueryAsync(ctx);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }

    public async Task DeleteLikeAsync(int userId, int songId)
    {
        const string commandStringToLikes = @"delete from favorite_songs where user_id = @user_id and song_id = @song_id";
        await _connection.OpenAsync();
        
        try
        {
            using (var command = new NpgsqlCommand(commandStringToLikes, _connection))
            {
                command.Parameters.AddWithValue("@user_id", userId);
                command.Parameters.AddWithValue("@song_id", songId);

                await command.ExecuteNonQueryAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
        finally
        { 
            await _connection.CloseAsync();
        }
    }

    /// <summary>
    /// Delete something from table from params
    /// </summary>
    /// <param name="id"></param>
    /// <param name="tableName"></param>
    /// <param name="needConnection"></param>
    /// <param name="ctx"></param>
    public async Task DeleteSmthFromSTableById(int id, string tableName, bool needConnection = true, 
        CancellationToken ctx = default)
    {
        var commandStringToSongs = $"delete from {tableName} where id = @id_in_table";

        if (needConnection)
        {
            await _connection.OpenAsync(ctx);
        }
        
        try
        {
            using (var command = new NpgsqlCommand(commandStringToSongs, _connection))
            {
                command.Parameters.AddWithValue("@id_in_table", id);

                await command.ExecuteNonQueryAsync(ctx);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
        finally
        {
            if (needConnection)
            {
                await _connection.CloseAsync();
            }
        }
    }
}