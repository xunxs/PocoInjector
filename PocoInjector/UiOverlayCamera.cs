using System;
using UnityEngine;

namespace PocoInjector
{
    // 使用独立相机 + GL 绘制高亮边框，避免 OnGUI 被遮挡
    public class UiOverlayCamera : MonoBehaviour
    {
        private Camera _cam;
        private Material _lineMat;
        private UiHighlighter _highlighter;

        public UiOverlayCamera() { }
        public UiOverlayCamera(IntPtr ptr) : base(ptr) { }

        private void Awake()
        {
            _highlighter = FindObjectOfType<UiHighlighter>();

            _cam = gameObject.AddComponent<Camera>();
            _cam.clearFlags = CameraClearFlags.Nothing;
            _cam.cullingMask = 0; // 不渲染任何 3D/2D 图层
            _cam.orthographic = true;
            _cam.orthographicSize = Screen.height * 0.5f;
            _cam.nearClipPlane = -10f;
            _cam.farClipPlane = 10f;
            _cam.depth = 10000; // 最上层

            _lineMat = new Material(Shader.Find("Hidden/Internal-Colored"));
            _lineMat.hideFlags = HideFlags.HideAndDontSave;
            _lineMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _lineMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _lineMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            _lineMat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
        }

        private void OnPostRender()
        {
            if (_lineMat == null || _highlighter == null) return;
            if (!_highlighter.HighlightEnabled) return;

            if (!_highlighter.TryGetCurrentRect(out var rect)) return;
            if (rect.width <= 0 || rect.height <= 0) return;

            _lineMat.SetPass(0);
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, Screen.width, Screen.height, 0); // 与屏幕像素对齐（左上角为(0,0)）
            GL.Begin(GL.QUADS);
            var col = new Color(1f, 0.84f, 0.2f, 0.9f);
            GL.Color(col);
            int t = 3; // 边框厚度
            // 上
            DrawQuad(rect.x, rect.y, rect.width, t);
            // 下
            DrawQuad(rect.x, rect.y + rect.height - t, rect.width, t);
            // 左
            DrawQuad(rect.x, rect.y, t, rect.height);
            // 右
            DrawQuad(rect.x + rect.width - t, rect.y, t, rect.height);
            GL.End();
            GL.PopMatrix();
        }

        private static void DrawQuad(float x, float y, float w, float h)
        {
            GL.Vertex3(x, y, 0);
            GL.Vertex3(x + w, y, 0);
            GL.Vertex3(x + w, y + h, 0);
            GL.Vertex3(x, y + h, 0);
        }
    }
}


