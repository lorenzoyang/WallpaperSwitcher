# Wallpaper Switcher

**Wallpaper Switcher** is a lightweight and user-friendly wallpaper manager for Windows. It allows users to manage multiple wallpaper folders and quickly switch between images with ease.

<img src="./assets/gifs/GUI_Demo.gif" alt="GUI Demo" width="350"/>
<img src="./assets/gifs/SystemTray_Demo.gif" alt="System Tray Demo" width="350"/>

## Features 

- [x] **Folder Management:** 
  - Add new wallpaper folders
  - Remove existing wallpaper folders
  - Switch between wallpaper folders
- [x] **Manual Wallpaper Switching:** 
  - Switch to the next wallpaper
- [x] **System Tray Integration**
  - Minimize to system tray
  - Right-click menu with options: Open, Exit
- [x] **Two Wallpaper Switching Modes**
  - Native Mode (System Slideshow): Uses Windows' built-in wallpaper SlideShow feature.
  - Custom Mode (Fast Switching): Uses the `SetWallpaper` API to simulate the SlideShow feature for faster transitions.
- [ ] **Global Hotkey Support**
  - Hotkey for "Next Wallpaper"
  - Hotkey for switching wallpaper folders
  - Configuration through a settings file
- [ ] **Auto Start on Boot**
  - Option to launch automatically when Windows starts

## Installation & Usage

You can run **Wallpaper Switcher** without needing to install anything. Choose from two deployment options based on your preference:

### Download

Visit the [Releases](https://github.com/lorenzoyang/WallpaperSwitcher/releases) page on GitHub and choose one of the following options:

#### Option 1: Single Executable (Simplest)

1. Download `WallpaperSwitcher.exe`
2. Save it to any location you prefer (e.g., Desktop, `C:\Programs`, etc.)

> **Note:** The single executable may have a slightly slower startup time as it needs to extract required files on first launch.

#### Option 2: Full Package (Recommended)

1. Download the `WallpaperSwitcher.zip` file
2. Extract the contents of the .zip file. You will get a folder containing:
   - A `bin` folder with all application files
   - The main `WallpaperSwitcher.exe` inside the `bin` folder
3. Move the extracted folder to your preferred location (e.g., `C:\Programs\WallpaperSwitcher`, `D:\Apps\WallpaperSwitcher`, or Desktop)

> **⚠️ Important for Full Package:**
> - **Do not modify, move, or delete any contents inside the `bin` folder**
> - Always keep all files in the `bin` folder together
> - The `WallpaperSwitcher.exe` must remain inside the `bin` folder to function properly

### Run

#### For Single Executable (Option 1):

1. Navigate to where you saved `WallpaperSwitcher.exe`
2. Double-click the file to launch the application

#### For Full Package (Option 2):

1. Open the extracted WallpaperSwitcher folder
2. Navigate to the `bin` folder
3. Double-click `WallpaperSwitcher.exe` to launch the application
4. Optionally, create a desktop shortcut or pin to Start Menu for easier access:
   - Right-click `WallpaperSwitcher.exe` → **Create shortcut**
   - Move the shortcut to your Desktop or right-click it → **Pin to Start**

#### Usage Tips:
- When running, a main window will appear. You can close this window to minimize the application to the system tray—it will keep running in the background.
- To exit the application, right-click the tray icon and select **Exit**.


## License

[GPL-3.0](LICENSE)
