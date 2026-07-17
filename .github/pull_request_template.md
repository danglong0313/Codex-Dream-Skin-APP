## 摘要

<!-- 改了什么，为什么？ -->

-

## 类型

- [ ] 缺陷修复
- [ ] 新功能
- [ ] Studio UI
- [ ] 主题 / 注入适配
- [ ] 文档 / 发布

## 自测

- [ ] Release x64 构建通过
- [ ] `windows/studio/tests/appearance-roundtrip.test.ps1` 通过
- [ ] `windows/studio/tests/pet-overlay.test.mjs` 通过
- [ ] 涉及 UI 时检查了四个导航页面和高 DPI 布局
- [ ] 已更新 `CHANGELOG.md`，或本次没有用户可见变化

## 安全边界

- [ ] 未修改 WindowsApps、`app.asar`、官方二进制或签名
- [ ] 未读取或改写 API Key / Base URL
- [ ] CDP 仍仅使用本机回环地址
- [ ] 恢复操作仍能还原用户应用主题前的实际外观配置

## 补充

<!-- 测试结果、截图、兼容性说明。请勿附带密钥、私人对话或个人配置。 -->
