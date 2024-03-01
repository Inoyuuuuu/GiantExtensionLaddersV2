using BepInEx.Logging;
using GameNetcodeStuff;
using GiantExtensionLaddersV2.ConfigStuff;
using HarmonyLib;

namespace GiantExtensionLaddersV2.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    public class testSprintPatch
    {
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void SwitchTVPrefix(PlayerControllerB __instance)
        {
            if (__instance.isPlayerControlled && __instance.isSprinting)
            {
                GiantExtensionLaddersV2.mls.LogInfo(MySyncedConfigs.Instance.TINY_LADDER_PRICE.Value);
                GiantExtensionLaddersV2.mls.LogInfo(MySyncedConfigs.Instance.BIG_LADDER_PRICE.Value);
                GiantExtensionLaddersV2.mls.LogInfo(MySyncedConfigs.Instance.HUGE_LADDER_PRICE.Value);
            }
        }
    }
}
