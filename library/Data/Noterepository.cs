using library.Models;
using Npgsql;
namespace library.Data;

public class Noterepository
{
    private readonly NpgsqlDataSource _dataSource;

    // ASP.NET Core hands in the data source that Program.cs registered.
    public Noterepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    // Copies the current row of the reader into a Book.
    // The positions 0 to 3 match the column order in both SELECTs above.
    private static Notes ReadNote(NpgsqlDataReader reader)
    {
        return new Notes
        {
            note_id = reader.GetInt64(0),
            title = reader.GetString(1),
            body = reader.IsDBNull(2) ? null : reader.GetString(2),   // NULL becomes null
            created_on = reader.IsDBNull(3) ? DateTime.UtcNow : reader.GetDateTime(3)
        };
    }

    // READ: every book, sorted by title.
    public async Task<List<Notes>> GetAllAsync()
    {
        const string sql = "SELECT note_id, title, body, created_on FROM lending.note ORDER BY title;";

        var notes = new List<Notes>();

        // The command borrows a connection; 'await using' gives it back when the method ends.
        await using var command = _dataSource.CreateCommand(sql);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())        // once for each row
        {
            notes.Add(ReadNote(reader));
        }
        return notes;
    }

    // CREATE: insert a new book. The database chooses its book_id.
    public async Task AddAsync(Notes notes)
    {
        const string sql = "INSERT INTO lending.note (title, body, created_on) " +
                           "VALUES (@title, @body, @created_on);";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("title", notes.title);
        // A C# null must be sent as DBNull.Value, which is how a database NULL is written.
        command.Parameters.AddWithValue("category", (object?)notes.body ?? DBNull.Value);
        command.Parameters.AddWithValue("price", (object?)notes.created_on ?? DBNull.Value);

        await command.ExecuteNonQueryAsync();           // runs SQL that returns no rows
    }


}

