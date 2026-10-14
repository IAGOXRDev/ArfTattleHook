using System;
using MelonLoader;
using UnityEngine;
using ArfTattleHook.UI;

namespace ArfTattleHook.Modules
{
    public class DebugModule : IModModule
    {
        public static DebugModule Instance { get; private set; }

        public string Id => "TattleDebug";
        public string Name => "Herramientas de Desarrollador";
        public string Description => "Consola oficial de Tattletail, selector de noches, visualizador de nodos y funciones de prueba";
        public string Category => "Debug";

        public bool Enabled { get; set; } = true;
        public bool IsExpanded { get; set; } = true;

        // Debug States
        private bool _hideRoof = false;
        private bool _nodeVis = false;
        private bool _screenshotMode = false;
        private bool _spoofBeaten = false;
        private bool _disableJumpscares = false;

        public DebugModule()
        {
            Instance = this;
        }

        public void OnInitialize()
        {
        }

        public void OnUpdate()
        {
            // Keep developer mode active
            GS.developerMode = true;

            // Enforce screenshot mode if toggled
            if (_screenshotMode)
            {
                GS.screenshotMode = true;
            }

            // Enforce roof hidden if requested
            if (_hideRoof)
            {
                GameObject roof = GameObject.Find("Roof_TOP");
                if (roof != null && roof.activeSelf)
                {
                    roof.SetActive(false);
                }
            }
        }

        public void OnFixedUpdate()
        {
        }

