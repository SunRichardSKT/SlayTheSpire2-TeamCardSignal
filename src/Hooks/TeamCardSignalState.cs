using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;

namespace TeamCardSignal.Hooks;

internal static partial class TeamCardSignalState
{
    private static readonly Dictionary<ulong, string> LastTargetNames = new();
    private static readonly Dictionary<ulong, string> LastTargetIds = new();

    private static ulong _lastInputHandledTick;
    private static Key _lastInputHandledKey = Key.None;
    private static ulong _nextAllowedSendTick;

    public static void HandleCombatInput(InputEvent inputEvent)
    {
        if (inputEvent is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo)
        {
            return;
        }

        var key = ResolveKey(keyEvent);
        if (key == Key.None)
        {
            return;
        }

        // Avoid duplicate handling when multiple _Input patches observe the same key event.
        var inputNow = Time.GetTicksMsec();
        if (_lastInputHandledKey == key && inputNow - _lastInputHandledTick < 40)
        {
            return;
        }
        _lastInputHandledKey = key;
        _lastInputHandledTick = inputNow;

        if (key == Key.F7)
        {
            ToggleSettingsWindow();
            return;
        }

        if (IsSettingsWindowVisible)
        {
            HandleSettingsWindowKey(key);
            return;
        }

        var current = TeamCardSignalSettingsStore.Current;
        if (key == ToKey(current.Hotkey))
        {
            Logger.Log($"[Hook] TeamCardSignal key detected: {key}");
        }

        if (key != ToKey(current.Hotkey))
        {
            return;
        }

        if (!current.Enabled)
        {
            ShowText("TeamCardSignal\uff1a\u5df2\u7981\u7528\uff08F7 \u6253\u5f00\u8bbe\u7f6e\uff09");
            return;
        }

        TrySendLocalSignal(current);
    }

    public static void OnLocalTargetHovered(NCreature creature)
    {
        if (!TryGetLocalNetId(out var localId) || creature.Entity == null)
        {
            return;
        }

        LastTargetNames[localId] = creature.Entity.Name;
        LastTargetIds[localId] = creature.Entity.ModelId.Entry;
    }

    public static void OnLocalTargetUnhovered()
    {
        if (!TryGetLocalNetId(out var localId))
        {
            return;
        }

        LastTargetNames.Remove(localId);
        LastTargetIds.Remove(localId);
    }

    private static void TrySendLocalSignal(TeamCardSignalSettings settings)
    {
        if (!TryGetRun(out var run) || NCombatRoom.Instance == null)
        {
            Logger.Log("[Hook] TeamCardSignal skipped: not in combat.");
            ShowText("TeamCardSignal\uff1a\u5f53\u524d\u4e0d\u5728\u6218\u6597\u4e2d");
            return;
        }

        var now = Time.GetTicksMsec();
        var cooldownMsec = (ulong)Math.Max(1000f, settings.CooldownSeconds * 1000f);
        if (now < _nextAllowedSendTick)
        {
            Logger.Log("[Hook] TeamCardSignal skipped: cooldown active.");
            ShowText("TeamCardSignal\uff1a\u51b7\u5374\u4e2d");
            return;
        }

        var localId = run.NetService.NetId;
        if (run.HoveredModelTracker.GetHoveredModel(localId) is not CardModel card)
        {
            Logger.Log("[Hook] TeamCardSignal skipped: no hovered card.");
            ShowText("TeamCardSignal\uff1a\u8bf7\u5148\u60ac\u505c\u4e00\u5f20\u624b\u724c");
            return;
        }

        var senderName = PlatformUtil.GetPlayerName(run.NetService.Platform, localId);
        var targetName = ResolveTargetName(localId, card.CurrentTarget?.Name);

        if (run.IsSinglePlayerOrFakeMultiplayer)
        {
            // SoloOne/fake-multiplayer fallback: keep local feedback even without stable broadcast.
            ShowCardSignalPreview(card, senderName);
            _nextAllowedSendTick = now + cooldownMsec;
            Logger.Log("[Hook] TeamCardSignal local-only preview (single/fake multiplayer).");
            return;
        }

        EnsureMessageHandler(run);

        var targetId = ResolveTargetId(localId, card.CurrentTarget?.ModelId.Entry);
        var payload = BuildPayload(localId, senderName, card.Id.Entry, card.Title, targetId, targetName, now);

        try
        {
            run.NetService.SendMessage(new TeamCardSignalMessage
            {
                Payload = payload
            });
        }
        catch (Exception ex)
        {
            Logger.Log($"[Hook] TeamCardSignal send failed: {ex.Message}");
            ShowText("TeamCardSignal\uff1a\u53d1\u9001\u5931\u8d25");
            return;
        }

        ShowCardSignalPreview(card, senderName);
        _nextAllowedSendTick = now + cooldownMsec;

        Logger.Log($"[Hook] TeamCardSignal sent {payload}");
    }

}
