using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace GiantExtensionLaddersV2.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class TinyLadderClimbingSpeedPatch
    {
        private static float normalClimbSpeedValue;
        private static bool isNormalClimbSpeedValueSet = false;
        private const float CLIMB_SPEED_MULTIPLIER = 0.4f;


        [HarmonyPatch("Update")]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPostfix]
        static void crankThatClimbingSpeed(PlayerControllerB __instance)
        {
            if (!isNormalClimbSpeedValueSet)
            {
                normalClimbSpeedValue = __instance.climbSpeed;
                isNormalClimbSpeedValueSet = true;
            }


            if (LadderPlayerSnapPatch.isPlayerOnTinyLadder && __instance.isPlayerControlled && __instance.isClimbingLadder)
            {
                GiantExtensionLaddersV2.mls.LogInfo("os " + __instance.climbSpeed);
                __instance.climbSpeed = normalClimbSpeedValue * CLIMB_SPEED_MULTIPLIER;
                GiantExtensionLaddersV2.mls.LogInfo("news " + __instance.climbSpeed);
            }
            else
            {
                __instance.climbSpeed = normalClimbSpeedValue;
                GiantExtensionLaddersV2.mls.LogInfo("not climbing news " + __instance.climbSpeed);
            }
        }
    }
}
