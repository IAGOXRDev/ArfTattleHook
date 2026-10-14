# 🦭 ArfTattleHook

Just some modules i made for Tattletail

![Game](https://img.shields.io/badge/Game-Tattletail-emerald?style=for-the-badge)
![MelonLoader](https://img.shields.io/badge/Loader-MelonLoader%20v0.5.7+-red?style=for-the-badge)
![Runtime](https://img.shields.io/badge/Runtime-.NET%203.5%20(Mono)-blue?style=for-the-badge)
![License](https://img.shields.io/badge/License-MIT-purple?style=for-the-badge)

---

## Features

### Movement
- **Auto Bhop:** Automatically hops upon landing when holding `[Space]`. Custom jump velocity and gravity parameters prevent hitting low ceilings in the house.
- **Air Strafe & Momentum:** Source-engine styled air strafing with configurable acceleration and air friction, allowing smooth speed accumulation.
- **Speedometer HUD:** Clean on-screen digital HUD displaying real-time horizontal velocity in units per second.

### Visuals
- **Field of View (FOV) Slider:** Smoothly adjust player FOV between 35° and 130° (default 85°).
- **Quick Zoom:** Hold `[C]` to zoom in with customizable FOV (default 30°) and smooth camera interpolation.
- **Clean Fullbright:** Illuminates the house with a balanced point light and ambient lighting, avoiding blinding bloom or washed-out post-processing.
- **Disable Bloom:** Option to automatically disable blinding post-effects when Fullbright is active.

### World Utilities
- **God Mode:** Complete invulnerability against Mama Tattletail and death triggers.
- **Hacker Mode:** Matrix cheat macro combining God Mode, Instant Interact, and a fast 6.0 sprint speed.
- **Infinite Flashlight:** Locks flashlight battery at 100% capacity without needing to shake it.
- **Instant Interact (No Hold Time):** Removes interaction delay timers on items, chargers, doors, and tasks.
- **Quick Actions:**
  - *Recharge Flashlight:* Instantly restores flashlight battery to 100%.
  - *Silence Tattletail:* Instantly quiet down noisy Tattletail (`ShutUp`).
  - *Complete Current Night:* Instantly triggers level completion logic.

### Developer & Debug
- **Official In-Game Console:** Instant shortcut to open Tattletail's hidden native developer `GameConsole`.
- **Level Warper (Night Selector):** Jump directly to any night (Night 1 to 8 / Christmas) or the Kaleidoscope DLC:
  - *Night 1: A Night to Remember*
  - *Night 2: Minor Mischief*
  - *Night 3: Mama's Boy*
  - *Night 4: Funny Games*
  - *Night 5: Video Horror System*
  - *Night 6: The Party*
  - *Night 7: There She Is*
  - *Night 8: Christmas*
  - *DLC: Kaleidoscope*
- **Easter Egg Unlocker:** Unlock all 22 collectible eggs instantly or wipe egg save data.
- **Mama & Jumpscare Controls:** Test jumpscares, trigger quick Mama attacks, or completely toggle Mama's jumpscares on/off.
- **Environment & Patrol Visualizer:** Toggle 3D visualizers for Mama's patrol waypoint nodes, hide the house roof (`Roof_TOP`), or activate clean Screenshot Mode (`[T]`).
- **Spoof 100% Game Beaten:** Simulates completed save game status and bypasses TV commercials.

---

## Keybindings

| Key / Action | Description |
| :--- | :--- |
| **`[Insert]`** or **`[F1]`** | Toggle the Mod Menu GUI on / off |
| **`Drag ◢ (Bottom-right)`** | Resize the Mod Menu window (persists across launches) |
| **`[Space] (Hold)`** | Continuous Bunny Hop with air strafing |
| **`[C] (Hold)`** | Camera Zoom (configurable FOV) |
| **`[~] + [=] + [Tab]`** | Native developer console toggle |
| **`[T]`** | Capture screenshot (when Screenshot Mode is active) |

---

## Installation

1. Install [MelonLoader](https://melonloader.com/) (v0.5.7 recommended for Unity Mono games) into your **Tattletail** game directory.
2. Download the latest `ArfTattleHook.dll` from the [Releases](https://github.com/) tab.
3. Place `ArfTattleHook.dll` inside the `Mods` folder:
   ```text
   ...\Tattletail\Mods\ArfTattleHook.dll
   ```
4. Launch the game and press **`[Insert]`** or **`[F1]`** to open the menu.

---

## 💻 Building from Source

### Requirements
- .NET SDK (with support for targeting `net35`).
- Visual Studio 2022 or VS Code.
- Game assemblies from Tattletail:
  - `...\Tattletail\tattletailWindows_Data\Managed\`
  - `...\Tattletail\MelonLoader\`

### Compilation
```bash
git clone https://github.com/YOUR_USERNAME/ArfTattleHook.git
cd ArfTattleHook
dotnet build -c Release
```
The compiled library will be output to `bin/Release/net35/ArfTattleHook.dll`.

---
