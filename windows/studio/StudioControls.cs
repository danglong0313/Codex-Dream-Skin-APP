using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CodexDreamSkinStudio
{
  internal static class StudioDrawing
  {
    public static GraphicsPath RoundedRectangle(Rectangle rectangle, int radius)
    {
      var diameter = Math.Max(2, radius * 2);
      var path = new GraphicsPath();
      path.AddArc(rectangle.X, rectangle.Y, diameter, diameter, 180, 90);
      path.AddArc(rectangle.Right - diameter, rectangle.Y, diameter, diameter, 270, 90);
      path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0, 90);
      path.AddArc(rectangle.X, rectangle.Bottom - diameter, diameter, diameter, 90, 90);
      path.CloseFigure();
      return path;
    }

    public static Color Blend(Color left, Color right, float amount)
    {
      amount = Math.Max(0, Math.Min(1, amount));
      return Color.FromArgb(
        (int)(left.A + (right.A - left.A) * amount),
        (int)(left.R + (right.R - left.R) * amount),
        (int)(left.G + (right.G - left.G) * amount),
        (int)(left.B + (right.B - left.B) * amount));
    }

    public static void DrawImageCover(Graphics graphics, Image image, Rectangle destination, float focusX = 0.5f, float focusY = 0.5f)
    {
      if (image == null || destination.Width <= 0 || destination.Height <= 0) return;
      var scale = Math.Max((float)destination.Width / image.Width, (float)destination.Height / image.Height);
      var sourceWidth = destination.Width / scale;
      var sourceHeight = destination.Height / scale;
      var sourceX = (image.Width - sourceWidth) * Math.Max(0, Math.Min(1, focusX));
      var sourceY = (image.Height - sourceHeight) * Math.Max(0, Math.Min(1, focusY));
      graphics.DrawImage(
        image,
        destination,
        sourceX,
        sourceY,
        sourceWidth,
        sourceHeight,
        GraphicsUnit.Pixel);
    }
  }

  internal class GradientPanel : Panel
  {
    public Color GradientStart { get; set; } = Color.White;
    public Color GradientEnd { get; set; } = Color.White;
    public LinearGradientMode GradientMode { get; set; } = LinearGradientMode.Vertical;

    public GradientPanel()
    {
      DoubleBuffered = true;
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
      using (var brush = new LinearGradientBrush(ClientRectangle, GradientStart, GradientEnd, GradientMode))
      {
        e.Graphics.FillRectangle(brush, ClientRectangle);
      }
    }
  }

  internal class RoundedPanel : Panel
  {
    public int Radius { get; set; } = 18;
    public Color BorderColor { get; set; } = Color.FromArgb(194, 230, 237);
    public int BorderSize { get; set; } = 1;

    public RoundedPanel()
    {
      DoubleBuffered = true;
      BackColor = Color.White;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
      var rect = new Rectangle(0, 0, Width - 1, Height - 1);
      using (var path = StudioDrawing.RoundedRectangle(rect, Radius))
      using (var brush = new SolidBrush(BackColor))
      using (var pen = new Pen(BorderColor, BorderSize))
      {
        e.Graphics.FillPath(brush, path);
        if (BorderSize > 0) e.Graphics.DrawPath(pen, path);
      }
      base.OnPaint(e);
    }
  }

  internal class RoundedButton : Button
  {
    private Color _baseColor = Color.FromArgb(237, 249, 252);
    public int Radius { get; set; } = 13;
    public Color BorderColor { get; set; } = Color.Transparent;
    public int BorderSize { get; set; }

    public new Color BackColor
    {
      get { return _baseColor; }
      set { _baseColor = value; base.BackColor = value; Invalidate(); }
    }

    public RoundedButton()
    {
      FlatStyle = FlatStyle.Flat;
      FlatAppearance.BorderSize = 0;
      Cursor = Cursors.Hand;
      Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
      ForeColor = Color.FromArgb(31, 89, 108);
      _baseColor = Color.FromArgb(237, 249, 252);
      UseVisualStyleBackColor = false;
      DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
      pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
      var rect = new Rectangle(0, 0, Width - 1, Height - 1);
      var color = !Enabled
        ? StudioDrawing.Blend(_baseColor, Color.White, 0.55f)
        : ClientRectangle.Contains(PointToClient(MousePosition))
          ? StudioDrawing.Blend(_baseColor, Color.White, 0.16f)
          : _baseColor;
      using (var path = StudioDrawing.RoundedRectangle(rect, Radius))
      using (var brush = new SolidBrush(color))
      using (var pen = new Pen(BorderColor, BorderSize))
      {
        pevent.Graphics.FillPath(brush, path);
        if (BorderSize > 0) pevent.Graphics.DrawPath(pen, path);
      }
      TextRenderer.DrawText(
        pevent.Graphics,
        Text,
        Font,
        ClientRectangle,
        Enabled ? ForeColor : Color.FromArgb(145, ForeColor),
        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); Invalidate(); }
    protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); Invalidate(); }
    protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); Invalidate(); }
  }

  internal class HeroPanel : Panel
  {
    public Image Artwork { get; set; }

    public HeroPanel()
    {
      DoubleBuffered = true;
      ResizeRedraw = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      var graphics = e.Graphics;
      graphics.SmoothingMode = SmoothingMode.AntiAlias;
      graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
      var rect = new Rectangle(0, 0, Width - 1, Height - 1);
      using (var clipPath = StudioDrawing.RoundedRectangle(rect, 23))
      {
        graphics.SetClip(clipPath);
        StudioDrawing.DrawImageCover(graphics, Artwork, ClientRectangle, 0.5f, 0.43f);
        var overlayWidth = Math.Max(1, (int)(Width * 0.69));
        using (var overlay = new LinearGradientBrush(
          new Rectangle(0, 0, overlayWidth, Height),
          Color.FromArgb(218, 34, 105, 137),
          Color.FromArgb(0, 45, 151, 177),
          LinearGradientMode.Horizontal))
        {
          graphics.FillRectangle(overlay, 0, 0, overlayWidth, Height);
        }

        var titleY = Math.Max(122, (int)(Height * 0.33));
        var subtitleY = titleY + 66;
        var featureY = subtitleY + 53;
        DrawChip(graphics, new Rectangle(46, 42, 254, 34), "MIKU CREATIVE STUDIO · 01", Color.FromArgb(155, 255, 255, 255), Color.White);
        using (var titleFont = new Font("Microsoft YaHei UI", 26F, FontStyle.Bold))
        using (var subtitleFont = new Font("Microsoft YaHei UI", 11F, FontStyle.Regular))
        using (var titleBrush = new SolidBrush(Color.White))
        using (var subtitleBrush = new SolidBrush(Color.FromArgb(238, 253, 255)))
        {
          graphics.DrawString("让灵感与代码一起起飞", titleFont, titleBrush, new PointF(46, titleY));
          graphics.DrawString("青蓝玻璃侧栏 · Chat / Work 双模式 · 原生控件完整保留", subtitleFont, subtitleBrush, new PointF(48, subtitleY));
        }

        DrawChip(graphics, new Rectangle(46, featureY, 88, 33), "Chat 青绿", Color.FromArgb(226, 255, 255, 255), Color.FromArgb(23, 121, 141));
        DrawChip(graphics, new Rectangle(144, featureY, 88, 33), "Work 淡紫", Color.FromArgb(226, 255, 255, 255), Color.FromArgb(108, 99, 183));
        DrawChip(graphics, new Rectangle(242, featureY, 88, 33), "完整恢复", Color.FromArgb(226, 255, 255, 255), Color.FromArgb(176, 91, 141));
        DrawChip(graphics, new Rectangle(Width - 91, 22, 70, 31), "已内置", Color.FromArgb(216, 255, 255, 255), Color.FromArgb(19, 147, 161));
        DrawChip(graphics, new Rectangle(Width - 166, Height - 55, 144, 34), "♫  AQUA EDITION", Color.FromArgb(204, 246, 252, 255), Color.FromArgb(45, 116, 138));
        graphics.ResetClip();
      }
      using (var border = StudioDrawing.RoundedRectangle(rect, 23))
      using (var pen = new Pen(Color.FromArgb(128, 218, 232), 2))
      {
        graphics.DrawPath(pen, border);
      }
    }

    private static void DrawChip(Graphics graphics, Rectangle rectangle, string text, Color background, Color foreground)
    {
      using (var path = StudioDrawing.RoundedRectangle(rectangle, rectangle.Height / 2))
      using (var brush = new SolidBrush(background))
      using (var font = new Font("Microsoft YaHei UI", 8F, FontStyle.Bold))
      {
        graphics.FillPath(brush, path);
        TextRenderer.DrawText(
          graphics,
          text,
          font,
          rectangle,
          foreground,
          TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
      }
    }
  }

  internal class ThemeCardPanel : RoundedPanel
  {
    public Image Artwork { get; set; }

    protected override void OnPaint(PaintEventArgs e)
    {
      base.OnPaint(e);
      e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
      var previewWidth = Math.Min(152, Math.Max(126, (int)(Width * 0.35)));
      var preview = new Rectangle(17, 17, previewWidth, Math.Max(40, Height - 34));
      using (var path = StudioDrawing.RoundedRectangle(preview, 14))
      {
        e.Graphics.SetClip(path);
        StudioDrawing.DrawImageCover(e.Graphics, Artwork, preview, 0.88f, 0.43f);
        e.Graphics.ResetClip();
      }
      using (var borderPath = StudioDrawing.RoundedRectangle(preview, 14))
      using (var pen = new Pen(Color.FromArgb(168, 220, 231)))
      {
        e.Graphics.DrawPath(pen, borderPath);
      }

      var x = preview.Right + 20;
      using (var small = new Font("Microsoft YaHei UI", 8F, FontStyle.Bold))
      using (var title = new Font("Microsoft YaHei UI", 15F, FontStyle.Bold))
      using (var normal = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular))
      using (var muted = new SolidBrush(Color.FromArgb(116, 151, 163)))
      using (var ink = new SolidBrush(Color.FromArgb(28, 96, 118)))
      {
        e.Graphics.DrawString("当前主题", small, muted, x, 28);
        e.Graphics.DrawString("Miku Aqua 01", title, ink, x, 55);
        e.Graphics.DrawString("明亮 · 创作舞台", normal, muted, x, 91);
        DrawSwatch(e.Graphics, x, 127, Color.FromArgb(33, 198, 201));
        DrawSwatch(e.Graphics, x + 25, 127, Color.FromArgb(107, 145, 234));
        DrawSwatch(e.Graphics, x + 50, 127, Color.FromArgb(233, 133, 201));
        e.Graphics.DrawString("适配新版 Chat / Work\n与 Codex 工作区主页", normal, muted, new RectangleF(x, 163, Width - x - 14, 48));
      }
    }

    private static void DrawSwatch(Graphics graphics, int x, int y, Color color)
    {
      using (var brush = new SolidBrush(color)) graphics.FillEllipse(brush, x, y, 16, 16);
    }
  }

  internal class StatusPill : Control
  {
    public Color DotColor { get; set; } = Color.FromArgb(115, 166, 180);
    public Color PillColor { get; set; } = Color.FromArgb(234, 248, 251);
    public Color PillBorderColor { get; set; } = Color.FromArgb(185, 228, 235);

    public StatusPill()
    {
      DoubleBuffered = true;
      Font = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Bold);
      ForeColor = Color.FromArgb(62, 112, 128);
      Text = "正在检测…";
      Size = new Size(142, 32);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
      var rect = new Rectangle(0, 0, Width - 1, Height - 1);
      using (var path = StudioDrawing.RoundedRectangle(rect, Height / 2))
      using (var brush = new SolidBrush(PillColor))
      using (var pen = new Pen(PillBorderColor))
      {
        e.Graphics.FillPath(brush, path);
        e.Graphics.DrawPath(pen, path);
      }
      using (var dot = new SolidBrush(DotColor)) e.Graphics.FillEllipse(dot, 12, Height / 2 - 4, 8, 8);
      TextRenderer.DrawText(e.Graphics, Text, Font, new Rectangle(28, 0, Width - 36, Height), ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
  }
}
