using GameNetcodeStuff;
using GiantExtensionLaddersV2.Behaviours;
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
        private const float NEGATIVE_OFFSET_THRESHOLD = 2f;
        private const float NEGATIVE_OFFSET_BASE_VALUE = 0.25f;
        private const float OFFSET_BASE_VALUE = 0.035f;
        private const float BIGGER_LADDER_SIZE_START = 20f;
        private const int CLOSE_POS_ADDITIONAL_CHECKS = 10;
        private const int LADDER_BOT_RESET_AMOUNT = 2;
        private const int LADDER_TOP_RESET_AMOUNT = 5;

        [HarmonyPrefix]
        [HarmonyPatch("ladderClimbAnimation")]
        public static void PatchLadderPlayerSnap(InteractTrigger __instance, ref PlayerControllerB playerController)
        {
            LadderItemScript ladderItemScript = __instance.GetComponentInParent<LadderItemScript>();
            if (ladderItemScript != null)
            {
                if (ladderItemScript.giantLadderType == GiantLadderType.TINY)
                {
                    GiantExtensionLaddersV2.mls.LogInfo("changing tiny ladder");
                    __instance.topOfLadderPosition.position = new Vector3(__instance.topOfLadderPosition.position.x, __instance.topOfLadderPosition.position.y + 1.5f, __instance.topOfLadderPosition.position.z);
                    __instance.bottomOfLadderPosition.position = new Vector3(__instance.bottomOfLadderPosition.position.x, playerController.thisPlayerBody.position.y - 0.05f, __instance.bottomOfLadderPosition.position.z);
                }
                else
                {
                    Vector3 ladderDirectionVector = __instance.topOfLadderPosition.position - __instance.bottomOfLadderPosition.position;
                    Vector3 normalLDV = Vector3.Normalize(ladderDirectionVector);

                    float closestPositionToLadder = GetVector3CloseToLadder(__instance.bottomOfLadderPosition.position,
                        __instance.topOfLadderPosition.position, normalLDV, playerController.thisPlayerBody.position);

                    Vector3 newPosition = __instance.bottomOfLadderPosition.position + closestPositionToLadder * normalLDV;

                    //------------- ladder correction correction lmao
                    float ladderSize = Vector3.Distance(__instance.topOfLadderPosition.position, __instance.bottomOfLadderPosition.position);

                    Vector3 offset = new Vector3(0, OFFSET_BASE_VALUE, 0);
                    offset = offset * closestPositionToLadder;
                    GiantExtensionLaddersV2.mls.LogDebug("player pos offset: " + offset.ToString());

                    if (newPosition.y + offset.y >= __instance.topOfLadderPosition.position.y && closestPositionToLadder > LADDER_TOP_RESET_AMOUNT)
                    {
                        newPosition = __instance.bottomOfLadderPosition.position + (closestPositionToLadder - LADDER_TOP_RESET_AMOUNT) * normalLDV;
                    }
                    else if (closestPositionToLadder < LADDER_BOT_RESET_AMOUNT && ladderSize >= BIGGER_LADDER_SIZE_START)
                    {
                        offset = offset + playerController.thisPlayerBody.forward * NEGATIVE_OFFSET_BASE_VALUE;
                    }

                    newPosition = newPosition + offset;
                    __instance.ladderPlayerPositionNode.position = newPosition;

                    GiantExtensionLaddersV2.mls.LogDebug("new player pos node: " + __instance.playerPositionNode.position.ToString());

                }
            }
            else
            {
                GiantExtensionLaddersV2.mls.LogInfo("was not a ladder from this mod");

            }
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
