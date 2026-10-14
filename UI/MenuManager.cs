using System;
using System.Collections.Generic;
using UnityEngine;
using MelonLoader;
using ArfTattleHook.Modules;

namespace ArfTattleHook.UI
{
    public class MenuManager
    {
        public static MenuManager Instance { get; private set; }
        public static bool IsMenuOpen { get; private set; } = false;

        public List<IModModule> Modules { get; } = new List<IModModule>();

        private Rect _windowRect = new Rect(80f, 80f, 480f, 500f);
        public const float MinWindowWidth = 360f;
        public const float MinWindowHeight = 300f;
        private bool _isResizing = false;
        private Vector2 _resizeStartMouse = Vector2.zero;
        private Vector2 _resizeStartSize = Vector2.zero;

        private MelonPreferences_Category _prefCategory;
        private MelonPreferences_Entry<float> _prefWindowWidth;
        private MelonPreferences_Entry<float> _prefWindowHeight;

        private int _selectedCategory = 0;
        private readonly string[] _categories = new string[] { "Movement", "Visuals", "Stuff", "Debug", "About" };
        private Vector2 _scrollPos = Vector2.zero;

        // Custom Styles
        private Texture2D _texWindowBg;
        private Texture2D _texCardBg;
        private Texture2D _texButtonOn;
        private Texture2D _texButtonOff;
        private Texture2D _texSpeedoBg;

        private GUIStyle _styleWindow;
        private GUIStyle _styleTitle;
        private GUIStyle _styleCard;
        private GUIStyle _styleTabButton;
        private GUIStyle _styleTabActive;
        private GUIStyle _styleToggleOn;
        private GUIStyle _styleToggleOff;
        private GUIStyle _styleExpandButton;
        private GUIStyle _styleModuleHeader;
        private GUIStyle _styleModuleDesc;
        private GUIStyle _styleResizeGrip;

        private GUIStyle _styleSpeedoBox;
        private GUIStyle _styleSpeedoLabel;
        private GUIStyle _styleSpeedoVal;

        private CursorLockMode _previousLockMode = CursorLockMode.Locked;
        private bool _previousCursorVisible = false;

        public MenuManager()
        {
            Instance = this;
            try
            {
                _prefCategory = MelonPreferences.CreateCategory("ArfTattleHook_Menu", "Menu Settings");
                _prefWindowWidth = _prefCategory.CreateEntry("WindowWidth", 480f, "Menu Width");
                _prefWindowHeight = _prefCategory.CreateEntry("WindowHeight", 500f, "Menu Height");

                float w = Mathf.Max(MinWindowWidth, _prefWindowWidth.Value);
                float h = Mathf.Max(MinWindowHeight, _prefWindowHeight.Value);
                _windowRect = new Rect(80f, 80f, w, h);
            }
            catch { }
        }

        public void RegisterModule(IModModule module)
        {
            if (module != null && !Modules.Contains(module))
            {
                Modules.Add(module);
            }
        }

        public void OnUpdate()
        {
            // Toggle menu with Insert or F1
            if (Input.GetKeyDown(KeyCode.Insert) || Input.GetKeyDown(KeyCode.F1))
            {
                ToggleMenu();
            }

            // Update registered modules
            for (int i = 0; i < Modules.Count; i++)
            {
                try
                {
                    Modules[i].OnUpdate();
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"[Module {Modules[i].Name}] Error in OnUpdate: {ex.Message}");
                }
            }

