using System;
using MelonLoader;
using UnityEngine;
using ArfTattleHook.UI;

namespace ArfTattleHook.Modules
{
    public class StuffModule : IModModule
    {
        public static StuffModule Instance { get; private set; }

        public string Id => "TattleStuff";
        public string Name => "Utilidades de Tattletail";
        public string Description => "Opciones de juego, linterna infinita, Fullbright balanceado y cheats";
        public string Category => "Stuff";

        public bool Enabled { get; set; } = true;
        public bool IsExpanded { get; set; } = true;

        public bool GodMode
        {
            get => _godMode;
            set
            {
                _godMode = value;
                GS.godMode = value;
            }
        }

        public bool HackerMode
        {
            get => _hackerMode;
            set
            {
                _hackerMode = value;
                GS.hacker = value;
                if (value)
                {
                    GS.godMode = true;
                    GS.noHoldTime = true;
                    if (RM.fpsController != null)
                    {
                        RM.fpsController.SetRunSpeed(6.0f);
                    }
                }
                else
                {
                    if (!_godMode) GS.godMode = false;
                    if (!_noHoldTime) GS.noHoldTime = false;
                    if (RM.fpsController != null)
                    {
                        RM.fpsController.SetRunSpeed(2.4f);
                    }
                }
            }
        }

        public bool Fullbright
        {
            get => _fullbright;
            set
            {
                _fullbright = value;
                ApplyFullbright(value);
            }
        }

        public float FullbrightIntensity { get; set; } = 0.55f;
        public float FullbrightRange { get; set; } = 35f;
        public float AmbientIntensity { get; set; } = 0.45f;
        public bool DisableBloomWithFullbright { get; set; } = true;

        public bool NoHoldTime
        {
            get => _noHoldTime;
            set
            {
                _noHoldTime = value;
                GS.noHoldTime = value;
            }
        }

        public bool InfiniteFlashlight
        {
            get => _infiniteFlashlight;
            set
            {
                _infiniteFlashlight = value;
                ApplyFlashlight();
            }
        }

        private bool _godMode = false;
        private bool _hackerMode = false;
        private bool _fullbright = false;
        private bool _noHoldTime = false;
        private bool _infiniteFlashlight = false;

        // Fullbright persistent light & settings
        private GameObject _fullbrightObj = null;
        private Light _fullbrightLight = null;
        private Color _origAmbientLight = Color.black;
        private float _origAmbientIntensity = 1.0f;
        private bool _origFog = true;
        private bool _cachedOrigLighting = false;

        public StuffModule()
        {
            Instance = this;
        }

        public void OnInitialize()
        {
        }

        public void OnUpdate()
        {
            // Keep God Mode flags synchronized
            if (_godMode || _hackerMode)
            {
                GS.godMode = true;
            }

            // Keep Hacker Mode speed applied
            if (_hackerMode)
            {
                GS.noHoldTime = true;
                if (RM.fpsController != null)
                {
                    RM.fpsController.SetRunSpeed(6.0f);
                }
            }
            else if (_noHoldTime)
            {
                GS.noHoldTime = true;
            }

            // Keep Fullbright active and position light at player camera
            if (_fullbright)
            {
                EnforceFullbright();
            }

            // Keep Flashlight full if infinite is enabled
            if (_infiniteFlashlight)
            {
                ApplyFlashlight();
            }
        }

        public void OnFixedUpdate()
        {
        }

        private void ApplyFullbright(bool enable)
        {
            if (enable)
            {
                if (!_cachedOrigLighting)
                {
                    _origAmbientLight = RenderSettings.ambientLight;
                    _origAmbientIntensity = RenderSettings.ambientIntensity;
                    _origFog = RenderSettings.fog;
                    _cachedOrigLighting = true;
                }

                if (DisableBloomWithFullbright)
                {
                    try { GS.PostEffects(false); } catch { }
                }

                EnforceFullbright();
            }
            else
            {
                if (_fullbrightObj != null)
                {
                    _fullbrightObj.SetActive(false);
                }

                if (DisableBloomWithFullbright)
                {
                    try { GS.PostEffects(true); } catch { }
                }

                if (_cachedOrigLighting)
                {
                    RenderSettings.ambientLight = _origAmbientLight;
                    RenderSettings.ambientIntensity = _origAmbientIntensity;
                    RenderSettings.fog = _origFog;
                }
            }
        }

        private void EnforceFullbright()
        {
            // Set soft, readable ambient light without burning out bloom
            Color softAmb = new Color(AmbientIntensity, AmbientIntensity, AmbientIntensity);
            RenderSettings.ambientLight = softAmb;
            RenderSettings.ambientIntensity = AmbientIntensity;
            RenderSettings.fog = false;

            if (DisableBloomWithFullbright)
            {
                try { GS.PostEffects(false); } catch { }
            }

            // Ensure balanced fullbright point light is attached to camera/player
            if (_fullbrightObj == null)
            {
                _fullbrightObj = new GameObject("ArfFullbrightLight");
                UnityEngine.Object.DontDestroyOnLoad(_fullbrightObj);

                _fullbrightLight = _fullbrightObj.AddComponent<Light>();
                _fullbrightLight.type = LightType.Point;
                _fullbrightLight.cullingMask = -1;
                _fullbrightLight.shadows = LightShadows.None;
            }

            _fullbrightObj.SetActive(true);
            if (_fullbrightLight != null)
            {
                _fullbrightLight.enabled = true;
                _fullbrightLight.range = FullbrightRange;
                _fullbrightLight.intensity = FullbrightIntensity;
                _fullbrightLight.color = new Color(0.96f, 0.96f, 0.96f);
            }

            // Follow camera or player position
            Camera cam = Camera.main;
            if (cam != null)
            {
                _fullbrightObj.transform.position = cam.transform.position;
            }
            else if (RM.fpsController != null)
            {
                _fullbrightObj.transform.position = RM.fpsController.transform.position + Vector3.up * 1.5f;
            }
        }

