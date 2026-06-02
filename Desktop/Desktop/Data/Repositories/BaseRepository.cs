using Npgsql;

namespace Desktop.Data.Repositories;

public abstract class BaseRepository
{
    protected string ConnectionString { get; }

    protected BaseRepository(string connectionString)
    {
        ConnectionString = connectionString;
    }

    protected NpgsqlConnection CreateConnection() => new(ConnectionString);
}