using CodexDreamSkinStudio.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace CodexDreamSkinStudio
{
  public sealed class EngineClient
  {
    private readonly string _bridgePath;
    private readonly int _port;

    public EngineClient(int port = 9335)
    {
      _port = port;
      _bridgePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", "studio-bridge.ps1");
    }

    public async Task<EngineResult> RunAsync(string action)
    {
      if (!File.Exists(_bridgePath))
      {
        return EngineResult.Failed("控制脚本缺失，请重新构建或安装 Studio。", _bridgePath);
      }

      var resultPath = Path.Combine(
        Path.GetTempPath(),
        "codex-dream-skin-studio-" + Guid.NewGuid().ToString("N") + ".json");
      var startInfo = new ProcessStartInfo
      {
        FileName = "powershell.exe",
        Arguments = string.Format(
          "-NoProfile -ExecutionPolicy Bypass -File \"{0}\" -Action {1} -Port {2} -ResultPath \"{3}\"",
          _bridgePath,
          action,
          _port,
          resultPath),
        WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
        UseShellExecute = false,
        CreateNoWindow = true
      };

      try
      {
        using (var process = new Process { StartInfo = startInfo })
        {
          process.Start();
          var waitTask = process.WaitForExitAsync();
          var timeout = action.Equals("Status", StringComparison.OrdinalIgnoreCase)
            ? TimeSpan.FromSeconds(10)
            : action.Equals("Verify", StringComparison.OrdinalIgnoreCase)
              ? TimeSpan.FromSeconds(25)
              : TimeSpan.FromSeconds(55);
          var completed = await Task.WhenAny(waitTask, Task.Delay(timeout)).ConfigureAwait(false);
          if (completed != waitTask)
          {
            try { process.Kill(); } catch { }
            return EngineResult.Failed(
              "换肤操作等待超时，进度动画已停止。",
              "主题可能已经应用；请点击“验证”确认当前状态，或查看本地日志。");
          }
          await waitTask.ConfigureAwait(false);
          var output = File.Exists(resultPath)
            ? (await File.ReadAllTextAsync(resultPath, Encoding.UTF8).ConfigureAwait(false)).Trim()
            : string.Empty;

          if (string.IsNullOrWhiteSpace(output))
          {
            return EngineResult.Failed(
              "换肤引擎没有返回状态。",
              "PowerShell 已退出，但没有生成结果文件。退出代码：" + process.ExitCode);
          }

          var serializer = new DataContractJsonSerializer(typeof(EngineResult));
          using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(output)))
          {
            var result = (EngineResult)serializer.ReadObject(stream);
            return result;
          }
        }
      }
      catch (Exception exception)
      {
        return EngineResult.Failed("无法启动换肤引擎。", exception.ToString());
      }
      finally
      {
        try { if (File.Exists(resultPath)) File.Delete(resultPath); } catch { }
      }
    }
  }
}
