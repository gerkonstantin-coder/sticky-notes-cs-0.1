# Sticky Notes (C# version)

Classic sticky notes app for Windows - Native C# version, no Python required!

## Features

- No Python required - written in C#
- Stylish design - bright colors, soft shadows
- Always on top mode
- Desktop pinning mode
- Smart buttons - auto-hide after 3 seconds
- 6 color themes
- Auto-save notes
- Drag and resize

## Installation

### Option 1: Compile from source

Requirements:
- Windows 7/8/10/11
- .NET Framework 3.5+ (built into Windows 10/11)

Steps:
```
1. Download or clone this repository
2. Run compile.bat
3. Get output\StickyNotes.exe
```

### Option 2: Build installer

Requirements:
- Inno Setup 6 (download from https://jrsoftware.org/isdl.php)

Steps:
```
1. Run build_installer.bat
2. Get Output\StickyNotesSetup.exe
```

## Controls

| Button | Function |
|--------|----------|
| + | New note |
| pin | Pin to desktop |
| up | Always on top |
| palette | Change color |
| X | Close |

### Actions

- Drag header to move
- Drag bottom-right corner to resize
- Double-click header to minimize
- Right-click for context menu

## Colors

Yellow, Green, Blue, Pink, Orange, Purple

## Data Storage

```
%APPDATA%\StickyNotes\notes.json
```

## System Requirements

- Windows 7 SP1 / 8 / 10 / 11
- .NET Framework 3.5+ (built into Windows 10/11)
- Python NOT required!

## License

MIT License
