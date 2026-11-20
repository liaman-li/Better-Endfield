using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BetterEndfieldNext.UI.Controls;

public sealed class PageContentPanel : Grid
{
    public double MaxContentWidth
    {
        get => ColumnDefinitions[0].MaxWidth;
        set => ColumnDefinitions[0].MaxWidth = value;
    }

    public PageContentPanel()
    {
        ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = new GridLength(1, GridUnitType.Star)
        });
    }
}
