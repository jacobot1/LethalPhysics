﻿using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using LethalPhysics.Patches;

namespace LethalPhysics
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class LethalPhysicsMod : BaseUnityPlugin
    {
        // Mod metadata
        private const string modGUID = "jacobot5.LethalPhysics";
        private const string modName = "LethalPhysics";
        private const string modVersion = "1.2.0";

        // Configuration
        public static ConfigEntry<bool> configGravityOnMoons;
        public static ConfigEntry<float> configMoonGravityLevel;
        public static ConfigEntry<float> configShotgunKnockback;

        // Initalize Harmony
        private readonly Harmony harmony = new Harmony(modGUID);

        // Create static instance
        private static LethalPhysicsMod Instance;

        // Initialize logging
        public static ManualLogSource mls;

        private void Awake()
        {
            // Ensure static instance
            if (Instance == null)
            {
                Instance = this;
            }

            // Send alive message
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            mls.LogInfo("LethalPhysics has awoken.");

            // Bind configuration
            configGravityOnMoons = Config.Bind("General.Toggles",
                                                "GravityOnMoons",
                                                true,
                                                "Whether or not to enable gravity on moons.");

            configMoonGravityLevel = Config.Bind("General.Toggles",
                                                "MoonGravityLevel",
                                                5f,
                                                "Moon gravity constant. 5 is default. Only applies when GravityOnMoons = true. Not recommended to set to 0; use GravityOnMoons = false instead.");

            configShotgunKnockback = Config.Bind("General.Toggles",
                                                "ShotgunKnockback",
                                                8f,
                                                "How much knockback is applied when player shoots the Shotgun in zero gravity. Default 8.");

            // Do the patching
            harmony.PatchAll(typeof(LethalPhysicsMod));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(GrabbableObjectPatch));
            harmony.PatchAll(typeof(ShotgunItemPatch));
        }
    }
}
