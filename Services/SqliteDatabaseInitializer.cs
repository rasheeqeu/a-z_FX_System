using Microsoft.Data.Sqlite;

namespace ForexTradingWorkspace.Services;

public sealed class SqliteDatabaseInitializer : IDatabaseInitializer
{
    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(AppPaths.DataPath);
        await using var connection = new SqliteConnection($"Data Source={AppPaths.DatabaseFile}");
        await connection.OpenAsync();

        var schemaPath = Path.Combine(AppContext.BaseDirectory, "Data", "schema.sql");
        var schema = File.Exists(schemaPath)
            ? await File.ReadAllTextAsync(schemaPath)
            : await File.ReadAllTextAsync(Path.Combine("Data", "schema.sql"));

        await using var command = connection.CreateCommand();
        command.CommandText = schema;
        await command.ExecuteNonQueryAsync();
    }
}
