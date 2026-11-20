using System.Runtime.InteropServices;
using System.Text;
using BetterEndfieldNext.UI.Controls;
using BetterEndfieldNext.UI.Models;
using BetterEndfieldNext.UI.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using Windows.Storage.Pickers;
using WinRT.Interop;
namespace BetterEndfieldNext.UI.Views;
public sealed class MediaScreenPage : UserControl
{
    internal event EventHandler? ConfigurationChanged;
    private readonly ToggleSwitch enabled = new(), audio = new(), loop = new();
    private readonly ToggleSwitch pauseBgm = new() { Name = "MediaScreenPauseBgmToggle", IsOn = true };
    private readonly ToggleSwitch wallScreen = new() { Name = "MediaScreenWallToggle", IsOn = true }, deskScreen = new() { Name = "MediaScreenDeskToggle", IsOn = true };
    private readonly ComboBox deskFit = new() { Name = "MediaScreenDeskFitMode", HorizontalAlignment = HorizontalAlignment.Stretch };
    private readonly ComboBox source = new() { HorizontalAlignment = HorizontalAlignment.Stretch }, windows = new() { HorizontalAlignment = HorizontalAlignment.Stretch };
    private readonly TextBox hotkey = new() { Name = "MediaScreenToggleHotkeyBox", Text = "-", IsSpellCheckEnabled = false, Width = 140 };
    private readonly TextBox pauseHotkey = new() { Name = "MediaScreenPauseHotkeyBox", Text = "CTRL+-", IsSpellCheckEnabled = false, Width = 140 };
    private readonly TextBox controlHotkey = new() { Name = "MediaScreenControlHotkeyBox", Text = "ALT+-", IsSpellCheckEnabled = false, Width = 140 };
    private readonly TextBox path = new(), images = new() { AcceptsReturn = true, MinHeight = 96 }, target = new();
    private readonly NumberBox interval = Number(10, 1, 3600), gain = Number(1, 0, 2);
    private readonly NumberBox width = Number(1920, 64, MediaScreenConfiguration.MaxDimension), height = Number(1080, 64, MediaScreenConfiguration.MaxDimension), fps = Number(60, 1, MediaScreenConfiguration.MaxFps);
    private readonly ComboBox resolution = new() { Name = "MediaScreenResolutionPreset", HorizontalAlignment = HorizontalAlignment.Stretch }, frameRate = new() { Name = "MediaScreenFrameRatePreset", HorizontalAlignment = HorizontalAlignment.Stretch };
    private readonly Grid customResolution = new() { ColumnSpacing = 12 };
    private static readonly (int Width, int Height)[] Resolutions = [(1280, 720), (1920, 1080), (2560, 1440), (3840, 2160)];
    private static readonly int[] FrameRates = [30, 60, 90, 120];
    private readonly TextBlock message = new() { TextWrapping = TextWrapping.Wrap };
    private readonly Button pick = new(), refresh = new();
    private readonly TextBlock title = new() { FontSize = 24, FontWeight = Microsoft.UI.Text.FontWeights.Bold }, introduction = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock leftCardTitle = new() { FontSize = 16, FontWeight = Microsoft.UI.Text.FontWeights.Bold };
    private readonly TextBlock rightCardTitle = new() { FontSize = 16, FontWeight = Microsoft.UI.Text.FontWeights.Bold };
    private readonly TextBlock enabledRowTitle = new() { FontSize = 14, FontWeight = Microsoft.UI.Text.FontWeights.Bold };
    private readonly TextBlock wallRowTitle = new() { FontSize = 14, FontWeight = Microsoft.UI.Text.FontWeights.Bold };
    private readonly TextBlock deskRowTitle = new() { FontSize = 14, FontWeight = Microsoft.UI.Text.FontWeights.Bold };
    private readonly TextBlock loopRowTitle = new() { FontSize = 14, FontWeight = Microsoft.UI.Text.FontWeights.Bold };
    private readonly TextBlock audioRowTitle = new() { FontSize = 14, FontWeight = Microsoft.UI.Text.FontWeights.Bold };
    private readonly TextBlock pauseBgmRowTitle = new() { FontSize = 14, FontWeight = Microsoft.UI.Text.FontWeights.Bold };
    private readonly TextBlock hotkeyRowTitle = new() { FontSize = 14, FontWeight = Microsoft.UI.Text.FontWeights.Bold };
    private readonly TextBlock pauseHotkeyRowTitle = new() { FontSize = 14, FontWeight = Microsoft.UI.Text.FontWeights.Bold };
    private readonly TextBlock controlHotkeyRowTitle = new() { FontSize = 14, FontWeight = Microsoft.UI.Text.FontWeights.Bold };
    private readonly Grid audioRow, pauseBgmRow, filePathRow, windowRow, hotkeyRow, pauseHotkeyRow, controlHotkeyRow;
    private readonly Expander advanced = new() { HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Stretch };
    private ulong windowHandle; private uint windowPid;
    private static string T(string zh, string en) => LocalizationService.Instance.IsChinese ? zh : en;
    private static NumberBox Number(double value, double min, double max) => new()
    { Value = value, Minimum = min, Maximum = max, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact };
    private sealed record CaptureWindow(ulong Handle, uint Pid, string Title) { public override string ToString() => Title; }
    private string Source => (source.SelectedItem as ComboBoxItem)?.Tag as string ?? "image";

