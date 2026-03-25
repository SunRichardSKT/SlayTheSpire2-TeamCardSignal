using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;
using System.Text.RegularExpressions;

namespace TeamCardSignal.Hooks;

internal static partial class TeamCardSignalState
{
    private const string ExclamationPath = "res://images/ui/emote/exclaim.png";
    private const string PayloadPrefix = "[TCS]|v1|";
    private const string DefaultTargetName = "\u65e0\u76ee\u6807";
    private const double SignalPreviewSeconds = 2.2;
    private const string UnknownCardDescription = "\u5361\u724c\u63cf\u8ff0\u6682\u4e0d\u53ef\u7528";
    private static readonly Regex BbCodeRegex = new(@"\[[^\]]+\]", RegexOptions.Compiled);
    private static readonly Regex MultiSpaceRegex = new(@"\s+", RegexOptions.Compiled);
    private static ulong _signalHideToken;
    private static Control? _signalOverlayRoot;
    private static PanelContainer? _signalDialogPanel;
    private static Label? _signalDialogLabel;
    private static NCard? _signalPreviewCard;

    private readonly record struct TeamSignalPayload(
        ulong SenderNetId,
        string SenderName,
        string CardId,
        string CardName,
        string TargetId,
        string TargetName,
        ulong Timestamp
    );

    private static bool TryParsePayload(string payload, out TeamSignalPayload parsed)
    {
        parsed = default;
        var parts = payload.Split('|');
        if (parts.Length != 9 || parts[0] != "[TCS]" || parts[1] != "v1")
        {
            return false;
        }

        if (!ulong.TryParse(parts[2], out var senderNetId) || !ulong.TryParse(parts[8], out var ts))
        {
            return false;
        }

        parsed = new TeamSignalPayload(
            senderNetId,
            parts[3],
            parts[4],
            parts[5],
            parts[6],
            parts[7],
            ts
        );
        return true;
    }

    private static Texture2D GetCardSignalIcon(CardModel card)
    {
        return card.Portrait ?? PreloadManager.Cache.GetTexture2D(ExclamationPath);
    }

    private static Texture2D GetCardSignalIcon(string cardId)
    {
        var model = FindCardById(cardId);
        return model?.Portrait ?? PreloadManager.Cache.GetTexture2D(ExclamationPath);
    }

    private static void ShowCardSignalPreview(CardModel card, string senderName)
    {
        var mode = TeamCardSignalSettingsStore.Current.PreviewMode;
        if (mode == TeamCardSignalPreviewMode.IconOnly)
        {
            HideSignalPreviewVisuals();
            ShowCenterSignal(GetCardSignalIcon(card));
            return;
        }

        var message = BuildSignalDialogMessage(senderName, card);
        if (!TryShowCenterCardAndDialog(card, message))
        {
            Logger.Log("[Hook] TeamCardSignal center-card preview unavailable, fallback to icon.");
            ShowCenterSignal(GetCardSignalIcon(card));
            ShowText(message);
        }
    }

    private static void ShowCardSignalPreview(string cardId, string senderName, string fallbackCardName)
    {
        var mode = TeamCardSignalSettingsStore.Current.PreviewMode;
        if (mode == TeamCardSignalPreviewMode.IconOnly)
        {
            HideSignalPreviewVisuals();
            ShowCenterSignal(GetCardSignalIcon(cardId));
            return;
        }

        var card = FindCardById(cardId);
        if (card != null)
        {
            ShowCardSignalPreview(card, senderName);
            return;
        }

        Logger.Log($"[Hook] TeamCardSignal card not found for id: {cardId}");
        var fallbackMessage = $"{senderName}\u60f3\u8981\u51fa{fallbackCardName}\uff0c?\u8d39\n\u201c{UnknownCardDescription}\u201d";
        if (!TryShowDialogOnly(fallbackMessage))
        {
            ShowText(fallbackMessage);
        }
        ShowCenterSignal(GetCardSignalIcon(cardId));
    }

    private static void HideSignalPreviewVisuals()
    {
        ++_signalHideToken;
        if (IsValidNode(_signalPreviewCard))
        {
            _signalPreviewCard!.Visible = false;
        }

        if (IsValidNode(_signalDialogPanel))
        {
            _signalDialogPanel!.Visible = false;
        }
    }

