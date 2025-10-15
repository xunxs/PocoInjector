using BepInEx;
using BepInEx.Unity.IL2CPP;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Il2CppInterop.Runtime.Injection;

namespace PocoInjector
{
    // 定义插件的元数据
    [BepInPlugin("com.yourname.pocoinjector", "Poco Injector", "1.0.0")]
    public class Injector : BasePlugin
    {
        private TcpListener _tcpListener;
        private bool _isRunning = false;
        private UiHighlighter _highlighter;

        // Awake 方法是 BepInEx 插件的入口，游戏启动时会自动执行
        public override void Load()
        {
            try
            {
                // 在 BepInEx 的控制台窗口打印日志，方便调试
                Log.LogInfo("Poco Injector starting...");

                // 先注册自定义 MonoBehaviour 到 IL2CPP 域
                try
                {
                    if (!ClassInjector.IsTypeRegisteredInIl2Cpp(typeof(UiHighlighter)))
                        ClassInjector.RegisterTypeInIl2Cpp<UiHighlighter>();
                    if (!ClassInjector.IsTypeRegisteredInIl2Cpp(typeof(UiAnnotator)))
                        ClassInjector.RegisterTypeInIl2Cpp<UiAnnotator>();
                    if (!ClassInjector.IsTypeRegisteredInIl2Cpp(typeof(UiOverlayCamera)))
                        ClassInjector.RegisterTypeInIl2Cpp<UiOverlayCamera>();
                }
                catch (System.Exception regEx)
                {
                    Log.LogWarning("IL2CPP type registration failed or already registered: " + regEx.Message);
                }

                // 核心：启动简化的 Poco 服务
                StartSimplePocoService();

                // 创建并挂载高亮组件 + 注释标注器
                var go = new GameObject("PocoInjector_RuntimeTools");
                GameObject.DontDestroyOnLoad(go);
                // IL2CPP 场景下，直接使用泛型 AddComponent 并确保类有 IntPtr 构造
                _highlighter = go.AddComponent<UiHighlighter>();
                go.AddComponent<UiAnnotator>();

                // 顶层叠加摄像机用于 GL 方式绘制高亮
                var camGo = new GameObject("PocoInjector_OverlayCamera");
                GameObject.DontDestroyOnLoad(camGo);
                camGo.AddComponent<UiOverlayCamera>();

                Log.LogInfo("Poco service started successfully! You can now connect from Airtest IDE.");
                Log.LogInfo("Poco service listening on port: 5001");
            }
            catch (System.Exception e)
            {
                Log.LogError("Failed to start Poco service: " + e.ToString());
            }
        }

        private void StartSimplePocoService()
        {
            try
            {
                Log.LogInfo("Starting simplified Poco service for IL2CPP game...");
                
                _tcpListener = new TcpListener(IPAddress.Any, 5001);
                _tcpListener.Start();
                _isRunning = true;
                
                Thread serverThread = new Thread(HandleConnections);
                serverThread.IsBackground = true;
                serverThread.Start();
                
                Log.LogInfo("TCP server started, listening on port 5001");
                Log.LogWarning("Note: This is a simplified version, may not support full Poco SDK features");
            }
            catch (Exception ex)
            {
                Log.LogError("Error starting Poco service: " + ex.Message);
                throw;
            }
        }

        private void HandleConnections()
        {
            while (_isRunning)
            {
                try
                {
                    TcpClient client = _tcpListener.AcceptTcpClient();
                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThread.IsBackground = true;
                    clientThread.Start();
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                    {
                        Log.LogError("Error handling connection: " + ex.Message);
                    }
                }
            }
        }

        private void HandleClient(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[4096];
                
                while (_isRunning && client.Connected)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string message = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Log.LogInfo("Received message: " + message);
                        
                        // 处理 JSON-RPC 请求
                        string response = ProcessJsonRpcRequest(message);
                        
                        Log.LogInfo("Sending response: " + response);
                        
                        byte[] responseBytes = System.Text.Encoding.UTF8.GetBytes(response);
                        stream.Write(responseBytes, 0, responseBytes.Length);
                        
                        Log.LogInfo("Response sent, bytes: " + responseBytes.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError("Error handling client: " + ex.Message);
            }
            finally
            {
                client.Close();
            }
        }
        
