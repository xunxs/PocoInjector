# PocoInjector 部署指南

## 编译成功！

✅ **编译状态**: 成功  
✅ **生成文件**: `PocoInjector.dll` (7KB)  
✅ **BepInEx 兼容**: 支持 IL2CPP 游戏  

## 部署步骤

### 1. 复制插件文件

将以下文件复制到游戏的 BepInEx 插件目录：

```
源文件: d:\workspace\PocoInjector\bin\Debug\PocoInjector.dll
目标: D:\Program Files (x86)\Steam\steamapps\common\Astral Party\BepInEx\plugins\PocoInjector.dll
```

### 2. 启动游戏

1. 启动 Astral Party 游戏
2. 等待游戏完全加载
3. 查看 BepInEx 控制台窗口（如果可见）

### 3. 验证插件加载

在 BepInEx 控制台中应该看到以下日志：

```
[Info] Poco Injector 正在启动...
[Info] 正在为 IL2CPP 游戏启动简化的 Poco 服务...
[Info] TCP 服务器已启动，监听端口 5001
[Warning] 注意：这是简化版本，可能不支持完整的 Poco SDK 功能
[Info] Poco 服务启动成功！您现在可以从 Airtest IDE 连接。
[Info] Poco 服务监听端口: 5001
```

### 4. 测试连接

使用提供的测试脚本：

```bash
cd d:\workspace
python test_connection.py
```

预期输出：
```
成功连接到 127.0.0.1:5001
发送: {"jsonrpc":"2.0","method":"ping","id":1}
收到: {"jsonrpc":"2.0","result":"Poco service is running","id":1}
Poco 服务响应正常！
```

## 功能说明

### 当前功能（简化版本）

- ✅ **TCP 服务器**: 监听端口 5001
- ✅ **基本连接**: 接受客户端连接
- ✅ **简单响应**: 返回基本状态信息
- ✅ **BepInEx 集成**: 作为游戏插件运行

### 限制

- ❌ **UI 元素检测**: 暂时不可用
- ❌ **游戏对象遍历**: 暂时不可用
- ❌ **坐标转换**: 暂时不可用
- ❌ **完整 Poco SDK**: 由于 IL2CPP 兼容性问题暂时禁用

## 故障排除

### 插件未加载

1. 检查文件路径是否正确
2. 确认游戏支持 BepInEx
3. 查看 BepInEx 日志文件

### 连接失败

1. 确认游戏正在运行
2. 检查端口 5001 是否被占用
3. 尝试重启游戏

### 端口冲突

如果端口 5001 被占用，可以修改 `Injector.cs` 中的端口号：

```csharp
_tcpListener = new TcpListener(IPAddress.Any, 5002); // 改为其他端口
```

## 下一步

要启用完整的 Poco SDK 功能，需要：

1. 解决 IL2CPP 类型转换问题
2. 添加 Unity 引擎的完整支持
3. 实现 UI 元素检测和操作

## 联系支持

如果遇到问题，请提供：
- BepInEx 日志文件
- 游戏版本信息
- 错误消息截图