            // Keep cursor visible while menu is open
            if (IsMenuOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void ToggleMenu()
        {
            IsMenuOpen = !IsMenuOpen;

            if (IsMenuOpen)
            {
                _previousLockMode = Cursor.lockState;
                _previousCursorVisible = Cursor.visible;

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = _previousLockMode == CursorLockMode.None ? CursorLockMode.Locked : _previousLockMode;
                Cursor.visible = _previousCursorVisible;
                SaveWindowSize();
            }
        }

        private void SaveWindowSize()
        {
            if (_prefWindowWidth != null && _prefWindowHeight != null)
            {
                _prefWindowWidth.Value = _windowRect.width;
                _prefWindowHeight.Value = _windowRect.height;
                _prefCategory.SaveToFile();
            }
        }

        public void OnGUI()
        {
            InitStyles();

            // Draw Speedometer HUD
            var bhop = BhopModule.Instance;
            if (bhop != null && bhop.ShowSpeedometer)
            {
                DrawSpeedometer(bhop);
            }

            if (IsMenuOpen)
            {
                HandleWindowResize();
                DrawRainbowBorder(_windowRect, 2.5f);
                _windowRect = GUI.Window(7777, _windowRect, (GUI.WindowFunction)DrawWindow, GUIContent.none, _styleWindow);
            }

            HudNotificationManager.OnGUI();
        }

        private void DrawSpeedometer(BhopModule bhop)
        {
            float boxWidth = 190f;
            float boxHeight = 65f;
            float margin = 20f;
            Rect rect = new Rect(Screen.width - boxWidth - margin, Screen.height - boxHeight - margin, boxWidth, boxHeight);

            GUI.Box(rect, GUIContent.none, _styleSpeedoBox);

            GUILayout.BeginArea(new Rect(rect.x + 10, rect.y + 8, rect.width - 20, rect.height - 16));

            GUILayout.BeginHorizontal();
            GUILayout.Label("BHOP", _styleSpeedoLabel);
            string statusText = bhop.Enabled ? "<color=#10B981>ON</color>" : "<color=#EF4444>OFF</color>";
            GUILayout.Label(statusText, new GUIStyle(_styleSpeedoLabel) { alignment = TextAnchor.MiddleRight });
            GUILayout.EndHorizontal();

            float displaySpeed = bhop.CurrentSpeed;
            string speedColor = displaySpeed > 16f ? "#06B6D4" : (displaySpeed > 8f ? "#10B981" : "#F3F4F6");
            string speedString = $"<color={speedColor}>{displaySpeed:F1}</color> <size=12><color=#9CA3AF>m/s</color></size>";
            GUILayout.Label(speedString, _styleSpeedoVal);

            GUILayout.EndArea();
        }

        private void HandleWindowResize()
        {
            Event e = Event.current;
            if (e == null) return;

            Rect gripScreenRect = new Rect(
                _windowRect.x + _windowRect.width - 24f,
                _windowRect.y + _windowRect.height - 24f,
                24f,
                24f
            );

            if (e.type == EventType.MouseDown && e.button == 0 && gripScreenRect.Contains(e.mousePosition))
            {
                _isResizing = true;
                _resizeStartMouse = e.mousePosition;
                _resizeStartSize = new Vector2(_windowRect.width, _windowRect.height);
                e.Use();
            }
            else if (e.rawType == EventType.MouseUp || (e.type == EventType.MouseUp && e.button == 0))
            {
                if (_isResizing)
                {
                    _isResizing = false;
                    SaveWindowSize();
                }
            }
            else if (_isResizing)
            {
                if (!Input.GetMouseButton(0))
                {
                    _isResizing = false;
                    SaveWindowSize();
                    return;
                }

                Vector2 delta = e.mousePosition - _resizeStartMouse;
                float maxW = Mathf.Max(MinWindowWidth, Screen.width - _windowRect.x - 10f);
                float maxH = Mathf.Max(MinWindowHeight, Screen.height - _windowRect.y - 10f);

                _windowRect.width = Mathf.Clamp(_resizeStartSize.x + delta.x, MinWindowWidth, maxW);
                _windowRect.height = Mathf.Clamp(_resizeStartSize.y + delta.y, MinWindowHeight, maxH);
            }
        }

        private void DrawWindow(int windowId)
        {
            GUI.DragWindow(new Rect(0, 0, _windowRect.width - 35, 32));

            GUILayout.BeginArea(new Rect(12, 8, _windowRect.width - 24, _windowRect.height - 16));

            // Title Bar
            GUILayout.BeginHorizontal();
            GUILayout.Label("<color=#A78BFA><b>ARFTATTLEHOOK</b></color> <color=#9CA3AF>|</color> <size=12><color=#E5E7EB>Tattletail</color></size>", _styleTitle);
            if (GUILayout.Button("✕", new GUIStyle(GUI.skin.button)
            {
                fixedWidth = 24,
                fixedHeight = 22,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.85f, 0.35f, 0.35f) }
            }))
            {
                ToggleMenu();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6f);

            // Tabs
            GUILayout.BeginHorizontal();
            for (int i = 0; i < _categories.Length; i++)
            {
                bool isSelected = (_selectedCategory == i);
                GUIStyle tabStyle = isSelected ? _styleTabActive : _styleTabButton;
                if (GUILayout.Button(_categories[i], tabStyle, GUILayout.Height(26)))
                {
                    _selectedCategory = i;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6f);
            DrawRainbowSeparator(2f);
            GUILayout.Space(6f);

            // Content
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, false, false);

            if (_selectedCategory == 0)
            {
                DrawModulesForCategory("Movement");
            }
            else if (_selectedCategory == 1)
            {
                DrawModulesForCategory("Visuals");
            }
            else if (_selectedCategory == 2)
            {
                DrawModulesForCategory("Stuff");
            }
            else if (_selectedCategory == 3)
            {
                DrawModulesForCategory("Debug");
            }
            else if (_selectedCategory == 4)
            {
                DrawAboutTab();
            }

            GUILayout.EndScrollView();

