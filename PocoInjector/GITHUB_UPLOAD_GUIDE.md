# GitHub 上传指南

## 🚀 上传到 GitHub 的步骤

### 1. 在 GitHub 上创建新仓库

1. 登录 [GitHub](https://github.com)
2. 点击右上角的 "+" 按钮，选择 "New repository"
3. 填写仓库信息：
   - **Repository name**: `PocoInjector`
   - **Description**: `BepInEx plugin for injecting Poco SDK into Unity games`
   - **Visibility**: 选择 "Public" (公共仓库)
   - **不要**勾选 "Add a README file" (我们已经有了)
   - **不要**勾选 "Add .gitignore" (我们已经有了)
   - **不要**勾选 "Choose a license" (我们已经有了)
4. 点击 "Create repository"

### 2. 连接本地仓库到 GitHub

在命令行中执行以下命令（替换 `yourusername` 为您的 GitHub 用户名）：

```bash
# 添加远程仓库
git remote add origin https://github.com/yourusername/PocoInjector.git

# 推送到 GitHub
git branch -M main
git push -u origin main
```

### 3. 验证上传

1. 刷新 GitHub 页面
2. 确认所有文件都已上传
3. 检查 README.md 是否正确显示

## 📋 上传的文件列表

以下文件已准备好上传：

### 核心文件
- ✅ `PocoInjector.csproj` - 项目文件
- ✅ `Injector.cs` - 主插件代码
- ✅ `Properties/AssemblyInfo.cs` - 程序集信息

### Poco SDK 源码
- ✅ `PocoManager.cs` - Poco 管理器
- ✅ `UnityDumper.cs` - Unity 节点转储器
- ✅ `AbstractDumper.cs` - 抽象转储器
- ✅ `AbstractNode.cs` - 抽象节点
- ✅ `UnityNode.cs` - Unity 节点实现
- ✅ 其他支持文件...

### 文档文件
- ✅ `README.md` - 项目主文档
- ✅ `IL2CPP_GUIDE.md` - IL2CPP 配置指南
- ✅ `GAME_CONFIG.md` - 游戏配置指南
- ✅ `REFERENCES.md` - 引用配置指南
- ✅ `COMPILATION_FIX.md` - 编译错误解决方案
- ✅ `MANUAL_REFERENCES.md` - 手动引用指南

### 配置文件
- ✅ `.gitignore` - Git 忽略文件
- ✅ `LICENSE` - MIT 许可证

## 🔧 后续操作

### 更新仓库信息
上传后，您可以：
1. 在 GitHub 仓库页面添加描述
2. 设置标签 (topics)
3. 添加仓库徽章
4. 配置 GitHub Pages (如果需要)

### 维护仓库
- 定期更新代码
- 处理 Issues 和 Pull Requests
- 更新文档

## ⚠️ 注意事项

1. **许可证**: 项目使用 MIT 许可证，允许他人自由使用
2. **隐私**: 确保没有包含敏感信息
3. **文档**: 保持文档的更新和准确性
4. **版本控制**: 使用语义化版本号

## 📞 支持

如果上传过程中遇到问题：
1. 检查网络连接
2. 确认 GitHub 凭据
3. 查看 Git 错误信息
4. 参考 GitHub 官方文档

