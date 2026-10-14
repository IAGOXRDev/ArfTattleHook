using System;
using MelonLoader;
using UnityEngine;
using ArfTattleHook.Modules;
using ArfTattleHook.UI;

[assembly: MelonInfo(typeof(ArfTattleHook.ArfTattleHookMod), "ArfTattleHook", "1.0.0", "IAGOXR")]
[assembly: MelonGame("Waygetter Electronics", "Tattletail")]
[assembly: MelonColor(System.ConsoleColor.Magenta)]

namespace ArfTattleHook
{
    public class ArfTattleHookMod : MelonMod
    {
        public static ArfTattleHookMod Instance { get; private set; }

        public BhopModule Bhop { get; private set; }
        public FovModule Fov { get; private set; }
        public StuffModule Stuff { get; private set; }
        public DebugModule Debug { get; private set; }
        public MenuManager Menu { get; private set; }

        public override void OnInitializeMelon()
        {
            Instance = this;

            MelonLogger.Msg("=================================================");
            MelonLogger.Msg("     ArfTattleHook v1.0.0 (by IAGOXR)");
            MelonLogger.Msg("     Mod Client para Tattletail");
            MelonLogger.Msg("  * BunnyHop: Auto-jump, air strafe & velocímetro HUD");
            MelonLogger.Msg("  * Field of View (FOV): Slider de 35° a 130° & zoom [C]");
            MelonLogger.Msg("  * Utilidades: Linterna infinita, Fullbright & Hacks");
            MelonLogger.Msg("  * Debug: Consola oficial, selector de noches & cheats");
            MelonLogger.Msg("  * Menú en juego: Presiona [Insert] o [F1]");
            MelonLogger.Msg("=================================================");

            // Initialize Modules
            Bhop = new BhopModule();
            Bhop.OnInitialize();

            Fov = new FovModule();
            Fov.OnInitialize();

            Stuff = new StuffModule();
            Stuff.OnInitialize();

            Debug = new DebugModule();
            Debug.OnInitialize();

            // Initialize Menu and register modules
            Menu = new MenuManager();
            Menu.RegisterModule(Bhop);
            Menu.RegisterModule(Fov);
            Menu.RegisterModule(Stuff);
            Menu.RegisterModule(Debug);

            HudNotificationManager.LogCustom("<color=#C084FC>ArfTattleHook v1.0.0</color> cargado. Pulsa <color=#FCD34D>[Insert]</color> o <color=#FCD34D>[F1]</color>.");
        }

        public override void OnUpdate()
        {
            try
            {
                Menu?.OnUpdate();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[ArfTattleHook] Error in OnUpdate: {ex.Message}");
            }
        }

        public override void OnFixedUpdate()
        {
            try
            {
                for (int i = 0; i < Menu?.Modules.Count; i++)
                {
                    Menu.Modules[i].OnFixedUpdate();
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[ArfTattleHook] Error in OnFixedUpdate: {ex.Message}");
            }
        }

        public override void OnGUI()
        {
            try
            {
                Menu?.OnGUI();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[ArfTattleHook] Error in OnGUI: {ex.Message}");
            }
        }
    }
}
