# TeamCardSignal：完整卡牌信息展示增强计划

## 目标
- 在 TeamCardSignal 触发时（本地发送与远端接收），优先显示游戏原生 `NInspectCardScreen`，展示完整卡牌信息（图像、费用、描述、关键词等）。
- 保留原有“中央图标 + 文本”提示作为兜底和并行反馈，避免 UI 调用失败时无提示。

## 已验证签名（来自反编译）
- `NGame.GetInspectCardScreen(): NInspectCardScreen`
- `NInspectCardScreen.Open(List<CardModel> cards, int index, bool viewAllUpgraded = false)`
- `NInspectCardScreen.Close()`

## 实现点
1. 在 `src/Hooks/TeamCardSignalStateHelpers.cs` 新增完整卡牌展示方法：
   - 传入本地 `CardModel` 时，使用可变克隆后打开检查界面。
   - 传入 `cardId` 时，从 `ModelDb.AllCards` 查卡并打开检查界面。
2. 增加自动关闭机制（短延时）：
   - 每次打开后安排延时关闭，防止长时间阻塞输入。
   - 如短时间内再次触发，刷新关闭时机。
3. 在发送与接收路径接入：
   - `TrySendLocalSignal`：发送成功后展示完整卡牌。
   - `OnSignalMessageReceived`：解析到卡牌后展示完整卡牌。
4. 失败降级：
   - 任一步骤失败则记录 `[Hook]` 日志，并继续显示中心图标与文本。

## 验收标准
- 按下快捷键发送后，本机可看到完整卡牌弹窗（随后自动关闭）。
- 收到队友消息时，可看到对应卡牌的完整卡牌弹窗（随后自动关闭）。
- 即使完整卡牌弹窗失败，仍有原有文本与图标提示，不影响主流程。
