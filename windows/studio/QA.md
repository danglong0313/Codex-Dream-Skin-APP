# QA inventory

## User-visible claims

1. The Codex 26.715+ start screen visibly matches the reference mood: a wide aqua/pink Miku music hero sits behind the native welcome title and composer without blocking interaction.
2. The native Chat / Work switch has distinct aqua and lavender active states. Chat keeps its compact single-line composer; Work keeps its multiline composer plus project/plugin utility bar.
3. The Codex workspace home remains a separate four-action layout with one centered welcome title/subtitle, a themed project composer, and no duplicate Chat/Work decorations.
4. The sidebar is aqua glass rather than merely changing the accent color; its ChatGPT / Codex mode switch text matches the theme while the bottom account avatar remains native. Pull Requests has its own blue navigation treatment.
5. All real Codex controls remain interactive; the skin is not a screenshot overlay.
6. The skin survives Chat / Work changes, route changes, and renderer reloads while the injector daemon runs without periodically removing/readding theme classes.
7. The official Store package and `app.asar` remain unchanged.
8. Restore removes the injected DOM/CSS and install/restore can be repeated.

## Functional checks

- Studio: launch the Windows preview, confirm the Miku gallery renders, status refreshes, minimize-to-tray works, and Verify reports success without changing the active theme.
- Appearance restore: start from custom, default, and absent appearance keys; apply twice; restore once; confirm the exact pre-apply values/presence return and the snapshot is not replaced by Miku values.
- Chat / Work: switch both ways and confirm the active mode styling follows the native `aria-pressed` state.
- Workspace switch: change between ChatGPT and Codex, confirm each home uses only its own layout rules, then wait through one watchdog interval and confirm there is no mode-switch flash.
- Codex pet: show a pet and confirm its auxiliary window stays transparent with no Miku wallpaper, surface color, chrome, or polaroid decoration behind the sprite.
- Work project selector: click the real project chip under the composer and confirm the native project menu opens.
- Sidebar: open a real task, then return to New Chat / New Task.
- Composer: type text, verify caret/readability, then clear it without sending.
- Reload: use CDP `Page.reload`, wait, and confirm the injection marker returns.
- Restore/reapply cycle: remove live skin, verify marker absent, apply again, verify marker present.
- Update resilience: resolve the current `OpenAI.Codex` Appx location dynamically; never store a versioned WindowsApps path.

## Visual checks

- 1280x820 Chat start: hero, welcome title, compact composer, sidebar, and Chat / Work switch are visible without horizontal scrolling.
- 1280x820 Work start: hero, welcome title, multiline composer, project/plugin utility bar, sidebar, and Chat / Work switch remain visible.
- 1280x820 Codex home: centered hero copy, four separated native action cards, project composer, and Codex sidebar navigation are visible without duplicate subtitles.
- Narrower window: the hero and composer shrink with the native layout; no essential control is covered and optional header branding may hide.
- Normal task: messages remain readable and composer does not overlap content.
- Inspect the sidebar, workspace brand, bottom status row, header mode switch, hero edges, welcome title, composer controls, Work utility bar, model/permission menus, and scrollbar.
- Reject black/transparent sidebar artifacts, clipped hero art, disconnected project labels, rasterized native controls, weak contrast, or decorations intercepting clicks.

## Exploratory checks

- Start when the debug port is occupied: fail with a clear message or use a caller-selected port.
- Start after Codex updates: package discovery and injection still work without patching installed files.
