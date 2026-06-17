namespace ForexTradingWorkspace;

public static class AppPaths
{
    public static string RootPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ForexTradingWorkspace");

    public static string DataPath => Path.Combine(RootPath, "Data");
    public static string LogsPath => Path.Combine(RootPath, "Logs");
    public static string ScreenshotsPath => Path.Combine(RootPath, "Screenshots");
    public static string SettingsFile => Path.Combine(DataPath, "settings.secure.json");
    public static string LayoutFile => Path.Combine(DataPath, "workspace-layout.json");
    public static string DatabaseFile => Path.Combine(DataPath, "journal.db");
}
