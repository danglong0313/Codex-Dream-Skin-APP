using CodexDreamSkinStudio.Models;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodexDreamSkinStudio
{
  internal sealed class MainForm : Form
  {
    private enum StudioPage
    {
      Gallery,
      CurrentTheme,
      Recovery,
      About
    }

    private const int HeaderHeight = 62;
    private const int SidebarWidth = 250;
    private const int ResizeBorder = 7;

    private readonly EngineClient _engine = new EngineClient();
    private readonly GradientPanel _titleBar;
    private readonly GradientPanel _sidebar;
    private readonly GradientPanel _content;
    private readonly Label _pageTitle;
    private readonly Label _pageSubtitle;
    private readonly StatusPill _statusPill;
    private readonly HeroPanel _hero;
    private readonly ThemeCardPanel _themeCard;
    private readonly RoundedPanel _actionCard;
    private readonly Label _actionTitle;
    private readonly Label _actionMessage;
    private readonly Label _detailMessage;
    private readonly ProgressBar _progress;
    private readonly RoundedPanel _infoCard;
    private readonly Label _infoTitle;
    private readonly Label _infoBody;
    private readonly Label _infoNote;
    private readonly RoundedButton _applyButton;
    private readonly RoundedButton _reapplyButton;
    private readonly RoundedButton _verifyButton;
    private readonly RoundedButton _restoreButton;
    private readonly RoundedButton _logButton;
    private readonly NotifyIcon _notifyIcon;
    private readonly Timer _statusTimer;
    private readonly Image _artwork;
    private Button _galleryNavButton;
    private Button _currentNavButton;
    private Button _recoveryNavButton;
    private Button _aboutNavButton;
    private RoundedButton _maximizeButton;
    private StudioPage _activePage = StudioPage.Gallery;
    private bool _busy;
    private bool _allowExit;
    private DateTime _holdActionResultUntil = DateTime.MinValue;

    public MainForm()
    {
      Text = "Codex Dream Skin Studio";
      StartPosition = FormStartPosition.CenterScreen;
      FormBorderStyle = FormBorderStyle.None;
      MinimumSize = new Size(1280, 820);
      Size = PreferredWindowSize();
      BackColor = Color.FromArgb(235, 247, 251);
      Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular);
      Icon = SystemIcons.Application;
      SetStyle(ControlStyles.ResizeRedraw, true);

      _artwork = LoadArtwork();

      _titleBar = new GradientPanel
      {
        GradientStart = Color.FromArgb(251, 255, 255),
        GradientEnd = Color.FromArgb(232, 237, 255),
        GradientMode = LinearGradientMode.Horizontal
      };
      _titleBar.MouseDown += TitleBarMouseDown;
      Controls.Add(_titleBar);
      BuildTitleBar();

      _sidebar = new GradientPanel
      {
        GradientStart = Color.FromArgb(245, 255, 255),
        GradientEnd = Color.FromArgb(232, 244, 255),
        GradientMode = LinearGradientMode.Vertical
      };
      Controls.Add(_sidebar);
      BuildSidebar();

      _content = new GradientPanel
      {
        GradientStart = Color.FromArgb(247, 254, 255),
        GradientEnd = Color.FromArgb(237, 248, 255),
        GradientMode = LinearGradientMode.Vertical
      };
      Controls.Add(_content);

      _pageTitle = MakeLabel("主题画廊", 20F, FontStyle.Bold, Color.FromArgb(30, 91, 113));
      _pageSubtitle = MakeLabel("选择喜欢的视觉风格，一键应用到 Codex。", 9F, FontStyle.Regular, Color.FromArgb(112, 148, 162));
      _pageTitle.AutoSize = true;
      _pageSubtitle.AutoSize = true;
      _statusPill = new StatusPill();
      _content.Controls.Add(_pageTitle);
      _content.Controls.Add(_pageSubtitle);
      _content.Controls.Add(_statusPill);

      _hero = new HeroPanel { Artwork = _artwork };
      _content.Controls.Add(_hero);

      _themeCard = new ThemeCardPanel
      {
        Artwork = _artwork,
        Radius = 20,
        BorderColor = Color.FromArgb(177, 224, 234),
        BackColor = Color.FromArgb(248, 254, 255)
      };
      _content.Controls.Add(_themeCard);

      _actionCard = new RoundedPanel
      {
        Radius = 20,
        BorderColor = Color.FromArgb(177, 224, 234),
        BackColor = Color.FromArgb(248, 254, 255)
      };
      _content.Controls.Add(_actionCard);

      _infoCard = new RoundedPanel
      {
        Radius = 20,
        BorderColor = Color.FromArgb(177, 224, 234),
        BackColor = Color.FromArgb(248, 254, 255),
        Visible = false
      };
      _infoTitle = MakeLabel("主题安全说明", 15F, FontStyle.Bold, Color.FromArgb(32, 91, 111));
      _infoBody = MakeLabel(string.Empty, 10F, FontStyle.Regular, Color.FromArgb(66, 113, 132));
      _infoNote = MakeLabel(string.Empty, 8.5F, FontStyle.Regular, Color.FromArgb(118, 153, 166));
      _infoCard.Controls.Add(_infoTitle);
      _infoCard.Controls.Add(_infoBody);
      _infoCard.Controls.Add(_infoNote);
      _content.Controls.Add(_infoCard);

      _actionTitle = MakeLabel("准备应用 Miku Aqua 01", 13F, FontStyle.Bold, Color.FromArgb(32, 91, 111));
      _actionMessage = MakeLabel("正在检查 Codex 和换肤引擎状态…", 9F, FontStyle.Regular, Color.FromArgb(75, 125, 142));
      _detailMessage = MakeLabel("所有操作都通过本机回环连接完成，不修改官方程序文件。", 8F, FontStyle.Regular, Color.FromArgb(127, 158, 169));
      _progress = new ProgressBar { Style = ProgressBarStyle.Marquee, MarqueeAnimationSpeed = 24, Visible = false };

      _applyButton = MakeButton("应用并启动", Color.FromArgb(26, 190, 196), Color.White);
      _reapplyButton = MakeButton("重新应用", Color.FromArgb(225, 242, 255), Color.FromArgb(60, 104, 157));
      _verifyButton = MakeButton("验证", Color.FromArgb(236, 232, 255), Color.FromArgb(100, 83, 167));
      _restoreButton = MakeButton("恢复官方", Color.FromArgb(255, 235, 246), Color.FromArgb(165, 75, 126));
      _logButton = MakeButton("打开日志", Color.FromArgb(239, 249, 251), Color.FromArgb(75, 119, 134));

      _applyButton.Click += async (sender, args) => await ApplyAsync();
      _reapplyButton.Click += async (sender, args) => await RunActionAsync("Reapply", "正在重新应用主题…");
      _verifyButton.Click += async (sender, args) => await RunActionAsync("Verify", "正在验证主题状态…");
      _restoreButton.Click += async (sender, args) => await RestoreAsync();
      _logButton.Click += (sender, args) => OpenLogs();

      _actionCard.Controls.Add(_actionTitle);
      _actionCard.Controls.Add(_actionMessage);
      _actionCard.Controls.Add(_detailMessage);
      _actionCard.Controls.Add(_progress);
      _actionCard.Controls.Add(_applyButton);
      _actionCard.Controls.Add(_reapplyButton);
      _actionCard.Controls.Add(_verifyButton);
      _actionCard.Controls.Add(_restoreButton);
      _actionCard.Controls.Add(_logButton);

      _notifyIcon = BuildNotifyIcon();
      _statusTimer = new Timer { Interval = 4000 };
      _statusTimer.Tick += async (sender, args) => await RefreshStatusAsync();

      Resize += (sender, args) => LayoutSurface();
      Shown += async (sender, args) =>
      {
        LayoutSurface();
        _statusTimer.Start();
        await RefreshStatusAsync();
      };
      FormClosing += MainFormClosing;
      FormClosed += MainFormClosed;
      SwitchPage(StudioPage.Gallery);
      LayoutSurface();
    }

    private void BuildTitleBar()
    {
      var brand = MakeLabel("♫  CODEX DREAM SKIN", 10F, FontStyle.Bold, Color.FromArgb(36, 119, 141));
      brand.AutoSize = true;
      brand.Location = new Point(24, 21);
      brand.MouseDown += TitleBarMouseDown;
      _titleBar.Controls.Add(brand);

      var edition = MakeLabel("MIKU EDITION  ·  PREVIEW 0.1", 8F, FontStyle.Bold, Color.FromArgb(117, 132, 178));
      edition.AutoSize = true;
      edition.MouseDown += TitleBarMouseDown;
      _titleBar.Controls.Add(edition);

      var minimize = MakeCaptionButton("—");
      _maximizeButton = MakeCaptionButton("▢");
      var close = MakeCaptionButton("×");
      minimize.Click += (sender, args) => WindowState = FormWindowState.Minimized;
      _maximizeButton.Click += (sender, args) => ToggleMaximize();
      close.Click += (sender, args) => HideToTray();
      _titleBar.Controls.Add(minimize);
      _titleBar.Controls.Add(_maximizeButton);
      _titleBar.Controls.Add(close);
      _titleBar.DoubleClick += (sender, args) => ToggleMaximize();

      _titleBar.Resize += (sender, args) =>
      {
        close.Bounds = new Rectangle(_titleBar.Width - 54, 9, 42, 42);
        _maximizeButton.Bounds = new Rectangle(_titleBar.Width - 102, 9, 42, 42);
        minimize.Bounds = new Rectangle(_titleBar.Width - 150, 9, 42, 42);
        edition.Location = new Point(Math.Max(250, _titleBar.Width - edition.Width - 170), 23);
      };
    }

    private void BuildSidebar()
    {
      var brand = MakeLabel("Miku Studio", 14.5F, FontStyle.Bold | FontStyle.Italic, Color.FromArgb(26, 166, 177));
      brand.AutoSize = true;
      brand.Location = new Point(24, 31);
      _sidebar.Controls.Add(brand);

      var subtitle = MakeLabel("你的 Codex 主题衣橱  ♡", 8F, FontStyle.Regular, Color.FromArgb(112, 151, 165));
      subtitle.AutoSize = true;
      subtitle.Location = new Point(25, 70);
      _sidebar.Controls.Add(subtitle);

      var divider = new Panel { BackColor = Color.FromArgb(198, 230, 237), Location = new Point(22, 105), Size = new Size(206, 1) };
      _sidebar.Controls.Add(divider);

      _galleryNavButton = MakeNavButton("✦   主题画廊", true);
      _currentNavButton = MakeNavButton("♫   当前主题", false);
      _recoveryNavButton = MakeNavButton("↻   启动与恢复", false);
      _aboutNavButton = MakeNavButton("♡   关于与安全", false);
      _galleryNavButton.Location = new Point(16, 126);
      _currentNavButton.Location = new Point(16, 180);
      _recoveryNavButton.Location = new Point(16, 234);
      _aboutNavButton.Location = new Point(16, 288);
      _galleryNavButton.Click += (sender, args) => SwitchPage(StudioPage.Gallery);
      _currentNavButton.Click += (sender, args) => SwitchPage(StudioPage.CurrentTheme);
      _recoveryNavButton.Click += (sender, args) => SwitchPage(StudioPage.Recovery);
      _aboutNavButton.Click += (sender, args) => SwitchPage(StudioPage.About);
      _sidebar.Controls.Add(_galleryNavButton);
      _sidebar.Controls.Add(_currentNavButton);
      _sidebar.Controls.Add(_recoveryNavButton);
      _sidebar.Controls.Add(_aboutNavButton);

      var safety = new RoundedPanel
      {
        Radius = 17,
        BorderColor = Color.FromArgb(187, 226, 235),
        BackColor = Color.FromArgb(240, 253, 253),
        Location = new Point(18, 370),
        Size = new Size(214, 150)
      };
      var safetyTitle = MakeLabel("✓  安全换肤", 10F, FontStyle.Bold, Color.FromArgb(35, 133, 146));
      safetyTitle.Location = new Point(18, 18);
      safetyTitle.Size = new Size(175, 24);
      var safetyCopy = MakeLabel("本机回环 CDP\n不修改 app.asar\n随时恢复官方外观", 8.5F, FontStyle.Regular, Color.FromArgb(83, 128, 143));
      safetyCopy.Location = new Point(19, 53);
      safetyCopy.Size = new Size(175, 76);
      safety.Controls.Add(safetyTitle);
      safety.Controls.Add(safetyCopy);
      _sidebar.Controls.Add(safety);

      var trayHint = MakeLabel("关闭窗口会收至系统托盘", 8F, FontStyle.Regular, Color.FromArgb(125, 154, 166));
      trayHint.AutoSize = true;
      _sidebar.Controls.Add(trayHint);
      _sidebar.Resize += (sender, args) => trayHint.Location = new Point(24, _sidebar.Height - 43);
    }

    private void LayoutSurface()
    {
      if (ClientSize.Width <= 0 || ClientSize.Height <= 0) return;
      _titleBar.Bounds = new Rectangle(0, 0, ClientSize.Width, HeaderHeight);
      _sidebar.Bounds = new Rectangle(0, HeaderHeight, SidebarWidth, Math.Max(0, ClientSize.Height - HeaderHeight));
      _content.Bounds = new Rectangle(SidebarWidth, HeaderHeight, Math.Max(0, ClientSize.Width - SidebarWidth), Math.Max(0, ClientSize.Height - HeaderHeight));

      var margin = Math.Max(34, Math.Min(52, _content.Width / 25));
      var usableWidth = Math.Max(400, _content.Width - margin * 2);
      _pageTitle.Location = new Point(margin, 22);
      _pageSubtitle.Location = new Point(margin, 68);
      _statusPill.Location = new Point(_content.Width - margin - _statusPill.Width, 34);

      var bodyTop = 116;
      var bottomMargin = 34;
      var columnGap = 24;
      var bodyHeight = Math.Max(300, _content.Height - bodyTop - bottomMargin);

      if (_activePage == StudioPage.Gallery)
      {
        var lowerGap = 24;
        var lowerHeight = Math.Max(230, Math.Min(270, bodyHeight / 3));
        var heroHeight = Math.Max(300, bodyHeight - lowerGap - lowerHeight);
        _hero.Bounds = new Rectangle(margin, bodyTop, usableWidth, heroHeight);

        var lowerTop = bodyTop + heroHeight + lowerGap;
        var themeWidth = Math.Max(410, (int)(usableWidth * 0.38));
        _themeCard.Bounds = new Rectangle(margin, lowerTop, themeWidth, lowerHeight);
        _actionCard.Bounds = new Rectangle(margin + themeWidth + columnGap, lowerTop, Math.Max(430, usableWidth - themeWidth - columnGap), lowerHeight);
      }
      else if (_activePage == StudioPage.CurrentTheme)
      {
        var rowGap = 24;
        var upperHeight = Math.Max(280, Math.Min(330, (bodyHeight - rowGap) / 2));
        var themeWidth = Math.Max(430, (int)(usableWidth * 0.4));
        _themeCard.Bounds = new Rectangle(margin, bodyTop, themeWidth, upperHeight);
        _actionCard.Bounds = new Rectangle(margin + themeWidth + columnGap, bodyTop, Math.Max(460, usableWidth - themeWidth - columnGap), upperHeight);
        _infoCard.Bounds = new Rectangle(margin, bodyTop + upperHeight + rowGap, usableWidth, Math.Max(220, bodyHeight - upperHeight - rowGap));
      }
      else if (_activePage == StudioPage.Recovery)
      {
        var infoWidth = Math.Max(430, (int)(usableWidth * 0.42));
        _infoCard.Bounds = new Rectangle(margin, bodyTop, infoWidth, bodyHeight);
        _actionCard.Bounds = new Rectangle(margin + infoWidth + columnGap, bodyTop, Math.Max(480, usableWidth - infoWidth - columnGap), bodyHeight);
      }
      else
      {
        _infoCard.Bounds = new Rectangle(margin, bodyTop, usableWidth, bodyHeight);
      }

      LayoutActionCard();
      LayoutInfoCard();
      UpdateRegion();
    }

    private void LayoutInfoCard()
    {
      var width = _infoCard.Width;
      var height = _infoCard.Height;
      _infoTitle.Bounds = new Rectangle(34, 32, Math.Max(240, width - 68), 40);
      _infoBody.Bounds = new Rectangle(34, 88, Math.Max(240, width - 68), Math.Max(110, height - 174));
      _infoNote.Bounds = new Rectangle(34, Math.Max(150, height - 62), Math.Max(240, width - 68), 32);
    }

    private void LayoutActionCard()
    {
      var width = _actionCard.Width;
      var height = _actionCard.Height;
      _actionTitle.Bounds = new Rectangle(26, 22, Math.Max(220, width - 52), 29);
      _actionMessage.Bounds = new Rectangle(26, 57, Math.Max(220, width - 52), 23);
      _detailMessage.Bounds = new Rectangle(26, 84, Math.Max(220, width - 52), 22);
      _progress.Bounds = new Rectangle(26, 112, Math.Max(120, width - 52), 5);

      var gap = 10;
      var secondaryY = 128;
      var primaryY = height - 59;
      var buttonHeight = 39;
      _restoreButton.Bounds = new Rectangle(26, secondaryY, 110, buttonHeight);
      _verifyButton.Bounds = new Rectangle(136 + gap, secondaryY, 72, buttonHeight);
      _logButton.Bounds = new Rectangle(208 + gap * 2, secondaryY, 92, buttonHeight);
      var applyWidth = Math.Max(108, Math.Min(132, width / 5));
      var reapplyWidth = 100;
      _applyButton.Bounds = new Rectangle(width - 26 - applyWidth, primaryY, applyWidth, buttonHeight);
      _reapplyButton.Bounds = new Rectangle(_applyButton.Left - gap - reapplyWidth, primaryY, reapplyWidth, buttonHeight);
      _logButton.Visible = width >= 460;
    }

    private void SwitchPage(StudioPage page)
    {
      _activePage = page;
      SetNavButtonActive(_galleryNavButton, page == StudioPage.Gallery);
      SetNavButtonActive(_currentNavButton, page == StudioPage.CurrentTheme);
      SetNavButtonActive(_recoveryNavButton, page == StudioPage.Recovery);
      SetNavButtonActive(_aboutNavButton, page == StudioPage.About);

      _hero.Visible = page == StudioPage.Gallery;
      _themeCard.Visible = page == StudioPage.Gallery || page == StudioPage.CurrentTheme;
      _actionCard.Visible = page != StudioPage.About;
      _infoCard.Visible = page != StudioPage.Gallery;

      if (page == StudioPage.Gallery)
      {
        _pageTitle.Text = "主题画廊";
        _pageSubtitle.Text = "选择喜欢的视觉风格，一键应用到 Codex。";
      }
      else if (page == StudioPage.CurrentTheme)
      {
        _pageTitle.Text = "当前主题";
        _pageSubtitle.Text = "查看已安装主题、运行状态与 Codex 实时连接。";
        _infoTitle.Text = "Miku Aqua 01 · 当前配置";
        _infoBody.Text = "主题素材与注入引擎均随 Studio 本地保存。\n\n运行时仅连接 127.0.0.1 的调试端口，将样式应用到 Codex 主窗口；宠物、设置向导等辅助窗口会自动排除。\n\n右侧可以重新应用或验证主题，无需重复安装。";
        _infoNote.Text = "状态每 4 秒自动刷新；关闭 Studio 窗口后会收至系统托盘。";
      }
      else if (page == StudioPage.Recovery)
      {
        _pageTitle.Text = "启动与恢复";
        _pageSubtitle.Text = "重新应用、验证，或恢复换肤前的官方颜色配置。";
        _infoTitle.Text = "可逆的本地换肤";
        _infoBody.Text = "应用主题\n首次应用会备份当前外观颜色，然后启动本机主题守护。\n\n重新应用 / 验证\nCodex 已打开时可直接热更新，并检查主窗口是否正确注入。\n\n恢复官方\n精确恢复应用前的颜色配置，并停止主题守护。";
        _infoNote.Text = "恢复操作会先请求确认，避免意外关闭尚未发送的 Codex 输入。";
      }
      else
      {
        _pageTitle.Text = "关于与安全";
        _pageSubtitle.Text = "了解 Studio 的工作方式与安全边界。";
        _infoTitle.Text = "Codex Dream Skin Studio · Miku Edition";
        _infoBody.Text = "这是 Codex Dream Skin 的 Windows 一键换肤预览版。\n\n✓ 通过本机回环 CDP 注入主题\n✓ 不修改 WindowsApps、app.asar 或官方签名\n✓ 不向远程服务器上传主题状态或颜色配置\n✓ 应用前保存用户原始外观，支持完整恢复\n✓ 仅对完整 Codex 主窗口启用背景与布局\n\n主题文件、脚本和运行日志都保存在本机。";
        _infoNote.Text = "项目仍处于 Preview 0.1；后续可继续加入更多内置主题与主题包管理。";
      }

      LayoutSurface();
    }

    private static void SetNavButtonActive(Button button, bool active)
    {
      if (button == null) return;
      button.Font = new Font("Microsoft YaHei UI", 9.5F, active ? FontStyle.Bold : FontStyle.Regular);
      button.ForeColor = active ? Color.FromArgb(27, 127, 146) : Color.FromArgb(70, 111, 130);
      button.BackColor = active ? Color.FromArgb(218, 247, 249) : Color.Transparent;
      button.Invalidate();
    }

    private async Task RefreshStatusAsync()
    {
      if (_busy || DateTime.UtcNow < _holdActionResultUntil) return;
      var result = await _engine.RunAsync("Status");
      RenderStatus(result, false);
    }

    private async Task ApplyAsync()
    {
      if (_busy) return;
      var current = await _engine.RunAsync("Status");
      if (current.CodexRunning && !current.SkinActive)
      {
        var choice = MessageBox.Show(
          "应用主题需要重新启动当前 Codex 窗口。未发送的输入可能会丢失，是否继续？",
          "应用 Miku Aqua 01",
          MessageBoxButtons.YesNo,
          MessageBoxIcon.Question);
        if (choice != DialogResult.Yes) return;
      }
      await RunActionAsync("Apply", "正在安装主题并启动 Codex…");
    }

    private async Task RestoreAsync()
    {
      if (_busy) return;
      var choice = MessageBox.Show(
        "将精确恢复应用主题前的颜色配置，并重新启动 Codex。未发送的输入可能会丢失，是否继续？",
        "恢复官方外观",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question);
      if (choice != DialogResult.Yes) return;
      await RunActionAsync("Restore", "正在移除主题并恢复官方外观…");
    }

    private async Task RunActionAsync(string action, string workingText)
    {
      SetBusy(true, workingText);
      EngineResult result;
      try
      {
        result = await _engine.RunAsync(action);
      }
      finally
      {
        SetBusy(false, null);
      }
      _holdActionResultUntil = DateTime.UtcNow.AddSeconds(8);
      RenderStatus(result, true);
    }

    private void SetBusy(bool busy, string message)
    {
      _busy = busy;
      _progress.Visible = busy;
      _applyButton.Enabled = !busy;
      _reapplyButton.Enabled = !busy;
      _verifyButton.Enabled = !busy;
      _restoreButton.Enabled = !busy;
      _logButton.Enabled = !busy;
      if (busy && !string.IsNullOrWhiteSpace(message))
      {
        _actionTitle.Text = message;
        _actionMessage.Text = "请稍候，Studio 正在与本机换肤引擎通信。";
        _statusPill.Text = "正在处理";
        _statusPill.DotColor = Color.FromArgb(112, 133, 225);
      }
    }

    private void RenderStatus(EngineResult result, bool actionResult)
    {
      if (result == null)
      {
        result = EngineResult.Failed("无法读取换肤状态。");
      }

      if (!result.Success)
      {
        _statusPill.Text = "需要处理";
        _statusPill.DotColor = Color.FromArgb(225, 103, 144);
        _statusPill.PillColor = Color.FromArgb(255, 238, 246);
        _statusPill.PillBorderColor = Color.FromArgb(242, 192, 216);
        var windowsSetupRequired = result.Message != null && result.Message.Contains("WINDOWS_SETUP_REQUIRED", StringComparison.OrdinalIgnoreCase);
        _actionTitle.Text = windowsSetupRequired ? "需要先完成一次 Windows 设置" : "换肤引擎未完成操作";
        _actionMessage.Text = windowsSetupRequired
          ? "请在 Codex 窗口点击“完成设置”，并在 UAC 中选择“是”。"
          : string.IsNullOrWhiteSpace(result.Message) ? "请打开日志查看详细信息。" : result.Message;
        _detailMessage.Text = windowsSetupRequired
          ? "完成后回到 Studio，再点击一次“应用并启动”；原颜色配置已自动还原。"
          : LastMeaningfulLine(result.Log, "可以重试，或先恢复官方外观。 ");
      }
      else if (result.SkinActive)
      {
        _statusPill.Text = "主题运行中";
        _statusPill.DotColor = Color.FromArgb(24, 190, 175);
        _statusPill.PillColor = Color.FromArgb(232, 251, 248);
        _statusPill.PillBorderColor = Color.FromArgb(166, 230, 220);
        _actionTitle.Text = actionResult ? TranslateMessage(result.Message) : "Miku Aqua 01 正在运行";
        _actionMessage.Text = "Codex 已通过本机回环连接加载主题，守护进程正在保持显示。";
        _detailMessage.Text = actionResult && result.Message != null && result.Message.Contains("verification passed", StringComparison.OrdinalIgnoreCase)
          ? "主窗口标记、交互穿透和页面布局检查均已通过。"
          : LastMeaningfulLine(result.Log, "宠物等辅助窗口会自动排除，不会被主题背景覆盖。");
      }
      else if (result.CodexRunning)
      {
        _statusPill.Text = "Codex 已打开";
        _statusPill.DotColor = Color.FromArgb(115, 145, 224);
        _statusPill.PillColor = Color.FromArgb(239, 242, 255);
        _statusPill.PillBorderColor = Color.FromArgb(201, 207, 242);
        _actionTitle.Text = actionResult ? TranslateMessage(result.Message) : "Codex 当前使用官方外观";
        _actionMessage.Text = "点击“应用并启动”即可切换到 Miku Aqua 01。";
        _detailMessage.Text = "应用时会安全重启 Codex，不修改官方安装目录。";
      }
      else
      {
        _statusPill.Text = result.Installed ? "已安装 · 待启动" : "准备就绪";
        _statusPill.DotColor = Color.FromArgb(75, 180, 197);
        _statusPill.PillColor = Color.FromArgb(234, 249, 252);
        _statusPill.PillBorderColor = Color.FromArgb(185, 228, 235);
        _actionTitle.Text = actionResult ? TranslateMessage(result.Message) : "准备应用 Miku Aqua 01";
        _actionMessage.Text = "点击“应用并启动”，Studio 会安装主题并打开 Codex。";
        _detailMessage.Text = "首次应用会备份外观设置，之后可以一键恢复。";
      }
      _statusPill.Invalidate();
    }

    private static string TranslateMessage(string message)
    {
      if (string.IsNullOrWhiteSpace(message)) return "操作已完成";
      if (message.Contains("applied and Codex", StringComparison.OrdinalIgnoreCase)) return "Miku Aqua 01 已应用并启动";
      if (message.Contains("reapplied", StringComparison.OrdinalIgnoreCase)) return "Miku Aqua 01 已重新应用";
      if (message.Contains("verification passed", StringComparison.OrdinalIgnoreCase)) return "主题验证通过";
      if (message.Contains("official appearance", StringComparison.OrdinalIgnoreCase)) return "已恢复官方外观";
      return message;
    }

    private static string LastMeaningfulLine(string text, string fallback)
    {
      if (string.IsNullOrWhiteSpace(text)) return fallback;
      var line = text
        .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
        .Select(value => value.Trim())
        .LastOrDefault(value => !string.IsNullOrWhiteSpace(value));
      if (string.IsNullOrWhiteSpace(line)) return fallback;
      return line.Length > 118 ? line.Substring(0, 115) + "…" : line;
    }

    private NotifyIcon BuildNotifyIcon()
    {
      var menu = new ContextMenuStrip();
      menu.Items.Add("打开 Studio", null, (sender, args) => ShowFromTray());
      menu.Items.Add(new ToolStripSeparator());
      menu.Items.Add("应用 Miku Aqua 01", null, async (sender, args) => await RunActionAsync("Apply", "正在安装主题并启动 Codex…"));
      menu.Items.Add("恢复官方外观", null, async (sender, args) => await RestoreAsync());
      menu.Items.Add(new ToolStripSeparator());
      menu.Items.Add("退出", null, (sender, args) =>
      {
        _allowExit = true;
        Application.Exit();
      });

      var icon = new NotifyIcon
      {
        Icon = SystemIcons.Application,
        Text = "Codex Dream Skin Studio",
        ContextMenuStrip = menu,
        Visible = true
      };
      icon.DoubleClick += (sender, args) => ShowFromTray();
      return icon;
    }

    private void HideToTray()
    {
      Hide();
      _notifyIcon.ShowBalloonTip(1800, "Codex Dream Skin Studio", "Studio 已收至托盘，主题守护不会中断。", ToolTipIcon.Info);
    }

    private void ShowFromTray()
    {
      Show();
      WindowState = FormWindowState.Normal;
      Activate();
      BringToFront();
    }

    private void OpenLogs()
    {
      var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CodexDreamSkin");
      Directory.CreateDirectory(path);
      Process.Start(new ProcessStartInfo("explorer.exe", "\"" + path + "\"") { UseShellExecute = true });
    }

    private void MainFormClosing(object sender, FormClosingEventArgs args)
    {
      if (_allowExit || args.CloseReason == CloseReason.WindowsShutDown) return;
      args.Cancel = true;
      HideToTray();
    }

    private void MainFormClosed(object sender, FormClosedEventArgs args)
    {
      _statusTimer.Stop();
      _statusTimer.Dispose();
      _notifyIcon.Visible = false;
      _notifyIcon.Dispose();
      _artwork?.Dispose();
    }

    private static Image LoadArtwork()
    {
      var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Themes", "miku-aqua", "background.png");
      if (!File.Exists(path)) return null;
      using (var stream = File.OpenRead(path))
      using (var image = Image.FromStream(stream))
      {
        return new Bitmap(image);
      }
    }

    private static Label MakeLabel(string text, float size, FontStyle style, Color color)
    {
      return new Label
      {
        Text = text,
        Font = new Font("Microsoft YaHei UI", size, style),
        ForeColor = color,
        BackColor = Color.Transparent,
        AutoEllipsis = true
      };
    }

    private static RoundedButton MakeButton(string text, Color background, Color foreground)
    {
      return new RoundedButton
      {
        Text = text,
        BackColor = background,
        ForeColor = foreground,
        BorderColor = StudioDrawing.Blend(background, Color.FromArgb(116, 185, 200), 0.3f),
        BorderSize = 1,
        Radius = 12
      };
    }

    private static RoundedButton MakeCaptionButton(string text)
    {
      return new RoundedButton
      {
        Text = text,
        Font = new Font("Segoe UI", 12F, FontStyle.Regular),
        BackColor = Color.FromArgb(242, 250, 255),
        ForeColor = Color.FromArgb(70, 102, 129),
        BorderColor = Color.FromArgb(205, 224, 239),
        BorderSize = 1,
        Radius = 12
      };
    }

    private static Button MakeNavButton(string text, bool active)
    {
      var button = new Button
      {
        Text = text,
        TextAlign = ContentAlignment.MiddleLeft,
        Size = new Size(218, 43),
        Padding = new Padding(12, 0, 0, 0),
        FlatStyle = FlatStyle.Flat,
        Font = new Font("Microsoft YaHei UI", 9.5F, active ? FontStyle.Bold : FontStyle.Regular),
        ForeColor = active ? Color.FromArgb(27, 127, 146) : Color.FromArgb(70, 111, 130),
        BackColor = active ? Color.FromArgb(218, 247, 249) : Color.Transparent,
        Cursor = Cursors.Hand,
        UseVisualStyleBackColor = false
      };
      button.FlatAppearance.BorderSize = 0;
      button.FlatAppearance.MouseOverBackColor = Color.FromArgb(225, 246, 250);
      return button;
    }

    private static Size PreferredWindowSize()
    {
      var workingArea = Screen.PrimaryScreen == null
        ? new Rectangle(0, 0, 1920, 1080)
        : Screen.PrimaryScreen.WorkingArea;
      return new Size(
        Math.Max(1280, Math.Min(1720, workingArea.Width - 48)),
        Math.Max(820, Math.Min(1040, workingArea.Height - 48)));
    }

    private void ToggleMaximize()
    {
      WindowState = WindowState == FormWindowState.Maximized
        ? FormWindowState.Normal
        : FormWindowState.Maximized;
      if (_maximizeButton != null)
      {
        _maximizeButton.Text = WindowState == FormWindowState.Maximized ? "❐" : "▢";
      }
      LayoutSurface();
    }

    private void UpdateRegion()
    {
      if (WindowState == FormWindowState.Maximized)
      {
        Region = null;
        return;
      }
      using (var path = StudioDrawing.RoundedRectangle(new Rectangle(0, 0, Width, Height), 14))
      {
        Region = new Region(path);
      }
    }

    private void TitleBarMouseDown(object sender, MouseEventArgs args)
    {
      if (args.Button != MouseButtons.Left) return;
      ReleaseCapture();
      SendMessage(Handle, 0xA1, 0x2, 0);
    }

    protected override void WndProc(ref Message message)
    {
      const int wmNchittest = 0x0084;
      if (message.Msg == wmNchittest && WindowState == FormWindowState.Normal)
      {
        base.WndProc(ref message);
        var point = PointToClient(new Point((int)(message.LParam.ToInt64() & 0xffff), (int)(message.LParam.ToInt64() >> 16)));
        var left = point.X <= ResizeBorder;
        var right = point.X >= ClientSize.Width - ResizeBorder;
        var top = point.Y <= ResizeBorder;
        var bottom = point.Y >= ClientSize.Height - ResizeBorder;
        if (left && top) message.Result = (IntPtr)13;
        else if (right && top) message.Result = (IntPtr)14;
        else if (left && bottom) message.Result = (IntPtr)16;
        else if (right && bottom) message.Result = (IntPtr)17;
        else if (left) message.Result = (IntPtr)10;
        else if (right) message.Result = (IntPtr)11;
        else if (top) message.Result = (IntPtr)12;
        else if (bottom) message.Result = (IntPtr)15;
        return;
      }
      base.WndProc(ref message);
    }

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr windowHandle, int message, int parameter, int value);
  }
}
