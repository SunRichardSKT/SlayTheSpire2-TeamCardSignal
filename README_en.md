# TeamCardSignal

`TeamCardSignal` is a multiplayer intent-signaling mod for **Slay the Spire 2**.  
When the player presses a hotkey, teammates get a clear preview of the card the player is about to use.

This repository was originally based on `Sts2ModScaffold` and is now fully renamed/isolated as `TeamCardSignal` (mod id, csproj, manifest, outputs).

## Features

- Send card intent in multiplayer combat (default hotkey: `B`)
- Local player also receives feedback
- UI feedback style:
  - Center screen: card preview
  - Top-left dialog text:  
    `XXX wants to play XX, X cost`  
    `"Card description"`
- 1-second anti-spam cooldown
- Persistent settings (enable + hotkey + cooldown)
- Works with SoloOne fake-multiplayer mode for local preview

## Hotkeys

- `B` (default): send current hovered hand card intent
- `F7`: open/close settings popup

Settings popup supports:

1. Enable/disable signaling
2. Switch render mode (`IconOnly` / `FullCard`)
3. Rebind signal hotkey (press any key, `Esc` to cancel)

## Settings File

Saved automatically at:

`%AppData%\SlayTheSpire2\mods\TeamCardSignal\settings.json`

Fields:

- `Enabled`
- `Hotkey` (Godot Key enum int)
- `CooldownSeconds` (default `1.0`)

## Message Protocol

Custom network payload:

`[TCS]|v1|senderNetId|senderName|cardId|cardName|targetId|targetName|ts`

## Project Structure

```text
TeamCardSignal/
|- src/
|  |- Hooks/
|  |  |- TeamCardSignalInputHooks.cs
|  |  |- TeamCardSignalFrameInputState.cs
|  |  |- TeamCardSignalState.cs
|  |  |- TeamCardSignalStateHelpers.cs
|  |  |- TeamCardSignalNetworkState.cs
|  |  |- TeamCardSignalTargetHooks.cs
|  |  |- TeamCardSignalSettings.cs
|  |  `- TeamCardSignalMessage.cs
|  |- ModEntry.cs
|  |- TeamCardSignal.csproj
|  `- TeamCardSignal.json
|- docs/
|  `- plans/
|- tools/
|- install.bat
|- install-mod.bat
`- uninstall-mod.bat
```

## Local Development

### 1. First-time environment setup

```powershell
.\install.bat
```

### 2. Build and install mod

```powershell
.\install-mod.bat
```

Or with explicit game path:

```powershell
.\install-mod.bat "D:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2"
```

### 3. Read logs

```powershell
.\tools\read_sts2_logs.ps1
```

## Before Pushing To GitHub

- Do not commit local-only content (already covered by `.gitignore`):
  - `references/`
  - downloaded tool payloads under `tools`
  - `src/bin`, `src/obj`
  - `.env`, `opencode.jsonc`
- If you previously renamed from scaffold defaults, files like `Sts2ModScaffold.deps.json` are legacy build artifacts. This repo is cleaned; fresh builds should output only `TeamCardSignal.*`.
- Suggested preflight:

```powershell
git init
git add .
git status
```

Check tracked files before your first commit.

## Version & Release

- Current version: `0.2.0`
- Changelog: [CHANGELOG.md](CHANGELOG.md)
- Release note template: [release-v0.2.0.md](docs/release/release-v0.2.0.md)
- Publish checklist: [publish-checklist.md](docs/release/publish-checklist.md)

## License

MIT
