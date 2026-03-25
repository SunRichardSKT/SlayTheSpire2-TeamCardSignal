using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace TeamCardSignal.Hooks;

[HarmonyPatch(typeof(NCombatUi), nameof(NCombatUi._Input))]
public static class TeamCardSignalCombatInputPatch
{
    public static void Postfix(InputEvent inputEvent)
    {
        TeamCardSignalState.HandleCombatInput(inputEvent);
    }
}

[HarmonyPatch(typeof(NGame), nameof(NGame._Input))]
public static class TeamCardSignalGameInputPatch
{
    public static void Postfix(InputEvent inputEvent)
    {
        TeamCardSignalState.HandleCombatInput(inputEvent);
    }
}

[HarmonyPatch(typeof(NRun), nameof(NRun._Process))]
public static class TeamCardSignalRunProcessPatch
{
    public static void Postfix(double delta)
    {
        TeamCardSignalState.HandleCombatFrameInput();
    }
}

[HarmonyPatch(typeof(NTargetManager), nameof(NTargetManager._Input))]
public static class TeamCardSignalTargetManagerInputPatch
{
    public static void Postfix(InputEvent inputEvent)
    {
        TeamCardSignalState.HandleCombatInput(inputEvent);
    }
}

[HarmonyPatch(typeof(NMouseCardPlay), nameof(NMouseCardPlay._Input))]
public static class TeamCardSignalMouseCardPlayInputPatch
{
    public static void Postfix(InputEvent inputEvent)
    {
        TeamCardSignalState.HandleCombatInput(inputEvent);
    }
}

[HarmonyPatch(typeof(NControllerCardPlay), nameof(NControllerCardPlay._Input))]
public static class TeamCardSignalControllerCardPlayInputPatch
{
    public static void Postfix(InputEvent inputEvent)
    {
        TeamCardSignalState.HandleCombatInput(inputEvent);
    }
}
