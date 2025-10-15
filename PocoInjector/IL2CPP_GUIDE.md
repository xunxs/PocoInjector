# IL2CPP 游戏配置指南

## 🎯 重要发现

您的游戏 **Astral Party** 是一个 **IL2CPP** 游戏，这意味着：

- ✅ **BepInEx 已安装**: 游戏目录中有完整的 BepInEx 文件夹
- ❌ **没有 Managed 文件夹**: IL2CPP 游戏不包含传统的 .NET DLL 文件
- ⚠️ **Poco SDK 兼容性**: 需要检查 Poco SDK 是否支持 IL2CPP

## 📁 游戏目录结构

```
D:\Program Files (x86)\Steam\steamapps\common\Astral Party\
├── AstralParty.exe                    # 游戏主程序
├── AstralParty_Data\                  # 游戏数据文件夹
│   ├── il2cpp_data\                   # IL2CPP 数据
│   ├── Plugins\x86_64\                # 原生插件
│   └── Resources\                     # 游戏资源
├── BepInEx\                           # BepInEx 框架
│   └── core\                          # 核心文件
│       ├── BepInEx.Core.dll           # ✅ 已配置
│       ├── BepInEx.Unity.IL2CPP.dll   # ✅ 已配置
│       └── 其他支持文件...
└── 其他文件...
```

## 🔧 已完成的配置

### 1. BepInEx 引用
项目已配置为使用游戏目录中的 BepInEx：
- `BepInEx.Core.dll` - 核心功能
- `BepInEx.Unity.IL2CPP.dll` - IL2CPP 支持

### 2. 插件基类
已更新为使用 `BasePlugin`（IL2CPP 版本）而不是 `BaseUnityPlugin`

### 3. 日志系统
使用 `Log.LogInfo()` 而不是 `Logger.LogInfo()`

## ⚠️ IL2CPP 限制

### Poco SDK 兼容性问题
IL2CPP 游戏有以下限制：
- **反射限制**: 某些反射功能可能不可用
- **动态类型**: 动态类型创建可能受限
- **JIT 编译**: 不支持运行时编译

### 可能的解决方案
1. **检查 Poco SDK 版本**: 确保使用支持 IL2CPP 的版本
2. **使用预编译版本**: 避免运行时动态加载
3. **简化功能**: 减少对反射的依赖

## 🚀 下一步操作

### 1. 编译项目
```bash
# 在 Visual Studio 中
生成 → 重新生成解决方案
```

### 2. 检查编译错误
如果仍有错误，可能需要：
- 添加额外的 BepInEx 引用
- 检查 Poco SDK 的 IL2CPP 兼容性

### 3. 部署测试
1. 将编译的 DLL 复制到 `BepInEx/plugins/`
2. 启动游戏
3. 查看 BepInEx 控制台输出

## 📋 引用清单

### 已配置的引用
- ✅ `BepInEx.Core.dll`
- ✅ `BepInEx.Unity.IL2CPP.dll`
- ✅ `Newtonsoft.Json.dll`

### 可能需要的手动引用
如果编译时出现错误，可能需要添加：
- `BepInEx.Unity.Common.dll`
- `Il2CppInterop.Runtime.dll`
- `Il2CppInterop.Common.dll`

## 🔍 故障排除

### 常见问题
1. **编译错误**: 检查 BepInEx 版本是否匹配
2. **运行时错误**: 检查 Poco SDK 的 IL2CPP 支持
3. **功能限制**: IL2CPP 可能不支持某些 Poco SDK 功能

### 调试建议
- 查看 BepInEx 控制台输出
- 检查游戏日志文件
- 确认插件是否正确加载

## 📞 技术支持

如果遇到问题：
1. 检查 BepInEx 版本兼容性
2. 查看 Poco SDK 的 IL2CPP 支持文档
3. 考虑使用替代方案或简化功能
