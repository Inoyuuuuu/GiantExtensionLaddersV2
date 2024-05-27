using GameNetcodeStuff;
using GiantExtensionLaddersV2.Behaviours;
using HarmonyLib;
using UnityEngine;

namespace GiantExtensionLaddersV2.Patches
{
    [HarmonyPatch(typeof(InteractTrigger))]
    internal class UseLadderInShipPatch
    {

        internal static bool changedPlayerLocation = false;
        internal static PlayerControllerB playerWithChangedLocation = null;
        private const float minPlayerSizeForTinyLadder = 0.25f;

        [HarmonyPatch(nameof(InteractTrigger.Interact))]
        [HarmonyPrefix]
        public static void PlayerIsInShipPatch(InteractTrigger __instance, ref Transform playerTransform)
        {
            LadderItemScript ladderItemScript = __instance.GetComponentInParent<LadderItemScript>();
            PlayerControllerB component = playerTransform.GetComponent<PlayerControllerB>();
            
            //tiny ladder is climbable outside ship if player is small
            if (ladderItemScript != null && component != null && !component.isInHangarShipRoom)
            {
                if (ladderItemScript.giantLadderType == GiantLadderType.TINY && component.thisPlayerBody.localScale.y > minPlayerSizeForTinyLadder)
                {
                    changedPlayerLocation = true;
                    component.isInHangarShipRoom = true;
                    playerWithChangedLocation = component;
                }
            }       //tiny ladder is climbable in ship if player is small
            else if (ladderItemScript != null && component != null && component.isInHangarShipRoom)
            {
                if (ladderItemScript.giantLadderType == GiantLadderType.TINY && component.thisPlayerBody.localScale.y <= minPlayerSizeForTinyLadder)
                {
                    changedPlayerLocation = true;
                    component.isInHangarShipRoom = false;
                    playerWithChangedLocation = component;
                }
            }
        }

        [HarmonyPatch("Update")]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPrefix]
        public static void ResetPlayerIsInShipPatch()
        {

            if (playerWithChangedLocation != null && changedPlayerLocation)
            {
                playerWithChangedLocation.isInHangarShipRoom = !playerWithChangedLocation.isInHangarShipRoom;
                changedPlayerLocation = false;
                playerWithChangedLocation = null;
            }
        }
    }
}
