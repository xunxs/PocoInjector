# 手动添加引用指南

由于游戏目录中可能没有安装 BepInEx 或 Unity 的 Managed 文件，您需要手动添加以下引用：

## 必需的引用

### 1. BepInEx.dll
- **位置**: `D:\workspace\BepInEx_win_x64_5.4.23.4\BepInEx\core\BepInEx.dll`
- **状态**: ✅ 已自动配置

### 2. UnityEngine.CoreModule.dll
- **位置**: `D:\Program Files (x86)\Steam\steamapps\common\Astral Party\Astral Party_Data\Managed\UnityEngine.CoreModule.dll`
- **状态**: ❌ 需要手动添加

### 3. UnityEngine.dll
- **位置**: `D:\Program Files (x86)\Steam\steamapps\common\Astral Party\Astral Party_Data\Managed\UnityEngine.dll`
- **状态**: ❌ 需要手动添加

### 4. Newtonsoft.Json.dll
- **位置**: `D:\workspace\Poco-SDK-master\Unity3D\3rdLib\Newtonsoft.Json.dll`
- **状态**: ✅ 已自动配置

## 手动添加步骤

1. 在 Visual Studio 中打开项目
2. 右键点击"引用" → "添加引用"
3. 点击"浏览"按钮
4. 导航到上述文件位置并添加

## 注意事项

- 确保游戏已安装并运行过至少一次
- 如果找不到 `Astral Party_Data` 文件夹，请检查游戏是否正确安装
- 某些 Unity 库可能需要设置为 `Copy Local = False`

## 替代方案

如果无法找到游戏的 Unity 文件，您可以：
1. 从 Unity Hub 下载对应版本的 Unity
2. 使用 Unity 安装目录中的 DLL 文件
3. 或者等待游戏安装完成后再添加引用
