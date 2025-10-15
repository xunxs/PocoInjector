using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using BepInEx;

namespace PocoInjector
{
    // 负责：
    // 1) 监听热键，针对当前悬停按钮弹出命名输入对话框
    // 2) 监测页面变化，无别名时弹窗命名页面
    // 3) 将“页面->按钮”的原始信息与自定义别名保存为 JSON
    public class UiAnnotator : MonoBehaviour
    {
        // 显式构造（IL2CPP 适配）
        public UiAnnotator() { }
        public UiAnnotator(IntPtr handle) : base(handle) { }

        // 配置
        public bool annotatorEnabled = true;
        public KeyCode renameHotkey = KeyCode.BackQuote; // “`” 键；部分中文键盘此键位显示为 “、”

        // 依赖
        private UiHighlighter _highlighter;

        // 页面跟踪
        private string _currentPagePath;
        private string _currentPageAlias;
        private float _lastPageCheckTime;
        private const float PageCheckInterval = 0.2f; // 降频，避免开销

        // 命名输入 UI 状态
        private bool _showButtonPrompt;
        private string _buttonInput = "";
        private string _buttonOriginalName = "";
        private string _buttonFullPath = "";

        private bool _showPagePrompt;
        private string _pageInput = "";
        private string _pageFullPath = "";

        // 数据存储
        private MappingData _data = new MappingData();
        private string _savePath;

        private void Awake()
        {
            _highlighter = FindObjectOfType<UiHighlighter>();
            _savePath = Path.Combine(Paths.ConfigPath, "PocoInjector_mappings.json");
            Load();
        }

        private void Update()
        {
            if (!annotatorEnabled) return;

            // 页面变化检测
            if (Time.unscaledTime - _lastPageCheckTime > PageCheckInterval)
            {
                _lastPageCheckTime = Time.unscaledTime;
                DetectPageChangeAndPrompt();
            }

            // 按钮命名热键
            try
            {
                if (Input.GetKeyDown(renameHotkey))
                {
                    var hoveredJson = _highlighter != null ? _highlighter.GetHoveredInfoJson() : "{}";
                    var info = SimpleJson.ParseObject(hoveredJson);
                    if (info != null && info.TryGetValue("fullPath", out var pathObj))
                    {
                        _buttonFullPath = pathObj as string;
                        _buttonOriginalName = info.TryGetValue("name", out var n) ? (n as string ?? "") : "";
                        _buttonInput = GetButtonAlias(_currentPagePath, _buttonFullPath) ?? _buttonOriginalName;
                        _showButtonPrompt = true;
                    }
                }
            }
            catch { }
        }

        private void OnGUI()
        {
            if (!annotatorEnabled) return;

            var prevColor = GUI.color;
            try
            {
                if (_showPagePrompt)
                {
                    var rect = new Rect(10, 10, 420, 110);
                    GUI.color = new Color(0, 0, 0, 0.85f);
                    GUI.Box(rect, GUIContent.none);
                    GUILayout.BeginArea(rect);
                    GUILayout.Label("页面命名");
                    GUILayout.Label("检测到新页面，请输入页面名称：");
                    GUI.SetNextControlName("pageInput");
                    _pageInput = GUILayout.TextField(_pageInput ?? "", GUILayout.MinWidth(380));
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("保存", GUILayout.Width(90)))
                    {
                        SavePageAlias(_pageFullPath, _pageInput);
                        _currentPageAlias = _pageInput;
                        _showPagePrompt = false;
                    }
                    if (GUILayout.Button("跳过", GUILayout.Width(90)))
                    {
                        _showPagePrompt = false;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.EndArea();
                    FocusText("pageInput");
                }

                if (_showButtonPrompt)
                {
                    var rect = new Rect(10, 140, 420, 140);
                    GUI.color = new Color(0, 0, 0, 0.85f);
                    GUI.Box(rect, GUIContent.none);
                    GUILayout.BeginArea(rect);
                    GUILayout.Label("按钮命名");
                    GUILayout.Label("为当前按钮设置名称：");
                    GUILayout.Label("原名：" + (_buttonOriginalName ?? ""));
                    GUI.SetNextControlName("btnInput");
                    _buttonInput = GUILayout.TextField(_buttonInput ?? "", GUILayout.MinWidth(380));
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("保存", GUILayout.Width(90)))
                    {
                        SaveButtonAlias(_currentPagePath, _buttonFullPath, _buttonOriginalName, _buttonInput);
                        _showButtonPrompt = false;
                    }
                    if (GUILayout.Button("取消", GUILayout.Width(90)))
                    {
                        _showButtonPrompt = false;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.EndArea();
                    FocusText("btnInput");
                }
            }
            finally
            {
                GUI.color = prevColor;
            }
        }

        private void FocusText(string name)
        {
            if (Event.current.type == EventType.Repaint)
            {
                GUI.FocusControl(name);
            }
        }

        private void DetectPageChangeAndPrompt()
        {
            string page = DetectCurrentPagePath();
            if (!string.Equals(page, _currentPagePath, StringComparison.Ordinal))
            {
                _currentPagePath = page;
                _currentPageAlias = GetPageAlias(page);
                if (!string.IsNullOrEmpty(page) && string.IsNullOrEmpty(_currentPageAlias))
                {
                    _pageFullPath = page;
                    _pageInput = "";
                    _showPagePrompt = true;
                }
            }
        }

        // -------- 页面/按钮识别工具 --------
        private string DetectCurrentPagePath()
        {
            try
            {
                var allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                string bestPath = null;
                int bestScore = -1;
                for (int i = 0; i < allObjects.Length; i++)
                {
                    var go = allObjects[i];
                    if (go == null || !go.activeInHierarchy) continue;
                    if (!IsLikelyPageContainer(go) && !IsFullScreenUiContainer(go)) continue;

                    int score = go.transform.childCount;
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestPath = BuildFullPath(go);
                    }
                }
                return bestPath;
            }
            catch { return null; }
        }

