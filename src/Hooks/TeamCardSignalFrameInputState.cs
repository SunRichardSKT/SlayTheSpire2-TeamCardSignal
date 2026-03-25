using Godot;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace TeamCardSignal.Hooks;

internal static partial class TeamCardSignalState
{
    private static bool _frameInputSeen;
    private static bool _f7WasDown;
    private static bool _upWasDown;
    private static bool _downWasDown;
    private static bool _leftWasDown;
    private static bool _rightWasDown;
    private static bool _enterWasDown;
    private static bool _escapeWasDown;
    private static bool _hotkeyWasDown;

    public static void HandleCombatFrameInput()
    {
        if (!TryGetRun(out _) || NCombatRoom.Instance == null)
        {
            _frameInputSeen = false;
            _f7WasDown = false;
            _upWasDown = false;
            _downWasDown = false;
            _leftWasDown = false;
            _rightWasDown = false;
            _enterWasDown = false;
            _escapeWasDown = false;
            _hotkeyWasDown = false;
            CleanupSettingsWindow();
            return;
        }

        if (!_frameInputSeen)
        {
            Logger.Log("[Hook] TeamCardSignal frame input active.");
            _frameInputSeen = true;
        }

        var settings = TeamCardSignalSettingsStore.Current;

        var f7Down = IsKeyDown(Key.F7);
        if (f7Down && !_f7WasDown)
        {
            ToggleSettingsWindow();
            Logger.Log("[Hook] TeamCardSignal key detected (frame): F7");
        }
        _f7WasDown = f7Down;

        if (IsSettingsWindowVisible)
        {
            ProcessSettingsWindowFrameNav();
            _hotkeyWasDown = false;
            return;
        }
        ResetSettingsWindowFrameNavState();

        var hotkey = ToKey(settings.Hotkey);
        var hotkeyDown = hotkey != Key.None && IsKeyDown(hotkey);
        if (hotkeyDown && !_hotkeyWasDown)
        {
            Logger.Log($"[Hook] TeamCardSignal key detected (frame): {hotkey}");
            if (!settings.Enabled)
            {
                ShowText("TeamCardSignal\uff1a\u5df2\u7981\u7528\uff08F7 \u6253\u5f00\u8bbe\u7f6e\uff09");
            }
            else
            {
                TrySendLocalSignal(settings);
            }
        }
        _hotkeyWasDown = hotkeyDown;
    }

    private static bool IsKeyDown(Key key)
    {
        return Input.IsPhysicalKeyPressed(key) || Input.IsKeyPressed(key);
    }

    private static void ProcessSettingsWindowFrameNav()
    {
        ProcessFrameEdge(Key.Up, ref _upWasDown);
        ProcessFrameEdge(Key.Down, ref _downWasDown);
        ProcessFrameEdge(Key.Left, ref _leftWasDown);
        ProcessFrameEdge(Key.Right, ref _rightWasDown);
        ProcessFrameEdge(Key.Enter, ref _enterWasDown);
        ProcessFrameEdge(Key.Escape, ref _escapeWasDown);
    }

    private static void ProcessFrameEdge(Key key, ref bool wasDown)
    {
        var isDown = IsKeyDown(key);
        if (isDown && !wasDown)
        {
            HandleSettingsWindowKey(key);
        }

        wasDown = isDown;
    }

    private static void ResetSettingsWindowFrameNavState()
    {
        _upWasDown = false;
        _downWasDown = false;
        _leftWasDown = false;
        _rightWasDown = false;
        _enterWasDown = false;
        _escapeWasDown = false;
    }
}
