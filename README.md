# FireballOS

FireballOS is the in-game desktop operating system for a Unity game I'm building with a teammate. In the game, the player sits at a computer, and FireballOS is what runs on its screen.

**My part:** the OS layer: login screen, desktop, windows, taskbar, start menu, system UI, and the built-in apps.
**My teammate's part:** the game itself (gameplay, story, and the world around the computer).

This repository contains only the OS layer.

> Work in progress. The project is under active development, so features and structure may change.

## Features

### Desktop and window system

- **Login screen** to enter the desktop session
- **Windows** that can be dragged, resized, opened, and closed
- **Taskbar** that shows open windows and lets the player switch between them
- **Start menu** for launching apps, locking the screen, and logging off
- **Desktop icons** that open apps
- **System tray** with a sound menu and a live clock
- **System sounds** for OS events

### Built-in apps

| App | Status |
| --- | --- |
| Paint | Working (basic canvas drawing) |
| Minesweeper | Working |
| Notepad | Planned |
| Web browser | Planned |
| This PC (file explorer) | Planned |

## Project structure

```text
FireballOS/FireballOS/
├── Art/            # Backgrounds and icons
├── Prefabs/        # Reusable UI elements (taskbar items, Minesweeper cells)
└── Scripts/
    ├── Core/
    │   ├── Window.cs, WindowDragger.cs, WindowResizer.cs   # Window behavior
    │   ├── TaskbarManager.cs, TaskbarItem.cs               # Taskbar
    │   ├── DesktopIcon.cs                                  # Desktop shortcuts
    │   ├── Controllers/    # Login screen, start menu, sound menu
    │   └── Audio/          # System sounds
    ├── Apps/
    │   ├── PaintCanvas.cs
    │   └── Minesweeper/
    └── SystemClock.cs
```

Each app is self-contained, so new apps can be added without changing the window or taskbar code.

## Tech stack

- **Engine:** Unity
- **Language:** C#
