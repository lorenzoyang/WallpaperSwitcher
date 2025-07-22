namespace WallpaperSwitcher.Desktop;

internal static class FormHelper
{
    public static void ShowSuccessMessage(string message)
    {
        MessageBox.Show(message,
            @"Success",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    public static void ShowErrorMessage(string message)
    {
        MessageBox.Show(message,
            @"Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }

    public static void ShowWarningMessage(string message)
    {
        MessageBox.Show(message,
            @"Warning",
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
}