CREATE TABLE IF NOT EXISTS Trades (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    OpenedAt TEXT NOT NULL,
    Pair TEXT NOT NULL,
    Direction TEXT NOT NULL,
    Entry REAL NOT NULL,
    StopLoss REAL NOT NULL,
    TakeProfit REAL NOT NULL,
    Risk REAL NOT NULL,
    ProfitLoss REAL NOT NULL,
    Notes TEXT NOT NULL,
    BeforeScreenshotPath TEXT NOT NULL,
    AfterScreenshotPath TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS IX_Trades_Pair ON Trades(Pair);
CREATE INDEX IF NOT EXISTS IX_Trades_OpenedAt ON Trades(OpenedAt);
