# TeamCardSignal

`TeamCardSignal` 是一个用于 **Slay the Spire 2 多人战斗** 的卡牌标注 Mod。  
在战斗中悬停手牌并按下快捷键后，你的队友会收到明显提示，知道你准备打出哪张牌。

本项目由 `Sts2ModScaffold` 改造而来，当前已独立为 `TeamCardSignal`（`id`、manifest、csproj、产物命名均一致）。

## 功能概览

- 多人战斗中发送“将要打出的牌”标注（默认键：`B`）
- 本机与队友都可看到提示反馈
- 1 秒冷却防刷
- 设置持久化（启用状态、显示模式、快捷键）
- 支持 SoloOne 单人开多人场景（本地预览可用）

## 显示模式

可在设置窗口切换：

- `小图标模式`：仅显示中央卡牌图标（轻量）
- `完整卡牌模式`：显示中央卡牌预览 + 左上角文本对话框

重要说明：  
接收端总是按“自己的本地设置”渲染，不依赖发送端使用的模式。

## 快捷键

- `B`（默认）：发送当前悬停手牌的标注
- `F7`：打开/关闭设置窗口

设置窗口支持：

1. 启用/关闭标记功能
2. 切换小图标/完整卡牌模式
3. 修改标注快捷键（按任意键绑定，`Esc` 取消）

## 配置文件

配置自动保存在：

`%AppData%\SlayTheSpire2\mods\TeamCardSignal\settings.json`

当前字段：

- `Enabled`
- `Hotkey`
- `CooldownSeconds`
- `PreviewMode`（`IconOnly` / `FullCard`）

## 联机消息协议

`[TCS]|v1|senderNetId|senderName|cardId|cardName|targetId|targetName|ts`

## 项目结构

```text
TeamCardSignal/
|- src/
|  |- Hooks/
|  |- ModEntry.cs
|  |- TeamCardSignal.csproj
|  `- TeamCardSignal.json
|- docs/
|  `- plans/
|- tools/
|- install.bat
|- install-mod.bat
`- uninstall-mod.bat
```

## 本地开发

### 1. 首次安装环境

```powershell
.\install.bat
```

### 2. 构建并安装 Mod

```powershell
.\install-mod.bat
```

或指定游戏目录：

```powershell
.\install-mod.bat "D:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2"
```

### 3. 查看日志

```powershell
.\tools\read_sts2_logs.ps1
```

## 上传 GitHub 前检查

- 确认不提交本地依赖/产物（`.gitignore` 已覆盖）：
  - `references/`
  - `tools` 下载内容
  - `src/bin`、`src/obj`
  - `.env`、`opencode.jsonc`
- 若你曾看到 `Sts2ModScaffold.*` 产物，那是旧缓存；当前仓库以 `TeamCardSignal.*` 为准。

推荐检查：

```powershell
git add .
git status
```

## 版本与发布

- 当前版本：`0.2.0`
- 变更记录：见 [CHANGELOG.md](CHANGELOG.md)
- 发布文案模板：见 [release-v0.2.0.md](docs/release/release-v0.2.0.md)
- 发布操作清单：见 [publish-checklist.md](docs/release/publish-checklist.md)

## 许可证

MIT
