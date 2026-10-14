using System;
using MelonLoader;
using UnityEngine;

namespace ArfTattleHook.Modules
{
    public class FovModule : IModModule
    {
        public static FovModule Instance { get; private set; }

        public string Id => "CameraFov";
        public string Name => "Field of View (FOV)";
        public string Description => "Slider de FOV de cámara, preajustes rápidos y tecla de zoom";
        public string Category => "Visuals";

        public bool Enabled
        {
            get => _prefEnabled?.Value ?? true;
            set
            {
                if (_prefEnabled != null)
                {
                    _prefEnabled.Value = value;
                    _prefCategory.SaveToFile();
                    if (!value)
                    {
                        ResetFov();
                    }
                }
            }
        }

        public bool IsExpanded { get; set; } = true;

        public float TargetFov
        {
            get => _prefTargetFov?.Value ?? 85f;
            set
            {
                if (_prefTargetFov != null)
                {
                    _prefTargetFov.Value = value;
                    _prefCategory.SaveToFile();
                }
            }
        }

        public bool EnableZoom
        {
            get => _prefEnableZoom?.Value ?? true;
            set { if (_prefEnableZoom != null) { _prefEnableZoom.Value = value; _prefCategory.SaveToFile(); } }
        }

        public float ZoomFov
        {
            get => _prefZoomFov?.Value ?? 30f;
            set { if (_prefZoomFov != null) { _prefZoomFov.Value = value; _prefCategory.SaveToFile(); } }
        }

        public bool SmoothTransitions
        {
            get => _prefSmoothTransitions?.Value ?? true;
            set { if (_prefSmoothTransitions != null) { _prefSmoothTransitions.Value = value; _prefCategory.SaveToFile(); } }
        }

        private MelonPreferences_Category _prefCategory;
        private MelonPreferences_Entry<bool> _prefEnabled;
        private MelonPreferences_Entry<float> _prefTargetFov;
        private MelonPreferences_Entry<bool> _prefEnableZoom;
        private MelonPreferences_Entry<float> _prefZoomFov;
        private MelonPreferences_Entry<bool> _prefSmoothTransitions;

        private float _originalFov = 60f;
        private bool _hasCachedOriginalFov = false;

        public FovModule()
        {
            Instance = this;
        }

        public void OnInitialize()
        {
            try
            {
                _prefCategory = MelonPreferences.CreateCategory("ArfTattleHook_Fov", "Camera FOV Settings");
                _prefEnabled = _prefCategory.CreateEntry("Enabled", true, "Enable custom FOV");
                _prefTargetFov = _prefCategory.CreateEntry("TargetFov", 85f, "Default target FOV in degrees");
                _prefEnableZoom = _prefCategory.CreateEntry("EnableZoom", true, "Enable zoom hotkey [C]");
                _prefZoomFov = _prefCategory.CreateEntry("ZoomFov", 30f, "Zoom FOV in degrees");
                _prefSmoothTransitions = _prefCategory.CreateEntry("SmoothTransitions", true, "Smoothly interpolate FOV changes");
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[FovModule] Failed to load preferences: {ex.Message}");
            }
        }

        public void OnUpdate()
        {
            Camera cam = GetActiveCamera();
            if (cam == null) return;

            if (!_hasCachedOriginalFov)
            {
                _originalFov = cam.fieldOfView > 10f ? cam.fieldOfView : 60f;
                _hasCachedOriginalFov = true;
            }

            if (!Enabled)
            {
                if (Mathf.Abs(cam.fieldOfView - _originalFov) > 0.05f)
                {
                    cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, _originalFov, Time.deltaTime * 12f);
                }
                return;
            }

            // Check zoom key
            bool isZooming = EnableZoom && Input.GetKey(KeyCode.C);
            float desiredFov = isZooming ? ZoomFov : TargetFov;

            if (SmoothTransitions)
            {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, desiredFov, Time.deltaTime * 14f);
            }
            else
            {
                cam.fieldOfView = desiredFov;
            }
        }

        public void OnFixedUpdate()
        {
        }

        private Camera GetActiveCamera()
        {
            Camera cam = Camera.main;
            if (cam == null && RM.fpsController != null)
            {
                cam = RM.fpsController.GetComponentInChildren<Camera>();
            }
            return cam;
        }

        public void ResetFov()
        {
            Camera cam = GetActiveCamera();
            if (cam != null && _hasCachedOriginalFov)
            {
                cam.fieldOfView = _originalFov;
            }
        }

        public void OnDrawSettings()
        {
            GUILayout.BeginVertical();

            Camera cam = GetActiveCamera();
            float currentCamFov = cam != null ? cam.fieldOfView : TargetFov;

            GUILayout.BeginHorizontal();
            GUILayout.Label($"FOV Objetivo: <color=#38BDF8><b>{TargetFov:F0}°</b></color>", GUILayout.Width(140));
            GUILayout.FlexibleSpace();
            GUILayout.Label($"<color=#9CA3AF>(En cámara: {currentCamFov:F0}°)</color>");
            GUILayout.EndHorizontal();

            // FOV Slider
            float newFov = GUILayout.HorizontalSlider(TargetFov, 35f, 130f);
            if (Mathf.Abs(newFov - TargetFov) > 0.5f)
            {
                TargetFov = (float)Math.Round(newFov, 0);
            }

            GUILayout.Space(6f);

            // Quick Presets
            GUILayout.BeginHorizontal();
            GUILayout.Label("Preajustes:", GUILayout.Width(70));
            if (GUILayout.Button("60° (Base)", GUILayout.Height(22))) TargetFov = 60f;
            if (GUILayout.Button("75°", GUILayout.Height(22))) TargetFov = 75f;
            if (GUILayout.Button("85°", GUILayout.Height(22))) TargetFov = 85f;
            if (GUILayout.Button("95°", GUILayout.Height(22))) TargetFov = 95f;
            if (GUILayout.Button("110°", GUILayout.Height(22))) TargetFov = 110f;
            GUILayout.EndHorizontal();

            GUILayout.Space(8f);

            // Smooth transitions toggle
            bool smooth = GUILayout.Toggle(SmoothTransitions, "  Transición Suave / Interpolación");
            if (smooth != SmoothTransitions) SmoothTransitions = smooth;

            // Zoom toggle
            bool zoom = GUILayout.Toggle(EnableZoom, "  Zoom con tecla [C]");
            if (zoom != EnableZoom) EnableZoom = zoom;

            if (EnableZoom)
            {
                GUILayout.Space(4f);
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Zoom FOV: <color=#FCD34D><b>{ZoomFov:F0}°</b></color>", GUILayout.Width(120));
                float newZoom = GUILayout.HorizontalSlider(ZoomFov, 10f, 60f);
                if (Mathf.Abs(newZoom - ZoomFov) > 0.5f)
                {
                    ZoomFov = (float)Math.Round(newZoom, 0);
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }
    }
}