        private string ProcessJsonRpcRequest(string jsonMessage)
        {
            try
            {
                Log.LogInfo("Received JSON message: " + jsonMessage);
                
                // 简单的字符串解析，避免依赖 Newtonsoft.Json
                string method = "";
                string id = "1";
                
                Log.LogInfo("Starting method parsing...");
                
                // 通过正则稳定提取 method 名称
                try
                {
                    var regex = new System.Text.RegularExpressions.Regex("\\\"method\\\"\\s*:\\s*\\\"([^\\\"]+)\\\"");
                    var match = regex.Match(jsonMessage);
                    if (match.Success)
                    {
                        method = match.Groups[1].Value;
                        Log.LogInfo("Detected method by regex: " + method);
                    }
                }
                catch { }
                
                // 备用的宽松匹配：直接按方法名关键字（不区分大小写）
                if (string.IsNullOrEmpty(method))
                {
                    string lower = jsonMessage.ToLowerInvariant();
                    if (lower.Contains("\"method\"") && lower.Contains("listpages"))
                    {
                        method = "listPages";
                        Log.LogInfo("Detected listPages method (loose)");
                    }
                    else if (lower.Contains("\"method\"") && lower.Contains("getpagebuttons"))
                    {
                        method = "getPageButtons";
                        Log.LogInfo("Detected getPageButtons method (loose)");
                    }
                    else if (lower.Contains("\"method\"") && lower.Contains("getscreensize"))
                    {
                        method = "getScreenSize";
                        Log.LogInfo("Detected getScreenSize method (loose)");
                    }
                    else if (lower.Contains("\"method\"") && lower.Contains("dump"))
                    {
                        method = "dump";
                        Log.LogInfo("Detected dump method (loose)");
                    }
                    else if (lower.Contains("\"method\"") && lower.Contains("getsdkversion"))
                    {
                        method = "getSDKVersion";
                        Log.LogInfo("Detected getSDKVersion method (loose)");
                    }
                    else if (lower.Contains("\"method\"") && lower.Contains("ping"))
                    {
                        method = "ping";
                        Log.LogInfo("Detected ping method (loose)");
                    }
                    else if (lower.Contains("\"method\"") && lower.Contains("sethighlightenabled"))
                    {
                        method = "setHighlightEnabled";
                        Log.LogInfo("Detected setHighlightEnabled method (loose)");
                    }
                    else if (lower.Contains("\"method\"") && lower.Contains("gethoveredbutton"))
                    {
                        method = "getHoveredButton";
                        Log.LogInfo("Detected getHoveredButton method (loose)");
                    }
                    else if (lower.Contains("\"method\"") && lower.Contains("renamebypath"))
                    {
                        method = "renameByPath";
                        Log.LogInfo("Detected renameByPath method (loose)");
                    }
                }
                else
                {
                    Log.LogInfo("No known method detected");
                }
                
                Log.LogInfo("Parsing completed, method: '" + method + "', ID: " + id);
                
                switch (method)
                {
                    case "ping":
                        return "{\"jsonrpc\":\"2.0\",\"result\":\"Poco service is running\",\"id\":" + id + "}";
                    
                    case "getSDKVersion":
                        return "{\"jsonrpc\":\"2.0\",\"result\":\"1.0.0\",\"id\":" + id + "}";
                    
                    case "getScreenSize":
                        var screenSize = GetScreenSize();
                        return "{\"jsonrpc\":\"2.0\",\"result\":" + screenSize + ",\"id\":" + id + "}";
                    
                    case "dump":
                        var hierarchy = DumpGameObjects();
                        return "{\"jsonrpc\":\"2.0\",\"result\":" + hierarchy + ",\"id\":" + id + "}";
                    
                    case "listPages":
                        var pagesJson = ListPagesJson();
                        return "{\"jsonrpc\":\"2.0\",\"result\":" + pagesJson + ",\"id\":" + id + "}";

                    case "getPageButtons":
                        var pageButtons = GetPageButtonsJson();
                        return "{\"jsonrpc\":\"2.0\",\"result\":" + pageButtons + ",\"id\":" + id + "}";
                    case "setHighlightEnabled":
                        bool enabled = ParseBoolParam(jsonMessage, true);
                        if (_highlighter != null) _highlighter.HighlightEnabled = enabled;
                        return "{\"jsonrpc\":\"2.0\",\"result\":" + (enabled ? "true" : "false") + ",\"id\":" + id + "}";
                    case "getHoveredButton":
                        string hovered = _highlighter != null ? _highlighter.GetHoveredInfoJson() : "{}";
                        return "{\"jsonrpc\":\"2.0\",\"result\":" + hovered + ",\"id\":" + id + "}";
                    case "renameByPath":
                        string fullPath = ParseStringParam(jsonMessage, "fullPath");
                        string newName = ParseStringParam(jsonMessage, "newName");
                        bool ok = false;
                        if (!string.IsNullOrEmpty(fullPath) && !string.IsNullOrEmpty(newName) && _highlighter != null)
                        {
                            ok = _highlighter.RenameByPath(fullPath, newName);
                        }
                        return "{\"jsonrpc\":\"2.0\",\"result\":" + (ok ? "true" : "false") + ",\"id\":" + id + "}";
                    
                    default:
                        return "{\"jsonrpc\":\"2.0\",\"error\":{\"code\":-32601,\"message\":\"Method not found\"},\"id\":" + id + "}";
                }
            }
            catch (Exception ex)
            {
                Log.LogError("Error processing JSON-RPC request: " + ex.Message);
                Log.LogError("Exception stack trace: " + ex.StackTrace);
                return "{\"jsonrpc\":\"2.0\",\"error\":{\"code\":-32603,\"message\":\"Internal error: " + ex.Message + "\"},\"id\":1}";
            }
        }

