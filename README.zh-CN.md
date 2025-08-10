🌐 [English](README.md) | 🇨🇳 [中文](README.zh-CN.md)



# 壁纸切换器 (Wallpaper Switcher)

**壁纸切换器**是一款轻量级、用户友好的Windows壁纸管理工具。它支持管理多个壁纸文件夹，轻松快速切换壁纸，并提供全局快捷键、系统托盘集成、开机自启等功能，配有简洁直观的设置界面。

<img src="./assets/gifs/GUI_Demo.gif" alt="界面演示" width="350"/>

## 功能特性

- [x] **文件夹管理：**
  - 添加/移除壁纸文件夹
  - 快速切换不同文件夹
- [x] **手动壁纸切换：**
  - 立即切换到下一张壁纸
- [x] **两种切换模式：**
  - 原生模式（系统幻灯片）：使用Windows内置壁纸轮播功能
  - 自定义模式（快速切换）：通过`SetWallpaper` API实现更快的切换速度
- [x] **系统托盘集成**
  
  <img src="./assets/gifs/SystemTray_Demo.gif" alt="系统托盘演示" width="350"/>

  - 点击"X"关闭时自动最小化到托盘
  - 右键菜单功能：切换文件夹、下一张壁纸、设置
  - 左键点击恢复主窗口
- [x] **全局快捷键支持：**
  - 切换壁纸快捷键
  - 切换文件夹快捷键
- [x] **开机自启：**
  - 可选开机自动启动
- [x] **设置界面：**

  <img src="./assets/gifs/Settings_Demo.gif" alt="设置界面演示" width="350"/>

  - 简洁直观的快捷键和偏好设置界面

## 安装与使用

**壁纸切换器**是便携版软件，无需安装。提供两种部署方式：

### 🔹 方案1：单文件版（最简单）

1. 从[发布页面](https://github.com/lorenzoyang/WallpaperSwitcher/releases)下载`WallpaperSwitcher.exe`
2. 保存到任意目录（如桌面或`C:\Programs`）
3. 双击运行

> ⚠️ **注意：** 首次启动因自解压过程可能稍慢

### 🔹 方案2：完整包（推荐）

1. 下载[发布页面](https://github.com/lorenzoyang/WallpaperSwitcher/releases)的`WallpaperSwitcher.zip`
2. 解压到目标目录（如`C:\Programs\WallpaperSwitcher`）
3. 进入解压后的`bin`目录运行`WallpaperSwitcher.exe`

> ⚠️ **重要提示：**
>
> - 请勿移动或删除`bin`目录内文件
> - `WallpaperSwitcher.exe`必须保持在`bin`目录内运行

## 运行指南

### ▶️ 启动方式

* **单文件版：** 双击`WallpaperSwitcher.exe`
* **完整包：** 运行`bin/WallpaperSwitcher.exe`

### 📌 创建快捷方式（可选）

* 右键`WallpaperSwitcher.exe` → **创建快捷方式**
* 将快捷方式放置桌面或固定到开始菜单

## 💡 使用技巧

### 🖥️ 常规操作

- 点击窗口"X"关闭按钮时，程序将最小化到**系统托盘**继续运行
- **完全退出**需右击托盘图标选择**退出**
- 通过托盘图标可快速：
  - 切换下一张壁纸
  - 更换壁纸文件夹
  - 打开设置窗口
  - 退出程序

### ⚙️ 用户数据存储

所有用户数据存储在：
```
C:\Users\<用户名>\AppData\Local\WallpaperSwitcher
```
包含：
* `hotkeys.json`：保存快捷键配置
* `WallpaperSwitcher*`前缀文件夹：存储`user.config.xml`设置文件

### 🔄 重置程序

完全重置步骤：
1. **删除用户数据文件夹：**
   ```
   C:\Users\<用户名>\AppData\Local\WallpaperSwitcher
   ```
2. **移除开机启动项（可选）：**
   - `Win + R`输入`regedit`回车
   - 定位路径：
     ```
     HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run
     ```
   - 删除`WallpaperSwitcher`键值
3. **应用内重置：**
   - 通过**设置**窗口可重置配置和快捷键

> ⚠️ **注意：** 重启程序将自动生成默认配置

### ⌨️ 快捷键设置规则

- **默认快捷键：** `Ctrl + Alt + N`（切换下一张壁纸）
- 通过**设置**窗口修改快捷键
- **设置规则：**
  - 使用`+`分隔符（不区分大小写和空格）
  - 仅支持**单个字母键**（如`N`）
  - 可组合以下修饰键：
    - `Ctrl`
    - `Alt`
    - `Shift`
- **有效示例：**
  - `Ctrl + Alt + N`
  - `Ctrl + Shift + N`
  - `Ctrl + Alt + Shift + N`

## 开源许可

[GPL-3.0](LICENSE)
