# GitHub 发布清单（TeamCardSignal）

## 1. 发布前检查

1. 版本号已更新（当前为 `src/TeamCardSignal.json` 的 `0.2.0`）
2. `README.md` / `README_en.md` / `CHANGELOG.md` 已同步
3. 工作区不包含本地临时文件（`bin/obj/references/.env` 等）

## 2. 生成发布文件

先执行：

```powershell
.\install-mod.bat
```

然后从游戏目录收集这 3 个文件（`mods/TeamCardSignal/`）：

- `TeamCardSignal.dll`
- `TeamCardSignal.pck`
- `TeamCardSignal.json`

可选：打包为 zip（示例命令）

```powershell
$gameDir = "D:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2"
$modDir = Join-Path $gameDir "mods\TeamCardSignal"
$outZip = Join-Path $PWD "TeamCardSignal-v0.2.0.zip"
if (Test-Path $outZip) { Remove-Item $outZip -Force }
Compress-Archive -Path (Join-Path $modDir "*") -DestinationPath $outZip
```

## 3. Git 提交与打标签

```powershell
git add .
git commit -m "release: v0.2.0"
git tag v0.2.0
git push origin main
git push origin v0.2.0
```

## 4. 创建 GitHub Release

1. Tag 选择 `v0.2.0`
2. Title: `TeamCardSignal v0.2.0`
3. 描述可直接复制：
   - `docs/release/release-v0.2.0.md`
4. 上传附件：
   - `TeamCardSignal-v0.2.0.zip`（或 3 个独立文件）

## 5. 发布后快速验证

1. 新环境下载附件并安装
2. 进战斗按 `F7` 验证设置窗口
3. 验证 3 项设置均可生效并持久化