        private bool IsLikelyPageContainer(GameObject go)
        {
            if (go == null || !go.activeInHierarchy) return false;
            var name = go.name;
            if (string.IsNullOrEmpty(name)) return false;
            string upper = name.ToUpperInvariant();
            bool nameHit = upper.Contains("PAGE") || upper.Contains("PANEL") || upper.Contains("VIEW") || upper.Contains("WINDOW") || upper.Contains("SCREEN") || upper.Contains("ROOT") || upper.Contains("CANVAS") || upper.Contains("MENU") || upper.Contains("DIALOG") || upper.Contains("HUD") || upper.Contains("POPUP");
            if (!nameHit) return false;
            var rect = go.GetComponent<RectTransform>();
            if (rect == null) return false;
            if (go.transform.childCount <= 0) return false;
            return true;
        }

        private bool IsFullScreenUiContainer(GameObject go)
        {
            if (go == null || !go.activeInHierarchy) return false;
            var rect = go.GetComponent<RectTransform>();
            if (rect == null) return false;
            bool hasCanvas = false;
            try
            {
                var comps = go.GetComponents<Component>();
                for (int i = 0; i < comps.Length; i++)
                {
                    var c = comps[i]; if (c == null) continue;
                    var n = c.GetType().Name;
                    if (string.Equals(n, "Canvas", StringComparison.OrdinalIgnoreCase)) { hasCanvas = true; break; }
                }
            }
            catch { }
            bool fullAnchored = Mathf.Abs(rect.anchorMin.x - 0f) < 0.0001f && Mathf.Abs(rect.anchorMin.y - 0f) < 0.0001f && Mathf.Abs(rect.anchorMax.x - 1f) < 0.0001f && Mathf.Abs(rect.anchorMax.y - 1f) < 0.0001f;
            return (hasCanvas || fullAnchored) && go.transform.childCount > 0;
        }

        private static string BuildFullPath(GameObject go)
        {
            var sb = new StringBuilder();
            var current = go != null ? go.transform : null;
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

        // -------- 数据存取 --------
        private string GetPageAlias(string pagePath)
        {
            if (string.IsNullOrEmpty(pagePath)) return null;
            return _data.pageAliases.TryGetValue(pagePath, out var v) ? v : null;
        }

        private string GetButtonAlias(string pagePath, string buttonPath)
        {
            if (string.IsNullOrEmpty(pagePath) || string.IsNullOrEmpty(buttonPath)) return null;
            if (!_data.buttonAliasesByPage.TryGetValue(pagePath, out var dict)) return null;
            return dict.TryGetValue(buttonPath, out var rec) ? rec.alias : null;
        }

        private void SavePageAlias(string pagePath, string alias)
        {
            if (string.IsNullOrEmpty(pagePath)) return;
            _data.pageAliases[pagePath] = alias ?? string.Empty;
            Persist();
        }

        private void SaveButtonAlias(string pagePath, string buttonPath, string originalName, string alias)
        {
            if (string.IsNullOrEmpty(pagePath) || string.IsNullOrEmpty(buttonPath)) return;
            if (!_data.buttonAliasesByPage.TryGetValue(pagePath, out var dict))
            {
                dict = new Dictionary<string, ButtonRecord>();
                _data.buttonAliasesByPage[pagePath] = dict;
            }
            dict[buttonPath] = new ButtonRecord
            {
                fullPath = buttonPath,
                originalName = originalName ?? string.Empty,
                alias = alias ?? string.Empty,
            };
            Persist();
        }

        private void Load()
        {
            try
            {
                if (File.Exists(_savePath))
                {
                    var json = File.ReadAllText(_savePath, Encoding.UTF8);
                    var data = JsonConvert.DeserializeObject<MappingData>(json);
                    if (data != null)
                    {
                        _data = data;
                        if (_data.pageAliases == null) _data.pageAliases = new Dictionary<string, string>();
                        if (_data.buttonAliasesByPage == null) _data.buttonAliasesByPage = new Dictionary<string, Dictionary<string, ButtonRecord>>();
                        return;
                    }
                }
            }
            catch { }
            _data = new MappingData();
        }

        private void Persist()
        {
            try
            {
                var dir = Path.GetDirectoryName(_savePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(_savePath, JsonConvert.SerializeObject(_data, Formatting.Indented), Encoding.UTF8);
            }
            catch { }
        }
    }

    // ------ 数据结构 ------
    [Serializable]
    public class MappingData
    {
        public Dictionary<string, string> pageAliases = new Dictionary<string, string>();
        public Dictionary<string, Dictionary<string, ButtonRecord>> buttonAliasesByPage = new Dictionary<string, Dictionary<string, ButtonRecord>>();
    }

    [Serializable]
    public class ButtonRecord
    {
        public string fullPath;
        public string originalName;
        public string alias;
    }

    // ------ 轻量 JSON 读取（避免引入完整解析器）------
    internal static class SimpleJson
    {
        public static Dictionary<string, object> ParseObject(string json)
        {
            try
            {
                // 仅提取一级 key: "name" / "fullPath" / "rect"
                var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                ExtractString(json, "name", dict);
                ExtractString(json, "fullPath", dict);
                return dict;
            }
            catch { return null; }
        }

        private static void ExtractString(string json, string key, Dictionary<string, object> outDict)
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
                    outDict[key] = json.Substring(start, end - start);
                }
            }
        }
    }
}


