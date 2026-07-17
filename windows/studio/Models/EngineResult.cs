using System.Runtime.Serialization;

namespace CodexDreamSkinStudio.Models
{
  [DataContract]
  public sealed class EngineResult
  {
    [DataMember(Name = "success")]
    public bool Success { get; set; }

    [DataMember(Name = "installed")]
    public bool Installed { get; set; }

    [DataMember(Name = "codexRunning")]
    public bool CodexRunning { get; set; }

    [DataMember(Name = "cdpReady")]
    public bool CdpReady { get; set; }

    [DataMember(Name = "injectorRunning")]
    public bool InjectorRunning { get; set; }

    [DataMember(Name = "skinActive")]
    public bool SkinActive { get; set; }

    [DataMember(Name = "activeTheme")]
    public string ActiveTheme { get; set; }

    [DataMember(Name = "message")]
    public string Message { get; set; }

    [DataMember(Name = "log")]
    public string Log { get; set; }

    public static EngineResult Failed(string message, string log = null)
    {
      return new EngineResult
      {
        Success = false,
        Message = message,
        Log = log ?? string.Empty,
        ActiveTheme = "miku-aqua"
      };
    }
  }
}
