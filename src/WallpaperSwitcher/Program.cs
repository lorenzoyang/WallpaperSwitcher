// See https://aka.ms/new-console-template for more information

Console.WriteLine("Hello, World!");

const string wallpaperPath = @"C:\Users\yangr\OneDrive\Immagini\wallpaper_images\动漫\luffy5th.png";

try
{
    WallpaperSwitcher.WallpaperChanger.SetWallpaper(wallpaperPath);
    Console.WriteLine("Wallpaper set successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to set wallpaper: {ex.Message}");
}