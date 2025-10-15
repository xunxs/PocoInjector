# 游戏特定配置指南

## 快速配置步骤

### 1. 修改项目文件中的路径变量

在 `PocoInjector.csproj` 文件中，找到以下变量并修改为您的实际路径：

```xml
<!-- 游戏路径配置 - 请根据您的实际游戏路径修改这些变量 -->
<GameRootPath Condition="'$(GameRootPath)' == ''">C:\YourGameDirectory</GameRootPath>
<GameName Condition="'$(GameName)' == ''">YourGameName</GameName>
<PocoSDKPath Condition="'$(PocoSDKPath)' == ''">C:\Poco-SDK</PocoSDKPath>
```

**示例配置：**
```xml
<GameRootPath Condition="'$(GameRootPath)' == ''">D:\Games\MyUnityGame</GameRootPath>
<GameName Condition="'$(GameName)' == ''">MyUnityGame</GameName>
<PocoSDKPath Condition="'$(PocoSDKPath)' == ''">D:\Downloads\Poco-SDK</PocoSDKPath>
```

### 2. 验证文件路径

确保以下文件存在：

#### BepInEx.dll
- **路径**: `$(GameRootPath)\BepInEx\core\BepInEx.dll`
- **示例**: `D:\Games\MyUnityGame\BepInEx\core\BepInEx.dll`

#### UnityEngine.CoreModule.dll
- **路径**: `$(GameRootPath)\$(GameName)_Data\Managed\UnityEngine.CoreModule.dll`
- **示例**: `D:\Games\MyUnityGame\MyUnityGame_Data\Managed\UnityEngine.CoreModule.dll`

#### Poco-SDK.dll
- **路径**: `$(PocoSDKPath)\Poco-SDK.dll`
- **示例**: `D:\Downloads\Poco-SDK\Poco-SDK.dll`

### 3. 添加额外的 Unity 库（如果需要）

如果您的游戏需要额外的 Unity 库，请在项目文件中取消注释相应的引用：

```xml
<!-- 取消注释这些行如果您的游戏需要 -->
<Reference Include="UnityEngine">
  <HintPath>$(GameRootPath)\$(GameName)_Data\Managed\UnityEngine.dll</HintPath>
  <Private>False</Private>
</Reference>
<Reference Include="UnityEngine.UI">
  <HintPath>$(GameRootPath)\$(GameName)_Data\Managed\UnityEngine.UI.dll</HintPath>
  <Private>False</Private>
</Reference>
```

## 常见游戏配置示例

### Unity 游戏示例
```xml
<GameRootPath>C:\Program Files\Steam\steamapps\common\MyUnityGame</GameRootPath>
<GameName>MyUnityGame</GameName>
```

### 独立游戏示例
```xml
<GameRootPath>D:\Games\IndieGame</GameRootPath>
<GameName>IndieGame</GameName>
```

## 故障排除

### 引用错误
如果遇到引用错误，请检查：
1. 路径是否正确
2. 文件是否存在
3. 游戏是否已安装 BepInEx

### 编译错误
如果编译时出现错误：
1. 确保所有必要的 Unity 库都已添加
2. 检查 Poco-SDK 的依赖项
3. 查看 Visual Studio 的错误列表获取详细信息

## 验证配置

配置完成后，尝试编译项目：
1. 在 Visual Studio 中选择"生成" > "重新生成解决方案"
2. 如果没有错误，说明配置正确
3. 如果有错误，请根据错误信息调整配置
