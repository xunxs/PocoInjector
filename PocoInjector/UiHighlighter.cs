using System;
using System.Text;
using UnityEngine;

namespace PocoInjector
{
    // 在屏幕上高亮当前鼠标悬停的 UI（Button/Selectable）元素
    public class UiHighlighter : MonoBehaviour
    {
        // IL2CPP 需要特殊构造函数
        public UiHighlighter() { }
        public UiHighlighter(IntPtr handle) : base(handle) { }

        // 是否启用高亮渲染
        public bool HighlightEnabled = true;

        // 当前命中的对象与其屏幕矩形
        private GameObject _currentTarget;
        private Rect _currentScreenRect;

        // 简单的边框绘制参数
        private readonly Color _borderColor = new Color(1f, 0.84f, 0.2f, 0.85f); // 金色高亮
        private readonly int _borderThickness = 3;

        // 每帧更新：根据鼠标位置选择命中对象
        private void Update()
        {
            if (!HighlightEnabled) { _currentTarget = null; return; }

            try
            {
                Vector2 mousePos = Input.mousePosition;
                // Unity 的屏幕坐标系：左下角为原点；本项目 UI 多为 ScreenSpace 模式，直接用 RectangleContainsScreenPoint
                GameObject best = null;
                float bestArea = -1f; // 选择屏幕面积较小（更具体）的对象作为命中

                var allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                for (int i = 0; i < allObjects.Length; i++)
                {
                    var go = allObjects[i];
                    if (go == null || !go.activeInHierarchy) continue;

                    // 仅考虑包含 Button/Selectable 字样的组件（避免直接依赖 UnityEngine.UI）
                    if (!LooksLikeButton(go)) continue;

                    var rect = GetRectInScreen(go);
                    if (rect.width <= 0 || rect.height <= 0) continue;
                    if (!rect.Contains(new Vector2(mousePos.x, Screen.height - mousePos.y))) // 与 OnGUI 坐标系对齐
                    {
                        continue;
                    }

                    float area = rect.width * rect.height;
                    if (best == null || area < bestArea)
                    {
                        best = go;
                        bestArea = area;
                        _currentScreenRect = rect;
                    }
                }

                _currentTarget = best;
            }
            catch { /* 运行时容错，避免打断游戏 */ }
        }

        // 在屏幕上绘制边框
        private void OnGUI()
        {
            if (!HighlightEnabled || _currentTarget == null) return;

            try
            {
                var prev = GUI.color;
                GUI.color = _borderColor;
                DrawRectBorder(_currentScreenRect, _borderThickness);
                GUI.color = prev;
            }
            catch { }
        }

        // 向外提供当前命中对象的信息（用于 JSON-RPC 返回）
        public string GetHoveredInfoJson()
        {
            if (_currentTarget == null)
            {
                return "{}";
            }

            var rect = _currentScreenRect;
            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"name\":\"").Append(_currentTarget.name).Append("\",");
            sb.Append("\"fullPath\":\"").Append(BuildFullPath(_currentTarget)).Append("\",");
            sb.Append("\"rect\":{")
              .Append("\"x\":").Append(rect.x).Append(",")
              .Append("\"y\":").Append(rect.y).Append(",")
              .Append("\"width\":").Append(rect.width).Append(",")
              .Append("\"height\":").Append(rect.height).Append("}");
            sb.Append("}");
            return sb.ToString();
        }

        // 提供给叠加渲染使用：尝试获取当前矩形
        public bool TryGetCurrentRect(out Rect rect)
        {
            if (_currentTarget != null && _currentScreenRect.width > 0 && _currentScreenRect.height > 0)
            {
                rect = _currentScreenRect;
                return true;
            }
            rect = new Rect(0, 0, 0, 0);
            return false;
        }

        // 根据层级路径查找对象并改名
        public bool RenameByPath(string fullPath, string newName)
        {
            try
            {
                var go = FindByFullPath(fullPath);
                if (go == null) return false;
                go.name = newName;
                return true;
            }
            catch { return false; }
        }

        // 工具：判断是否类似按钮
        private static bool LooksLikeButton(GameObject go)
        {
            try
            {
                var comps = go.GetComponents<Component>();
                bool hasRect = go.GetComponent<RectTransform>() != null;
                bool uiLike = false;
                for (int i = 0; i < comps.Length; i++)
                {
                    var c = comps[i];
                    if (c == null) continue;
                    var n = (c.GetType().FullName ?? c.GetType().Name);
                    if (n.EndsWith("Button", StringComparison.OrdinalIgnoreCase) ||
                        n.EndsWith("Selectable", StringComparison.OrdinalIgnoreCase) ||
                        n.EndsWith("Toggle", StringComparison.OrdinalIgnoreCase) ||
                        n.EndsWith("Tab", StringComparison.OrdinalIgnoreCase) ||
                        n.EndsWith("Pressable", StringComparison.OrdinalIgnoreCase) ||
                        n.EndsWith("Clickable", StringComparison.OrdinalIgnoreCase))
                    { uiLike = true; break; }
                }
                // 无法基于组件名识别时，如果有 RectTransform 也作为候选（兜底高亮）
                if (!uiLike && hasRect) return true;
                return uiLike;
            }
            catch { }
            return false;
        }

