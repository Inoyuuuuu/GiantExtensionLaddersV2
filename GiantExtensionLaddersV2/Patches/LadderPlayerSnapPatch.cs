using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace GiantExtensionLaddersV2.Patches
{
    [HarmonyPatch(typeof(InteractTrigger))]
    internal class LadderPlayerSnapPatch
    {
        private static float USE_OFFSET_THRESHOLD = 1.5f;
        private static float OFFSET_BASE_VALUE = 0.03f;
        private static int CLOSE_POS_ADDITIONAL_CHECKS = 10;

        [HarmonyPrefix]
        [HarmonyPatch("ladderClimbAnimation")]
        public static void PatchLadderPlayerSnap(InteractTrigger __instance, ref PlayerControllerB playerController)
        {
            Vector3 ladderDirectionVector = __instance.topOfLadderPosition.position - __instance.bottomOfLadderPosition.position;
            Vector3 normalLDV = Vector3.Normalize(ladderDirectionVector);

            float closestPositionToLadder = GetVector3CloseToLadder(__instance.bottomOfLadderPosition.position, 
                __instance.topOfLadderPosition.position, normalLDV, playerController.thisPlayerBody.position);

            Vector3 newPosition = __instance.bottomOfLadderPosition.position + closestPositionToLadder * normalLDV;

            Vector3 offset = new Vector3(0, OFFSET_BASE_VALUE, 0);
            offset = offset * closestPositionToLadder;
            GiantExtensionLaddersV2.mls.LogDebug("player pos offset: " + offset.ToString());

            if (newPosition.y + offset.y >= __instance.topOfLadderPosition.position.y)
            {
                offset = offset * 0.8f;
            }

            newPosition = newPosition + offset;
            __instance.ladderPlayerPositionNode.position = newPosition;

            GiantExtensionLaddersV2.mls.LogDebug("new player pos node: " + __instance.playerPositionNode.position.ToString());
        }

        private static float GetVector3CloseToLadder(Vector3 bottomPos, Vector3 topPos, Vector3 normalizedLDV, Vector3 playerVector)
        {
            int mulitplierWithLowestDistance = 0;
            float currentDistance;
            float lowestDistance = float.MaxValue;
            Vector3 newPosition;
            float maxChecks = Vector3.Distance(bottomPos, topPos) + CLOSE_POS_ADDITIONAL_CHECKS;

            for (int i = 0; i < maxChecks; i++)
            {
                newPosition = bottomPos + i * normalizedLDV;
                currentDistance = Vector3.Distance(playerVector, newPosition);

                GiantExtensionLaddersV2.mls.LogDebug(
                    "currentMultiplier: " + i 
                    + " mulitplierWLD: " + mulitplierWithLowestDistance
                    + " currentDistance: " + currentDistance 
                    + " lowestDistance: " + lowestDistance);

                if (currentDistance < lowestDistance)
                {
                    lowestDistance = currentDistance;
                    mulitplierWithLowestDistance = i;
                } else
                {
                    GiantExtensionLaddersV2.mls.LogDebug("found closest distance to ladder: " + mulitplierWithLowestDistance);
                    return mulitplierWithLowestDistance;
                }
            }

            return maxChecks / 2;
        }
    }
}
