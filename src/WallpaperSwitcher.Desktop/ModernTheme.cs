namespace WallpaperSwitcher.Desktop;

internal enum ModernButtonKind
{
    Primary,
    Secondary,
    Success,
    Danger
}

internal static class ModernTheme
{
    public static readonly Color AppBackground = Color.FromArgb(229, 235, 243);
    public static readonly Color CardBackground = Color.FromArgb(252, 253, 255);
    public static readonly Color CardBorder = Color.FromArgb(169, 181, 198);
    public static readonly Color TextPrimary = Color.FromArgb(17, 24, 39);
    public static readonly Color FieldBackground = Color.FromArgb(255, 255, 255);
    public static readonly Color DisabledBackground = Color.FromArgb(238, 242, 247);
    public static readonly Color DisabledBorder = Color.FromArgb(203, 213, 225);
    public static readonly Color DisabledText = Color.FromArgb(148, 163, 184);

    public static readonly Font BodyFont = new("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
    public static readonly Font LabelFont = new("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
    public static readonly Font SectionFont = new("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
    public static readonly Font ButtonFont = new("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);

    public static void ApplyForm(Form form)
    {
        form.BackColor = AppBackground;
    }

    public static void ApplySurface(Panel panel)
    {
        panel.BackColor = AppBackground;
        panel.Padding = new Padding(20);
    }

    public static void ApplyCard(GroupBox card)
    {
        card.BackColor = CardBackground;
        card.ForeColor = TextPrimary;
        card.Font = SectionFont;
        card.Padding = new Padding(18, 30, 18, 18);
    }

    public static void ApplyLabel(Label label)
    {
        label.BackColor = Color.Transparent;
        label.ForeColor = TextPrimary;
        label.Font = LabelFont;
    }

    public static void ApplyCheckBox(CheckBox checkBox)
    {
        checkBox.BackColor = Color.Transparent;
        checkBox.ForeColor = TextPrimary;
        checkBox.Font = LabelFont;
        checkBox.FlatStyle = FlatStyle.System;
    }

    public static void ApplyTextBox(TextBox textBox)
    {
        textBox.BackColor = FieldBackground;
        textBox.ForeColor = TextPrimary;
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.Font = BodyFont;
    }

    public static void ApplyComboBox(ComboBox comboBox)
    {
        comboBox.BackColor = FieldBackground;
        comboBox.ForeColor = TextPrimary;
        comboBox.FlatStyle = FlatStyle.Standard;
        comboBox.Font = BodyFont;
    }

    public static void ApplyButton(Button button, ModernButtonKind kind)
    {
        var palette = ModernButtonPalette.For(kind);

        button.Tag = palette;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 2;
        button.FlatAppearance.BorderColor = palette.BorderColor;
        button.FlatAppearance.MouseOverBackColor = palette.HoverBackColor;
        button.FlatAppearance.MouseDownBackColor = palette.PressedBackColor;
        button.Font = ButtonFont;
        button.TextAlign = ContentAlignment.MiddleCenter;
        button.UseVisualStyleBackColor = false;
        button.Cursor = Cursors.Hand;
        button.EnabledChanged -= ButtonEnabledChanged;
        button.EnabledChanged += ButtonEnabledChanged;

        ApplyButtonEnabledState(button);
    }

    private static void ApplyButtonEnabledState(Button button)
    {
        if (button.Tag is not ModernButtonPalette palette)
        {
            return;
        }

        if (button.Enabled)
        {
            button.BackColor = palette.BackColor;
            button.ForeColor = palette.ForeColor;
            button.FlatAppearance.BorderSize = 2;
            button.FlatAppearance.BorderColor = palette.BorderColor;
            button.FlatAppearance.MouseOverBackColor = palette.HoverBackColor;
            button.FlatAppearance.MouseDownBackColor = palette.PressedBackColor;
            button.Cursor = Cursors.Hand;
            return;
        }

        button.BackColor = DisabledBackground;
        button.ForeColor = DisabledText;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = DisabledBorder;
        button.FlatAppearance.MouseOverBackColor = DisabledBackground;
        button.FlatAppearance.MouseDownBackColor = DisabledBackground;
        button.Cursor = Cursors.Default;
    }

    private static void ButtonEnabledChanged(object? sender, EventArgs e)
    {
        if (sender is Button button)
        {
            ApplyButtonEnabledState(button);
        }
    }
}

internal sealed class ModernCard : GroupBox
{
    public ModernCard()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var borderBounds = ClientRectangle;
        borderBounds.Width -= 1;
        borderBounds.Height -= 1;

        using var borderPen = new Pen(ModernTheme.CardBorder);
        e.Graphics.DrawRectangle(borderPen, borderBounds);
    }
}

internal sealed class ModernButtonPalette
{
    private ModernButtonPalette(
        Color backColor,
        Color foreColor,
        Color hoverBackColor,
        Color pressedBackColor,
        Color borderColor)
    {
        BackColor = backColor;
        ForeColor = foreColor;
        HoverBackColor = hoverBackColor;
        PressedBackColor = pressedBackColor;
        BorderColor = borderColor;
    }

    public Color BackColor { get; }

    public Color ForeColor { get; }

    public Color HoverBackColor { get; }

    public Color PressedBackColor { get; }

    public Color BorderColor { get; }

    public static ModernButtonPalette For(ModernButtonKind kind)
    {
        return kind switch
        {
            ModernButtonKind.Primary => new ModernButtonPalette(
                Color.FromArgb(30, 87, 214),
                Color.White,
                Color.FromArgb(24, 75, 190),
                Color.FromArgb(23, 61, 150),
                Color.FromArgb(23, 61, 150)),
            ModernButtonKind.Success => new ModernButtonPalette(
                Color.FromArgb(23, 128, 78),
                Color.White,
                Color.FromArgb(17, 105, 64),
                Color.FromArgb(15, 84, 52),
                Color.FromArgb(15, 84, 52)),
            ModernButtonKind.Danger => new ModernButtonPalette(
                Color.FromArgb(196, 42, 58),
                Color.White,
                Color.FromArgb(169, 36, 50),
                Color.FromArgb(139, 30, 42),
                Color.FromArgb(139, 30, 42)),
            _ => new ModernButtonPalette(
                Color.FromArgb(255, 255, 255),
                Color.FromArgb(17, 24, 39),
                Color.FromArgb(226, 232, 240),
                Color.FromArgb(203, 213, 225),
                Color.FromArgb(100, 116, 139))
        };
    }
}
