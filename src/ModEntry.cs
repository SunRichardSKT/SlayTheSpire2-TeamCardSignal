using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace TeamCardSignal;

[ModInitializer("Initialize")]
public static class ModEntry
{
    public static readonly string ModId = "TeamCardSignal";
    public static readonly string ModName = "TeamCardSignal";

    public static void Initialize()
    {
        var harmony = new Harmony(ModId);
        harmony.PatchAll();
        Logger.Log($"[{ModName}] Mod loaded! Hooks active.");
    }
}

public static class Logger
{
    private static readonly string LogPath;

    static Logger()
    {
        LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SlayTheSpire2", "logs", "mod_log.txt"
        );
        Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
    }

    public static void Log(string message)
    {
        File.AppendAllText(LogPath, $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }
}
