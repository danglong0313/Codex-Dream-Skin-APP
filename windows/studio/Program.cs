using System;
using System.IO;
using System.Windows.Forms;

namespace CodexDreamSkinStudio
{
  internal static class Program
  {
    [STAThread]
    private static void Main()
    {
      Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.ThreadException += (sender, args) => WriteCrash(args.Exception);
      AppDomain.CurrentDomain.UnhandledException += (sender, args) => WriteCrash(args.ExceptionObject as Exception);
      Application.Run(new MainForm());
    }

    private static void WriteCrash(Exception exception)
    {
      try
      {
        var root = Path.Combine(
          Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
          "CodexDreamSkinStudio");
        Directory.CreateDirectory(root);
        File.AppendAllText(
          Path.Combine(root, "studio-error.log"),
          DateTime.Now.ToString("O") + Environment.NewLine + exception + Environment.NewLine + Environment.NewLine);
      }
      catch
      {
      }

      MessageBox.Show(
        "Studio 遇到错误，详细信息已写入本地日志。\n\n" + (exception == null ? "未知错误" : exception.Message),
        "Codex Dream Skin Studio",
        MessageBoxButtons.OK,
        MessageBoxIcon.Error);
    }
  }
}
