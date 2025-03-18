using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GameNetcodeStuff;
using HarmonyLib;
using LethalPhysics;
using UnityEngine;
using Unity.Netcode;
using BepInEx.Logging;
using JetBrains.Annotations;

namespace LethalPhysics.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        // Constants
        static float gravityConstant = -0.15f;
        static float jumpThreshhold = 0.07f;
        static float velocityThreshhold = 0.1f;
        static float maxExternalForces = 5f;
        static float flyAwayBounds = 200f;
        static float maxJumpFallValue = 4f;
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
            // If in space, do the modding.
            bool inSpace = StartOfRound.Instance?.inShipPhase ?? false;
            if (inSpace || !LethalPhysicsMod.configGravityOnMoons.Value || __instance.isInsideFactory)
            {
                // Access private variable isFallingFromJump with reflection
                FieldInfo isFallingFromJumpField = AccessTools.Field(typeof(PlayerControllerB), "isFallingFromJump");
                bool isFallingFromJump = false;

                if (isFallingFromJumpField != null)
                {
                    isFallingFromJump = (bool)isFallingFromJumpField.GetValue(__instance);
                }

                // If jumping, keep flying off into space.
                if (isFallingFromJump)
                {
                    if (__instance.jetpackControls || ((__instance.thisController.velocity.y >= -jumpThreshhold) && (__instance.thisController.velocity.y <= jumpThreshhold)))
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
        }
    }
}
