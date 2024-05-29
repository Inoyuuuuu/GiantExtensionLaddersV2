using GameNetcodeStuff;
using GiantExtensionLaddersV2.Behaviours;
using HarmonyLib;
using UnityEngine;

namespace GiantExtensionLaddersV2.Patches
{
    [HarmonyPatch(typeof(InteractTrigger))]
    internal class LadderPlayerSnapPatch
    {
        private const float NEGATIVE_OFFSET_BASE_VALUE = 0.25f;
        private const float TINY_LADDER_OFFSET_BASE_VALUE = -0.215f; //to increase the player to ladder distance
        private const float OFFSET_BASE_VALUE = 0.035f;
        private const float BIGGER_LADDER_SIZE_START = 20f;
        private const int CLOSE_POS_ADDITIONAL_CHECKS = 10;

        private const int LADDER_BOT_RESET_AMOUNT = 2;
        private const int LADDER_TOP_RESET_AMOUNT = 5;

        private const float GROUND_OFFSET = 0.5f;
        private const float TINY_LADDER_GROUND_OFFSET = 0.05f;

        [HarmonyPrefix]
        [HarmonyPatch("ladderClimbAnimation")]
        public static void PatchLadderPlayerSnap(InteractTrigger __instance, ref PlayerControllerB playerController)
        {

            LadderItemScript ladderItemScript = __instance.GetComponentInParent<LadderItemScript>();

            if (ladderItemScript != null)
            {
                if (ladderItemScript.giantLadderType == GiantLadderType.TINY)
                {
                    __instance.bottomOfLadderPosition.position = new Vector3(__instance.bottomOfLadderPosition.position.x, playerController.thisPlayerBody.position.y - 0.2f, __instance.bottomOfLadderPosition.position.z);
                    GiantExtensionLaddersV2.isPlayerOnTinyLadder = true;
                }
                else
                {
                    GiantExtensionLaddersV2.isPlayerOnTinyLadder = false;
                }

                //------------- snapping to correct ladder position
                Vector3 ladderDirectionVector = __instance.topOfLadderPosition.position - __instance.bottomOfLadderPosition.position;
                Vector3 normalLDV = Vector3.Normalize(ladderDirectionVector);
                float ladderDotProduct = Vector3.Dot(Vector3.up, normalLDV);

                float closestPositionToLadder = GetVector3CloseToLadder(__instance.bottomOfLadderPosition.position,
                    __instance.topOfLadderPosition.position, normalLDV, playerController.thisPlayerBody.position);

                Vector3 newPosition = __instance.bottomOfLadderPosition.position + closestPositionToLadder * normalLDV;


                //------------- ladder climbing angle correction correction + player offset on climb start (pain)
                float ladderSize = Vector3.Distance(__instance.topOfLadderPosition.position, __instance.bottomOfLadderPosition.position);

                Vector3 offset = new Vector3(0, OFFSET_BASE_VALUE, 0);
                offset = offset * closestPositionToLadder;

                if (newPosition.y + offset.y >= __instance.topOfLadderPosition.position.y && closestPositionToLadder > LADDER_TOP_RESET_AMOUNT)
                {
                    newPosition = __instance.bottomOfLadderPosition.position + (closestPositionToLadder - LADDER_TOP_RESET_AMOUNT) * normalLDV;
                }
                else if (closestPositionToLadder < LADDER_BOT_RESET_AMOUNT && ladderSize >= BIGGER_LADDER_SIZE_START)
                {
                    offset = offset + __instance.bottomOfLadderPosition.forward * NEGATIVE_OFFSET_BASE_VALUE;
                }

                //if tiny ladder and in ship, make vertical distance to ladder bigger, if not, increase height
                if (ladderItemScript.giantLadderType == GiantLadderType.TINY)
                {
                    if (ladderItemScript.isInShipRoom)
                    {
                        Vector3 tinyLadderOffset = __instance.bottomOfLadderPosition.forward * TINY_LADDER_OFFSET_BASE_VALUE * (1.3f - (ladderDotProduct * 2 - 1));
                        offset = offset + tinyLadderOffset;
                        offset = offset + normalLDV * TINY_LADDER_GROUND_OFFSET;
                    } else
                    {
                        Vector3 tinyLadderOffset = __instance.bottomOfLadderPosition.forward * TINY_LADDER_OFFSET_BASE_VALUE * 0.75f;
                        offset = offset + tinyLadderOffset;
                        offset = offset + normalLDV * TINY_LADDER_GROUND_OFFSET;
                    }
                    
                } else
                {
                    offset = offset + normalLDV * GROUND_OFFSET;
                }

                newPosition = newPosition + offset;

                __instance.ladderPlayerPositionNode.position = newPosition;
                playerController.thisPlayerBody.position = newPosition;

                //GiantExtensionLaddersV2.mls.LogDebug("new player snapping pos node: " + __instance.playerPositionNode.position.ToString());
            }
            else
            {
                //GiantExtensionLaddersV2.mls.LogDebug("target ladder is not a ladder from this mod");
                GiantExtensionLaddersV2.isPlayerOnTinyLadder = false;
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

                //GiantExtensionLaddersV2.mls.LogDebug(
                //    "currentMultiplier: " + i 
                //    + " mulitplierWLD: " + mulitplierWithLowestDistance
                //    + " currentDistance: " + currentDistance 
                //    + " lowestDistance: " + lowestDistance);

                if (currentDistance < lowestDistance)
                {
                    lowestDistance = currentDistance;
                    mulitplierWithLowestDistance = i;
                } else
                {
                    //GiantExtensionLaddersV2.mls.LogDebug("found closest distance to ladder: " + mulitplierWithLowestDistance);
                    return mulitplierWithLowestDistance;
                }
            }

            return maxChecks / 2;
        }
    }
}