    private static void ShowCenterSignal(Texture2D? texture)
    {
        var game = NGame.Instance;
        if (game?.ReactionContainer == null)
        {
            return;
        }

        var icon = texture ?? PreloadManager.Cache.GetTexture2D(ExclamationPath);
        var center = game.GetViewport().GetVisibleRect().Size / 2f;
        game.ReactionContainer.DoLocalReaction(icon, center);
    }

    private static void ShowText(string text)
    {
        var vfx = NFullscreenTextVfx.Create(text);
        if (vfx != null && NGame.Instance != null)
        {
            NGame.Instance.AddChildSafely(vfx);
        }
    }

    private static CardModel? FindCardById(string cardId)
    {
        return ModelDb.AllCards.FirstOrDefault(c => string.Equals(c.Id.Entry, cardId, StringComparison.OrdinalIgnoreCase));
    }

    private static bool TryShowCenterCardAndDialog(CardModel sourceCard, string message)
    {
        if (!EnsureSignalOverlay())
        {
            return false;
        }

        try
        {
            var previewCard = sourceCard.MutableClone() as CardModel ?? sourceCard;
            if (!EnsureSignalCardNode(previewCard))
            {
                return false;
            }

            _signalPreviewCard!.Visible = true;
            _signalPreviewCard.MouseFilter = Control.MouseFilterEnum.Ignore;
            _signalPreviewCard.ZIndex = 90;
            _signalPreviewCard.Scale = Vector2.One * 1.35f;
            _signalPreviewCard.PivotOffset = NCard.defaultSize / 2f;
            _signalPreviewCard.Position = _signalOverlayRoot!.GetViewportRect().Size / 2f;
            _signalPreviewCard.UpdateVisuals(PileType.None, CardPreviewMode.Normal);

            _signalDialogLabel!.Text = message;
            _signalDialogPanel!.Visible = true;
            ResizeDialogPanel();
            ScheduleSignalAutoHide();
            Logger.Log($"[Hook] TeamCardSignal center-card preview opened: {previewCard.Id.Entry}");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Log($"[Hook] TeamCardSignal center-card preview failed: {ex.Message}");
            return false;
        }
    }

    private static bool TryShowDialogOnly(string message)
    {
        if (!EnsureSignalOverlay())
        {
            return false;
        }

        _signalDialogLabel!.Text = message;
        _signalDialogPanel!.Visible = true;
        ResizeDialogPanel();
        ScheduleSignalAutoHide();
        return true;
    }

