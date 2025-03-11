using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameNetcodeStuff;
using HarmonyLib;

namespace LethalPhysics.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void PlayerZeroGravityPatch(PlayerControllerB __instance)
        {
            // If in space, do the modding.
            bool inSpace = StartOfRound.Instance?.inShipPhase ?? false;
            if (inSpace)
            {
                __instance.fallValue = -0.1f;

                // Fall damage still accumulates even when fallValue is static, so I disabled it.
                __instance.takingFallDamage = false;
            }
        }
    }
}
