using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes;

namespace TeamCardSignal.Hooks;

internal static partial class TeamCardSignalState
{
    private const int SettingsOptionEnabled = 0;
    private const int SettingsOptionPreviewMode = 1;
    private const int SettingsOptionHotkey = 2;
    private const int SettingsOptionCount = 3;

    private static Control? _settingsOverlayRoot;
    private static PanelContainer? _settingsPanel;
    private static Label? _settingsLabel;
    private static bool _settingsWindowVisible;
    private static bool _settingsAwaitingHotkey;
    private static int _settingsSelectedIndex;

    public static bool IsSettingsWindowVisible => _settingsWindowVisible;

    public static void ToggleSettingsWindow()
    {
        if (_settingsWindowVisible)
        {
            CloseSettingsWindow();
            return;
        }

        OpenSettingsWindow();
    }

    public static void HandleSettingsWindowKey(Key key)
    {
        if (!_settingsWindowVisible)
        {
            return;
        }

        if (_settingsAwaitingHotkey)
        {
            HandleHotkeyCapture(key);
            return;
        }

        switch (key)
        {
            case Key.Escape:
                CloseSettingsWindow();
                return;
            case Key.Up:
                ChangeSelection(-1);
                break;
            case Key.Down:
                ChangeSelection(1);
                break;
            case Key.Left:
            case Key.Right:
            case Key.Enter:
            case Key.KpEnter:
            case Key.Space:
                ActivateSelectedOption();
                break;
            default:
                break;
        }

        RefreshSettingsWindowText();
    }

    public static void CleanupSettingsWindow()
    {
        _settingsAwaitingHotkey = false;
        _settingsWindowVisible = false;
        if (IsValidNode(_settingsPanel))
        {
            _settingsPanel!.Visible = false;
        }
    }

    private static void OpenSettingsWindow()
    {
        if (!EnsureSettingsWindow())
        {
            ShowText("TeamCardSignal\uff1a\u65e0\u6cd5\u6253\u5f00\u8bbe\u7f6e\u7a97\u53e3");
            return;
        }

        _settingsWindowVisible = true;
        _settingsAwaitingHotkey = false;
        _settingsSelectedIndex = Mathf.Clamp(_settingsSelectedIndex, 0, SettingsOptionCount - 1);
        _settingsPanel!.Visible = true;
        ResizeSettingsWindow();
        RefreshSettingsWindowText();
        Logger.Log("[Hook] TeamCardSignal settings window opened.");
    }

    private static void CloseSettingsWindow()
    {
        _settingsWindowVisible = false;
        _settingsAwaitingHotkey = false;
        if (IsValidNode(_settingsPanel))
        {
            _settingsPanel!.Visible = false;
        }
        Logger.Log("[Hook] TeamCardSignal settings window closed.");
    }

    private static void ChangeSelection(int delta)
    {
        _settingsSelectedIndex = (_settingsSelectedIndex + delta + SettingsOptionCount) % SettingsOptionCount;
    }

    private static void ActivateSelectedOption()
    {
        var settings = TeamCardSignalSettingsStore.Current;
        switch (_settingsSelectedIndex)
        {
            case SettingsOptionEnabled:
                settings.Enabled = !settings.Enabled;
                TeamCardSignalSettingsStore.Save(settings);
                Logger.Log($"[Hook] TeamCardSignal setting changed: Enabled={settings.Enabled}");
                break;
            case SettingsOptionPreviewMode:
                settings.PreviewMode = settings.PreviewMode == TeamCardSignalPreviewMode.FullCard
                    ? TeamCardSignalPreviewMode.IconOnly
                    : TeamCardSignalPreviewMode.FullCard;
                TeamCardSignalSettingsStore.Save(settings);
                HideSignalPreviewVisuals();
                Logger.Log($"[Hook] TeamCardSignal setting changed: PreviewMode={settings.PreviewMode}");
                break;
            case SettingsOptionHotkey:
                _settingsAwaitingHotkey = true;
                break;
        }
    }

    private static void HandleHotkeyCapture(Key key)
    {
        if (key == Key.Escape)
        {
            _settingsAwaitingHotkey = false;
            RefreshSettingsWindowText();
            return;
        }

        if (key == Key.F7 || key == Key.None)
        {
            ShowText("TeamCardSignal\uff1aF7 \u4fdd\u7559\u4e3a\u8bbe\u7f6e\u7a97\u53e3\u5feb\u6377\u952e");
            return;
        }

        var settings = TeamCardSignalSettingsStore.Current;
        settings.Hotkey = (int)key;
        TeamCardSignalSettingsStore.Save(settings);
        _settingsAwaitingHotkey = false;
        Logger.Log($"[Hook] TeamCardSignal setting changed: Hotkey={key}");
        RefreshSettingsWindowText();
    }

