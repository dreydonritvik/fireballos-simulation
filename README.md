# FireballOS

FireballOS is the in-game desktop operating system for a Unity game I'm building with a teammate. In the game, the player sits at a computer, and FireballOS is what runs on its screen.

- **My part:** the OS layer: login screen, desktop, window system, taskbar, start menu, system tray, and built-in apps.
- **My teammate's part:** the game itself (gameplay, story, and the world around the computer).

This repository contains only the OS layer. It talks to the game through a small bridge interface (`OsBridgeLocator` / `OsSignal`), which lives in the game project.

> **Work in progress.** Features and structure may still change.

## Features

### Login screen
- Shown first whenever the OS starts or the player logs off
- `Enter` logs in, `Esc` leaves the computer screen
- When the player leaves the screen, FireballOS sends an `exit_screen` signal to the game through the bridge

### Window system
- Open, close, minimize, and maximize/restore windows
- Drag windows by the title bar (disabled while maximized)
- Resize windows from the corner, within a minimum and maximum size
- Clicking a window brings it to the front
- Windows can be configured to open maximized

### Taskbar
- Shows a button with an icon and title for every open window
- Clicking a button works like Windows: restores a minimized window, brings a background window to the front, or minimizes the window that is already in front

### Desktop, start menu, and system tray
- **Desktop icons** open apps on double-click
- **Start menu** opens from the start button and closes with `Esc` or a click outside
- **Sound menu** with a volume slider and mute button; settings are saved between sessions
- **Clock** showing the current time

### System sounds
- A central `SystemSoundManager` plays click, error, and notification sounds at the system volume
- OS sounds are kept separate from in-game website audio

## Built-in apps

| App | Status | Details |
| --- | --- | --- |
| Minesweeper | Working | 9×9 grid with 10 mines, safe first click, flags, mine counter, timer, and win/lose states. Empty areas are revealed with a BFS flood fill. |
| Paint | Working | Draws on a pixel canvas with a round brush; fast strokes are connected with Bresenham's line algorithm. Includes a clear-canvas action. |
| Notepad | Planned | |
| Web browser | Planned | |
| This PC (file explorer) | Planned | |

## Project structure

```text
FireballOS/FireballOS/
├── Art/                 # Wallpapers and icons
├── Prefabs/             # Taskbar item and Minesweeper cell prefabs
├── SampleScene.unity    # Demo scene with the full desktop
└── Scripts/
    ├── Core/
    │   ├── Window.cs            # Open, close, minimize, maximize, focus
    │   ├── WindowDragger.cs     # Dragging by the title bar
    │   ├── WindowResizer.cs     # Resizing with size limits
    │   ├── TaskbarManager.cs    # Adds and removes taskbar buttons
    │   ├── TaskbarItem.cs       # Taskbar button behavior
    │   ├── DesktopIcon.cs       # Double-click to open
    │   ├── Controllers/         # Login screen, start menu, sound menu
    │   └── Audio/               # SystemSoundManager
    ├── Apps/
    │   ├── PaintCanvas.cs
    │   └── Minesweeper/         # Game logic and cells
    └── SystemClock.cs
Fonts/                   # UI and symbol fonts
```

A window only needs a `Window` component to work with the taskbar, so new apps can be added without changing the window or taskbar code.

## Tech stack

- **Engine:** Unity (uGUI, TextMeshPro, Input System)
- **Language:** C#

## Running

1. Copy the `FireballOS` and `Fonts` folders into the `Assets` folder of a Unity project.
2. Install the **TextMeshPro** and **Input System** packages.
3. Open `SampleScene.unity` and press Play.

`LoginScreenController.cs` uses the bridge interface (`DeadFrame.Contracts`) from the game project. To run FireballOS on its own, add a stub for that interface or remove the bridge call.
