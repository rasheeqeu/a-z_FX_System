using ForexTradingWorkspace.Models;
using Microsoft.Data.Sqlite;

namespace ForexTradingWorkspace.Services.Repositories;

public sealed class SqliteTradeRepository : ITradeRepository
{
    private static SqliteConnection CreateConnection() => new($"Data Source={AppPaths.DatabaseFile}");

    public async Task<IReadOnlyList<Trade>> SearchAsync(string? pair = null, DateTime? from = null, DateTime? to = null)
    {
        var trades = new List<Trade>();
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, OpenedAt, Pair, Direction, Entry, StopLoss, TakeProfit, Risk, ProfitLoss, Notes, BeforeScreenshotPath, AfterScreenshotPath
            FROM Trades
            WHERE (@pair IS NULL OR Pair LIKE @pair)
              AND (@from IS NULL OR OpenedAt >= @from)
              AND (@to IS NULL OR OpenedAt <= @to)
            ORDER BY OpenedAt DESC
            """;
        command.Parameters.AddWithValue("@pair", string.IsNullOrWhiteSpace(pair) ? DBNull.Value : $"%{pair}%");
        command.Parameters.AddWithValue("@from", from?.ToString("O") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@to", to?.ToString("O") ?? (object)DBNull.Value);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            trades.Add(new Trade
            {
                Id = reader.GetInt64(0),
                OpenedAt = DateTime.Parse(reader.GetString(1)),
                Pair = reader.GetString(2),
                Direction = reader.GetString(3),
                Entry = reader.GetDecimal(4),
                StopLoss = reader.GetDecimal(5),
                TakeProfit = reader.GetDecimal(6),
                Risk = reader.GetDecimal(7),
                ProfitLoss = reader.GetDecimal(8),
                Notes = reader.GetString(9),
                BeforeScreenshotPath = reader.GetString(10),
                AfterScreenshotPath = reader.GetString(11)
            });
        }

        return trades;
    }

    public async Task<Trade> AddAsync(Trade trade)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Trades (OpenedAt, Pair, Direction, Entry, StopLoss, TakeProfit, Risk, ProfitLoss, Notes, BeforeScreenshotPath, AfterScreenshotPath)
            VALUES (@OpenedAt, @Pair, @Direction, @Entry, @StopLoss, @TakeProfit, @Risk, @ProfitLoss, @Notes, @BeforeScreenshotPath, @AfterScreenshotPath);
            SELECT last_insert_rowid();
            """;
        Bind(command, trade);
        trade.Id = (long)(await command.ExecuteScalarAsync() ?? 0L);
        return trade;
    }

    public async Task UpdateAsync(Trade trade)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Trades SET OpenedAt=@OpenedAt, Pair=@Pair, Direction=@Direction, Entry=@Entry, StopLoss=@StopLoss,
            TakeProfit=@TakeProfit, Risk=@Risk, ProfitLoss=@ProfitLoss, Notes=@Notes, BeforeScreenshotPath=@BeforeScreenshotPath,
            AfterScreenshotPath=@AfterScreenshotPath WHERE Id=@Id
            """;
        command.Parameters.AddWithValue("@Id", trade.Id);
        Bind(command, trade);
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(long id)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Trades WHERE Id=@Id";
        command.Parameters.AddWithValue("@Id", id);
        await command.ExecuteNonQueryAsync();
    }

    private static void Bind(SqliteCommand command, Trade trade)
    {
        command.Parameters.AddWithValue("@OpenedAt", trade.OpenedAt.ToString("O"));
        command.Parameters.AddWithValue("@Pair", trade.Pair);
        command.Parameters.AddWithValue("@Direction", trade.Direction);
        command.Parameters.AddWithValue("@Entry", trade.Entry);
        command.Parameters.AddWithValue("@StopLoss", trade.StopLoss);
        command.Parameters.AddWithValue("@TakeProfit", trade.TakeProfit);
        command.Parameters.AddWithValue("@Risk", trade.Risk);
        command.Parameters.AddWithValue("@ProfitLoss", trade.ProfitLoss);
        command.Parameters.AddWithValue("@Notes", trade.Notes);
        command.Parameters.AddWithValue("@BeforeScreenshotPath", trade.BeforeScreenshotPath);
        command.Parameters.AddWithValue("@AfterScreenshotPath", trade.AfterScreenshotPath);
    }
}
