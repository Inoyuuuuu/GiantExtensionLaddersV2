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
        private const float CLIMB_SPEED_MULTIPLIER = 0.1f;


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
                __instance.climbSpeed = normalClimbSpeedValue * CLIMB_SPEED_MULTIPLIER;
            }
            else
            {
                __instance.climbSpeed = normalClimbSpeedValue;
            }
        }
    }
}