            GUILayout.Space(4f);
            GUILayout.Label("<color=#6B7280>Presiona [<b>Insert</b>] o [<b>F1</b>] para abrir/cerrar | ArfHook Engine</color>",
                new GUIStyle(GUI.skin.label) { richText = true, fontSize = 10, alignment = TextAnchor.MiddleCenter });

            GUILayout.EndArea();

            // Resize handle
            Rect gripLocalRect = new Rect(_windowRect.width - 20f, _windowRect.height - 20f, 20f, 20f);
            GUI.Label(gripLocalRect, "<color=#8B5CF6><b>◢</b></color>", _styleResizeGrip);
        }

        private void DrawModulesForCategory(string category)
        {
            for (int i = 0; i < Modules.Count; i++)
            {
                var mod = Modules[i];
                if (mod.Category != category) continue;

                GUILayout.BeginVertical(_styleCard);

                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical();
                GUILayout.Label($"<b>{mod.Name}</b>", _styleModuleHeader);
                if (!string.IsNullOrEmpty(mod.Description))
                {
                    GUILayout.Label(mod.Description, _styleModuleDesc);
                }
                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                // Settings drawer button
                string expandText = mod.IsExpanded ? "▲ Ajustes" : "▼ Ajustes";
                if (GUILayout.Button(expandText, _styleExpandButton, GUILayout.Width(72), GUILayout.Height(26)))
                {
                    mod.IsExpanded = !mod.IsExpanded;
                }

                GUILayout.Space(6f);

                // Main ON/OFF toggle
                GUIStyle toggleStyle = mod.Enabled ? _styleToggleOn : _styleToggleOff;
                string toggleText = mod.Enabled ? "ON" : "OFF";
                if (GUILayout.Button(toggleText, toggleStyle, GUILayout.Width(46), GUILayout.Height(26)))
                {
                    mod.Enabled = !mod.Enabled;
                    HudNotificationManager.LogModuleToggle(mod.Name, mod.Enabled);
                }

                GUILayout.EndHorizontal();

                if (mod.IsExpanded)
                {
                    GUILayout.Space(6f);
                    GUILayout.Box(GUIContent.none, GUILayout.Height(1), GUILayout.ExpandWidth(true));
                    GUILayout.Space(4f);
                    mod.OnDrawSettings();
                }

                GUILayout.EndVertical();
                GUILayout.Space(6f);
            }
        }

        private void DrawAboutTab()
        {
            GUILayout.BeginVertical(_styleCard);
            GUILayout.Label("<size=16><color=#C084FC><b>ArfTattleHook v1.0.0</b></color></size>", _styleTitle);
            GUILayout.Label("<color=#9CA3AF>Created by</color> <color=#FCD34D><b>IAGOXR</b></color>", _styleModuleDesc);
            GUILayout.Space(8f);
            GUILayout.Label("<color=#D1D5DB>Mod Client para <b>Tattletail</b> (Waygetter Electronics).</color>", _styleModuleDesc);
            GUILayout.Label("<color=#D1D5DB>• <b>BunnyHop estilo GoldSrc/Source:</b> Auto-jump con aceleración de strafe en el aire (A/D + ratón) y velocímetro.</color>", _styleModuleDesc);
            GUILayout.Label("<color=#D1D5DB>• <b>Field of View (FOV):</b> Slider de cámara de 35° a 130°, preajustes y zoom con tecla [C].</color>", _styleModuleDesc);
            GUILayout.Label("<color=#D1D5DB>• <b>Utilidades:</b> Linterna con batería infinita, Fullbright, instant interact y hacks.</color>", _styleModuleDesc);
            GUILayout.Label("<color=#D1D5DB>• <b>Menú interactivo IMGUI:</b> Presiona <color=#FCD34D>[Insert]</color> o <color=#FCD34D>[F1]</color>.</color>", _styleModuleDesc);
            GUILayout.Space(10f);
            GUILayout.Label("<color=#60A5FA><b>Discord:</b> dsc.gg/xRd</color>", _styleModuleDesc);
            GUILayout.EndVertical();
        }

        private void InitStyles()
        {
            if (_styleWindow != null) return;

            _texWindowBg = MakeBorderTex(16, 16, new Color(0.08f, 0.08f, 0.11f, 0.94f), new Color(0.25f, 0.20f, 0.35f, 0.9f));
            _texCardBg = MakeBorderTex(16, 16, new Color(0.12f, 0.12f, 0.16f, 0.85f), new Color(0.20f, 0.18f, 0.28f, 0.8f));
            _texButtonOn = MakeTex(16, 16, new Color(0.15f, 0.55f, 0.35f, 1f));
            _texButtonOff = MakeTex(16, 16, new Color(0.28f, 0.28f, 0.32f, 1f));
            _texSpeedoBg = MakeTex(1, 1, new Color(0.08f, 0.08f, 0.10f, 0.85f));

            _styleWindow = new GUIStyle(GUI.skin.window);
            SetAllBackgrounds(_styleWindow, _texWindowBg);
            _styleWindow.padding = new RectOffset(4, 4, 4, 4);

            _styleTitle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                richText = true,
                alignment = TextAnchor.MiddleLeft
            };

