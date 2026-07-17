# Agent notes — Codex Dream Skin APP

## Product scope

- This repository ships the Windows x64 WinForms application in `windows/studio/`.
- Users launch `CodexDreamSkinStudio.exe`; internal PowerShell files are implementation details and must not be documented as a separate user workflow.
- Keep the external theme model based on loopback CDP. Never modify WindowsApps, `app.asar`, official binaries, or signatures.
- Preserve exact appearance backup/restore behavior and keep pet or auxiliary windows excluded from theme injection.

## Changelog and versioning

- Update the root `CHANGELOG.md` for every user-visible feature, UX, safety, packaging, or compatibility change.
- Keep the application version in `windows/studio/CodexDreamSkinStudio.csproj` synchronized with release tags.
- Use prerelease versions while the application is in preview, for example `0.1.0-preview.1` and tag `v0.1.0-preview.1`.

## Validation

- Build the Release x64 configuration.
- Run `windows/studio/tests/appearance-roundtrip.test.ps1`.
- Run `windows/studio/tests/pet-overlay.test.mjs`.
- For UI changes, inspect all four Studio navigation pages and the minimum/high-DPI layout.

## Licensing

- `windows/studio/LICENSE` applies only to original Studio UI source, excluding `engine/`, themes, images, and generated build output.
- Preserve upstream attribution and the scope notes in `OPEN_SOURCE_NOTICE.md`.
