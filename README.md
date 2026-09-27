# demo02

Godot 4.7 / GDScript 2D 游戏，使用 Forward Plus 渲染器。WASD 移动，静止时每秒自动射击；敌人逐渐加快生成，玩家死亡后立即停止刷怪和计分，保留死亡表现，3 秒后自动重开。

## 目录与职责

| 目录 | 内容 |
| --- | --- |
| `game/` | `game.tscn` 主场景、`GameController` 对局状态/分数/子弹生成/重开、`EnemySpawner` 刷怪 |
| `actors/player/` | 玩家场景、移动、射击时机、死亡动画与音效 |
| `actors/enemies/` | 史莱姆场景、移动、碰撞、一次性击杀通知与死亡表现 |
| `projectiles/` | 子弹场景、移动、5 秒生命周期、一次性消耗 |
| `ui/` | HUD 场景与显示接口 |
| `audio/` | 跨重开保留的 BGM 自动加载场景 |
| `tests/` | Godot 原生无界面回归脚本，无第三方依赖 |
| `AssetBundle/`、`Build/` | 原有素材及许可、原有导出文件 |

主场景将背景、边界、世界和 HUD 分开。玩家、敌人、子弹都是 `World` 的直接子节点，共用 Y 排序。旧场景 UID 和脚本 `.gd.uid` 已保留，素材路径不变。`AGENTS.md` 按要求保留原文，其中旧目录说明以本 README 为准。

## 信号与控制流程

- 玩家发出 `shot_requested(global_position)`，控制器在世界节点中生成子弹。
- 刷怪器发出 `enemy_spawned(enemy)`，控制器连接该敌人的 `defeated` 信号。
- 敌人确认子弹 `consume()` 成功后，先标记死亡，再发送一次 `defeated`；控制器只在对局运行中加分。
- 玩家先标记死亡，再发送一次 `died`。控制器同步标记结束、停止刷怪，显示 HUD 并启动 3 秒重开计时器。已有敌人和子弹继续执行自身逻辑。

玩家、敌人和子弹均不访问主场景字段。只有控制器负责重载当前场景，没有全局事件总线或通用状态机。

默认参数：玩家速度 100、敌人速度 50、子弹速度 300；射击间隔 1 秒；刷怪间隔从 3 秒按每秒 0.2 递减至 1 秒；出生位置为世界局部坐标 x=265、y=40～100。计时器沿用原有间隔递减方式，当前倒计时不会每帧重新启动。

## 运行与验证

在项目根目录执行，若 `godot` 不在 PATH，将其替换为本机可执行文件路径（PowerShell 使用 `& '完整路径'`）：

```powershell
godot --editor --path .
godot --path .
godot --headless --path . --editor --import --quit
godot --headless --path . --script res://tests/regression.gd
```

编辑器 F5 运行项目；打开 `game/game.tscn` 后 F6 运行当前场景。导出命令如下，需要匹配的导出模板，执行会替换原有输出：

```powershell
godot --headless --path . --export-release "Windows Desktop" Build/demo02.exe
```

回归脚本失败时返回退出码 1。覆盖独立实例化、重复碰撞/消耗/死亡、全局射击坐标、移动时停止射击、真实物理碰撞、四向移动、死亡清理、子弹寿命、刷怪加速及范围、HUD 更新、结束后拒绝刷怪与计分，以及实际等待 3 秒重开后的状态恢复。

实施验证使用 Godot 4.7.2 Mono。资源导入与无界面回归已通过。沙箱内启动曾遇到用户日志/编辑器设置写入与系统证书读取限制，授权后运行正常。没有生成新导出文件。

仍需在图形窗口人工验收：WASD 手感、静止射击表现、刷怪加速观感、Y 排序遮挡、HUD 布局、死亡动画、所有音效与 BGM、3 秒自动重开。无界面测试不能代替画面和声音验收。

## 扩展示例

- 新增敌人变体：继承或复制 `actors/enemies/slime.tscn`，替换动画/素材并调整 `move_speed`；需要新行为时使用继承 `Enemy` 的脚本，保留一次性 `defeated` 语义。将刷怪器的 `enemy_scene` 指向新场景，无需修改玩家。
- 修改 HUD：在 `ui/hud.tscn` 调整布局或在 `ui/hud.gd` 调整显示，保持 `set_score(value)`、`show_game_over()` 接口；无需修改玩家。
- 调整难度：修改 `EnemySpawner` 的类型化导出属性；修改玩家射速或子弹速度时调整相应场景的导出属性。
