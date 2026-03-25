using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace TeamCardSignal.Hooks;

[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCombatStart))]
public static class TeamCardSignalBeforeCombatStartPatch
{
    public static void Postfix(IRunState runState, CombatState? combatState)
    {
        TeamCardSignalState.OnCombatStarted();
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCombatEnd))]
public static class TeamCardSignalAfterCombatEndPatch
{
    public static void Postfix(IRunState runState, CombatState? combatState, CombatRoom room)
    {
        TeamCardSignalState.OnCombatEnded();
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.BeforePlayPhaseStart))]
public static class TeamCardSignalBeforePlayPhaseStartPatch
{
    public static void Postfix(CombatState combatState, Player player)
    {
        TeamCardSignalState.OnCombatStarted();
    }
}

[HarmonyPatch(typeof(NTargetManager), "OnCreatureHovered")]
public static class TeamCardSignalCreatureHoveredPatch
{
    public static void Postfix(NCreature creature)
    {
        TeamCardSignalState.OnLocalTargetHovered(creature);
    }
}

[HarmonyPatch(typeof(NTargetManager), "OnCreatureUnhovered")]
public static class TeamCardSignalCreatureUnhoveredPatch
{
    public static void Postfix(NCreature creature)
    {
        TeamCardSignalState.OnLocalTargetUnhovered();
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCardPlayed))]
public static class TeamCardSignalBeforeCardPlayedPatch
{
    public static void Postfix(CombatState combatState, CardPlay cardPlay)
    {
        var ownerName = cardPlay.Card.Owner?.Creature?.Name ?? "Unknown";
        var targetName = cardPlay.Target?.Name ?? "\u65e0\u76ee\u6807";
        Logger.Log($"[Hook] BeforeCardPlayed owner={ownerName} card={cardPlay.Card.Title} target={targetName}");
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
public static class TeamCardSignalAfterCardPlayedPatch
{
    public static void Postfix(CombatState combatState, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMe(cardPlay.Card.Owner))
        {
            TeamCardSignalState.OnLocalTargetUnhovered();
        }
    }
}