    private static bool EnsureSignalOverlay()
    {
        var game = NGame.Instance;
        if (game == null)
        {
            return false;
        }

        if (!IsValidNode(_signalOverlayRoot))
        {
            _signalOverlayRoot = new Control
            {
                Name = "TeamCardSignalOverlayRoot",
                MouseFilter = Control.MouseFilterEnum.Ignore,
                ZIndex = 80
            };
            _signalOverlayRoot.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _signalOverlayRoot.SetOffsetsPreset(Control.LayoutPreset.FullRect);
            game.AddChildSafely(_signalOverlayRoot);

            _signalDialogPanel = null;
            _signalDialogLabel = null;
            _signalPreviewCard = null;
        }

        if (!IsValidNode(_signalDialogPanel))
        {
            _signalDialogPanel = new PanelContainer
            {
                Name = "TeamCardSignalDialog",
                MouseFilter = Control.MouseFilterEnum.Ignore,
                ZIndex = 85
            };
            _signalOverlayRoot!.AddChildSafely(_signalDialogPanel);
            _signalDialogPanel.Position = new Vector2(20f, 20f);

            _signalDialogLabel = new Label
            {
                Name = "TeamCardSignalDialogText",
                MouseFilter = Control.MouseFilterEnum.Ignore,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            _signalDialogLabel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _signalDialogLabel.OffsetLeft = 14f;
            _signalDialogLabel.OffsetTop = 10f;
            _signalDialogLabel.OffsetRight = -14f;
            _signalDialogLabel.OffsetBottom = -10f;
            _signalDialogPanel.AddChildSafely(_signalDialogLabel);
        }

        return IsValidNode(_signalOverlayRoot) && IsValidNode(_signalDialogPanel) && IsValidNode(_signalDialogLabel);
    }

    private static bool EnsureSignalCardNode(CardModel previewCard)
    {
        if (!IsValidNode(_signalOverlayRoot))
        {
            return false;
        }

        if (!IsValidNode(_signalPreviewCard))
        {
            _signalPreviewCard = NCard.Create(previewCard);
            if (_signalPreviewCard == null)
            {
                return false;
            }
            _signalOverlayRoot!.AddChildSafely(_signalPreviewCard);
        }
        else
        {
            _signalPreviewCard!.Model = previewCard;
        }

        return true;
    }

    private static void ResizeDialogPanel()
    {
        if (!IsValidNode(_signalOverlayRoot) || !IsValidNode(_signalDialogPanel))
        {
            return;
        }

        var viewportSize = _signalOverlayRoot!.GetViewportRect().Size;
        var width = Mathf.Clamp(viewportSize.X * 0.48f, 420f, 900f);
        _signalDialogPanel!.Size = new Vector2(width, 190f);
    }

    private static void ScheduleSignalAutoHide()
    {
        var tree = NGame.Instance?.GetTree();
        if (tree == null)
        {
            return;
        }

        var token = ++_signalHideToken;
        var timer = tree.CreateTimer(SignalPreviewSeconds);
        _ = HideSignalAfterDelay(timer, token);
    }

    private static async Task HideSignalAfterDelay(SceneTreeTimer timer, ulong token)
    {
        await timer.ToSignal(timer, SceneTreeTimer.SignalName.Timeout);
        if (token != _signalHideToken)
        {
            return;
        }

        if (IsValidNode(_signalPreviewCard))
        {
            _signalPreviewCard!.Visible = false;
        }

        if (IsValidNode(_signalDialogPanel))
        {
            _signalDialogPanel!.Visible = false;
        }
    }

    private static string BuildSignalDialogMessage(string senderName, CardModel card)
    {
        var energyCost = card.EnergyCost.CostsX
            ? "X"
            : card.EnergyCost.GetWithModifiers(CostModifiers.All).ToString();
        var description = StripCardMarkup(card.GetDescriptionForPile(PileType.Hand, card.CurrentTarget));
        if (string.IsNullOrWhiteSpace(description))
        {
            description = UnknownCardDescription;
        }

        return $"{senderName}\u60f3\u8981\u51fa{card.Title}\uff0c{energyCost}\u8d39\n\u201c{description}\u201d";
    }

    private static string StripCardMarkup(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        var noTags = BbCodeRegex.Replace(raw, " ");
        var noBreaks = noTags.Replace("\r", " ").Replace("\n", " ");
        return MultiSpaceRegex.Replace(noBreaks, " ").Trim();
    }

    private static bool IsValidNode(GodotObject? node)
    {
        return node != null && GodotObject.IsInstanceValid(node);
    }

    private static string BuildPayload(ulong senderId, string senderName, string cardId, string cardName, string targetId, string targetName, ulong ts) =>
        $"{PayloadPrefix}{senderId}|{Sanitize(senderName)}|{Sanitize(cardId)}|{Sanitize(cardName)}|{Sanitize(targetId)}|{Sanitize(targetName)}|{ts}";

    private static string ResolveTargetName(ulong senderId, string? fallback) =>
        LastTargetNames.TryGetValue(senderId, out var value)
            ? value
            : (!string.IsNullOrWhiteSpace(fallback) ? fallback : DefaultTargetName);

    private static string ResolveTargetId(ulong senderId, string? fallback) =>
        LastTargetIds.TryGetValue(senderId, out var value)
            ? value
            : (!string.IsNullOrWhiteSpace(fallback) ? fallback : "none");

    private static string Sanitize(string value) => value.Replace("|", "/").Replace("\r", " ").Replace("\n", " ").Trim();
    private static Key ResolveKey(InputEventKey keyEvent) => keyEvent.PhysicalKeycode != Key.None ? keyEvent.PhysicalKeycode : keyEvent.Keycode;
    private static Key ToKey(int raw)
    {
        var value = (long)raw;
        return Enum.IsDefined(typeof(Key), value) ? (Key)value : Key.B;
    }

    private static bool TryGetLocalNetId(out ulong localId)
    {
        if (!TryGetRun(out var run))
        {
            localId = 0;
            return false;
        }

        localId = run.NetService.NetId;
        return true;
    }

    private static bool TryGetRun(out RunManager run) => (run = RunManager.Instance) != null && run.IsInProgress;
}
