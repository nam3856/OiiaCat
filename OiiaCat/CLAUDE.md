# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**OiiaCat** is a Unity 6 desktop application featuring an animated character that responds to global keyboard and mouse input on Windows. The app displays an interactive character that animates when user input is detected, with options for window transparency and "always on top" behavior.

### Core Features
- **Global Input Detection**: Uses Windows low-level hooks to detect keyboard and mouse input system-wide (not just when the app is focused)
- **Character Animation**: Plays animated GIF-style character animations triggered by input activity
- **Window Control**: Transparent window mode and "always on top" functionality using UniWindowController
- **Activity Counter**: Tracks and displays total input activity count

## Technology Stack

- **Unity Version**: 6000.0.60f1
- **Rendering Pipeline**: Universal Render Pipeline (URP) for 2D
- **Input System**: New Unity Input System (com.unity.inputsystem@1.14.2)
- **Language**: C# 9.0
- **Platform**: Windows-specific (uses Win32 API via P/Invoke)
- **External Dependencies**:
  - `UniWindowController` (com.kirurobo.uniwinc) - Window transparency and positioning
  - TextMesh Pro - UI text rendering
  - Unity 2D packages (Animation, Sprite Shape, Tilemap, Pixel Perfect)

## Architecture

### Core Components

#### `GlobalInputActivityDetector_Windows.cs`
**Purpose**: System-wide input activity detection using Windows hooks

**Key Features**:
- Uses Win32 low-level keyboard and mouse hooks (`WH_KEYBOARD_LL`, `WH_MOUSE_LL`)
- Filters keyboard repeat events (only counts initial key press, not auto-repeat)
- Thread-safe activity counting using `Interlocked` operations
- Lock-free architecture: Hook callbacks run on separate thread, main thread processes events in `Update()`

**Critical Implementation Details**:
- `_pressedKeys` HashSet tracks currently pressed keys to filter repeats
- `_pendingActivityCount` uses atomic operations for thread safety
- `OnActivity` event with `uint` counter is invoked on main thread
- Hooks are properly cleaned up in `OnDisable()` and `OnApplicationQuit()`

**Thread Safety**: All hook callbacks run on a Windows thread separate from Unity's main thread. Use `Interlocked` operations for any shared state.

#### `GifBurstPlayer.cs`
**Purpose**: Controls animator playback based on input activity

**Key Features**:
- Subscribes to `GlobalInputActivityDetector_Windows.OnActivity` events
- Plays animation for configurable duration after last input
- Returns to idle/default state after timeout
- Optional: restart animation from first frame on re-trigger

**Configuration**:
- `playDuration`: How long to continue animation after last input (default 0.5s)
- `restartFromFirstFrameOnTrigger`: Whether to reset animation to frame 0 when triggered again
- `useUnscaledTime`: Use `Time.unscaledTime` instead of `Time.time` for independent timing

#### `Settings.cs`
**Purpose**: Manages window settings via UniWindowController

**Key Features**:
- Controls `isTopmost` (always on top) via toggle
- Controls background visibility for transparent mode via toggle
- Uses `UniWindowController.current` singleton

#### `CounterText.cs`
**Purpose**: Displays activity count in UI

**Key Features**:
- Subscribes to activity events and updates TextMeshProUGUI
- Simple integer counter display

#### `QuitGame.cs`
**Purpose**: Application quit functionality

**Key Features**:
- Calls `Application.Quit()` for clean shutdown

### Asset Organization

Assets are numbered for logical ordering:
- `Assets/01. Scenes/` - Unity scenes
- `Assets/02. Scripts/` - All C# scripts
- `Assets/03. Images/` - Character sprite sheets and textures
- `Assets/04. Animations/` - Animator controllers and animation clips
- `Assets/99. External Assets/` - Third-party packages (UniWindowController)

## Development Commands

This project does not use custom build scripts or command-line tools. Development is primarily through Unity Editor.

### Building
1. Open project in Unity Editor 6000.0.60f1
2. File → Build Settings
3. Select target platform (Windows, macOS, Linux)
4. Click "Build" or "Build and Run"

### Testing
- Use Unity Editor's Play mode for testing
- Input detection works in editor and builds
- For global input testing, run in built executable (not editor) for full Windows hook behavior

### Scene Setup
- Main scene: `Assets/01. Scenes/Oiia.unity`
- Character should have `Animator` component with states: "Gif" (triggered) and "Idle" (default)

## Critical Implementation Notes

### Windows Hooks Security
**⚠️ Important**: `GlobalInputActivityDetector_Windows` uses Windows hooks which require:
- Proper cleanup in `OnDisable()` and `OnApplicationQuit()` to prevent system-wide input lag
- Lock-free patterns when communicating between hook thread and main thread
- Filtering of keyboard repeat events to prevent flooding

### Thread Safety Patterns
When working with the input detector:
- **DO**: Use `Interlocked.Increment()` for counters in hook callbacks
- **DO**: Use `lock()` for collections shared between threads
- **DON'T**: Access Unity API from hook callbacks (only from main thread `Update()`)
- **DON'T**: Block hook callbacks with heavy computation

### Animation System
- Animator must have two states configured: default idle state and triggered state
- `GifBurstPlayer` expects exact state names: "Gif" and "Idle" (configurable in inspector)
- Animation timing uses coroutines for timeout handling

### Platform Compatibility
- **Windows Only**: `GlobalInputActivityDetector_Windows` only works on Windows due to Win32 API usage
- For cross-platform support, implement platform-specific versions or use Unity's new Input System with background input enabled

## Common Patterns

### Adding New Input-Reactive Behaviors
1. Create component subscribing to `GlobalInputActivityDetector_Windows.OnActivity`
2. Implement `void Trigger(uint count)` method matching event signature
3. Use `GameObject.FindObjectOfType<GlobalInputActivityDetector_Windows>()` or wire via Inspector

### Modifying Animation Behavior
- Adjust `GifBurstPlayer.playDuration` for longer/shorter animation
- Set `restartFromFirstFrameOnTrigger = true` to restart animation on each input
- Modify animation clips in `Assets/04. Animations/` folder

### Window Control Customization
- Access window controller via `UniWindowController.current`
- Set `isTopmost` for always-on-top behavior
- Modify window transparency via background GameObject active state
