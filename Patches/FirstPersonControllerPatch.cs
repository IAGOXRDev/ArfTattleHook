using System;
using HarmonyLib;
using UnityEngine;
using ArfTattleHook.Modules;
using ArfTattleHook.UI;

namespace ArfTattleHook.Patches
{
    [HarmonyPatch(typeof(FirstPersonController), "FixedUpdate")]
    public static class FpcFixedUpdatePatch
    {
        [HarmonyPrefix]
        public static void Prefix(FirstPersonController __instance)
        {
            if (__instance == null) return;

            // Block movement input while menu is open
            if (MenuManager.IsMenuOpen)
            {
                BhopModule.Instance?.ClearHorizontalInput(__instance);
                return;
            }

            // In air during bhop, nullify horizontal input to give pure camera-oriented steering
            var bhop = BhopModule.Instance;
            if (bhop != null && bhop.IsBhopActive)
            {
                CharacterController cc = __instance.GetComponent<CharacterController>();
                if (cc != null && !cc.isGrounded)
                {
                    bhop.ClearHorizontalInput(__instance);
                }
            }
        }

        [HarmonyPostfix]
        public static void Postfix(FirstPersonController __instance)
        {
            if (MenuManager.IsMenuOpen || __instance == null) return;

            var bhop = BhopModule.Instance;
            if (bhop != null && bhop.IsBhopActive)
            {
                bhop.ApplyBhopVelocity(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(FirstPersonController), "RotateView")]
    public static class FpcRotateViewPatch
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            // Freeze view rotation when mod menu is open
            if (MenuManager.IsMenuOpen)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MouseLook), "UpdateCursorLock")]
    public static class MouseLookUpdateCursorLockPatch
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            // Keep cursor unlocked and visible when mod menu is open
            if (MenuManager.IsMenuOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                return false;
            }
            return true;
        }
    }
}
