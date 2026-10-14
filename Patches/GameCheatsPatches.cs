using System;
using HarmonyLib;
using UnityEngine;
using ArfTattleHook.Modules;

namespace ArfTattleHook.Patches
{
    [HarmonyPatch(typeof(MamaManager), "RequestJumpscareDeath")]
    public static class MamaManagerJumpscareDeathPatch
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            var stuff = StuffModule.Instance;
            if (stuff != null && (stuff.GodMode || stuff.HackerMode))
            {
                // Real God Mode: Block death entirely
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(JumpscareManager), "DoJumpscare")]
    public static class JumpscareManagerDoJumpscarePatch
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            var stuff = StuffModule.Instance;
            if (stuff != null && (stuff.GodMode || stuff.HackerMode))
            {
                // Real God Mode: Block jumpscare sequence
                return false;
            }
            return true;
        }
    }
}
