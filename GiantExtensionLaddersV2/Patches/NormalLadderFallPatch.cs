using GameNetcodeStuff;
using GiantExtensionLaddersV2.Behaviours;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GiantExtensionLaddersV2.Patches
{
    [HarmonyPatch(typeof(ExtensionLadderItem))]
    internal class NormalLadderFallPatch
    {
        private static bool isLadderOnAnotherLadder = false;

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        static void LadderFallPatch(ExtensionLadderItem __instance)
        {
            if (__instance.playerHeldBy == null && !__instance.isHeld && !__instance.isHeldByEnemy && __instance.reachedFloorTarget && __instance.ladderActivated)
            {

                if (Physics.Raycast(__instance.transform.position, Vector3.down, out var hitInfo, 80f, 268437760, QueryTriggerInteraction.Ignore))
                {
                    if (hitInfo.collider.GetComponentInParent<LadderItemScript>() != null)
                    {
                        isLadderOnAnotherLadder = true;
                    }
                    else
                    {
                        if (isLadderOnAnotherLadder)
                        {
                            __instance.FallToGround();
                        }
                        isLadderOnAnotherLadder = false;
                    }
                }
            }
        }
    }
}
