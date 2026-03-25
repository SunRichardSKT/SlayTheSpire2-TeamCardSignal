# TeamCardSignal：中央卡牌 + 左上角对话框方案

## 目标
- 放弃 `NInspectCardScreen` 大面板。
- 当触发 TeamCardSignal（本地发送/远端接收）时：
  - 屏幕中央显示卡牌图（使用 `NCard` 节点渲染）。
  - 左上角显示对话框文案：`XXX想要出XX，X费` + `“卡牌描述”`。

## 已验证可用接口
- `NCard.Create(CardModel card, ModelVisibility visibility = ModelVisibility.Visible)`
- `NCard.UpdateVisuals(PileType pileType, CardPreviewMode previewMode)`
- `CardModel.EnergyCost.CostsX / GetWithModifiers(CostModifiers.All)`
- `CardModel.GetDescriptionForPile(PileType pileType, Creature? target = null)`

## 实现步骤
1. 在 `TeamCardSignalStateHelpers.cs` 删除 inspect 大面板相关逻辑。
2. 增加轻量 UI：
   - 一个全屏 `Control` 作为承载层（不拦截输入）。
   - 中央 `NCard` 节点（短时显示）。
   - 左上角 `PanelContainer + Label` 对话框（短时显示）。
3. 展示文本构建：
   - 费用：`X` 费或数值费。
   - 描述：使用 `GetDescriptionForPile(...)`，去掉 BBCode 标签。
4. 发送与接收路径统一调用新展示入口。
5. 保留降级：
   - 无法构造 `NCard` 时回退到原中心图标提示；
   - 对话框仍尽量显示基础文案。

## 验收
- B 触发后本机中央出现卡牌，左上角出现指定格式文案。
- 收到队友消息也有同样效果。
- 不再弹出 inspect 大面板。
