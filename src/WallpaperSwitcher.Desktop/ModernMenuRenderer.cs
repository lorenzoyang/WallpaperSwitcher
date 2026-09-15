using System.Drawing.Drawing2D;

namespace WallpaperSwitcher.Desktop;

internal sealed class ModernMenuRenderer : ToolStripRenderer
{
    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        e.Graphics.Clear(SystemInformation.HighContrast ? SystemColors.Menu : ModernTheme.CardBackground);
    }

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        using var pen = new Pen(SystemInformation.HighContrast ? SystemColors.WindowFrame : ModernTheme.CardBorder);
        e.Graphics.DrawRectangle(pen, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
    }

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        var item = e.Item;
        var dpi = item.Owner?.DeviceDpi ?? 96;
        var inset = ModernTheme.ScaleMenu(4, dpi);
        if (item.Enabled && (item.Selected || item.Pressed))
        {
            var color = SystemInformation.HighContrast
                ? SystemColors.Highlight
                : item.Pressed ? ModernTheme.MenuPressedBackground : ModernTheme.MenuHoverBackground;
            using var brush = new SolidBrush(color);
            e.Graphics.FillRectangle(brush, inset, 0, item.Width - inset * 2, item.Height);
        }

        var icon = (item as ModernMenuItem)?.MenuIcon ?? ModernMenuIcon.None;
        var isChecked = item is ToolStripMenuItem { Checked: true };
        if (icon == ModernMenuIcon.None && !isChecked)
        {
            return;
        }

        var size = ModernTheme.ScaleMenu(ModernTheme.MenuIconSize, dpi);
        var x = ModernTheme.ScaleMenu(12, dpi);
        if (item.RightToLeft == RightToLeft.Yes)
        {
            x = item.Width - x - size;
        }

        var state = e.Graphics.Save();
        try
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TranslateTransform(x, (item.Height - size) / 2F);
            e.Graphics.ScaleTransform(size / 16F, size / 16F);
            using var pen = new Pen(GetItemColor(item, isChecked), 1.5F)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
            if (isChecked)
            {
                e.Graphics.DrawLines(pen, [new PointF(3, 8), new(6.5F, 11.5F), new(13, 4.5F)]);
            }
            else
            {
                DrawIcon(e.Graphics, pen, icon);
            }
        }
        finally
        {
            e.Graphics.Restore(state);
        }
    }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        var dpi = e.Item.Owner?.DeviceDpi ?? 96;
        var leading = ModernTheme.ScaleMenu(ModernTheme.MenuTextInset, dpi);
        var trailing = ModernTheme.ScaleMenu(ModernTheme.MenuTrailingInset, dpi);
        var rightToLeft = e.Item.RightToLeft == RightToLeft.Yes;
        var bounds = new Rectangle(rightToLeft ? trailing : leading, 0,
            Math.Max(0, e.Item.Width - leading - trailing), e.Item.Height);
        var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine
            | TextFormatFlags.EndEllipsis | TextFormatFlags.HidePrefix | TextFormatFlags.NoPadding;
        if (rightToLeft)
        {
            flags |= TextFormatFlags.RightToLeft | TextFormatFlags.Right;
        }

        TextRenderer.DrawText(e.Graphics, e.Text, e.TextFont, bounds, GetItemColor(e.Item), flags);
    }

    protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
    {
        if (e.Item is not ToolStripMenuItem item)
        {
            e.ArrowColor = SystemInformation.HighContrast ? SystemColors.MenuText : ModernTheme.TextPrimary;
            base.OnRenderArrow(e);
            return;
        }

        var scale = (item.Owner?.DeviceDpi ?? 96) / 96F;
        var rightToLeft = e.Direction == ArrowDirection.Left;
        var x = rightToLeft ? 16 * scale : item.Width - 16 * scale;
        var y = item.Height / 2F;
        var direction = rightToLeft ? -1 : 1;
        var state = e.Graphics.Save();
        try
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen = new Pen(GetItemColor(item), 1.5F * scale)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
            e.Graphics.DrawLines(pen,
                [new PointF(x - direction * 2 * scale, y - 3.5F * scale),
                 new(x + direction * 1.5F * scale, y),
                 new(x - direction * 2 * scale, y + 3.5F * scale)]);
        }
        finally
        {
            e.Graphics.Restore(state);
        }
    }

    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        var inset = ModernTheme.ScaleMenu(12, e.Item.Owner?.DeviceDpi ?? 96);
        using var pen = new Pen(SystemInformation.HighContrast ? SystemColors.WindowFrame : ModernTheme.DisabledBorder);
        e.Graphics.DrawLine(pen, inset, e.Item.Height / 2, e.Item.Width - inset, e.Item.Height / 2);
    }

    private static Color GetItemColor(ToolStripItem item, bool isCheck = false)
    {
        if (SystemInformation.HighContrast)
        {
            return !item.Enabled ? SystemColors.GrayText
                : item.Selected || item.Pressed ? SystemColors.HighlightText : SystemColors.MenuText;
        }

        if (!item.Enabled)
        {
            return ModernTheme.DisabledText;
        }

        return isCheck ? ModernTheme.PrimaryAccent : item.ForeColor;
    }

    private static void DrawIcon(Graphics graphics, Pen pen, ModernMenuIcon icon)
    {
        switch (icon)
        {
            case ModernMenuIcon.Folder:
                graphics.DrawPolygon(pen,
                    [new PointF(1.5F, 4), new(6, 4), new(7.5F, 6),
                     new(14.5F, 6), new(14.5F, 13), new(1.5F, 13)]);
                break;
            case ModernMenuIcon.NextWallpaper:
                graphics.DrawPolygon(pen, [new PointF(3, 3), new(10, 8), new(3, 13)]);
                graphics.DrawLine(pen, 13, 3, 13, 13);
                break;
            case ModernMenuIcon.Settings:
                graphics.DrawEllipse(pen, 3.5F, 3.5F, 9, 9);
                graphics.DrawEllipse(pen, 6, 6, 4, 4);
                for (var i = 0; i < 8; i++)
                {
                    var angle = i * MathF.PI / 4;
                    var x = MathF.Cos(angle);
                    var y = MathF.Sin(angle);
                    graphics.DrawLine(pen, 8 + x * 4.5F, 8 + y * 4.5F, 8 + x * 6.5F, 8 + y * 6.5F);
                }
                break;
            case ModernMenuIcon.Exit:
                graphics.DrawArc(pen, 2, 2.5F, 12, 12, -55, 290);
                graphics.DrawLine(pen, 8, 1.5F, 8, 7);
                break;
        }
    }
}
