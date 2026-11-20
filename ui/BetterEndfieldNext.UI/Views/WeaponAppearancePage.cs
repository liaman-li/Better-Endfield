using System;
using System.Collections.Generic;
using BetterEndfieldNext.UI.Controls;
using BetterEndfieldNext.UI.Models;
using BetterEndfieldNext.UI.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace BetterEndfieldNext.UI.Views;

public sealed class WeaponAppearancePage : UserControl
{
    internal event EventHandler? ConfigurationChanged;

    private readonly ToggleSwitch enabledToggle = new() { Name = "AppearanceEnabledToggle" };

    // Weapon
    private readonly ToggleSwitch replaceWeaponToggle = new() { Name = "ReplaceWeaponToggle", IsOn = true };
    private readonly ComboBox weaponModeCombo = new() { Name = "WeaponModeCombo", HorizontalAlignment = HorizontalAlignment.Stretch };

    // Character
    private readonly ToggleSwitch replaceCharacterToggle = new() { Name = "ReplaceCharacterToggle", IsOn = true };
    private readonly ComboBox characterModeCombo = new() { Name = "CharacterModeCombo", HorizontalAlignment = HorizontalAlignment.Stretch };

    // Effects
    private readonly ToggleSwitch replaceEffectsToggle = new() { Name = "ReplaceEffectsToggle", IsOn = true };

