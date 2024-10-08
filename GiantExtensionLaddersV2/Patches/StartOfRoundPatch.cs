﻿using GiantExtensionLaddersV2.Behaviours;
using GiantExtensionLaddersV2.ConfigStuff;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace GiantExtensionLaddersV2.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        [HarmonyPatch(nameof(StartOfRound.ShipLeave))]
        [HarmonyPrefix]
        public static void CollectLadders(StartOfRound __instance)
        {
            if (!GiantExtensionLaddersV2.mySyncedConfigs.isAutoCollectLaddersEnabled)
            {
                return;
            }

            List<GrabbableObject> grabableObjectsInScene = [.. Object.FindObjectsOfType<LadderItemScript>()];
            grabableObjectsInScene.AddRange([.. Object.FindObjectsOfType<ExtensionLadderItem>()]);

            for (int i = 0; i < grabableObjectsInScene.Count; i++)
            {
                LadderItemScript? ladder = grabableObjectsInScene[i] as LadderItemScript;
                ExtensionLadderItem? normalLadder = grabableObjectsInScene[i] as ExtensionLadderItem;

                if (!(ladder == null && normalLadder == null))
                {
                    TeleportLadderItem(ladder, normalLadder, __instance.middleOfShipNode.position);
                }
            }
        }

        private static void TeleportLadderItem(LadderItemScript? ladder, ExtensionLadderItem? normalLadder, Vector3 position)
        {
            GrabbableObject? item = (GrabbableObject?)ladder ?? normalLadder;

            if (item == null)
            {
                return;
            }

            bool isPocketed = ladder?.isPocketed ?? normalLadder?.isPocketed ?? false;
            bool isHeld = ladder?.isHeld ?? normalLadder?.isHeld ?? false;
            bool isHeldByEnemy = ladder?.isHeldByEnemy ?? normalLadder?.isHeldByEnemy ?? false;
            bool isBeingUsed = ladder?.isBeingUsed ?? normalLadder?.isBeingUsed ?? false;

            if (!isPocketed && !isHeld && !isHeldByEnemy && (!isBeingUsed || GiantExtensionLaddersV2.mySyncedConfigs.isCollectExtendedLaddersEnabled))
            {
                item.transform.position = position;
                item.FallToGround();
                StartOfRound.Instance.localPlayerController.SetItemInElevator(true, false, item);
            }
        }
    }
}