        public void OnDrawSettings()
        {
            GUILayout.BeginVertical();

            // 1. Official Game Console
            GUILayout.Label("<color=#A78BFA><b>Consola Oficial del Juego:</b></color>");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Abrir GameConsole Oficial", GUILayout.Height(28)))
            {
                GS.developerMode = true;
                GameConsole.OpenConsole();
                MenuManager.Instance?.ToggleMenu();
                HudNotificationManager.LogCustom("<color=#10B981>Consola abierta.</color> Pulsa <color=#FCD34D>[Tab]</color> para cerrarla.");
            }
            GUILayout.EndHorizontal();
            GUILayout.Label("<color=#9CA3AF><size=10>Atajo nativo: Mantén <b>[~]</b> + <b>[=]</b> y presiona <b>[Tab]</b> para alternar la consola.</size></color>");

            GUILayout.Space(8f);

            // 2. Visualizers & Environment
            GUILayout.Label("<color=#A78BFA><b>Visualización y Entorno:</b></color>");

            // Toggle Roof
            bool roof = GUILayout.Toggle(_hideRoof, "  Ocultar Techo de la Casa (Roof_TOP)");
            if (roof != _hideRoof)
            {
                _hideRoof = roof;
                GS.ToggleRoof(!_hideRoof);
                HudNotificationManager.LogCustom($"Techo de la Casa: {(_hideRoof ? "<color=#EF4444>Oculto</color>" : "<color=#10B981>Visible</color>")}");
            }

            // Node Debug Visibility
            bool nodes = GUILayout.Toggle(_nodeVis, "  Visualizar Nodos de Patrulla de Mamá en 3D");
            if (nodes != _nodeVis)
            {
                _nodeVis = nodes;
                PatrolNode[] allNodes = UnityEngine.Object.FindObjectsOfType<PatrolNode>();
                for (int i = 0; i < allNodes.Length; i++)
                {
                    allNodes[i].SetDebugVis(_nodeVis);
                }
                HudNotificationManager.LogCustom($"Nodos de Patrulla: {(_nodeVis ? "<color=#10B981>Visibles en 3D</color>" : "<color=#EF4444>Ocultos</color>")}");
            }

            // Screenshot Mode
            bool ssm = GUILayout.Toggle(_screenshotMode, "  Modo Fotografía / Captura Limpia (Pulsa [T])");
            if (ssm != _screenshotMode)
            {
                _screenshotMode = ssm;
                GS.ScreenshotMode(ssm);
                HudNotificationManager.LogCustom($"Modo Fotografía: {(ssm ? "<color=#10B981>ON (Pulsa [T] para capturar)</color>" : "<color=#EF4444>OFF</color>")}");
            }

            GUILayout.Space(8f);

            // 3. Mama & Jumpscare Controls
            GUILayout.Label("<color=#A78BFA><b>Controles de Mamá & Jumpscares:</b></color>");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Ejecutar Jumpscare de Prueba", GUILayout.Height(26)))
            {
                if (RM.jumpscare != null)
                {
                    RM.jumpscare.TestJumpscare();
                    HudNotificationManager.LogCustom("<color=#F87171>Jumpscare de prueba ejecutado.</color>");
                }
            }

            if (GUILayout.Button("Forzar Ataque de Mamá (Quick)", GUILayout.Height(26)))
            {
                if (RM.mamaManager != null)
                {
                    RM.mamaManager.QuickMama();
                    HudNotificationManager.LogCustom("<color=#F87171>Quick Mama activado.</color>");
                }
            }
            GUILayout.EndHorizontal();

            bool noJumps = GUILayout.Toggle(_disableJumpscares, "  Desactivar Jumpscares de Mamá (ToggleJumpscares)");
            if (noJumps != _disableJumpscares)
            {
                _disableJumpscares = noJumps;
                if (RM.mamaManager != null)
                {
                    RM.mamaManager.ToggleJumpscares(!_disableJumpscares);
                }
                HudNotificationManager.LogCustom($"Jumpscares de Mamá: {(_disableJumpscares ? "<color=#EF4444>Desactivados</color>" : "<color=#10B981>Activados</color>")}");
            }

            GUILayout.Space(8f);

            // 4. Easter Eggs & Save Cheats
            GUILayout.Label("<color=#A78BFA><b>Huevos Coleccionables & Progreso:</b></color>");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Desbloquear Todos los Huevos (1 al 22)", GUILayout.Height(26)))
            {
                for (int i = 1; i <= 22; i++)
                {
                    GS.GetEgg(i);
                }
                HudNotificationManager.LogCustom("<color=#10B981>¡Los 22 Huevos desbloqueados!</color>");
            }

            if (GUILayout.Button("Borrar Huevos Guardados", GUILayout.Height(26)))
            {
                GD.ClearEggs();
                HudNotificationManager.LogCustom("<color=#EF4444>Huevos guardados eliminados.</color>");
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4f);

            GUILayout.BeginHorizontal();
            bool beaten = GUILayout.Toggle(_spoofBeaten, "  Simular Juego 100% Pasado (Spoof Game Beaten)");
            if (beaten != _spoofBeaten)
            {
                _spoofBeaten = beaten;
                GD.SetSpoofBeatenStatus(beaten);
                HudNotificationManager.LogCustom($"Estado Juego Pasado: {(beaten ? "<color=#10B981>Simulado al 100%</color>" : "<color=#EF4444>Normal</color>")}");
            }

            if (GUILayout.Button("Saltar Comercial TV", GUILayout.Height(24), GUILayout.Width(140)))
            {
                CommercialDirectorPlayable cdp = UnityEngine.Object.FindObjectOfType<CommercialDirectorPlayable>();
                if (cdp != null)
                {
                    cdp.OnCommercialComplete();
                    HudNotificationManager.LogCustom("<color=#10B981>Comercial de TV omitido.</color>");
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(8f);

            // 5. Level Warper (Selector de Noches)
            GUILayout.Label("<color=#A78BFA><b>Selector de Noches (Level Warper):</b></color>");
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Noche 1\n<size=10><color=#9CA3AF>A Night to Remember</color></size>", GUILayout.Height(36)))
            {
                GameManager.PlayLevel(LevelEnum._1_ANightToRemember);
                HudNotificationManager.LogCustom("<color=#38BDF8>Cargando Noche 1...</color>");
            }
            if (GUILayout.Button("Noche 2\n<size=10><color=#9CA3AF>Minor Mischief</color></size>", GUILayout.Height(36)))
            {
                GameManager.PlayLevel(LevelEnum._2_MinorMischief);
                HudNotificationManager.LogCustom("<color=#38BDF8>Cargando Noche 2...</color>");
            }
            if (GUILayout.Button("Noche 3\n<size=10><color=#9CA3AF>Mama's Boy</color></size>", GUILayout.Height(36)))
            {
                GameManager.PlayLevel(LevelEnum._3_MamasBoy);
                HudNotificationManager.LogCustom("<color=#38BDF8>Cargando Noche 3...</color>");
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4f);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Noche 4\n<size=10><color=#9CA3AF>Funny Games</color></size>", GUILayout.Height(36)))
            {
                GameManager.PlayLevel(LevelEnum._4_FunnyGames);
                HudNotificationManager.LogCustom("<color=#38BDF8>Cargando Noche 4...</color>");
            }
            if (GUILayout.Button("Noche 5\n<size=10><color=#9CA3AF>Video Horror System</color></size>", GUILayout.Height(36)))
            {
                GameManager.PlayLevel(LevelEnum._5_VideoHorrorSystem);
                HudNotificationManager.LogCustom("<color=#38BDF8>Cargando Noche 5...</color>");
            }
            if (GUILayout.Button("Noche 6\n<size=10><color=#9CA3AF>The Party</color></size>", GUILayout.Height(36)))
            {
                GameManager.PlayLevel(LevelEnum._6_TheParty);
                HudNotificationManager.LogCustom("<color=#38BDF8>Cargando Noche 6...</color>");
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4f);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Noche 7\n<size=10><color=#9CA3AF>There She Is</color></size>", GUILayout.Height(36)))
            {
                GameManager.PlayLevel(LevelEnum._7_ThereSheIs);
                HudNotificationManager.LogCustom("<color=#38BDF8>Cargando Noche 7...</color>");
            }
            if (GUILayout.Button("Noche 8\n<size=10><color=#9CA3AF>Christmas</color></size>", GUILayout.Height(36)))
            {
                GameManager.PlayLevel(LevelEnum._8_Christmas);
                HudNotificationManager.LogCustom("<color=#38BDF8>Cargando Noche 8 (Navidad)...</color>");
            }
            if (GUILayout.Button("DLC\n<size=10><color=#9CA3AF>Kaleidoscope</color></size>", GUILayout.Height(36)))
            {
                GameManager.PlayLevel(LevelEnum._K_5_Kaleidoscope);
                HudNotificationManager.LogCustom("<color=#38BDF8>Cargando DLC Kaleidoscope...</color>");
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
    }
}