        private bool ParseBoolParam(string json, bool defaultValue)
        {
            try
            {
                string lower = json.ToLowerInvariant();
                int idx = lower.IndexOf("\"enabled\"");
                if (idx >= 0)
                {
                    int t = lower.IndexOf("true", idx);
                    int f = lower.IndexOf("false", idx);
                    if (t >= 0 && (f < 0 || t < f)) return true;
                    if (f >= 0 && (t < 0 || f < t)) return false;
                }
                if (lower.Contains("false")) return false;
                if (lower.Contains("true")) return true;
            }
            catch { }
            return defaultValue;
        }

        private string ParseStringParam(string json, string key)
        {
            try
            {
                string pattern = "\"" + key + "\"\\s*:\\s*\"";
                var regex = new System.Text.RegularExpressions.Regex(pattern);
                var m = regex.Match(json);
                if (m.Success)
                {
                    int start = m.Index + m.Length;
                    int end = json.IndexOf('"', start);
                    if (end > start)
                    {
                        return json.Substring(start, end - start);
                    }
                }
            }
            catch { }
            return null;
        }
        
        private string GetScreenSize()
        {
            try
            {
                // 获取屏幕尺寸
                int width = Screen.width;
                int height = Screen.height;
                return "{\"width\":" + width + ",\"height\":" + height + "}";
            }
            catch (Exception ex)
            {
                Log.LogError("Failed to get screen size: " + ex.Message);
                return "{\"width\":1920,\"height\":1080}";
            }
        }
        
        private string DumpGameObjects()
        {
            try
            {
                Log.LogInfo("Starting game object dump...");
                
                // 获取所有游戏对象
                var allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                var json = new StringBuilder();
                json.Append("[");
                
                int count = 0;
                int maxObjects = 100; // 限制最大对象数量，避免响应过长
                
                foreach (var obj in allObjects)
                {
                    if (obj != null && obj.activeInHierarchy && count < maxObjects)
                    {
                        if (count > 0) json.Append(",");
                        
                        var objInfo = GetGameObjectInfo(obj);
                        if (objInfo != null)
                        {
                            json.Append(objInfo);
                            count++;
                        }
                    }
                }
                
                json.Append("]");
                
                Log.LogInfo("Dump completed, found " + count + " game objects (limited to " + maxObjects + ")");
                
                return json.ToString();
            }
            catch (Exception ex)
            {
                Log.LogError("Failed to dump game objects: " + ex.Message);
                return "[]";
            }
        }
        