        private void ApplyFlashlight()
        {
            if (RM.flashlight != null)
            {
                if (_infiniteFlashlight)
                {
                    RM.flashlight.MaxLight();
                    RM.flashlight.FreezeCharge();
                }
                else
                {
                    RM.flashlight.UnfreezeCharge();
                }
            }
        }

        public void OnDrawSettings()
        {
            GUILayout.BeginVertical();

            // Fullbright Toggle & Sliders
            bool fb = GUILayout.Toggle(Fullbright, "  Fullbright Limpio (Sin explosión de Bloom)");
            if (fb != Fullbright)
            {
                Fullbright = fb;
                HudNotificationManager.LogModuleToggle("Fullbright Limpio", fb);
            }

            if (Fullbright)
            {
                GUILayout.Space(4f);
                bool disBloom = GUILayout.Toggle(DisableBloomWithFullbright, "    Desactivar Bloom / Post-Processing con Fullbright");
                if (disBloom != DisableBloomWithFullbright)
                {
                    DisableBloomWithFullbright = disBloom;
                    if (!disBloom)
                    {
                        try { GS.PostEffects(true); } catch { }
                    }
                    else
                    {
                        try { GS.PostEffects(false); } catch { }
                    }
                }

                GUILayout.Label($"    Intensidad de Luz: <color=#38BDF8><b>{FullbrightIntensity:F2}</b></color>");
                float newInt = GUILayout.HorizontalSlider(FullbrightIntensity, 0.1f, 1.5f);
                if (Math.Abs(newInt - FullbrightIntensity) > 0.02f) FullbrightIntensity = (float)Math.Round(newInt, 2);

                GUILayout.Label($"    Rango de Iluminación: <color=#38BDF8><b>{FullbrightRange:F0} m</b></color>");
                float newRng = GUILayout.HorizontalSlider(FullbrightRange, 10f, 70f);
                if (Math.Abs(newRng - FullbrightRange) > 0.5f) FullbrightRange = (float)Math.Round(newRng, 0);

                GUILayout.Label($"    Luz Ambiental Base: <color=#38BDF8><b>{AmbientIntensity:F2}</b></color>");
                float newAmb = GUILayout.HorizontalSlider(AmbientIntensity, 0.0f, 1.0f);
                if (Math.Abs(newAmb - AmbientIntensity) > 0.02f) AmbientIntensity = (float)Math.Round(newAmb, 2);
                GUILayout.Space(6f);
            }

            GUILayout.Space(6f);

            // God Mode Toggle
            bool god = GUILayout.Toggle(GodMode, "  Modo Dios / Inmortal (Inmunidad Total contra Mamá)");
            if (god != GodMode)
            {
                GodMode = god;
                HudNotificationManager.LogModuleToggle("Modo Dios (Inmortal)", god);
            }

            // Hacker Mode Toggle (Matrix Macro Cheat)
            bool hk = GUILayout.Toggle(HackerMode, "  Modo Hacker (Modo Dios + Instant Interact + Sprint 6.0)");
            if (hk != HackerMode)
            {
                HackerMode = hk;
                HudNotificationManager.LogModuleToggle("Modo Hacker (There is no spoon)", hk);
            }

            // Infinite Flashlight
            bool infFl = GUILayout.Toggle(InfiniteFlashlight, "  Linterna Infinita (Batería 100% Congelada)");
            if (infFl != InfiniteFlashlight)
            {
                InfiniteFlashlight = infFl;
                HudNotificationManager.LogModuleToggle("Linterna Infinita", infFl);
            }

            // No Hold Time Toggle
            bool nht = GUILayout.Toggle(NoHoldTime, "  Sin Espera de Interacción (Instant Interact)");
            if (nht != NoHoldTime)
            {
                NoHoldTime = nht;
                HudNotificationManager.LogModuleToggle("Instant Interact", nht);
            }

            GUILayout.Space(8f);
            GUILayout.Label("<b>Acciones Rápidas:</b>");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Recargar Linterna", GUILayout.Height(26)))
            {
                if (RM.flashlight != null)
                {
                    RM.flashlight.MaxLight();
                    HudNotificationManager.LogCustom("<color=#FCD34D>Linterna recargada al 100%</color>");
                }
            }

            if (GUILayout.Button("Callar a Tattletail", GUILayout.Height(26)))
            {
                if (RM.tattletail != null)
                {
                    RM.tattletail.ShutUp();
                    HudNotificationManager.LogCustom("<color=#C084FC>Tattletail silenciado (ShutUp)</color>");
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Completar Nivel Actual", GUILayout.Height(26)))
            {
                GameManager.OnLevelComplete();
                HudNotificationManager.LogCustom("<color=#10B981>¡Nivel completado con éxito!</color>");
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
    }
}
