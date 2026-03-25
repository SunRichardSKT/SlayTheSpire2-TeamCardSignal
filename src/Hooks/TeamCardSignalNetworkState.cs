using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace TeamCardSignal.Hooks;

internal static partial class TeamCardSignalState
{
    private static INetGameService? _registeredNetService;

    public static void OnCombatStarted()
    {
        if (!TryGetRun(out var run))
        {
            return;
        }

        EnsureMessageHandler(run);
    }

    public static void OnCombatEnded()
    {
        ClearMessageHandler();
        LastTargetNames.Clear();
        LastTargetIds.Clear();
        HideSignalPreviewVisuals();
        CleanupSettingsWindow();
    }

    private static void OnSignalMessageReceived(TeamCardSignalMessage message, ulong senderId)
    {
        if (string.IsNullOrWhiteSpace(message.Payload) || !message.Payload.StartsWith(PayloadPrefix, StringComparison.Ordinal))
        {
            return;
        }

        if (!TryGetRun(out var run) || run.IsSinglePlayerOrFakeMultiplayer || NCombatRoom.Instance == null)
        {
            return;
        }

        if (run.NetService.NetId == senderId)
        {
            return;
        }

        if (!TryParsePayload(message.Payload, out var parsed))
        {
            Logger.Log($"[Hook] TeamCardSignal skipped invalid payload: {message.Payload}");
            return;
        }

        Logger.Log($"[Hook] TeamCardSignal received {message.Payload}");
        ShowCardSignalPreview(parsed.CardId, parsed.SenderName, parsed.CardName);
    }

    private static void EnsureMessageHandler(RunManager run)
    {
        var netService = run.NetService;
        if (ReferenceEquals(_registeredNetService, netService))
        {
            return;
        }

        if (_registeredNetService != null)
        {
            try
            {
                _registeredNetService.UnregisterMessageHandler<TeamCardSignalMessage>(OnSignalMessageReceived);
            }
            catch (Exception ex)
            {
                Logger.Log($"[Hook] TeamCardSignal unbind previous handler failed: {ex.Message}");
            }
        }

        try
        {
            netService.RegisterMessageHandler<TeamCardSignalMessage>(OnSignalMessageReceived);
            _registeredNetService = netService;
            Logger.Log("[Hook] TeamCardSignal network handler registered.");
        }
        catch (Exception ex)
        {
            _registeredNetService = null;
            Logger.Log($"[Hook] TeamCardSignal register handler failed: {ex.Message}");
        }
    }

    private static void ClearMessageHandler()
    {
        if (_registeredNetService == null)
        {
            return;
        }

        try
        {
            _registeredNetService.UnregisterMessageHandler<TeamCardSignalMessage>(OnSignalMessageReceived);
            Logger.Log("[Hook] TeamCardSignal network handler unregistered.");
        }
        catch (Exception ex)
        {
            Logger.Log($"[Hook] TeamCardSignal unregister handler failed: {ex.Message}");
        }
        finally
        {
            _registeredNetService = null;
        }
    }
}
