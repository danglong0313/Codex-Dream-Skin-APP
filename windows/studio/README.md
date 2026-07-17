# Codex Dream Skin Studio

Windows x64 可视化换肤应用。当前内置 Miku Aqua 01，提供状态检测、应用并启动、热重新应用、验证、恢复官方外观和系统托盘入口。

## 目录

- `MainForm.cs` / `StudioControls.cs`：WinForms 界面与交互
- `EngineClient.cs`：应用与本地换肤引擎之间的进程桥接
- `scripts/`：Studio 私有桥接脚本，不作为用户启动入口
- `engine/`：随应用打包的本机 CDP 换肤引擎
- `themes/`：内置主题定义与素材
- `tests/`：外观恢复和辅助窗口隔离测试

## 构建

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\windows\studio\build-studio.ps1
```

输出文件：

```text
windows\studio\bin\x64\Release\net8.0-windows\CodexDreamSkinStudio.exe
```

## 测试

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\windows\studio\tests\appearance-roundtrip.test.ps1
node .\windows\studio\tests\pet-overlay.test.mjs
```

应用通过本机回环 CDP 工作，不修改 WindowsApps、`app.asar` 或官方程序签名。用户只需运行 EXE，不需要手动执行内部脚本。
