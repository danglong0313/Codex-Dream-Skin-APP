# Codex Dream Skin APP

<p align="center">
  <strong>给 Windows 版 Codex 一键换上可恢复的主题。</strong><br>
  Miku Aqua 测试版 · 本机 CDP 注入 · 不修改 WindowsApps / app.asar / 官方签名
</p>

<p align="center">
  <a href="https://github.com/danglong0313/Codex-Dream-Skin-APP/releases">下载测试版</a>
  ·
  <a href="./CHANGELOG.md">更新记录</a>
  ·
  <a href="https://github.com/Fei-Away/Codex-Dream-Skin">上游项目</a>
</p>

> [!WARNING]
> 当前版本为 `v0.1.0-preview.1` 测试版，只建议用于体验和反馈。Codex 更新后页面结构可能变化，应用前请保存尚未发送的输入。

![Miku Aqua 主题效果预览](windows/studio/themes/miku-aqua/preview.jpg)

## 这是做什么的

Codex Dream Skin APP 是基于 [Fei-Away/Codex-Dream-Skin](https://github.com/Fei-Away/Codex-Dream-Skin) 制作的 Windows 可视化换肤工具。当前内置 **Miku Aqua 01**，用户无需手动执行多段脚本，即可在 Studio 中完成：

- 一键应用主题并启动 Codex
- 热重新应用与主题状态验证
- 精确恢复应用前的官方颜色配置
- Chat / Work、Codex 工作区主页、权限与模型菜单主题适配
- 自动排除 Codex 宠物、设置向导等辅助窗口
- 关闭主窗口后收至系统托盘继续守护主题

Studio 只连接本机 `127.0.0.1` 的 CDP 调试端口，**不会修改** Microsoft Store 中的 Codex 文件、`app.asar` 或程序签名。

## 下载与使用

前往 [Releases](https://github.com/danglong0313/Codex-Dream-Skin-APP/releases) 下载：

```text
Codex-Dream-Skin-Studio-v0.1.0-preview.1-win-x64.zip
```

解压后运行 `CodexDreamSkinStudio.exe`。测试版发布包已携带 .NET 运行时，但仍需要：

- Windows 10/11 x64
- Microsoft Store 安装的 Codex 桌面端
- [Node.js](https://nodejs.org/) 18 或更高版本

首次使用：

1. 打开 Studio，点击“应用并启动”。
2. 如果 Codex 显示“一次性 Windows 设置”，请在 Codex 中点击“完成设置”，并在系统 UAC 窗口选择“是”。
3. 返回 Studio 再点击一次“应用并启动”。
4. 状态显示“主题运行中”即完成。

Studio 本身不需要管理员权限；上述 UAC 来自 Codex 官方的一次性 Windows 设置流程。

## 恢复官方外观

打开 Studio 的“启动与恢复”页面，点击“恢复官方”。程序会：

1. 停止本机主题守护；
2. 移除当前页面注入；
3. 恢复首次应用前保存的颜色、字体和语义颜色配置；
4. 在必要时重新启动 Codex。

恢复不会使用预设的“近似官方颜色”，而是还原用户应用主题前的实际配置。

## 从源码构建

需要 .NET 8 SDK、Node.js 18+ 和 PowerShell 5.1：

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\windows\studio\build-studio.ps1
```

构建结果：

```text
windows\studio\bin\x64\Release\net8.0-windows\CodexDreamSkinStudio.exe
```

生成自包含发布包：

```powershell
dotnet publish .\windows\studio\CodexDreamSkinStudio.csproj `
  -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

## 测试

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\windows\studio\tests\appearance-roundtrip.test.ps1
node .\windows\studio\tests\pet-overlay.test.mjs
dotnet build .\windows\studio\CodexDreamSkinStudio.csproj -c Release -p:Platform=x64
```

人工检查项见 [`windows/studio/QA.md`](./windows/studio/QA.md)。

## 安全边界

- CDP 仅绑定 `127.0.0.1`；主题运行时不要执行来路不明的本机程序。
- 不修改 WindowsApps、官方安装文件、`app.asar` 或代码签名。
- 不读取或改写 API Key、Base URL 和模型供应商配置。
- 只保存恢复主题所需的本地外观快照和运行日志，不上传主题状态。
- 主题依赖 Codex 当前界面结构，官方更新后应先使用“验证”检查兼容性。

## 源码公开、许可与署名

本仓库保留上游作者署名与项目链接，并明确标注为 [Fei-Away/Codex-Dream-Skin](https://github.com/Fei-Away/Codex-Dream-Skin) 的衍生项目。

- 本项目新增的 Windows Studio UI 源码按 [`windows/studio/LICENSE`](./windows/studio/LICENSE) 中的 MIT License 提供。
- 内置换肤引擎由上游代码继续适配而来；上游仓库根目录当前没有统一许可证，因此本仓库的 MIT 声明不会自动覆盖这些引擎代码、脚本或素材。
- 完整的分目录授权范围和第三方声明见 [`OPEN_SOURCE_NOTICE.md`](./OPEN_SOURCE_NOTICE.md)。
- Codex、OpenAI、初音未来及相关名称、商标和角色形象归各自权利人所有。本项目与 OpenAI 及相关角色权利方无隶属或授权关系。

因此，本仓库是**源码公开的混合许可项目**，并非所有文件都自动获得 MIT 授权。复制、再分发或商业使用前，请确认目标文件的许可范围和素材权利。

## 贡献

欢迎提交 Issue 和 Pull Request。提交前建议：

1. 说明 Codex 版本、Windows 版本和复现步骤；
2. 运行上面的自动测试；
3. 涉及视觉变更时附上主界面、Chat / Work、Codex 工作区和普通任务页截图；
4. 不提交包含个人配置、日志、令牌或本机路径的文件。

---

非 OpenAI 官方产品。使用本测试版即表示你理解外部主题可能随 Codex 更新而需要重新适配。
