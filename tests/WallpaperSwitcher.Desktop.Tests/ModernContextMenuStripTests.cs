using System.ComponentModel;

namespace WallpaperSwitcher.Desktop.Tests;

[Apartment(ApartmentState.STA)]
public class ModernContextMenuStripTests
{
    [Test]
    public void Layout_WithMixedLengthItems_AlignsRowsInsideBoundedMenu()
    {
        using var components = new Container();
        var menu = new ModernContextMenuStrip(components);
        menu.Items.Add(new ModernMenuItem("Short"));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ModernMenuItem(new string('W', 200)));

        menu.PerformLayout();

        Assert.Multiple(() =>
        {
            Assert.That(menu.Width, Is.LessThanOrEqualTo(360 * menu.DeviceDpi / 96));
            foreach (var item in menu.Items.OfType<ModernMenuItem>())
            {
                Assert.That(menu.ClientRectangle.Contains(item.Bounds), Is.True,
                    "Every row must fit inside the menu, including its icon and submenu arrow.");
                Assert.That(item.Width, Is.EqualTo(menu.ClientSize.Width),
                    "Short and ellipsized rows must use the same highlight width.");
            }
        });
    }

    [Test]
    public void Layout_AfterReplacingLongFolderWithShortFolder_RecalculatesWidth()
    {
        using var components = new Container();
        var menu = new ModernContextMenuStrip(components);
        var longFolder = new ModernMenuItem(new string('W', 200));
        menu.Items.Add(longFolder);
        menu.PerformLayout();
        var previousWidth = menu.Width;

        longFolder.Dispose();
        var shortFolder = new ModernMenuItem("Nature");
        menu.Items.Add(shortFolder);
        menu.PerformLayout();

        Assert.Multiple(() =>
        {
            Assert.That(menu.Width, Is.LessThan(previousWidth));
            Assert.That(menu.ClientRectangle.Contains(shortFolder.Bounds), Is.True);
            Assert.That(menu.Items.Count, Is.EqualTo(1));
        });
    }

    [Test]
    public void Layout_AfterChangingItemText_ResizesAllRows()
    {
        using var components = new Container();
        var menu = new ModernContextMenuStrip(components);
        var changingItem = new ModernMenuItem("Short");
        var otherItem = new ModernMenuItem("Other");
        menu.Items.AddRange([changingItem, otherItem]);
        menu.PerformLayout();
        var previousWidth = menu.Width;

        changingItem.Text = new string('W', 200);
        menu.PerformLayout();

        Assert.Multiple(() =>
        {
            Assert.That(menu.Width, Is.GreaterThan(previousWidth));
            Assert.That(changingItem.Width, Is.EqualTo(menu.ClientSize.Width));
            Assert.That(otherItem.Width, Is.EqualTo(menu.ClientSize.Width));
        });
    }

    [Test]
    public void Layout_AfterHidingLongItem_ShrinksToVisibleItems()
    {
        using var components = new Container();
        var menu = new ModernContextMenuStrip(components);
        var longItem = new ModernMenuItem(new string('W', 200));
        menu.Items.AddRange([new ModernMenuItem("Short"), longItem]);
        menu.PerformLayout();
        var previousSize = menu.Size;

        longItem.Available = false;
        menu.PerformLayout();

        Assert.Multiple(() =>
        {
            Assert.That(menu.Width, Is.LessThan(previousSize.Width));
            Assert.That(menu.Height, Is.LessThan(previousSize.Height));
            Assert.That(menu.ClientRectangle.Contains(menu.Items[0].Bounds), Is.True);
        });
    }

    [Test]
    public void Layout_AfterIncreasingFont_ExpandsRowsToFitText()
    {
        using var components = new Container();
        var menu = new ModernContextMenuStrip(components);
        menu.Items.AddRange([new ModernMenuItem("First"), new ModernMenuItem("Second")]);
        menu.PerformLayout();
        var previousHeight = menu.Height;
        using var largerFont = new Font(menu.Font.FontFamily, menu.Font.SizeInPoints * 3);

        menu.Font = largerFont;
        menu.PerformLayout();

        Assert.Multiple(() =>
        {
            Assert.That(menu.Height, Is.GreaterThan(previousHeight));
            foreach (ToolStripItem item in menu.Items)
            {
                Assert.That(menu.ClientRectangle.Contains(item.Bounds), Is.True);
                Assert.That(item.Height, Is.GreaterThanOrEqualTo(largerFont.Height));
            }
        });
    }
}
