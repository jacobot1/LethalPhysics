using HarmonyLib;

namespace LethalPhysics.Patches
{

    [HarmonyPatch(typeof(GrabbableObject))]
    internal class GrabbableObjectPatch
    {
        [HarmonyPatch("FallWithCurve")]
        [HarmonyPrefix]
        static bool ItemZeroGravityPatch()
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
