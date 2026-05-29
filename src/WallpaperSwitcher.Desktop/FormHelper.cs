using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace WallpaperSwitcher.Desktop;

internal static class FormHelper
{
    private const string GitHubIssuesUrl = "https://github.com/lorenzoyang/WallpaperSwitcher/issues";

    public static void ShowSuccessMessage(string message, string caption = "Success")
    {
        MessageBox.Show(
            message,
            caption,
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
    }

    public static void ShowErrorMessage(string message, string caption = "Error")
    {
        MessageBox.Show(
            message,
            caption,
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        );
    }

    public static void ShowErrorMessageWithLink(string message, string caption = "Error")
    {
        var fullMessage = $"{message}\n\nPlease report this bug on GitHub: {GitHubIssuesUrl}";

        MessageBox.Show(
            fullMessage,
            caption,
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        );
    }

    public static void ShowWarningMessage(string message, string caption = "Warning")
    {
        MessageBox.Show(
            message,
            caption,
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning
        );
    }

    public static void OpenUrl(Uri uri)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = uri.AbsoluteUri,
            UseShellExecute = true
        });
    }

    public static void ShowFolderToolTipForComboBox(ToolTip toolTip, ComboBox? comboBox)
    {
        if (comboBox is { SelectedItem: not null })
        {
            var fullText = comboBox.SelectedItem?.ToString() ?? string.Empty;

            // Reserve space for the dropdown arrow when deciding whether the text is clipped.
            toolTip.SetToolTip(
                comboBox,
                (TextRenderer.MeasureText(fullText, comboBox.Font).Width > comboBox.Width - 20)
                    ? fullText
                    : "");
        }
    }

    /// <summary>
    /// Windows reserves WM_USER (which is 0x0400) and higher for custom application-defined messages.
    /// 0x0401 is simply the next available value (like WM_USER + 1).
    /// </summary>
    public const int WmShowFirstInstanceMessage = 0x0401;

    public static bool TryActivateExistingInstance(string appName)
    {
        var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Application.ExecutablePath));
        foreach (var process in processes)
        {
            if (process.Id == Environment.ProcessId) continue;

            var hWnd = process.MainWindowHandle;
            if (hWnd == IntPtr.Zero)
            {
                // Fallback to FindWindow if MainWindowHandle is zero (minimized/hidden)
                hWnd = PInvoke.FindWindow(null, appName);
            }

            if (hWnd == IntPtr.Zero) continue;

            var result = PInvoke.PostMessage(
                (HWND)hWnd, WmShowFirstInstanceMessage, new WPARAM(UIntPtr.Zero), IntPtr.Zero
            );
            if (!result)
            {
                throw new InvalidOperationException(
                    $"Failed to send message to existing instance: {Marshal.GetLastWin32Error()}"
                );
            }

            return true;
        }

        return false;
    }
}
