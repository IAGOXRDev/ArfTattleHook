using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArfTattleHook.UI
{
    public class HudToast
    {
        public string Source;
        public string Content;
        public Color AccentColor;
        public float StartTime;
        public float Duration;

        public float Elapsed => Time.realtimeSinceStartup - StartTime;
        public float Progress => Mathf.Clamp01(Elapsed / Duration);
        public bool IsExpired => Elapsed >= Duration;
    }

    public static class HudNotificationManager
    {
        private static readonly List<HudToast> _toasts = new List<HudToast>();
        private static readonly List<string> _logHistory = new List<string>();
        private static GUIStyle _logStyle = null;

        private const float DefaultLifeTime = 5.0f;

        public static void LogModuleToggle(string moduleName, bool enabled)
        {
            string statusStr = enabled ? "<color=#00FF00>ON</color>" : "<color=#FF0000>OFF</color>";
            string content = $"<color=#00FFFF>{moduleName}:</color> {statusStr}";
            Color accent = enabled ? new Color(0.0f, 1f, 0.5f, 1f) : new Color(1f, 0.25f, 0.25f, 1f);

            AddLog("ArfHook", content, accent, 5.0f);
        }

        public static void LogCustom(string content, float duration = DefaultLifeTime)
        {
            AddLog("ArfHook", content, new Color(0.7f, 0.7f, 1f, 1f), duration);
        }

        public static void Log(string source, string content, Color accent, float duration = DefaultLifeTime)
        {
            AddLog(source, content, accent, duration);
        }

        private static void AddLog(string source, string content, Color accent, float duration)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            _logHistory.Add($"<color=#777777>[{time}]</color> <color=#00FFFF>[{source}]</color> {content}");
            if (_logHistory.Count > 300) _logHistory.RemoveAt(0);

            _toasts.Add(new HudToast
            {
                Source = source,
                Content = content,
                AccentColor = accent,
                StartTime = Time.realtimeSinceStartup,
                Duration = duration
            });
        }

        public static void OnGUI()
        {
            if (_toasts.Count == 0) return;

            InitStyles();

            // Clear expired
            for (int i = _toasts.Count - 1; i >= 0; i--)
            {
                if (_toasts[i].IsExpired)
                {
                    _toasts.RemoveAt(i);
                }
            }

            if (_toasts.Count == 0) return;

            float marginX = 22f;
            float marginY = 22f;
            float rowHeight = 22f;
            float startY = Screen.height - marginY;

            for (int i = 0; i < _toasts.Count; i++)
            {
                var toast = _toasts[i];
                float elapsed = toast.Elapsed;

                // 1. Smooth Fade-in (0.15s) and Fade-out (last 1.2s)
                float alpha = 1f;
                if (elapsed < 0.15f)
                {
                    alpha = Mathf.Clamp01(elapsed / 0.15f);
                }
                else
                {
                    float fadeStart = toast.Duration - 1.2f;
                    if (elapsed > fadeStart)
                    {
                        alpha = Mathf.Clamp01(1f - ((elapsed - fadeStart) / 1.2f));
                    }
                }

                // 2. Smooth Slide-in from left (no erratic random jitter!)
                float slideOffset = 0f;
                if (elapsed < 0.20f)
                {
                    float t = elapsed / 0.20f;
                    // Ease-out cubic: fast entry, smooth deceleration to 0
                    slideOffset = (1f - Mathf.Pow(1f - t, 3)) * 24f - 24f;
                }

                // Positioning bottom-to-top
                float y = startY - (_toasts.Count - i) * (rowHeight + 2f);
                if (y < 10f) continue;

                float x = marginX + slideOffset;

                // 3. Tag rendering
                string renderedText;
                bool isArfHook = toast.Source.IndexOf("ArfHook", StringComparison.OrdinalIgnoreCase) >= 0;
                if (isArfHook)
                {
                    string rainbowTag = BuildRainbowTag($"[{toast.Source}]");
                    renderedText = $"{rainbowTag} {toast.Content}";
                }
                else
                {
                    renderedText = $"<color=#00FFFF>[{toast.Source}]</color> {toast.Content}";
                }

                GUIContent guiContent = new GUIContent(renderedText);
                Vector2 size = _logStyle.CalcSize(guiContent);
                float textWidth = size.x + 30f;

                Color prevColor = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, alpha);

                _logStyle.normal.textColor = new Color(1f, 1f, 1f, alpha);
                GUI.Label(new Rect(x, y, textWidth, rowHeight), renderedText, _logStyle);

                GUI.color = prevColor;
            }
        }

        private static string BuildRainbowTag(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            float time = Time.time * 1.2f;
            for (int i = 0; i < text.Length; i++)
            {
                float hue = (time + (i * 0.06f)) % 1f;
                Color32 col = Color.HSVToRGB(hue, 1f, 1f);
                string hex = col.r.ToString("X2") + col.g.ToString("X2") + col.b.ToString("X2");
                sb.Append($"<color=#{hex}>{text[i]}</color>");
            }
            return sb.ToString();
        }

        private static void InitStyles()
        {
            if (_logStyle == null)
            {
                _logStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    richText = true,
                    alignment = TextAnchor.MiddleLeft
                };
            }
        }
    }
}
