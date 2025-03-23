using HarmonyLib;

namespace LethalPhysics.Patches
{
    [HarmonyPatch(typeof(ShotgunItem))]
    internal class ShotgunItemPatch
    {
        [HarmonyPatch("ItemActivate")]
        [HarmonyPostfix]
        static void KnockbackPatch(ShotgunItem __instance)
        {
            // If in space or zero gravity, apply knockback
            bool inSpace = StartOfRound.Instance?.inShipPhase ?? false;
            if (inSpace || !LethalPhysicsMod.configGravityOnMoons.Value)
            {
                if ((__instance.playerHeldBy == GameNetworkManager.Instance.localPlayerController) && (!__instance.safetyOn))
                {
                    __instance.playerHeldBy.externalForces += (__instance.transform.forward * -LethalPhysicsMod.configShotgunKnockback.Value);
                }
            }
        }
    }
}
