# Changelog

All notable changes to this project are documented in this file.

## [0.2.0] - 2026-03-23

### Added
- F7 settings popup in combat.
- In-popup options:
  - Enable/disable signaling.
  - Render mode switch (`IconOnly` / `FullCard`).
  - Signal hotkey rebinding.
- Persistent `PreviewMode` setting with migration guard.
- New release documentation:
  - Chinese README refresh.
  - `docs/release/release-v0.2.0.md`.

### Changed
- Default signal key remains `B`; settings management moved to F7 popup.
- Removed old F8 rebind flow.
- Receiver-side rendering is now strictly local-setting driven:
  - Sender mode does not affect how receiver renders.
- UI behavior in `FullCard` mode:
  - Center card preview + top-left dialog text.

### Fixed
- Better combat lifecycle cleanup:
  - Signal preview UI is hidden on combat end.
  - Settings popup is closed on combat end.

## [0.1.0] - 2026-03-23

### Added
- Initial TeamCardSignal multiplayer signaling core.
- Chat/network payload protocol:
  - `[TCS]|v1|senderNetId|senderName|cardId|cardName|targetId|targetName|ts`
- Cooldown protection and single/fake-multiplayer fallback.
