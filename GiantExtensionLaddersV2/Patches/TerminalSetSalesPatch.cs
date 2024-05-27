using GameNetcodeStuff;
using HarmonyLib;
using LethalLib.Modules;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GiantExtensionLaddersV2.Patches
{
    [HarmonyPatch()]
    internal class TerminalSetSalesPatch
    {
        [HarmonyPatch(typeof(Terminal), nameof(Terminal.SetItemSales))]
        [HarmonyPriority(Priority.Low)]
        [HarmonyPostfix]
        static void patchRemovedItemSales(Terminal __instance)
        {
            if (__instance.itemSalesPercentages == null || __instance.itemSalesPercentages.Length == 0)
            {
                __instance.InitializeItemSalesPercentages();
            }

            List<int> removedItemsIndexes = new List<int>();

            for (int i = 0; i < __instance.buyableItemsList.Length; i++)
            {
                foreach (var shopItem in Items.shopItems)
                {
                    if (__instance.buyableItemsList[i].itemName == shopItem.item.itemName && shopItem.wasRemoved)
                    {
                        removedItemsIndexes.Add(i);
                        break;
                    }
                }
            }

            for (int i = 0; i < removedItemsIndexes.Count; i++)
            {
                Item removedItem = __instance.buyableItemsList[removedItemsIndexes[i]];
                __instance.buyableItemsList[removedItemsIndexes[i]] = __instance.buyableItemsList[__instance.buyableItemsList.Length - i - 1];
                __instance.buyableItemsList[__instance.buyableItemsList.Length - i - 1] = removedItem;
            }

            System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed + 90);
            int num = Mathf.Clamp(random.Next(-10, 5), 0, 5);
            num = 15;

            if (num <= 0)
            {
                return;
            }

            List<int> list = new List<int>();
            for (int i = 0; i < __instance.buyableItemsList.Length; i++)
            {
                list.Add(i);
                __instance.itemSalesPercentages[i] = 100;
            }

            for (int j = 0; j < num; j++)
            {
                if (list.Count <= 0)
                {
                    break;
                }

                int indexOfItemList = random.Next(0, list.Count);
                list.RemoveAt(indexOfItemList);
                //indexOfItemList = indexPositionAfterRemovedItems(removedItems, indexOfItemList);

                if (indexOfItemList < __instance.buyableItemsList.Length - 1)
                {
                    int maxValue = Mathf.Clamp(__instance.buyableItemsList[indexOfItemList].highestSalePercentage, 0, 90);
                    int salePercentage = 100 - random.Next(0, maxValue);
                    salePercentage = __instance.RoundToNearestTen(salePercentage);
                    __instance.itemSalesPercentages[indexOfItemList] = salePercentage;
                }
            }
        }
    }
}
