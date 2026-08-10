# BPLE-2022.1.8

基于 **Unity 2021.3.45f2c1** 的游戏项目（Bad Piggies 风格关卡 / 编辑器项目，产品名"新创Unity 科技版"）。

## 环境要求

- **Unity 版本**：2021.3.45f2c1（LTS）
- 建议使用 Unity Hub 安装对应版本后打开本项目

## 项目结构

```
Assets/
├── Scripts/                 # 游戏逻辑脚本（C#）
│   └── Assembly-CSharp/     # 主程序集脚本（成就、关卡、加装包、广告、外星合成等）
├── Scene/
│   └── Scenes/              # 游戏场景
│       ├── SplashScreen.unity          # 启动画面
│       ├── MainMenu.unity              # 主菜单
│       ├── UI/                         # UI 场景（选关、工坊、挑战等）
│       │   ├── LevelSelection/         # 各章节关卡选择（Episode1~6、Race、Sandbox）
│       │   ├── CakeRaceMenu.unity      # 蛋糕竞速菜单
│       │   ├── Workshop.unity          # 工坊
│       │   └── ...
│       └── Cutscenes/                  # 剧情过场动画（各章节开始/结束）
├── Resources/               # 运行时资源与配置
│   ├── gameconfig.json      # 游戏配置
│   ├── rawAppConfig.json    # 应用配置
│   ├── levels/              # 关卡数据
│   ├── achievements/        # 成就数据
│   ├── localization/        # 本地化文本
│   └── ...
├── Plugins/                 # 第三方 / 内部 DLL
│   ├── Innovation.*.dll     # Innovation 模块
│   ├── Newtonsoft.Json.dll  # JSON 序列化
│   └── ...
├── assetbundles/            # AssetBundle 资源
└── ...
```

## 运行方法

1. 使用 Unity Hub 安装 **Unity 2021.3.45f2c1**
2. 在 Unity Hub 中添加并打开本项目目录（`BPLE-2022.1.8`）
3. 等待项目导入完成后，打开场景 `Assets/Scene/Scenes/SplashScreen.unity`
4. 点击编辑器顶部的 **Play** 按钮运行

## 注意事项

- 首次打开项目时 Unity 需要生成 `Library` 目录，耗时较长，请耐心等待
- 项目依赖 `Plugins` 目录下的 DLL，请勿删除或移动
