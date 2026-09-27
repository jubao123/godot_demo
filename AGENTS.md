# Repository Guidelines

## Project Structure & Module Organization

This is a GDScript-based 2D Godot game. `project.godot` declares Godot 4.7 and the Forward Plus renderer.

- `Scenes/Game.tscn` is the main scene; player, slime, bullet, and background music scenes live alongside it.
- `Script/` contains gameplay scripts: `player.gd`, `enemy.gd`, `bullet.gd`, and `gameManager.gd` (spawning, score, and game-over UI).
- `AssetBundle/` holds sprites, audio, a font, and accompanying notices.
- `Build/` contains Windows exports; `export_presets.cfg` defines the export preset.
- `.godot/` is generated editor/import data and is ignored. No test directory is present.

## Build, Test, and Development Commands

Run from the repository root using a compatible Godot executable. These examples assume it is available as `godot` on PATH; otherwise use its full path (with PowerShell's `&` operator).

- `godot --editor --path .` opens the project for editing.
- `godot --path .` launches the main scene; F6 runs the current scene in the editor, and F5 runs the project.
- `godot --headless --path . --editor --import` imports assets and exposes loading or script errors.
- `godot --headless --path . --export-release "Windows Desktop" Build/demo02.exe` creates the Windows release export and requires matching export templates. It replaces the existing output.

## Coding Style & Naming Conventions

Use UTF-8, as specified by `.editorconfig`, and tab indentation to match existing GDScript. Prefer typed parameters, return values, and exported properties. Use `snake_case` for new functions and variables; preserve existing file casing and serialized property names unless updating every reference. Keep signal handlers consistent with `_on_*` naming. No formatter or linter is configured.

## Testing Guidelines

No automated framework or coverage target is configured. After changes, run the import check and play `Scenes/Game.tscn`. Verify WASD movement, firing while stationary, bullet collisions and scoring, slime spawning, game-over behavior, automatic restart, and audio. Check the debugger for errors. Headless import does not validate gameplay or rendering.

## Commit & Pull Request Guidelines

Git history is unavailable in this checkout, so no established commit convention can be verified. Use concise, imperative messages such as `Fix slime collision scoring`. Keep changes focused. PRs should explain behavior changes, reference relevant issues, list validation performed, and include screenshots or short recordings for visual changes.

## Resource Maintenance

Preserve `.gd.uid` files and asset `.import` sidecars. Use the editor to move resources and update references. Retain asset notices, and avoid unrelated generated cache or build changes.
