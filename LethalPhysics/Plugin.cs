using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
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
        private const string modVersion = "0.2.0";

        // Configuration
        public static ConfigEntry<bool> configGravityOnMoons;

        // Initalize Harmony
        private readonly Harmony harmony = new Harmony(modGUID);

        // Create static instance
        private static LethalPhysicsMod Instance;

        // Initialize logging
        internal ManualLogSource mls;

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
                                                "Whether or not to enable gravity on moons");

            // Do the patching
            harmony.PatchAll(typeof(LethalPhysicsMod));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(GrabbableObjectPatch));
        }
    }
}
