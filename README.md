# Pet Offline

这是一个使用 Unity **2021.3.8f1** 制作的两关 2D 教学项目，目标是让新手可以直接顺着代码看懂游戏流程。

项目使用内置渲染管线、UGUI 和旧版 Input Manager，不需要额外插件。

## 运行方式

1. 使用 Unity `2021.3.8f1` 打开项目。
2. 打开 `Assets/Scenes/StartPanel.unity`。
3. 进入 Play Mode，点击标题页的 `NEW GAME`。

请不要直接从 `Main1` 或 `Main2` 开始运行。这两个场景只保存关卡世界，摄像机、UI 和输入入口都在 `StartPanel` 中。

## 操作

| 按键 | 功能 |
| --- | --- |
| `W / A / S / D` | 移动 |
| `E` | 拿取、放下、互动或推进对话 |
| `Space` | 吠叫 |
| 按住 `Shift` | 趴下或晒太阳 |
| `Q` | 空手冲刺 |
| `Esc` | 暂停或继续 |

## 游戏流程

- Day 1：搬运鞋子和枕头、回应老板、躲避摄像头，最后在狗窝吠叫。
- Day 1 报告演出结束后自动进入 Day 2。
- Day 2：完成晒太阳、香蕉和扫地机器人、备用摄像头任务，然后选择两个结局之一。
- 结局页可以重新从 Day 1 开始，或返回标题页。

项目不保存进度。关闭游戏、返回标题或重新开始后，都会从 Day 1 开始。

## 代码入口

- `GameSession`：读取输入、暂停、切换场景。
- `GameUI`：控制标题、HUD、对话、报告、选择和结局界面。
- `Day1Level` / `Day2Level`：按阶段直接执行两关玩法。
- `PlayerController`：移动、冲刺、携带和自动演出移动。
- `CameraSensor`、`RobotPatrol` 等小组件：各自只处理一种世界行为。

所有 Inspector 引用都是必填项。场景已经配置完整，运行时代码不会反复为漏配引用兜底。

移动速度、摄像头范围、任务时长等参数直接放在对应组件的 Inspector 中，方便学习和调整。

## 场景

Build Settings 保持以下顺序：

1. `Assets/Scenes/StartPanel.unity`
2. `Assets/Scenes/Main1.unity`
3. `Assets/Scenes/Main2.unity`

三个场景就是项目的唯一来源，直接在 Unity Inspector 中维护，不需要运行场景生成器。

## 验证

在 Unity Test Runner 的 EditMode 页运行 `BeginnerSmokeTests`：

- 检查三个场景没有 Missing Script；
- 检查标题页能够进入 Day 1 并返回标题页。
