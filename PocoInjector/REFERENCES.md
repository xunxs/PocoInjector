# PocoInjector - 引用配置指南

## 自动配置（推荐）

项目已配置为自动引用，您只需要修改项目文件中的路径变量即可：

### 1. 修改路径变量
在 `PocoInjector.csproj` 文件中修改以下变量：

```xml
<!-- 游戏路径配置 - 请根据您的实际游戏路径修改这些变量 -->
<GameRootPath Condition="'$(GameRootPath)' == ''">C:\YourGameDirectory</GameRootPath>
<GameName Condition="'$(GameName)' == ''">YourGameName</GameName>
<PocoSDKPath Condition="'$(PocoSDKPath)' == ''">C:\Poco-SDK</PocoSDKPath>
```

### 2. 自动引用的文件

项目会自动引用以下文件：

#### BepInEx.dll
- **自动路径**: `$(GameRootPath)\BepInEx\core\BepInEx.dll`
- **说明**: BepInEx 框架的核心库
- **状态**: ✅ 已自动配置

#### UnityEngine.CoreModule.dll
- **自动路径**: `$(GameRootPath)\$(GameName)_Data\Managed\UnityEngine.CoreModule.dll`
- **说明**: Unity 引擎核心模块
- **状态**: ✅ 已自动配置

#### Poco-SDK.dll
- **自动路径**: `$(PocoSDKPath)\Poco-SDK.dll`
- **说明**: Poco SDK 的主要库文件
- **状态**: ✅ 已自动配置

## 手动配置（备选方案）

如果自动配置不工作，您仍然可以手动添加引用：

### 手动添加引用的步骤

1. 在 Visual Studio 中打开项目
2. 在"解决方案资源管理器"中右键点击"引用"
3. 选择"添加引用"
4. 点击"浏览"按钮
5. 导航到相应的 DLL 文件位置并添加

### 手动引用的文件路径

#### BepInEx.dll
- **位置**: `[您的游戏根目录]\BepInEx\core\BepInEx.dll`
- **示例**: `D:\Games\MyUnityGame\BepInEx\core\BepInEx.dll`

#### UnityEngine.CoreModule.dll
- **位置**: `[您的游戏根目录]\[游戏名]_Data\Managed\UnityEngine.CoreModule.dll`
- **示例**: `D:\Games\MyUnityGame\MyUnityGame_Data\Managed\UnityEngine.CoreModule.dll`

#### Poco-SDK.dll
- **位置**: 从下载的 Poco-SDK 文件中获取
- **示例**: `D:\Downloads\Poco-SDK\Poco-SDK.dll`

## 额外的 Unity 库

根据具体游戏和 Poco SDK 的需求，可能还需要添加：

### 常用 Unity 库
- `UnityEngine.dll` - Unity 引擎基础库
- `UnityEngine.UI.dll` - Unity UI 系统
- `UnityEngine.TextRenderingModule.dll` - 文本渲染模块
- `UnityEngine.PhysicsModule.dll` - 物理系统
- `UnityEngine.AudioModule.dll` - 音频系统

### 如何添加额外库
在项目文件中取消注释相应的引用：

```xml
<!-- 取消注释这些行如果您的游戏需要 -->
<Reference Include="UnityEngine">
  <HintPath>$(GameRootPath)\$(GameName)_Data\Managed\UnityEngine.dll</HintPath>
  <Private>False</Private>
</Reference>
```

## 配置验证

### 检查配置是否正确
1. 修改路径变量后，尝试编译项目
2. 如果没有引用错误，说明配置正确
3. 如果有错误，请检查文件路径是否正确

### 常见问题
- **文件不存在**: 检查路径是否正确
- **权限问题**: 确保有读取文件的权限
- **版本不匹配**: 确保 DLL 版本与游戏兼容

## 快速开始

1. 参考 `GAME_CONFIG.md` 文件进行配置
2. 修改项目文件中的路径变量
3. 编译项目验证配置
4. 按照 `README.md` 部署到游戏