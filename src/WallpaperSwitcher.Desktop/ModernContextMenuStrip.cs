using System.ComponentModel;

namespace WallpaperSwitcher.Desktop;

internal sealed class ModernContextMenuStrip : ContextMenuStrip
{
    private static readonly ModernMenuRenderer SharedRenderer = new();
    private bool _isApplyingLayout;

    public ModernContextMenuStrip(IContainer container) : base(container)
    {
        Font = ModernTheme.BodyFont;
        Renderer = SharedRenderer;
        ShowImageMargin = false;
        ShowCheckMargin = false;
        ShowItemToolTips = true;
        ApplyThemeColors();
    }

    protected override Padding DefaultPadding => new(
        0, ModernTheme.ScaleMenu(ModernTheme.MenuVerticalPadding, DeviceDpi),
        0, ModernTheme.ScaleMenu(ModernTheme.MenuVerticalPadding, DeviceDpi));

    public override Size GetPreferredSize(Size proposedSize)
    {
        var size = MeasureContent();
        return new Size(size.Width, size.Height + Padding.Vertical);
    }

    private Size MeasureContent()
    {
        var width = ModernTheme.ScaleMenu(ModernTheme.MenuMinimumWidth, DeviceDpi);
        var height = 0;
        foreach (ToolStripItem item in Items)
        {
            if (!item.Available)
            {
                continue;
            }

            var size = MeasureItem(item);
            width = Math.Max(width, size.Width);
            height += size.Height + item.Margin.Vertical;
        }

        return new Size(width, height);
    }

    protected override void OnLayout(LayoutEventArgs e)
    {
        if (_isApplyingLayout || IsDisposed)
        {
            return;
        }

        _isApplyingLayout = true;
        try
        {
            var width = MeasureContent().Width;
            foreach (ToolStripItem item in Items)
            {
                if (!item.Available)
                {
                    continue;
                }

                // Native menu layout measures unbounded text. Assign every row the
                // same bounded width so ellipsized labels and highlights stay aligned.
                item.AutoSize = false;
                item.Size = new Size(width, MeasureItem(item).Height);
            }

            base.OnLayout(e);
        }
        finally
        {
            _isApplyingLayout = false;
        }
    }

    protected override void OnOpening(CancelEventArgs e)
    {
        ApplyThemeColors();
        base.OnOpening(e);
    }

    protected override void OnDpiChangedAfterParent(EventArgs e)
    {
        base.OnDpiChangedAfterParent(e);
        PerformLayout();
    }

    private Size MeasureItem(ToolStripItem item)
    {
        if (item is ToolStripSeparator)
        {
            return new Size(0, ModernTheme.ScaleMenu(ModernTheme.MenuSeparatorHeight, DeviceDpi));
        }

        var textSize = TextRenderer.MeasureText(item.Text, item.Font, Size.Empty,
            TextFormatFlags.SingleLine | TextFormatFlags.HidePrefix | TextFormatFlags.NoPadding);
        var width = textSize.Width + ModernTheme.ScaleMenu(
            ModernTheme.MenuTextInset + ModernTheme.MenuTrailingInset, DeviceDpi);

        return new Size(
            Math.Clamp(width, ModernTheme.ScaleMenu(ModernTheme.MenuMinimumWidth, DeviceDpi),
                ModernTheme.ScaleMenu(ModernTheme.MenuMaximumWidth, DeviceDpi)),
            Math.Max(ModernTheme.ScaleMenu(ModernTheme.MenuItemHeight, DeviceDpi),
                textSize.Height + ModernTheme.ScaleMenu(ModernTheme.MenuItemVerticalPadding, DeviceDpi)));
    }

    private void ApplyThemeColors()
    {
        BackColor = SystemInformation.HighContrast ? SystemColors.Menu : ModernTheme.CardBackground;
        ForeColor = SystemInformation.HighContrast ? SystemColors.MenuText : ModernTheme.TextPrimary;
    }
}
