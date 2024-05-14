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

            if (ladderItemScript != null && component != null && component.isInHangarShipRoom)
            {
                if (ladderItemScript.giantLadderType == GiantLadderType.TINY)
                {
                    changedPlayerLocation = true;
                    component.isInHangarShipRoom = false;
                }
            }
        }

        [HarmonyPatch(nameof(InteractTrigger.Interact))]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPrefix]
        public static void ResetPlayerIsInShipPatch(InteractTrigger __instance, ref Transform playerTransform)
        {
            PlayerControllerB component = playerTransform.GetComponent<PlayerControllerB>();

            if (component != null && changedPlayerLocation)
            {
                    component.isInHangarShipRoom = true;
                    changedPlayerLocation = false;
            }
        }
    }
}
