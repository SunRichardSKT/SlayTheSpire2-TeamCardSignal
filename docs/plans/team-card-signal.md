# TeamCardSignal Implementation Plan

## Goal
- In multiplayer combat, press configurable hotkey (default `B`) to broadcast "planned card" signal to teammates.
- Signal includes:
  - center-screen icon (card portrait first, fallback exclamation icon),
  - center-screen text: player + card + target.
- 1 second anti-spam cooldown.

## Verified Hooks / APIs (from decompilation)
- `Hook.BeforeCombatStart(IRunState, CombatState?)`
- `Hook.AfterCombatEnd(IRunState, CombatState?, CombatRoom)`
- `Hook.BeforeCardPlayed(CombatState, CardPlay)`
- `Hook.AfterCardPlayed(CombatState, PlayerChoiceContext, CardPlay)`
- `NCombatUi._Input(InputEvent)`
- `NTargetManager.OnCreatureHovered(NCreature)`
- `NTargetManager.OnCreatureUnhovered(NCreature)`
- `INetGameService.SendMessage<T>()`
- `INetGameService.RegisterMessageHandler<T>()`
- `INetGameService.UnregisterMessageHandler<T>()`

## Protocol
- Payload string format:
  - `[TCS]|v1|senderNetId|senderName|cardId|cardName|targetId|targetName|ts`
- Transport:
  - custom reliable net message `TeamCardSignalMessage : INetMessage`.

## Runtime Modules
- Input module:
  - process `V` send;
  - `F7` toggle enable;
  - `F8` capture next key for hotkey.
- State module:
  - cooldown tracking;
  - local hovered target cache.
- Network module:
  - register/unregister message handler on combat start/end;
  - send and receive payload.
- UI module:
  - show center text;
  - show icon with card portrait priority.
- Settings module:
  - persist `%APPDATA%/SlayTheSpire2/mods/TeamCardSignal/settings.json`.

## Test Checklist
- Build + install succeeds (`install-mod.bat`).
- Singleplayer combat hotkey does not send (log reason).
- Cooldown blocks repeated send within 1 second.
- No hovered card logs readable skip reason.
- Multiplayer combat send logs `[Hook] TeamCardSignal sent [TCS]|...`.
- Receiver logs `[Hook] TeamCardSignal received [TCS]|...` and sees center prompt.