    private static bool EnsureSettingsWindow()
    {
        var game = NGame.Instance;
        if (game == null)
        {
            return false;
        }

        if (!IsValidNode(_settingsOverlayRoot))
        {
            _settingsOverlayRoot = new Control
            {
                Name = "TeamCardSignalSettingsRoot",
                MouseFilter = Control.MouseFilterEnum.Ignore,
                ZIndex = 120
            };
            _settingsOverlayRoot.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _settingsOverlayRoot.SetOffsetsPreset(Control.LayoutPreset.FullRect);
            game.AddChildSafely(_settingsOverlayRoot);
            _settingsPanel = null;
            _settingsLabel = null;
        }

        if (!IsValidNode(_settingsPanel))
        {
            _settingsPanel = new PanelContainer
            {
                Name = "TeamCardSignalSettingsPanel",
                MouseFilter = Control.MouseFilterEnum.Ignore,
                ZIndex = 121,
                Visible = false
            };
            _settingsOverlayRoot!.AddChildSafely(_settingsPanel);

            _settingsLabel = new Label
            {
                Name = "TeamCardSignalSettingsLabel",
                MouseFilter = Control.MouseFilterEnum.Ignore,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            _settingsLabel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _settingsLabel.OffsetLeft = 12f;
            _settingsLabel.OffsetTop = 10f;
            _settingsLabel.OffsetRight = -12f;
            _settingsLabel.OffsetBottom = -10f;
            _settingsPanel.AddChildSafely(_settingsLabel);
        }

        return IsValidNode(_settingsOverlayRoot) && IsValidNode(_settingsPanel) && IsValidNode(_settingsLabel);
    }

    private static void ResizeSettingsWindow()
    {
        if (!IsValidNode(_settingsOverlayRoot) || !IsValidNode(_settingsPanel))
        {
            return;
        }

        var viewportSize = _settingsOverlayRoot!.GetViewportRect().Size;
        var width = Mathf.Clamp(viewportSize.X * 0.38f, 430f, 680f);
        _settingsPanel!.Size = new Vector2(width, 250f);
        _settingsPanel.Position = new Vector2(20f, 20f);
    }

    private static void RefreshSettingsWindowText()
    {
        if (!IsValidNode(_settingsLabel))
        {
            return;
        }

        var settings = TeamCardSignalSettingsStore.Current;
        var enabledText = settings.Enabled ? "\u5f00\u542f" : "\u5173\u95ed";
        var previewText = settings.PreviewMode == TeamCardSignalPreviewMode.FullCard ? "\u5b8c\u6574\u5361\u724c" : "\u5c0f\u56fe\u6807";
        var hotkeyText = ToKey(settings.Hotkey).ToString();

        var prefix0 = _settingsSelectedIndex == SettingsOptionEnabled ? "> " : "  ";
        var prefix1 = _settingsSelectedIndex == SettingsOptionPreviewMode ? "> " : "  ";
        var prefix2 = _settingsSelectedIndex == SettingsOptionHotkey ? "> " : "  ";

        var captureHint = _settingsAwaitingHotkey
            ? "\n\u6b63\u5728\u8bbe\u7f6e\u5feb\u6377\u952e\uff1a\u8bf7\u6309\u4e0b\u4efb\u610f\u6309\u952e\uff08Esc \u53d6\u6d88\uff0cF7 \u4fdd\u7559\uff09"
            : string.Empty;

        _settingsLabel!.Text =
            "TeamCardSignal \u8bbe\u7f6e\n" +
            $"{prefix0}1. \u6807\u8bb0\u529f\u80fd\uff1a{enabledText}\n" +
            $"{prefix1}2. \u663e\u793a\u6a21\u5f0f\uff1a{previewText}\n" +
            $"{prefix2}3. \u6807\u6ce8\u5feb\u6377\u952e\uff1a{hotkeyText}\n\n" +
            "\u64cd\u4f5c\uff1a\u2191\u2193 \u9009\u62e9  Enter/\u2190\u2192 \u6267\u884c  F7/Esc \u5173\u95ed" +
            captureHint;
    }
}