        private string GetGameObjectInfo(GameObject obj)
        {
            try
            {
                // 获取位置信息 - 尝试多种坐标系统
                var pos = obj.transform.position;
                var localPos = obj.transform.localPosition;
                var scale = obj.transform.localScale;
                
                // 尝试获取 RectTransform 信息（UI 元素）
                string rectInfo = "";
                try
                {
                    var rectTransform = obj.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        var rectPos = rectTransform.anchoredPosition;
                        var rectSize = rectTransform.sizeDelta;
                        var rectBuilder = new StringBuilder();
                        rectBuilder.Append(",\"rectPosition\":{\"x\":").Append(rectPos.x).Append(",\"y\":").Append(rectPos.y).Append("},");
                        rectBuilder.Append("\"rectSize\":{\"x\":").Append(rectSize.x).Append(",\"y\":").Append(rectSize.y).Append("},");
                        rectBuilder.Append("\"rectAnchors\":{\"min\":{\"x\":").Append(rectTransform.anchorMin.x).Append(",\"y\":").Append(rectTransform.anchorMin.y).Append("},");
                        rectBuilder.Append("\"max\":{\"x\":").Append(rectTransform.anchorMax.x).Append(",\"y\":").Append(rectTransform.anchorMax.y).Append("}}");
                        rectInfo = rectBuilder.ToString();
                    }
                }
                catch (Exception rectEx)
                {
                    Log.LogWarning("Failed to get RectTransform info: " + rectEx.Message);
                }
                
                // 获取组件信息
                var components = new StringBuilder();
                components.Append("[");
                
                var allComponents = obj.GetComponents<Component>();
                for (int i = 0; i < allComponents.Length; i++)
                {
                    if (allComponents[i] != null)
                    {
                        if (i > 0) components.Append(",");
                        components.Append("{\"name\":\"");
                        components.Append(allComponents[i].GetType().Name);
                        components.Append("\",\"type\":\"");
                        components.Append(allComponents[i].GetType().FullName);
                        components.Append("\"}");
                    }
                }
                
                components.Append("]");
                
                var json = new StringBuilder();
                json.Append("{");
                json.Append("\"name\":\"").Append(obj.name).Append("\",");
                json.Append("\"active\":").Append(obj.activeInHierarchy.ToString().ToLower()).Append(",");
                json.Append("\"tag\":\"").Append(obj.tag).Append("\",");
                json.Append("\"layer\":").Append(obj.layer).Append(",");
                json.Append("\"position\":{\"x\":").Append(pos.x).Append(",\"y\":").Append(pos.y).Append(",\"z\":").Append(pos.z).Append("},");
                json.Append("\"localPosition\":{\"x\":").Append(localPos.x).Append(",\"y\":").Append(localPos.y).Append(",\"z\":").Append(localPos.z).Append("},");
                json.Append("\"scale\":{\"x\":").Append(scale.x).Append(",\"y\":").Append(scale.y).Append(",\"z\":").Append(scale.z).Append("}");
                json.Append(rectInfo);
                json.Append(",\"components\":").Append(components.ToString());
                json.Append("}");
                
                return json.ToString();
            }
            catch (Exception ex)
            {
                Log.LogError("Failed to get game object info: " + ex.Message);
                return null;
            }
        }

        // 判断是否可能为“页面容器”
        private bool IsLikelyPageContainer(GameObject go)
        {
            if (go == null || !go.activeInHierarchy) return false;

            var name = go.name;
            if (string.IsNullOrEmpty(name)) return false;

            // 粗略关键字匹配：Page/Panel/View/Window/Screen（大小写不敏感）
            string upper = name.ToUpperInvariant();
            bool nameHit = upper.Contains("PAGE") || upper.Contains("PANEL") || upper.Contains("VIEW") || upper.Contains("WINDOW") || upper.Contains("SCREEN") || upper.Contains("ROOT") || upper.Contains("CANVAS");
            if (!nameHit) return false;

            // UI 一般带有 RectTransform
            var rect = go.GetComponent<RectTransform>();
            if (rect == null) return false;

            // 需要有一定的层级/子节点
            if (go.transform.childCount <= 0) return false;

            return true;
        }

