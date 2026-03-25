using System.Text.Json;
using Godot;

namespace TeamCardSignal.Hooks;

internal enum TeamCardSignalPreviewMode
{
    IconOnly = 0,
    FullCard = 1
}

internal sealed class TeamCardSignalSettings
{
    public bool Enabled { get; set; } = true;

    public int Hotkey { get; set; } = (int)Key.B;

    public float CooldownSeconds { get; set; } = 1.0f;

    public TeamCardSignalPreviewMode PreviewMode { get; set; } = TeamCardSignalPreviewMode.FullCard;
}

internal static class TeamCardSignalSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private static readonly string SettingsDir = Path.Combine(
        System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
        "SlayTheSpire2",
        "mods",
        ModEntry.ModId
    );

    private static readonly string SettingsPath = Path.Combine(SettingsDir, "settings.json");

    private static TeamCardSignalSettings _current = LoadInternal();

    public static TeamCardSignalSettings Current => _current;

    public static void Save(TeamCardSignalSettings settings)
    {
        _current = settings;
        try
        {
            Directory.CreateDirectory(SettingsDir);
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(SettingsPath, json);
            Logger.Log($"[Hook] TeamCardSignal settings saved: {SettingsPath}");
        }
        catch (Exception ex)
        {
            Logger.Log($"[Hook] TeamCardSignal settings save failed: {ex.Message}");
        }
    }

    private static TeamCardSignalSettings LoadInternal()
    {
        try
        {
            if (!File.Exists(SettingsPath))
            {
                var defaults = new TeamCardSignalSettings();
                Save(defaults);
                return defaults;
            }

            var json = File.ReadAllText(SettingsPath);
            var loaded = JsonSerializer.Deserialize<TeamCardSignalSettings>(json);
            if (loaded == null)
            {
                return new TeamCardSignalSettings();
            }

            var needsSave = false;

            // Migrate legacy default hotkey V -> B to avoid conflict with base game controls.
            if (loaded.Hotkey == (int)Key.V)
            {
                loaded.Hotkey = (int)Key.B;
                needsSave = true;
                Logger.Log("[Hook] TeamCardSignal settings migrated: default hotkey V -> B");
            }

            if (!Enum.IsDefined(typeof(TeamCardSignalPreviewMode), loaded.PreviewMode))
            {
                loaded.PreviewMode = TeamCardSignalPreviewMode.FullCard;
                needsSave = true;
                Logger.Log("[Hook] TeamCardSignal settings migrated: PreviewMode -> FullCard");
            }

            if (needsSave)
            {
                Save(loaded);
            }

            return loaded;
        }
        catch (Exception ex)
        {
            Logger.Log($"[Hook] TeamCardSignal settings load failed: {ex.Message}");
            return new TeamCardSignalSettings();
        }
    }
}
