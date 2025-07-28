// See https://aka.ms/new-console-template for more information

using WallpaperSwitcher.Core;

DesktopWallpaperManager manager = new DesktopWallpaperManager();
manager.ChangeWallpaperFolder(@"C:\Users\yangr\OneDrive\Immagini\wallpaper_images\动漫");
manager.SetSlideShow();