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
        private static float defaultClimbSpeed;
        private static bool isDefaultClimbSpeedSet = false;
        private const float CLIMB_SPEED_MULTIPLIER = 0.22f;


        [HarmonyPatch("Update")]
        [HarmonyPriority(Priority.Low)]
        [HarmonyPostfix]
        static void crankThatClimbingSpeed(PlayerControllerB __instance)
        {
            if (!isDefaultClimbSpeedSet)
            {   
                defaultClimbSpeed = __instance.climbSpeed;
                isDefaultClimbSpeedSet = true;
            }

            if (LadderPlayerSnapPatch.isPlayerOnTinyLadder)
            {
                if (__instance.isPlayerControlled && __instance.isClimbingLadder)
                {
                    __instance.climbSpeed = defaultClimbSpeed * CLIMB_SPEED_MULTIPLIER;
                }
                else
                {
                    __instance.climbSpeed = defaultClimbSpeed;
                }
            }
        }
    }
}
