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
        public const string modGUID = "com.jacobot5.LethalPhysics";
        public const string modName = "LethalPhysics";
        public const string modVersion = "2.1.0";

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
            configGravityOnMoons = Config.Bind("Gravity",
                                                "GravityOnMoons",
                                                true,
                                                "Whether or not to enable gravity on moons.");

            configMoonGravityLevel = Config.Bind("Gravity",
                                                "MoonGravityLevel",
                                                5f,
                                                "Moon gravity constant. 5 is default. Only applies when GravityOnMoons = true. Not recommended to set to 0; use GravityOnMoons = false instead.");

            // Do the patching
            harmony.PatchAll(typeof(LethalPhysicsMod));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(GrabbableObjectPatch));
            harmony.PatchAll(typeof(KickIfModNotInstalled));
        }
    }
}