            _styleCard = new GUIStyle(GUI.skin.box);
            SetAllBackgrounds(_styleCard, _texCardBg);
            _styleCard.padding = new RectOffset(10, 10, 8, 8);

            Texture2D texTabNormal = MakeTex(16, 16, new Color(0.14f, 0.14f, 0.18f, 1f));
            Texture2D texTabActive = MakeTex(16, 16, new Color(0.38f, 0.25f, 0.55f, 1f));

            _styleTabButton = new GUIStyle(GUI.skin.button);
            SetAllBackgrounds(_styleTabButton, texTabNormal);
            _styleTabButton.normal.textColor = new Color(0.75f, 0.75f, 0.8f);
            _styleTabButton.fontStyle = FontStyle.Bold;

            _styleTabActive = new GUIStyle(GUI.skin.button);
            SetAllBackgrounds(_styleTabActive, texTabActive);
            _styleTabActive.normal.textColor = Color.white;
            _styleTabActive.fontStyle = FontStyle.Bold;

            _styleToggleOn = new GUIStyle(GUI.skin.button);
            SetAllBackgrounds(_styleToggleOn, _texButtonOn);
            _styleToggleOn.normal.textColor = Color.white;
            _styleToggleOn.fontStyle = FontStyle.Bold;

            _styleToggleOff = new GUIStyle(GUI.skin.button);
            SetAllBackgrounds(_styleToggleOff, _texButtonOff);
            _styleToggleOff.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
            _styleToggleOff.fontStyle = FontStyle.Bold;

            _styleExpandButton = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                normal = { textColor = new Color(0.8f, 0.8f, 0.9f) }
            };

            _styleModuleHeader = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.95f, 0.95f, 1f) }
            };

            _styleModuleDesc = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                richText = true,
                normal = { textColor = new Color(0.7f, 0.7f, 0.75f) },
                wordWrap = true
            };

            _styleResizeGrip = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.LowerRight,
                fontSize = 14,
                richText = true
            };

            // Speedometer styles
            _styleSpeedoBox = new GUIStyle();
            SetAllBackgrounds(_styleSpeedoBox, _texSpeedoBg);
            _styleSpeedoBox.padding = new RectOffset(8, 8, 6, 6);

            _styleSpeedoLabel = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                richText = true,
                normal = { textColor = new Color(0.80f, 0.80f, 0.85f) }
            };

            _styleSpeedoVal = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                richText = true,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
        }

        private void SetAllBackgrounds(GUIStyle style, Texture2D tex)
        {
            style.normal.background = tex;
            style.hover.background = tex;
            style.active.background = tex;
            style.focused.background = tex;
            style.onNormal.background = tex;
            style.onHover.background = tex;
            style.onActive.background = tex;
            style.onFocused.background = tex;
        }

        private Texture2D MakeBorderTex(int width, int height, Color bg, Color border)
        {
            Color[] pix = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                        pix[y * width + x] = border;
                    else
                        pix[y * width + x] = bg;
                }
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private static void DrawRainbowSeparator(float thickness)
        {
            float time = Time.realtimeSinceStartup * 0.35f;
            Color lineCol = Color.HSVToRGB(time % 1f, 0.85f, 1f);
            Color old = GUI.color;
            GUI.color = lineCol;
            GUILayout.Box(GUIContent.none, GUILayout.Height(thickness), GUILayout.ExpandWidth(true));
            GUI.color = old;
        }

        private static void DrawRainbowBorder(Rect rect, float thickness)
        {
            float time = Time.realtimeSinceStartup * 0.35f;
            Color borderCol = Color.HSVToRGB(time % 1f, 0.85f, 1f);
            Color old = GUI.color;
            GUI.color = borderCol;

            // Top
            GUI.DrawTexture(new Rect(rect.x - thickness, rect.y - thickness, rect.width + thickness * 2f, thickness), Texture2D.whiteTexture);
            // Bottom
            GUI.DrawTexture(new Rect(rect.x - thickness, rect.y + rect.height, rect.width + thickness * 2f, thickness), Texture2D.whiteTexture);
            // Left
            GUI.DrawTexture(new Rect(rect.x - thickness, rect.y, thickness, rect.height), Texture2D.whiteTexture);
            // Right
            GUI.DrawTexture(new Rect(rect.x + rect.width, rect.y, thickness, rect.height), Texture2D.whiteTexture);

            GUI.color = old;
        }
    }
}
