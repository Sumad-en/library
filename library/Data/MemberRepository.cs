using library.Models;
using Npgsql;

namespace library.Data;

// Every SQL statement about books lives in this one class.
public class MemberRepository
{
    private readonly NpgsqlDataSource _dataSource;

    // ASP.NET Core hands in the data source that Program.cs registered.
    public MemberRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    // Copies the current row of the reader into a Book.
    // The positions 0 to 3 match the column order in both SELECTs above.
    private static Members ReadMember(NpgsqlDataReader reader)
    {
        return new Members
        {
            member_id = reader.GetInt64(0),
            full_name = reader.GetString(1),
            member_type = reader.GetString(2),
            email = reader.IsDBNull(3) ? null : reader.GetString(3)
        };
    }

    // READ: every book, sorted by title.
    public async Task<List<Members>> GetAllAsync()
    {
        const string sql = "SELECT member_id, full_name, member_type, email FROM lending.member ORDER BY full_name;";

        var members = new List<Members>();

        // The command borrows a connection; 'await using' gives it back when the method ends.
        await using var command = _dataSource.CreateCommand(sql);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())        // once for each row
        {
            members.Add(ReadMember(reader));
        }

        return members;
    }

    // CREATE: insert a new book. The database chooses its book_id.
    public async Task AddAsync(Members members)
    {
        const string sql = "INSERT INTO lending.member(full_name, member_type, email) " +
                           "VALUES (@full_name, @member_type, @email);";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("full_name", members.full_name);
        // A C# null must be sent as DBNull.Value, which is how a database NULL is written.
        command.Parameters.AddWithValue("member_type", (object?)members.member_type ?? DBNull.Value);
        command.Parameters.AddWithValue("email", (object?)members.email ?? DBNull.Value);

        await command.ExecuteNonQueryAsync();           // runs SQL that returns no rows
    }

}
