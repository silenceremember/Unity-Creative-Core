# Unity Creative Core

**Some Kind of Game Called Architecture Rendering for Unity Pathways**

A walking simulator prototype built in Unity.

> 🎮 [Play on itch.io](https://silenceremember.itch.io/) *(WebGL)*

---

## About

An experimental walking simulator that blends several mechanics together:

- 🚶 **Exploration**: first-person movement through the level with narrative triggers
- 🖱️ **Clicker**: an economic loop woven into the exploration
- 📖 **Visual Novel**: dialogue scenes with characters
- 🎨 **Quest**: painting-based tasks with XP and progression
- 🎬 **Final Sequence**: a cinematic ending

### Characters

The game features four layers of awareness, each character knows a different slice of the truth:

| Character | What they know |
|---|---|
| **Mary** | Unaware of the Narrator. Doesn't know where she is or what the Player is. |
| **John** | Can hear the Narrator, but knows nothing about the Developer. Tries his best to play the role he was given. |
| **The Narrator** | Knows about Mary, John, and the Developer. Doesn't quite understand who the Player is or why they're here. Does his best to entertain them with whatever tools he has. |
| **The Player** | Just a player. |

Solo-developed from concept to publication on itch.io as a proof of concept.

---

## Project Structure

```
Assets/_Project/
├── Audio/          # Sound & music
├── Models/         # 3D models
├── Plugins/        # Plugins
├── Prefabs/        # Prefabs
├── SO/             # ScriptableObject configs
├── Scenes/         # Scenes
├── Scripts/        # Source code (C#)
│   ├── Channels/       # ScriptableObject event channels
│   ├── Core/           # Configs, game state, utilities
│   ├── Dialogue/       # Narrator dialogue system
│   ├── Editor/         # Dialogue builder editor tools
│   ├── Exploration/    # Exploration & clicker mechanics
│   ├── Final/          # Final sequence
│   ├── Intro/          # Intro (camera transition, crawl)
│   ├── Player/         # Player controller, footsteps, input
│   ├── Quest/          # Painting quests, XP, levels
│   ├── UI/             # Menus, pause, HUD, audio settings
│   └── VisualNovel/    # Visual novel system
├── Sprites/        # 2D graphics
└── VFX/            # Visual effects
```

---

## Tech Stack

| | |
|---|---|
| **Engine** | Unity (URP) |
| **Language** | C# |
| **Platforms** | PC Windows / WebGL |
| **Localization** | Russian / English |

---

## Getting Started

1. Clone the repository
2. Open the project in Unity (version 6+ recommended)
3. Open a scene from `Assets/_Project/Scenes/`
4. Press ▶️ Play

---

## License

[![CC0](https://licensebuttons.net/p/zero/1.0/88x31.png)](http://creativecommons.org/publicdomain/zero/1.0/)

This project is dedicated to the **public domain** under the [CC0 1.0 Universal](LICENSE) license.

You are free to copy, modify, distribute, and use this code for any purpose, including commercial, with no restrictions and no attribution required.