        // 计算对象在屏幕坐标的矩形（左下角为原点）
        private static Rect GetRectInScreen(GameObject go)
        {
            try
            {
                var rt = go.GetComponent<RectTransform>();
                if (rt == null)
                {
                    // 非 UI，尝试 Renderer 投影
                    var renderer = go.GetComponent<Renderer>();
                    if (renderer == null) return new Rect(0, 0, 0, 0);
                    var cam = Camera.main;
                    if (cam == null) return new Rect(0, 0, 0, 0);
                    var cen = renderer.bounds.center;
                    var ext = renderer.bounds.extents;
                    Vector2[] pts = new Vector2[8]
                    {
                        WorldToGUIPoint(cam, new Vector3 (cen.x - ext.x, cen.y - ext.y, cen.z - ext.z)),
                        WorldToGUIPoint(cam, new Vector3 (cen.x + ext.x, cen.y - ext.y, cen.z - ext.z)),
                        WorldToGUIPoint(cam, new Vector3 (cen.x - ext.x, cen.y - ext.y, cen.z + ext.z)),
                        WorldToGUIPoint(cam, new Vector3 (cen.x + ext.x, cen.y - ext.y, cen.z + ext.z)),
                        WorldToGUIPoint(cam, new Vector3 (cen.x - ext.x, cen.y + ext.y, cen.z - ext.z)),
                        WorldToGUIPoint(cam, new Vector3 (cen.x + ext.x, cen.y + ext.y, cen.z - ext.z)),
                        WorldToGUIPoint(cam, new Vector3 (cen.x - ext.x, cen.y + ext.y, cen.z + ext.z)),
                        WorldToGUIPoint(cam, new Vector3 (cen.x + ext.x, cen.y + ext.y, cen.z + ext.z))
                    };
                    var min = pts[0]; var max = pts[0];
                    for (int i = 1; i < pts.Length; i++) { min = Vector2.Min(min, pts[i]); max = Vector2.Max(max, pts[i]); }
                    return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
                }

                // ScreenSpace：将 RectTransform 转为屏幕矩形
                Vector2 size = Vector2.Scale(rt.rect.size, rt.lossyScale);
                Rect rect = new Rect(rt.position.x, Screen.height - rt.position.y, size.x, size.y);
                rect.x -= (rt.pivot.x * size.x);
                rect.y -= ((1.0f - rt.pivot.y) * size.y);
                return rect;
            }
            catch { return new Rect(0, 0, 0, 0); }
        }

        private static Vector2 WorldToGUIPoint(Camera camera, Vector3 world)
        {
            Vector2 sp = camera.WorldToScreenPoint(world);
            sp.y = Screen.height - sp.y;
            return sp;
        }

        private static void DrawRectBorder(Rect rect, int thickness)
        {
            // 使用内置白纹理绘制 4 条边
            var tex = Texture2D.whiteTexture;
            // 上
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), tex);
            // 下
            GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - thickness, rect.width, thickness), tex);
            // 左
            GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), tex);
            // 右
            GUI.DrawTexture(new Rect(rect.x + rect.width - thickness, rect.y, thickness, rect.height), tex);
        }

        // 生成层级路径 Root/Parent/Child
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

        private static GameObject FindByFullPath(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return null;
            var parts = fullPath.Split('/');
            GameObject current = null;

            // 查找根对象
            var roots = UnityEngine.Object.FindObjectsOfType<GameObject>();
            for (int i = 0; i < roots.Length; i++)
            {
                var go = roots[i];
                if (go != null && go.transform.parent == null && go.name == parts[0])
                {
                    current = go;
                    break;
                }
            }
            if (current == null) return null;

            // 逐级下钻
            for (int i = 1; i < parts.Length; i++)
            {
                var parent = current.transform;
                current = null;
                for (int c = 0; c < parent.childCount; c++)
                {
                    var child = parent.GetChild(c).gameObject;
                    if (child.name == parts[i]) { current = child; break; }
                }
                if (current == null) return null;
            }
            return current;
        }
    }
}