    private static Grid CreateSwitchRow(TextBlock label, ToggleSwitch toggle)
    {
        toggle.Style = (Style)Application.Current.Resources["CompactToggleSwitchStyle"];
        var row = new Grid { ColumnSpacing = 12, Padding = new Thickness(0, 2, 0, 2) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        label.VerticalAlignment = VerticalAlignment.Center;
        label.TextWrapping = TextWrapping.Wrap;
        row.Children.Add(label);
        Grid.SetColumn(toggle, 1);
        row.Children.Add(toggle);
        return row;
    }

    private static Grid CreateHotkeyRow(TextBlock label, TextBox box)
    {
        box.HeaderTemplate = new DataTemplate();
        var row = new Grid { ColumnSpacing = 12, Padding = new Thickness(0, 2, 0, 2) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        label.VerticalAlignment = VerticalAlignment.Center;
        label.TextWrapping = TextWrapping.Wrap;
        row.Children.Add(label);
        Grid.SetColumn(box, 1);
        row.Children.Add(box);
        return row;
    }

    public MediaScreenPage()
    {
        var sectionStyle = (Style)Application.Current.Resources["SectionStyle"];
        var hintStyle = (Style)Application.Current.Resources["FieldHintStyle"];
        introduction.Style = hintStyle;
        introduction.FontSize = 14;
        message.Style = hintStyle;

        var root = new Grid();
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        // Row 0: Fixed Page Header
        var headerStack = new StackPanel { Padding = new Thickness(28, 22, 28, 12), Spacing = 4 };
        headerStack.Children.Add(title);
        headerStack.Children.Add(introduction);
        root.Children.Add(headerStack);

        // Setup combo items
        foreach (var mode in new[] { "fit", "crop", "stretch" }) deskFit.Items.Add(new ComboBoxItem { Tag = mode });
        deskFit.SelectedIndex = 0;
        foreach (var item in new[] { ("image", T("图片", "Image")), ("slideshow", T("图片轮播", "Slideshow")), ("video", T("视频文件", "Video file")), ("window", T("本地窗口", "Local window")) })
            source.Items.Add(new ComboBoxItem { Tag = item.Item1, Content = item.Item2 });
        source.SelectedIndex = 0;

        pick.Click += Pick;
        pick.VerticalAlignment = VerticalAlignment.Bottom;
        refresh.Click += (_, _) => RefreshWindows();
        refresh.VerticalAlignment = VerticalAlignment.Bottom;
        audio.IsOn = true;
        loop.IsOn = true;

        for (int i = 0; i < Resolutions.Length; i++) {
            var size = Resolutions[i];
            string label = size.Height == 2160 ? "4K" : $"{size.Height}p";
            resolution.Items.Add(new ComboBoxItem { Content = $"{label} · {size.Width} × {size.Height}" });
        }
        resolution.Items.Add(new ComboBoxItem()); resolution.SelectedIndex = 1;
        foreach (int rate in FrameRates) frameRate.Items.Add(new ComboBoxItem { Content = $"{rate} FPS" });
        frameRate.Items.Add(new ComboBoxItem()); frameRate.SelectedIndex = 1;

        customResolution.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        customResolution.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        customResolution.Children.Add(width);
        Grid.SetColumn(height, 1);
        customResolution.Children.Add(height);

        // Left Card: Projection Control & Display
        var leftStack = new StackPanel { Spacing = 12 };
        leftStack.Children.Add(leftCardTitle);
        leftStack.Children.Add(CreateSwitchRow(enabledRowTitle, enabled));
        leftStack.Children.Add(CreateSwitchRow(wallRowTitle, wallScreen));
        leftStack.Children.Add(CreateSwitchRow(deskRowTitle, deskScreen));
        leftStack.Children.Add(deskFit);
        leftStack.Children.Add(CreateSwitchRow(loopRowTitle, loop));
        audioRow = CreateSwitchRow(audioRowTitle, audio);
        leftStack.Children.Add(audioRow);
        leftStack.Children.Add(gain);
        pauseBgmRow = CreateSwitchRow(pauseBgmRowTitle, pauseBgm);
        leftStack.Children.Add(pauseBgmRow);
        leftStack.Children.Add(interval);
        var leftCard = new Border { Style = sectionStyle, VerticalAlignment = VerticalAlignment.Top, Child = leftStack };

        // Right Card: Media Source & Output
        var rightStack = new StackPanel { Spacing = 12 };
        rightStack.Children.Add(rightCardTitle);
        rightStack.Children.Add(source);

        filePathRow = new Grid { ColumnSpacing = 8 };
        filePathRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        filePathRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        filePathRow.Children.Add(path);
        Grid.SetColumn(pick, 1);
        filePathRow.Children.Add(pick);
        rightStack.Children.Add(filePathRow);
        rightStack.Children.Add(images);

        windowRow = new Grid { ColumnSpacing = 8 };
        windowRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        windowRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        windowRow.Children.Add(windows);
        Grid.SetColumn(refresh, 1);
        windowRow.Children.Add(refresh);
        rightStack.Children.Add(windowRow);

        rightStack.Children.Add(resolution);
        rightStack.Children.Add(customResolution);
        rightStack.Children.Add(frameRate);
        rightStack.Children.Add(fps);

        hotkeyRow = CreateHotkeyRow(hotkeyRowTitle, hotkey);
        pauseHotkeyRow = CreateHotkeyRow(pauseHotkeyRowTitle, pauseHotkey);
        controlHotkeyRow = CreateHotkeyRow(controlHotkeyRowTitle, controlHotkey);
        rightStack.Children.Add(hotkeyRow);
        rightStack.Children.Add(pauseHotkeyRow);
        rightStack.Children.Add(controlHotkeyRow);
        var rightCard = new Border { Style = sectionStyle, VerticalAlignment = VerticalAlignment.Top, Child = rightStack };

        hotkey.PreviewKeyDown += CaptureHotkey; pauseHotkey.PreviewKeyDown += CaptureHotkey; controlHotkey.PreviewKeyDown += CaptureHotkey;
        target.Header = T("屏幕对象路径", "Screen object path"); target.Text = "ReceptionRoomImagePoster";
        advanced.Content = target;

        var columnsGrid = new Grid { ColumnSpacing = 18 };
        columnsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        columnsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        columnsGrid.Children.Add(leftCard);
        Grid.SetColumn(rightCard, 1);
        columnsGrid.Children.Add(rightCard);

        var body = new StackPanel { Spacing = 16 };
        body.Children.Add(columnsGrid);
        body.Children.Add(advanced);
        body.Children.Add(message);

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

        source.SelectionChanged += (_, _) => Availability();
        resolution.SelectionChanged += (_, _) => {
            if (resolution.SelectedIndex >= 0 && resolution.SelectedIndex < Resolutions.Length) {
                var size = Resolutions[resolution.SelectedIndex]; width.Value = size.Width; height.Value = size.Height;
            }
            Availability(); ConfigurationChanged?.Invoke(this, EventArgs.Empty);
        };
        frameRate.SelectionChanged += (_, _) => {
            if (frameRate.SelectedIndex >= 0 && frameRate.SelectedIndex < FrameRates.Length) fps.Value = FrameRates[frameRate.SelectedIndex];
            Availability(); ConfigurationChanged?.Invoke(this, EventArgs.Empty);
        };
        windows.SelectionChanged += (_, _) => { if (windows.SelectedItem is CaptureWindow w) { windowHandle = w.Handle; windowPid = w.Pid; } };
        Availability(); UpdateLanguage();
        foreach (var item in new[] { enabled, audio, loop, pauseBgm, wallScreen, deskScreen }) item.Toggled += (_, _) => { Availability(); ConfigurationChanged?.Invoke(this, EventArgs.Empty); };
        foreach (var item in new[] { path, images, target, hotkey, pauseHotkey, controlHotkey }) item.TextChanged += (_, _) => ConfigurationChanged?.Invoke(this, EventArgs.Empty);
        foreach (var item in new[] { interval, gain, width, height, fps }) item.ValueChanged += (_, _) => ConfigurationChanged?.Invoke(this, EventArgs.Empty);
        foreach (var item in new[] { source, windows, deskFit }) item.SelectionChanged += (_, _) => ConfigurationChanged?.Invoke(this, EventArgs.Empty);
    }
    private void CaptureHotkey(object sender, KeyRoutedEventArgs args)
    {
        args.Handled = true;
        if (sender is not TextBox box || HotkeyService.IsModifier(args.Key)) return;
        if (args.Key is VirtualKey.Back or VirtualKey.Delete) { box.Text = ""; return; }
        string captured = HotkeyService.Capture(args.Key, args.KeyStatus.IsExtendedKey);
        if (captured.Length > 0) { box.Text = captured; box.SelectAll(); }
    }
    private static string ReadHotkey(TextBox box)
    {
        if (string.IsNullOrWhiteSpace(box.Text)) return "NONE";
        if (!HotkeyService.TryNormalize(box.Text, out string value))
            throw new InvalidDataException(T("快捷键无效", "Invalid hotkey"));
        return value;
    }
    internal void UpdateLanguage()
    {
        title.Text = T("会客室媒体屏", "Guest Room Media Screen");
        introduction.Text = T("进入会客室后按启停快捷键开始或恢复原画。默认启停键为主键盘 -。", "Use the toggle key in the guest room to start or restore. The default toggle is main-keyboard -.");
        leftCardTitle.Text = T("投屏控制与画面", "Projection & Display");
        rightCardTitle.Text = T("媒体源与输出", "Media Source & Output");
        enabled.Header = enabledRowTitle.Text = T("启用媒体屏", "Enable media screen");
        source.Header = T("来源", "Source");
        wallScreen.Header = wallRowTitle.Text = T("大屏投屏（墙面轮播屏）", "Project to large wall screens");
        deskScreen.Header = deskRowTitle.Text = T("小屏投屏（桌面电脑）", "Project to small desk monitors");
        deskFit.Header = T("小屏画面适配", "Desk monitor image fit");
        string[] fitLabels = [T("完整显示（保持比例，留黑边）", "Fit (keep aspect ratio, letterbox)"), T("裁剪填满（保持比例）", "Fill (keep aspect ratio, crop)"), T("拉伸填满", "Stretch")];
        for (int i = 0; i < deskFit.Items.Count; ++i) ((ComboBoxItem)deskFit.Items[i]).Content = fitLabels[i];
        string[] labels = [T("图片", "Image"), T("图片轮播", "Slideshow"), T("视频文件", "Video file"), T("窗口", "Window")];
        for (int i = 0; i < source.Items.Count; ++i) ((ComboBoxItem)source.Items[i]).Content = labels[i];
        path.Header = T("文件路径", "File path"); images.Header = T("轮播图片（每行一个路径）", "Slideshow images (one path per line)");
        pick.Content = T("选择文件", "Choose files"); windows.Header = T("窗口", "Window"); refresh.Content = T("刷新窗口列表", "Refresh windows");
        interval.Header = T("轮播间隔（秒）", "Slide interval (seconds)");
        audio.Header = audioRowTitle.Text = T("播放视频声音", "Play video audio");
        gain.Header = T("音量", "Volume");
        loop.Header = loopRowTitle.Text = T("循环播放", "Loop");
        pauseBgm.Header = pauseBgmRowTitle.Text = T("窗口投屏时暂停游戏 BGM", "Pause game BGM during window projection");
        resolution.Header = T("输出分辨率", "Output resolution"); frameRate.Header = T("窗口捕获帧率", "Window capture FPS");
        ((ComboBoxItem)resolution.Items[Resolutions.Length]).Content = T("自定义", "Custom");
        ((ComboBoxItem)frameRate.Items[FrameRates.Length]).Content = T("自定义", "Custom");
        controlHotkey.PlaceholderText = pauseHotkey.PlaceholderText = hotkey.PlaceholderText = T("未设置", "Not set");
        width.Header = T("画布宽度", "Canvas width"); height.Header = T("画布高度", "Canvas height"); fps.Header = T("窗口捕获帧率", "Window capture FPS");
        hotkey.Header = hotkeyRowTitle.Text = T("启停快捷键", "Start/stop key");
        target.Header = T("屏幕对象路径", "Screen object path"); advanced.Header = T("高级设置", "Advanced");
        pauseHotkey.Header = pauseHotkeyRowTitle.Text = T("视频暂停／继续快捷键", "Video pause/resume key");
        controlHotkey.Header = controlHotkeyRowTitle.Text = T("窗口操控快捷键", "Window control key");
    }
    private void Availability()
    {
        deskFit.IsEnabled = deskScreen.IsOn;
        path.Visibility = Source is "image" or "video" ? Visibility.Visible : Visibility.Collapsed;
        images.Visibility = interval.Visibility = Source == "slideshow" ? Visibility.Visible : Visibility.Collapsed;
        pick.Visibility = filePathRow.Visibility = Source == "window" ? Visibility.Collapsed : Visibility.Visible;
        windows.Visibility = refresh.Visibility = windowRow.Visibility = Source == "window" ? Visibility.Visible : Visibility.Collapsed;
        audio.Visibility = audioRow.Visibility = gain.Visibility = Source == "video" ? Visibility.Visible : Visibility.Collapsed;
        pauseHotkey.Visibility = pauseHotkeyRow.Visibility = Source == "video" ? Visibility.Visible : Visibility.Collapsed;
        controlHotkey.Visibility = controlHotkeyRow.Visibility = Source == "window" ? Visibility.Visible : Visibility.Collapsed;
        pauseBgm.Visibility = pauseBgmRow.Visibility = frameRate.Visibility = Source == "window" ? Visibility.Visible : Visibility.Collapsed;
        customResolution.Visibility = resolution.SelectedIndex == Resolutions.Length ? Visibility.Visible : Visibility.Collapsed;
        fps.Visibility = Source == "window" && frameRate.SelectedIndex == FrameRates.Length ? Visibility.Visible : Visibility.Collapsed;
    }
    private static int Integer(NumberBox control, int fallback) => double.IsFinite(control.Value) ? (int)control.Value : fallback;
    internal MediaScreenConfiguration ReadConfiguration()
    {
        var config = new MediaScreenConfiguration {
        Enabled = enabled.IsOn, Source = Source, SourcePath = path.Text.Trim(),
        WallScreenEnabled = wallScreen.IsOn, DeskScreenEnabled = deskScreen.IsOn, DeskFitMode = ((deskFit.SelectedItem as ComboBoxItem)?.Tag as string) ?? "fit",
        ImagePaths = string.Join('|', images.Text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)),
        TargetObject = target.Text.Trim(), AudioEnabled = audio.IsOn, PauseWindowBgm = pauseBgm.IsOn, Loop = loop.IsOn,
        Gain = double.IsFinite(gain.Value) ? gain.Value : 1, SlideInterval = double.IsFinite(interval.Value) ? interval.Value : 10,
        Width = Integer(width, 1920), Height = Integer(height, 1080), Fps = Integer(fps, 60),
        Hotkey = ReadHotkey(hotkey), PauseHotkey = ReadHotkey(pauseHotkey), ControlHotkey = ReadHotkey(controlHotkey), WindowHandle = windowHandle, WindowPid = windowPid
        };
        if ((long)config.Width * config.Height > MediaScreenConfiguration.MaxPixels)
            throw new InvalidDataException(T("分辨率超过 4K 像素上限", "Resolution exceeds the 4K pixel limit"));
        string[] bindings = [config.Hotkey, config.PauseHotkey, config.ControlHotkey];
        if (bindings.Where(key => key != "NONE").Distinct().Count() != bindings.Count(key => key != "NONE"))
            throw new InvalidDataException(T("媒体屏快捷键不能重复", "Media screen shortcuts must be distinct"));
        return config;
    }
    internal void ApplyConfiguration(MediaScreenConfiguration config)
    {
        enabled.IsOn = config.Enabled; source.SelectedIndex = Array.IndexOf(new[] { "image", "slideshow", "video", "window" }, config.Source);
        path.Text = config.SourcePath; images.Text = config.ImagePaths.Replace("|", Environment.NewLine);
        target.Text = config.TargetObject; audio.IsOn = config.AudioEnabled; pauseBgm.IsOn = config.PauseWindowBgm; loop.IsOn = config.Loop;
        wallScreen.IsOn = config.WallScreenEnabled; deskScreen.IsOn = config.DeskScreenEnabled;
        deskFit.SelectedIndex = config.DeskFitMode switch { "crop" => 1, "stretch" => 2, _ => 0 };
        gain.Value = config.Gain; interval.Value = config.SlideInterval; width.Value = config.Width; height.Value = config.Height; fps.Value = config.Fps;
        int preset = Array.IndexOf(Resolutions, (config.Width, config.Height)); resolution.SelectedIndex = preset < 0 ? Resolutions.Length : preset;
        preset = Array.IndexOf(FrameRates, config.Fps); frameRate.SelectedIndex = preset < 0 ? FrameRates.Length : preset;
        hotkey.Text = config.Hotkey.Replace("PLUS", "=", StringComparison.Ordinal);
        pauseHotkey.Text = config.PauseHotkey is "NONE" or "OFF" or "DISABLED" ? "" : config.PauseHotkey.Replace("PLUS", "=", StringComparison.Ordinal);
        controlHotkey.Text = config.ControlHotkey is "NONE" or "OFF" or "DISABLED" ? "" : config.ControlHotkey.Replace("PLUS", "=", StringComparison.Ordinal);
        windowHandle = config.WindowHandle; windowPid = config.WindowPid; Availability();
    }
    private async void Pick(object sender, RoutedEventArgs args)
    {
        try {
            var picker = new FileOpenPicker();
            foreach (var extension in Source == "video" ? new[] { ".mp4" } : new[] { ".png", ".jpg", ".jpeg" }) picker.FileTypeFilter.Add(extension);
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(App.MainWindowInstance));
            if (Source == "slideshow") {
                var files = await picker.PickMultipleFilesAsync(); if (files.Count > 0) images.Text = string.Join(Environment.NewLine, files.Select(file => file.Path));
            } else { var file = await picker.PickSingleFileAsync(); if (file is not null) path.Text = file.Path; }
            message.Text = "";
        } catch (Exception error) { message.Text = error.Message; }
    }
    private void RefreshWindows()
    {
        var list = new List<CaptureWindow>();
        EnumWindows((handle, _) => {
            if (!IsWindowVisible(handle)) return true;
            var title = new StringBuilder(512); GetWindowText(handle, title, title.Capacity); GetWindowThreadProcessId(handle, out uint pid);
            if (pid != (uint)Environment.ProcessId && title.Length > 0) list.Add(new(unchecked((ulong)handle.ToInt64()), pid, title.ToString()));
            return true;
        }, 0);
        windows.ItemsSource = list.OrderBy(item => item.Title).ToList();
        windows.SelectedItem = list.FirstOrDefault(item => item.Handle == windowHandle && item.Pid == windowPid);
        message.Text = list.Count == 0 ? T("没有可选窗口", "No windows available") : "";
    }
    private delegate bool EnumCallback(nint handle, nint state);
    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumCallback callback, nint state);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(nint handle);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowText(nint handle, StringBuilder text, int count);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(nint handle, out uint process);
}
