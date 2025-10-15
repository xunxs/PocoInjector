# PocoInjector 按钮检测使用指南

## 🎯 功能概述

PocoInjector 现在支持游戏对象检测功能，可以获取游戏内所有按钮的信息。

## 📋 当前状态

### ✅ 已完成
- **基础插件**: 成功编译并部署
- **游戏对象检测**: 实现了 `dump` 方法获取所有游戏对象
- **按钮检测脚本**: 创建了 `button_detector_simple.py`
- **调试工具**: 创建了 `debug_poco.py`

### ⚠️ 需要重启游戏
当前游戏使用的是旧版本插件，需要重启游戏以加载新功能。

## 🚀 使用步骤

### 1. 重启游戏
1. 关闭 Astral Party 游戏
2. 重新启动游戏
3. 等待 BepInEx 加载新插件

### 2. 验证新插件
运行调试脚本确认新功能：
```bash
python debug_poco.py
```

预期输出应该显示：
- `getScreenSize` 返回屏幕尺寸 JSON
- `dump` 返回游戏对象数组

### 3. 检测按钮
运行按钮检测脚本：
```bash
python button_detector_simple.py
```

## 📊 新功能说明

### 支持的 JSON-RPC 方法

1. **ping**
   - 测试连接状态
   - 返回: `"Poco service is running"`

2. **getSDKVersion**
   - 获取 SDK 版本
   - 返回: `"1.0.0"`

3. **getScreenSize**
   - 获取屏幕尺寸
   - 返回: `{"width": 1920, "height": 1080}`

4. **dump**
   - 获取所有游戏对象
   - 返回: 游戏对象数组

### 游戏对象信息结构

每个游戏对象包含：
```json
{
  "name": "ButtonName",
  "active": true,
  "tag": "Untagged",
  "layer": 5,
  "position": {
    "x": 100.0,
    "y": 200.0,
    "z": 0.0
  },
  "scale": {
    "x": 1.0,
    "y": 1.0,
    "z": 1.0
  },
  "components": [
    {
      "name": "Transform",
      "type": "UnityEngine.Transform"
    },
    {
      "name": "Button",
      "type": "UnityEngine.UI.Button"
    }
  ]
}
```

## 🔧 故障排除

### 问题1: 所有请求返回相同响应
**原因**: 游戏使用的是旧版本插件
**解决**: 重启游戏

### 问题2: 连接失败
**原因**: 游戏未运行或插件未加载
**解决**: 
1. 确认游戏正在运行
2. 检查 BepInEx 日志
3. 确认插件文件存在

### 问题3: 找不到按钮
**原因**: 按钮检测逻辑需要优化
**解决**: 
1. 检查游戏对象名称和组件
2. 调整按钮检测条件
3. 查看完整的游戏对象列表

## 📁 文件说明

- `button_detector_simple.py` - 按钮检测脚本
- `debug_poco.py` - 调试工具
- `test_connection_simple.py` - 基础连接测试

## 🎮 下一步

1. **重启游戏** 加载新插件
2. **运行调试脚本** 验证功能
3. **检测按钮** 获取游戏内按钮信息
4. **优化检测逻辑** 根据实际游戏调整

## 📝 注意事项

- 新插件需要重启游戏才能生效
- 按钮检测基于对象名称和组件类型
- 可能需要根据具体游戏调整检测逻辑
- 建议先运行调试脚本了解数据结构