        // 判断是否为潜在的“页面（全屏）容器”：RectTransform 锚定全屏或带 Canvas
        private bool IsFullScreenUiContainer(GameObject go)
        {
            if (go == null || !go.activeInHierarchy) return false;
            var rect = go.GetComponent<RectTransform>();
            if (rect == null) return false;

            // 识别 Canvas（避免直接依赖类型，按名称判断）
            bool hasCanvas = false;
            try
            {
                var comps = go.GetComponents<Component>();
                for (int i = 0; i < comps.Length; i++)
                {
                    var c = comps[i];
                    if (c == null) continue;
                    var t = c.GetType();
                    var n = t.Name;
                    if (string.Equals(n, "Canvas", StringComparison.OrdinalIgnoreCase))
                    {
                        hasCanvas = true;
                        break;
                    }
                }
            }
            catch { }

            // 全屏锚点
            bool fullAnchored =
                Mathf.Abs(rect.anchorMin.x - 0f) < 0.0001f &&
                Mathf.Abs(rect.anchorMin.y - 0f) < 0.0001f &&
                Mathf.Abs(rect.anchorMax.x - 1f) < 0.0001f &&
                Mathf.Abs(rect.anchorMax.y - 1f) < 0.0001f;

            return (hasCanvas || fullAnchored) && go.transform.childCount > 0;
        }

        // 生成层级路径：Root/Parent/Child
        private string BuildFullPath(GameObject go)
        {
            var sb = new StringBuilder();
            var current = go.transform;
            var stack = new System.Collections.Generic.Stack<string>();
            while (current != null)
            {
                stack.Push(current.name);
                current = current.parent;
            }
            bool first = true;
            while (stack.Count > 0)
            {
                if (!first) sb.Append("/");
                sb.Append(stack.Pop());
                first = false;
            }
            return sb.ToString();
        }

        // 获取 RectTransform 的位置信息 JSON 片段
        private void AppendRectTransformJson(StringBuilder sb, RectTransform rect)
        {
            if (rect == null) return;
            var pos = rect.anchoredPosition;
            var size = rect.sizeDelta;
            sb.Append(",\"rectPosition\":{\"x\":").Append(pos.x).Append(",\"y\":").Append(pos.y).Append("},");
            sb.Append("\"rectSize\":{\"x\":").Append(size.x).Append(",\"y\":").Append(size.y).Append("},");
            sb.Append("\"rectAnchors\":{\"min\":{\"x\":").Append(rect.anchorMin.x).Append(",\"y\":").Append(rect.anchorMin.y)
              .Append("},\"max\":{\"x\":").Append(rect.anchorMax.x).Append(",\"y\":").Append(rect.anchorMax.y).Append("}}");
        }

        // 尝试通过反射读取可交互状态（IL2CPP 下可能失败，失败则返回 null）
        private string TryGetInteractableJson(Component comp)
        {
            try
            {
                var t = comp.GetType();
                var prop = t.GetProperty("interactable");
                if (prop != null && prop.PropertyType == typeof(bool))
                {
                    bool value = (bool)prop.GetValue(comp, null);
                    return value ? "true" : "false";
                }
            }
            catch (Exception)
            {
                // 忽略 IL2CPP 反射失败
            }
            return "null"; // 用 null 表示未知
        }

        // 遍历全局，列出疑似页面容器
        private string ListPagesJson()
        {
            try
            {
                var allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                var list = new StringBuilder();
                list.Append("[");
                int count = 0;
                for (int i = 0; i < allObjects.Length; i++)
                {
                    var go = allObjects[i];
                    if (go == null) continue;
                    if (!IsLikelyPageContainer(go) && !IsFullScreenUiContainer(go)) continue;

                    if (count > 0) list.Append(",");
                    var rect = go.GetComponent<RectTransform>();
                    list.Append("{");
                    list.Append("\"name\":\"").Append(go.name).Append("\",");
                    list.Append("\"fullPath\":\"").Append(BuildFullPath(go)).Append("\",");
                    list.Append("\"active\":").Append(go.activeInHierarchy ? "true" : "false").Append(",");
                    list.Append("\"childCount\":").Append(go.transform.childCount);
                    AppendRectTransformJson(list, rect);
                    list.Append("}");
                    count++;
                }
                list.Append("]");
                return list.ToString();
            }
            catch (Exception ex)
            {
                Log.LogError("ListPages failed: " + ex.Message);
                return "[]";
            }
        }

