using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WallpaperSwitcher.Desktop;

internal static class FormHelper
{
    public static void ShowSuccessMessage(string message, string caption = "Success")
    {
        MessageBox.Show(message,
            caption,
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    public static void ShowErrorMessage(string message, string caption = "Error")
    {
        MessageBox.Show(message,
            caption,
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }

    public static void ShowWarningMessage(string message, string caption = "Warning")
    {
        MessageBox.Show(message,
            caption,
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }

    public static void ShowToolTipForComboBox(ToolTip toolTip, ComboBox? comboBox)
    {
        if (comboBox is { SelectedItem: not null })
        {
            var fullText = comboBox.SelectedItem?.ToString() ?? string.Empty;

            // Only show tooltip if text is longer than what can be displayed
            // -20: to account for padding and dropdown arrow
            toolTip.SetToolTip(comboBox,
                (TextRenderer.MeasureText(fullText, comboBox.Font).Width > comboBox.Width - 20)
                    ? fullText
                    : ""); // Clear tooltip for short text
        }
    }

    /// <summary>
    /// Windows reserves WM_USER (which is 0x0400) and higher for custom application-defined messages.
    /// 0x0401 is simply the next available value (like WM_USER + 1).
    /// </summary>
    public const int WmShowFirstInstanceMessage = 0x0401;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    public static bool TryActivateExistingInstance(string appName)
    {
        // Try multiple approaches for robustness
        // Get all processes with the same process name
        var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Application.ExecutablePath));
        foreach (var process in processes)
        {
            if (process.Id == Environment.ProcessId) continue;

            // Try to find main window
            var hWnd = process.MainWindowHandle;
            if (hWnd == IntPtr.Zero)
            {
                // Fallback to FindWindow if MainWindowHandle is zero (minimized/hidden)
                hWnd = FindWindow(null, appName);
            }

            if (hWnd == IntPtr.Zero) continue;

            // Send custom message to show the form
            var result = PostMessage(hWnd, WmShowFirstInstanceMessage, IntPtr.Zero, IntPtr.Zero);
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