# Wallpaper Switcher

**Wallpaper Switcher** is a lightweight and user-friendly wallpaper manager for Windows. It allows users to manage multiple wallpaper folders and quickly switch between images with ease. It also supports global hotkeys, system tray integration, and automatic startup, and includes a clean, intuitive settings interface for customizing your experience.

<img src="./assets/gifs/Gui_Demo.gif" alt="GUI Demo" width="350"/>
<img src="./assets/gifs/SystemTray_Demo.gif" alt="System Tray Demo" width="350"/>
<img src="./assets/gifs/Settings_Demo.gif" alt="Settings Demo" width="350"/>


## Features 

- [x] **Folder Management:** 
  - Add and remove wallpaper folders
  - Easily switch between folders
- [x] **Manual Wallpaper Switching:** 
  - Instantly switch to the next wallpaper
- [x] **Two Wallpaper Switching Modes**
  - Native Mode (System Slideshow): Uses Windows' built-in wallpaper SlideShow feature.
  - Custom Mode (Fast Switching): Uses the `SetWallpaper` API to simulate the SlideShow feature for faster transitions.
- [x] **System Tray Integration**
  - Automatically minimizes to the system tray when closed via the "X" button
  - Right-click tray menu options: Switch Folder, Next Wallpaper, Settings, Exit
  - Left-click to reopen the main window
- [x] **Global Hotkey Support**
  - Hotkey for "Next Wallpaper"
  - Hotkey for switching wallpaper folders
- [x] **Auto Start on Boot**
  - Optional setting to launch automatically on Windows startup
- [x] **Settings Interface**
  - Clean and intuitive UI for configuring hotkeys and preferences

## Installation & Usage

**Wallpaper Switcher** is portable and requires no installation. Choose from two deployment methods:


### 🔹 Option 1: Single Executable (Simplest)

1. Download `WallpaperSwitcher.exe` from the [Releases](https://github.com/lorenzoyang/WallpaperSwitcher/releases) page.
2. Save it to any folder (e.g., Desktop or `C:\Programs`).
3. Double-click to run.

> ⚠️ **Note:** The first launch may be slightly slower due to self-extraction.

### 🔹 Option 2: Full Package (Recommended)

1. Download the `WallpaperSwitcher.zip` file from the [Releases](https://github.com/lorenzoyang/WallpaperSwitcher/releases) page.
2. Extract the contents to a directory of your choice (e.g., `C:\Programs\WallpaperSwitcher`).
3. Inside the extracted folder, go to the `bin` directory and run `WallpaperSwitcher.exe`.

> ⚠️ **Important:**
>
> - Do **not** move or delete files inside the `bin` folder.
> - The `WallpaperSwitcher.exe` **must remain** inside the `bin` directory to function correctly.

## Running the App

### ▶️ Launching

* **Single Executable:** Double-click `WallpaperSwitcher.exe`.
* **Full Package:** Navigate to `bin/` and run `WallpaperSwitcher.exe`.

### 📌 Create Shortcut (Optional)

* Right-click `WallpaperSwitcher.exe` → **Create shortcut**
* Move the shortcut to Desktop or Pin it to Start

## 💡 Usage Tips

Here are some helpful tips and details to get the most out of **Wallpaper Switcher**:

### 🖥️ General Behavior

- When you close the main window (via the "X" button), the application minimizes to the **system tray** and continues running in the background.
- To **completely exit**, right-click the tray icon and choose **Exit**.
- From the tray icon, you can quickly:
  - Switch to the next wallpaper
  - Change wallpaper folders
  - Open the settings window
  - Exit the application

### ⚙️ User Data & Configuration

All user data is stored in:

```
C:\Users\<YourUsername>\AppData\Local\WallpaperSwitcher
```

This folder includes:

* `hotkeys.json`: Stores your custom global hotkey mappings
* A folder prefixed with `WallpaperSwitcher*` containing `user.config.xml`, which holds other settings

### 🔄 How to Reset the App

To fully reset Wallpaper Switcher to its default state:
1. **Delete the user data folder:**
   ```
   C:\Users\<YourUsername>\AppData\Local\WallpaperSwitcher
   ```
2. **Remove the app from Windows startup (optional):**
   - Press `Win + R`, type `regedit`, and hit Enter
   - Navigate to:
     ```
     HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run
     ```
   - Delete the `WallpaperSwitcher` entry
3. **Alternative method:**
   - You can reset settings and remove hotkeys from within the app via the **Settings** window

> ⚠️ **Note:** The app will regenerate default settings and hotkeys on next launch.

### ⌨️ Hotkey Usage & Format

- **Default hotkey**: `Ctrl + Alt + N` (for switching to the next wallpaper)
- You can change hotkeys via the **Settings** window
- **Hotkey rules:**
  - Use `+` as a separator (spaces and case are ignored)
  - Only **one letter key** (e.g., `N`) is allowed
  - Combine it with one or more of the following modifiers:
    - `Ctrl`
    - `Alt`
    - `Shift`
- **Examples:**
  - `Ctrl + Alt + N`
  - `Ctrl + Shift + N`
  - `Ctrl + Alt + Shift + N`

## License

[GPL-3.0](LICENSE)
