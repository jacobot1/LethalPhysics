using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameNetcodeStuff;
using HarmonyLib;


namespace LethalPhysics.Patches
{

    [HarmonyPatch(typeof(GrabbableObject))]
    internal class GrabbableObjectPatch
    {
        [HarmonyPatch("FallWithCurve")]
        [HarmonyPrefix]
        static bool ItemZeroGravityPatch(PlayerControllerB __instance)
        {
            // If in space, remove gravity for objects.
            bool inSpace = StartOfRound.Instance?.inShipPhase ?? false;
            if (inSpace || !LethalPhysicsMod.configGravityOnMoons.Value)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
