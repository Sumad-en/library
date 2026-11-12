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
        int emailOrdinal = reader.GetOrdinal("email");
        return new Members
        {
            member_id = reader.GetInt64(reader.GetOrdinal("member_id")),
            full_name = reader.GetString(reader.GetOrdinal("full_name")),
            member_type = reader.GetString(reader.GetOrdinal("member_type")),
            email = reader.IsDBNull(emailOrdinal) ? null : reader.GetString(emailOrdinal)
        };
    }

    // READ: every member, with how many books each has out now.
    public async Task<List<Members>> GetAllAsync()
    {
        // 1. The SQL: every member, joined to their open loans, which are counted.
        const string sql =
    "SELECT m.member_id, m.full_name, m.email, m.member_type, COUNT(l.loan_id) " +
    "FROM lending.member m " +
    "LEFT JOIN lending.loan l ON l.member_id = m.member_id AND l.returned_on IS NULL " +
    "GROUP BY m.member_id " +
    "ORDER BY m.full_name;";

        // 2. An empty list, to fill.
        var members = new List<Members>();
        // 3. A command that will run the SQL.
        await using var command = _dataSource.CreateCommand(sql);
        // 4. Run it, and get a reader for the rows.
        await using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            // 6. Copy the row into a Member.
            var member = ReadMember(reader);
            // 7. Add the count from the fifth column, which only this SELECT has.
            member.OpenLoans = reader.GetInt64(4);
            // 8. Add the member to the list.
            members.Add(member);
        }
        // 9. Give the list back.
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
        command.Parameters.AddWithValue("member_type", members.member_type);
        command.Parameters.AddWithValue("email", (object?)members.email ?? DBNull.Value);

        await command.ExecuteNonQueryAsync();           // runs SQL that returns no rows
    }

    // READ: one member, or null when no member has that id.
    public async Task<Members?> GetByIdAsync(long id)
    {
        const string sql = "SELECT member_id, full_name, member_type, email FROM lending.member WHERE member_id = @id;";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);      // @id in the SQL receives this value

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadMember(reader) : null;
    }

    // UPDATE: change every column of one member.
    public async Task UpdateAsync(Members members)
    {
        const string sql = "UPDATE lending.member " +
                           "SET full_name = @full_name, member_type = @member_type, email = @email " +
                           "WHERE member_id = @id;";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("Full name", members.full_name);
        command.Parameters.AddWithValue("Member Type", members.member_type);
        command.Parameters.AddWithValue("price", (object?)members.email ?? DBNull.Value);
        command.Parameters.AddWithValue("id", members.member_id);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(long id)
    {
        const string sql = "DELETE FROM lending.member WHERE member_id = @id;";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);

        await command.ExecuteNonQueryAsync();
    }

}