        // 收集每个页面下的按钮（Button/Selectable 等）
        private string GetPageButtonsJson()
        {
            try
            {
                var allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                var sb = new StringBuilder();
                sb.Append("[");
                int pageIdx = 0;
                for (int i = 0; i < allObjects.Length; i++)
                {
                    var page = allObjects[i];
                    if (page == null) continue;
                    if (!IsLikelyPageContainer(page) && !IsFullScreenUiContainer(page)) continue;

                    if (pageIdx > 0) sb.Append(",");
                    sb.Append("{");
                    sb.Append("\"pageName\":\"").Append(page.name).Append("\",");
                    sb.Append("\"pageFullPath\":\"").Append(BuildFullPath(page)).Append("\",");
                    sb.Append("\"buttons\":");

                    // 枚举子树中的按钮/可选中控件
                    sb.Append("[");
                    int btnIdx = 0;
                    AppendButtonsUnder(sb, page.transform, ref btnIdx);
                    sb.Append("]");
                    sb.Append("}");
                    pageIdx++;
                }
                // 如果未识别到任何页面，回退到全局按钮集合
                if (pageIdx == 0)
                {
                    var globalButtons = new StringBuilder();
                    globalButtons.Append("[");
                    int btnIdx = 0;
                    // 遍历所有对象，直接识别按钮
                    for (int i = 0; i < allObjects.Length; i++)
                    {
                        var tr = allObjects[i]?.transform;
                        if (tr == null) continue;
                        AppendButtonsUnder(globalButtons, tr, ref btnIdx);
                    }
                    globalButtons.Append("]");

                    sb.Append("{");
                    sb.Append("\"pageName\":\"GLOBAL\",");
                    sb.Append("\"pageFullPath\":\"GLOBAL\",");
                    sb.Append("\"buttons\":").Append(globalButtons.ToString());
                    sb.Append("}");
                }
                sb.Append("]");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Log.LogError("GetPageButtons failed: " + ex.Message);
                return "[]";
            }
        }

        private void AppendButtonsUnder(StringBuilder sb, Transform root, ref int btnIdx)
        {
            if (root == null) return;

            // 判断当前节点是否含有 Button/Selectable 组件（通过类型名判断，避免直接依赖 UnityEngine.UI）
            try
            {
                var comps = root.gameObject.GetComponents<Component>();
                bool isButtonLike = false;
                string interactableJson = "null";
                for (int i = 0; i < comps.Length; i++)
                {
                    var c = comps[i];
                    if (c == null) continue;
                    var type = c.GetType();
                    var full = type.FullName ?? type.Name;
                    // 常见 UI 按钮基类/类型名特征
                    if (full.EndsWith("Button", StringComparison.OrdinalIgnoreCase) ||
                        full.EndsWith("Selectable", StringComparison.OrdinalIgnoreCase))
                    {
                        isButtonLike = true;
                        // 尝试拿到 interactable
                        interactableJson = TryGetInteractableJson(c);
                        break;
                    }
                }

                if (isButtonLike)
                {
                    if (btnIdx > 0) sb.Append(",");
                    var go = root.gameObject;
                    var rect = go.GetComponent<RectTransform>();
                    sb.Append("{");
                    sb.Append("\"name\":\"").Append(go.name).Append("\",");
                    sb.Append("\"fullPath\":\"").Append(BuildFullPath(go)).Append("\",");
                    sb.Append("\"active\":").Append(go.activeInHierarchy ? "true" : "false");
                    sb.Append(",\"interactable\":").Append(interactableJson).Append(",");
                    // 位置：优先 RectTransform
                    AppendRectTransformJson(sb, rect);
                    sb.Append("}");
                    btnIdx++;
                }
            }
            catch (Exception)
            {
                // 忽略单节点异常，继续遍历
            }

            // 递归子节点
            int childCount = root.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = root.GetChild(i);
                AppendButtonsUnder(sb, child, ref btnIdx);
            }
        }

        public override bool Unload()
        {
            try
            {
                _isRunning = false;
                _tcpListener?.Stop();
                Log.LogInfo("Poco service stopped");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError("Error stopping Poco service: " + ex.Message);
                return false;
            }
        }
    }
}
