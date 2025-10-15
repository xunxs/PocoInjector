# PocoInjector

一个用于在 Unity 游戏中注入 Poco SDK 的 BepInEx 插件项目，支持 IL2CPP 游戏。

## 🎯 项目简介

PocoInjector 是一个 BepInEx 插件，用于在 Unity 游戏中自动启动 Poco SDK 服务，使您可以通过 Airtest IDE 进行 UI 自动化测试。

## ✨ 特性

- ✅ **BepInEx 支持**: 完全兼容 BepInEx 框架
- ✅ **IL2CPP 支持**: 支持 IL2CPP 编译的 Unity 游戏
- ✅ **自动配置**: 游戏启动时自动启动 Poco 服务
- ✅ **详细文档**: 提供完整的配置和部署指南
- ✅ **错误处理**: 完善的错误处理和日志记录

## 🚀 快速开始

### 1. 克隆项目
```bash
git clone https://github.com/yourusername/PocoInjector.git
cd PocoInjector
```

### 2. 配置游戏路径
在 `PocoInjector.csproj` 中修改以下变量：
```xml
<GameRootPath Condition="'$(GameRootPath)' == ''">D:\YourGameDirectory</GameRootPath>
<GameName Condition="'$(GameName)' == ''">YourGameName</GameName>
<PocoSDKPath Condition="'$(PocoSDKPath)' == ''">D:\Poco-SDK</PocoSDKPath>
```

### 3. 编译项目
在 Visual Studio 中打开项目并编译：
- 生成 → 重新生成解决方案

### 4. 部署到游戏
将生成的 `PocoInjector.dll` 复制到游戏的 `BepInEx/plugins/` 文件夹

## 📋 系统要求

- **.NET Framework 4.8**
- **Visual Studio 2019 或更高版本**
- **BepInEx 5.x** (游戏需要安装)
- **Unity 游戏** (支持 IL2CPP)

## 📁 项目结构

```
PocoInjector/
├── PocoInjector.csproj          # 项目文件
├── Injector.cs                  # 主插件代码
├── Properties/
│   └── AssemblyInfo.cs         # 程序集信息
├── README.md                    # 项目说明
├── IL2CPP_GUIDE.md             # IL2CPP 配置指南
├── GAME_CONFIG.md               # 游戏配置指南
├── REFERENCES.md                # 引用配置指南
├── COMPILATION_FIX.md           # 编译错误解决方案
├── MANUAL_REFERENCES.md         # 手动引用指南
└── Poco SDK 源码文件...         # Poco SDK Unity3D 源码
```

## 🔧 配置指南

### IL2CPP 游戏配置
如果您的游戏使用 IL2CPP 编译，请参考 [IL2CPP_GUIDE.md](IL2CPP_GUIDE.md)

### 游戏特定配置
根据您的游戏进行配置，请参考 [GAME_CONFIG.md](GAME_CONFIG.md)

### 引用配置
如果遇到引用问题，请参考 [REFERENCES.md](REFERENCES.md)

## 🐛 故障排除

### 编译错误
如果编译时出现错误，请参考 [COMPILATION_FIX.md](COMPILATION_FIX.md)

### 运行时错误
1. 检查 BepInEx 是否正确安装
2. 查看 BepInEx 控制台输出
3. 确认插件是否正确加载

### IL2CPP 限制
IL2CPP 游戏可能有一些限制，请参考 [IL2CPP_GUIDE.md](IL2CPP_GUIDE.md) 中的故障排除部分

## 📖 文档

- [IL2CPP 配置指南](IL2CPP_GUIDE.md)
- [游戏配置指南](GAME_CONFIG.md)
- [引用配置指南](REFERENCES.md)
- [编译错误解决方案](COMPILATION_FIX.md)
- [手动引用指南](MANUAL_REFERENCES.md)

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

1. Fork 项目
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 打开 Pull Request

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情

## 🙏 致谢

- [BepInEx](https://github.com/BepInEx/BepInEx) - Unity 游戏修改框架
- [Poco SDK](https://github.com/AirtestProject/Poco-SDK) - UI 自动化测试框架
- [Airtest](https://github.com/AirtestProject/Airtest) - 自动化测试平台

## 📞 支持

如果您遇到问题或有建议，请：
1. 查看文档和故障排除指南
2. 提交 Issue
3. 联系维护者

---

**注意**: 本项目仅供学习和研究目的使用。请确保您有权修改目标游戏，并遵守相关法律法规。