    // Headers & Labels
    private readonly TextBlock title = new() { FontSize = 24, FontWeight = Microsoft.UI.Text.FontWeights.Bold, TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock subtitle = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock masterRowTitle = new() { FontSize = 14, FontWeight = Microsoft.UI.Text.FontWeights.Bold };
    private readonly TextBlock masterRowHint = new() { FontSize = 12, TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock weaponSectionTitle = new() { FontSize = 16, FontWeight = Microsoft.UI.Text.FontWeights.Bold };
    private readonly TextBlock weaponRowTitle = new() { FontSize = 14, FontWeight = Microsoft.UI.Text.FontWeights.Bold };
    private readonly TextBlock weaponRowHint = new() { FontSize = 12, TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock characterSectionTitle = new() { FontSize = 16, FontWeight = Microsoft.UI.Text.FontWeights.Bold };
    private readonly TextBlock characterRowTitle = new() { FontSize = 14, FontWeight = Microsoft.UI.Text.FontWeights.Bold };
    private readonly TextBlock characterRowHint = new() { FontSize = 12, TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock effectsSectionTitle = new() { FontSize = 16, FontWeight = Microsoft.UI.Text.FontWeights.Bold };
    private readonly TextBlock effectsRowTitle = new() { FontSize = 14, FontWeight = Microsoft.UI.Text.FontWeights.Bold };
    private readonly TextBlock effectsRowHint = new() { FontSize = 12, TextWrapping = TextWrapping.Wrap };

    private bool _suppressEvents = false;

    private static string T(string zh, string en) =>
        LocalizationService.Instance.IsChinese ? zh : en;

    private static Grid CreateSwitchRow(TextBlock rowTitle, TextBlock rowHint, ToggleSwitch toggle)
    {
        rowHint.Style = (Style)Application.Current.Resources["FieldHintStyle"];
        toggle.Style = (Style)Application.Current.Resources["CompactToggleSwitchStyle"];

        var row = new Grid { ColumnSpacing = 12, Padding = new Thickness(0, 2, 0, 2) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var textStack = new StackPanel { Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
        textStack.Children.Add(rowTitle);
        textStack.Children.Add(rowHint);
        row.Children.Add(textStack);

        Grid.SetColumn(toggle, 1);
        row.Children.Add(toggle);
        return row;
    }

    public WeaponAppearancePage()
    {
        var sectionStyle = (Style)Application.Current.Resources["SectionStyle"];
        var hintStyle = (Style)Application.Current.Resources["FieldHintStyle"];
        subtitle.Style = hintStyle;
        subtitle.FontSize = 14;

        var root = new Grid();
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        // Row 0: Fixed Page Header
        var headerStack = new StackPanel { Padding = new Thickness(28, 22, 28, 12), Spacing = 4 };
        headerStack.Children.Add(title);
        headerStack.Children.Add(subtitle);
        root.Children.Add(headerStack);

        // Row 1: Scrollable Body
        var body = new StackPanel { Spacing = 16 };

        // Top Compact Master Switch Card
        var masterCard = new Border
        {
            Style = sectionStyle,
            Padding = new Thickness(22, 16, 22, 16),
            Child = CreateSwitchRow(masterRowTitle, masterRowHint, enabledToggle)
        };
        body.Children.Add(masterCard);

        // Bottom 2-Column Layout
        var columnsGrid = new Grid { ColumnSpacing = 18 };
        columnsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        columnsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var leftColumn = new StackPanel { Spacing = 16, VerticalAlignment = VerticalAlignment.Top };
        var rightColumn = new StackPanel { Spacing = 16, VerticalAlignment = VerticalAlignment.Top };

        // Left Card 1: Weapon Appearance
        var weaponCard = new Border { Style = sectionStyle };
        var weaponStack = new StackPanel { Spacing = 14 };
        weaponStack.Children.Add(weaponSectionTitle);
        weaponStack.Children.Add(CreateSwitchRow(weaponRowTitle, weaponRowHint, replaceWeaponToggle));
        weaponModeCombo.Items.Add(new ComboBoxItem { Tag = "ink" });
        weaponModeCombo.Items.Add(new ComboBoxItem { Tag = "default" });
        weaponModeCombo.SelectedIndex = 0;
        weaponStack.Children.Add(weaponModeCombo);
        weaponCard.Child = weaponStack;
        leftColumn.Children.Add(weaponCard);

        // Left Card 2: Combat & Blade VFX
        var effectCard = new Border { Style = sectionStyle };
        var effectStack = new StackPanel { Spacing = 14 };
        effectStack.Children.Add(effectsSectionTitle);
        effectStack.Children.Add(CreateSwitchRow(effectsRowTitle, effectsRowHint, replaceEffectsToggle));
        effectCard.Child = effectStack;
        leftColumn.Children.Add(effectCard);

        // Right Card: Administrator Character Appearance
        var charCard = new Border { Style = sectionStyle };
        var charStack = new StackPanel { Spacing = 14 };
        charStack.Children.Add(characterSectionTitle);
        charStack.Children.Add(CreateSwitchRow(characterRowTitle, characterRowHint, replaceCharacterToggle));
        characterModeCombo.Items.Add(new ComboBoxItem { Tag = "ink" });
        characterModeCombo.Items.Add(new ComboBoxItem { Tag = "unmasked" });
        characterModeCombo.Items.Add(new ComboBoxItem { Tag = "default" });
        characterModeCombo.SelectedIndex = 0;
        charStack.Children.Add(characterModeCombo);
        charCard.Child = charStack;
        rightColumn.Children.Add(charCard);

        columnsGrid.Children.Add(leftColumn);
        Grid.SetColumn(rightColumn, 1);
        columnsGrid.Children.Add(rightColumn);
        body.Children.Add(columnsGrid);

        var pagePanel = new PageContentPanel { Padding = new Thickness(28, 4, 28, 28) };
        pagePanel.Children.Add(body);

        var scrollViewer = new ScrollViewer
        {
            Content = pagePanel,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        Grid.SetRow(scrollViewer, 1);
        root.Children.Add(scrollViewer);

        Content = root;

        UpdateLanguage();
        UpdateAvailability();

        // Event hooks
        enabledToggle.Toggled += (_, _) => OnChanged();
        replaceWeaponToggle.Toggled += (_, _) => OnChanged();
        weaponModeCombo.SelectionChanged += (_, _) => OnChanged();
        replaceCharacterToggle.Toggled += (_, _) => OnChanged();
        characterModeCombo.SelectionChanged += (_, _) => OnChanged();
        replaceEffectsToggle.Toggled += (_, _) => OnChanged();
    }

    private void OnChanged()
    {
        if (_suppressEvents) return;
        UpdateAvailability();
        ConfigurationChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateAvailability()
    {
        bool master = enabledToggle.IsOn;
        replaceWeaponToggle.IsEnabled = master;
        weaponModeCombo.IsEnabled = master && replaceWeaponToggle.IsOn;
        replaceCharacterToggle.IsEnabled = master;
        characterModeCombo.IsEnabled = master && replaceCharacterToggle.IsOn;
        replaceEffectsToggle.IsEnabled = master;
    }

    internal void UpdateLanguage()
    {
        title.Text = T("管理员外观", "Administrator Appearance");
        subtitle.Text = T("覆写管理员武器、专属服装形态与战斗水墨剑光特效。", "Customize administrator weapon appearance, attire form, and ink blade combat VFX.");

        enabledToggle.Header = T("启用外观覆写", "Enable Appearance Override");
        masterRowTitle.Text = T("启用外观覆写", "Enable Appearance Override");
        masterRowHint.Text = T("总开关开启后，按下方各分项规则覆写管理员模型与特效。", "When enabled, overrides administrator weapon, attire, and VFX according to rules below.");

        weaponSectionTitle.Text = T("武器外观", "Weapon Appearance");
        replaceWeaponToggle.Header = T("替换宏愿外观", "Override Hongyuan Appearance");
        weaponRowTitle.Text = T("替换宏愿外观", "Override Hongyuan Appearance");
        weaponRowHint.Text = T("将管理员佩剑覆写为指定外观样式", "Override administrator sword model with selected style");
        weaponModeCombo.Header = T("样式选择", "Style");
        if (weaponModeCombo.Items.Count >= 2)
        {
            ((ComboBoxItem)weaponModeCombo.Items[0]).Content = T("宏愿 · 剧情水墨流光", "Hongyuan · Story Ink & Glow");
            ((ComboBoxItem)weaponModeCombo.Items[1]).Content = T("原版默认外观", "Vanilla Default");
        }

        characterSectionTitle.Text = T("管理员角色外观", "Administrator Character Appearance");
        replaceCharacterToggle.Header = T("替换角色外观", "Override Character Appearance");
        characterRowTitle.Text = T("替换角色外观", "Override Character Appearance");
        characterRowHint.Text = T("切换管理员在游戏内的服装与面具形态", "Switch administrator in-game attire and mask form");
        characterModeCombo.Header = T("形态选择", "Form");
        if (characterModeCombo.Items.Count >= 3)
        {
            ((ComboBoxItem)characterModeCombo.Items[0]).Content = T("剧情水墨专属服装", "Story Ink Attire");
            ((ComboBoxItem)characterModeCombo.Items[1]).Content = T("剧情摘面具形态", "Story Unmasked Form");
            ((ComboBoxItem)characterModeCombo.Items[2]).Content = T("原版默认形态", "Vanilla Default");
        }

        effectsSectionTitle.Text = T("战斗与剑光特效", "Combat & Blade VFX");
        replaceEffectsToggle.Header = T("替换为水墨剑气与打击特效", "Enable Ink Blade Slashing & Hit VFX");
        effectsRowTitle.Text = T("替换为水墨剑气与打击特效", "Enable Ink Blade Slashing & Hit VFX");
        effectsRowHint.Text = T("将普通剑光与命中效果替换为剧情水墨特效", "Replace standard blade slashes and hit effects with story ink VFX");
    }

    internal WeaponAppearanceConfiguration ReadConfiguration()
    {
        return new WeaponAppearanceConfiguration
        {
            Enabled = enabledToggle.IsOn,
            ReplaceWeapon = replaceWeaponToggle.IsOn,
            WeaponMode = ((weaponModeCombo.SelectedItem as ComboBoxItem)?.Tag as string) ?? "ink",
            ReplaceCharacter = replaceCharacterToggle.IsOn,
            CharacterMode = ((characterModeCombo.SelectedItem as ComboBoxItem)?.Tag as string) ?? "ink",
            ReplaceEffects = replaceEffectsToggle.IsOn,
            EffectsMode = replaceEffectsToggle.IsOn ? "ink" : "default"
        };
    }

    internal void ApplyConfiguration(WeaponAppearanceConfiguration config)
    {
        _suppressEvents = true;
        try
        {
            enabledToggle.IsOn = config.Enabled;
            replaceWeaponToggle.IsOn = config.ReplaceWeapon;

            for (int i = 0; i < weaponModeCombo.Items.Count; i++)
            {
                if (string.Equals(((ComboBoxItem)weaponModeCombo.Items[i]).Tag as string, config.WeaponMode, StringComparison.OrdinalIgnoreCase))
                {
                    weaponModeCombo.SelectedIndex = i;
                    break;
                }
            }

            replaceCharacterToggle.IsOn = config.ReplaceCharacter;
            for (int i = 0; i < characterModeCombo.Items.Count; i++)
            {
                if (string.Equals(((ComboBoxItem)characterModeCombo.Items[i]).Tag as string, config.CharacterMode, StringComparison.OrdinalIgnoreCase))
                {
                    characterModeCombo.SelectedIndex = i;
                    break;
                }
            }

            replaceEffectsToggle.IsOn = config.ReplaceEffects && !string.Equals(config.EffectsMode, "default", StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            _suppressEvents = false;
        }
        UpdateAvailability();
    }
}
