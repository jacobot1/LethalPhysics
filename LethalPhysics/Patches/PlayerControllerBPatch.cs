using System.Reflection;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;


namespace LethalPhysics.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        // Constants
        static float gravityConstant = -0.15f;
        static float jumpVelocityThreshhold = 0.05f;
        static float velocityThreshhold = 0.1f;
        static float maxExternalForces = 4.7f;
        static float flyAwayBounds = 200f;
        static float maxJumpFallValue = 4.2f;
        //static float forceDivider = 2f;
        static Vector3 homePos = new Vector3(3.866463f, 0f, -14.225f);

        // Declare saved forces outside of function
        static Vector3 savedExternalForces;

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        static bool NewtonsFirstLaw(PlayerControllerB __instance)
        {
            //__instance.externalForceAutoFade = Vector3.zero;

            // Save externalForces
            savedExternalForces = __instance.externalForces;

            // Keep forces from adding exponentially
            if (__instance.externalForces.x >= maxExternalForces)
            {
                savedExternalForces.x = maxExternalForces;
            }
            if (__instance.externalForces.x <= -maxExternalForces)
            {
                savedExternalForces.x = -maxExternalForces;
            }
            if (__instance.externalForces.y >= maxExternalForces)
            {
                savedExternalForces.y = maxExternalForces;
            }
            if (__instance.externalForces.y <= -maxExternalForces)
            {
                savedExternalForces.y = -maxExternalForces;
            }
            if (__instance.externalForces.z >= maxExternalForces)
            {
                savedExternalForces.z = maxExternalForces;
            }
            if (__instance.externalForces.z <= -maxExternalForces)
            {
                savedExternalForces.z = -maxExternalForces;
            }

            return true;
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void PlayerZeroGravityPatch(PlayerControllerB __instance)
        {
            // Determine whether ship is in space
            bool inSpace = StartOfRound.Instance?.inShipPhase ?? false;

            // Access private variable isFallingFromJump with reflection
            FieldInfo isFallingFromJumpField = AccessTools.Field(typeof(PlayerControllerB), "isFallingFromJump");
            bool isFallingFromJump = false;
            if (isFallingFromJumpField != null)
            {
                isFallingFromJump = (bool)isFallingFromJumpField.GetValue(__instance);
            }

            // Access private variable isJumping with reflection
            FieldInfo isJumpingField = AccessTools.Field(typeof(PlayerControllerB), "isJumping");
            bool isJumping = false;
            if (isJumpingField != null)
            {
                isJumping = (bool)isJumpingField.GetValue(__instance);
            }

            if (inSpace || !LethalPhysicsMod.configGravityOnMoons.Value)
            {
                // If jumping, keep flying off into space
                if (isFallingFromJump)
                {
                    if (__instance.jetpackControls || ((__instance.thisController.velocity.y >= -jumpVelocityThreshhold) && (__instance.thisController.velocity.y <= jumpVelocityThreshhold)))
                    {
                        isFallingFromJumpField.SetValue(__instance, false);
                    }
                    // Keep flying off into space
                    __instance.fallValue = maxJumpFallValue;

                    //LethalPhysicsMod.mls.LogInfo("pos:  " + __instance.thisController.transform.position.x + " " + __instance.thisController.transform.position.y + " " + __instance.thisController.transform.position.z);
                }
                else
                {
                    // Just float around
                    __instance.fallValue = gravityConstant;
                }

                // Fall damage still accumulates even when fallValue is static, so I disabled it.
                __instance.takingFallDamage = false;

                /* Log velocity
                if (Time.deltaTime > 0.01f)
                {
                    LethalPhysicsMod.mls.LogInfo(string.Format("velocity magnitude: {0}", __instance.thisController.velocity.magnitude));
                }
                */

                // Reset externalForces
                if ((__instance.thisController.velocity.magnitude <= velocityThreshhold) || __instance.thisController.isGrounded)
                {
                    __instance.externalForces = Vector3.zero;
                }
                else
                {
                    __instance.externalForces = savedExternalForces; // / f
                }

                // If out of considerable bounds, tp if in space or kill if on moon
                if (__instance.thisController.transform.position.y > flyAwayBounds)
                {
                    if (inSpace)
                    {
                        __instance.TeleportPlayer(homePos);
                    }
                    else
                    {
                        GameNetworkManager.Instance.localPlayerController.KillPlayer(Vector3.zero);
                    }
                }
            }
            else if (LethalPhysicsMod.configMoonGravityLevel.Value != 5f)
            {
                if (isJumping)
                {
                    __instance.fallValue = 25f / LethalPhysicsMod.configMoonGravityLevel.Value;
                }
                if (isFallingFromJump)
                {
                    __instance.fallValue -= Time.deltaTime;
                }
            }
        }
    }
}
