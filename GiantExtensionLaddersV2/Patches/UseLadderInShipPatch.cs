using GameNetcodeStuff;
using GiantExtensionLaddersV2.Behaviours;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GiantExtensionLaddersV2.Patches
{
    [HarmonyPatch(typeof(InteractTrigger))]
    internal class UseLadderInShipPatch
    {

        private static bool changedPlayerLocation = false;

        [HarmonyPatch(nameof(InteractTrigger.Interact))]
        [HarmonyPrefix]
        public static void PlayerIsInShipPatch(InteractTrigger __instance, ref Transform playerTransform)
        {
            LadderItemScript ladderItemScript = __instance.GetComponentInParent<LadderItemScript>();
            PlayerControllerB component = playerTransform.GetComponent<PlayerControllerB>();

            if (ladderItemScript != null && component != null && ladderItemScript.giantLadderType == GiantLadderType.HUGE)
            {
                if (component.isInHangarShipRoom)
                {
                    component.isInHangarShipRoom = false;
                    changedPlayerLocation = true;
                }
            }
        }

        [HarmonyPatch(nameof(InteractTrigger.Interact))]
        [HarmonyPostfix]
        public static void ResetPlayerIsInShipPatch(InteractTrigger __instance, ref Transform playerTransform)
        {
            PlayerControllerB component = playerTransform.GetComponent<PlayerControllerB>();

            if (component != null && changedPlayerLocation)
            {
                if (component.isInHangarShipRoom)
                {
                    component.isInHangarShipRoom = true;
                    changedPlayerLocation = false;
                }
            }
        }
    }
}
