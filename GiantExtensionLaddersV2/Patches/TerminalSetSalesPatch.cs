using HarmonyLib;
using LethalLib.Modules;
using System.Collections.Generic;
using UnityEngine;
using static LethalLib.Modules.Items;

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

            //for (int i = 0; i < removedItemsIndexes.Count; i++)
            //{
            //    Item removedItem = __instance.buyableItemsList[removedItemsIndexes[i]];
            //    __instance.buyableItemsList[removedItemsIndexes[i]] = __instance.buyableItemsList[__instance.buyableItemsList.Length - i - 1];
            //    __instance.buyableItemsList[__instance.buyableItemsList.Length - i - 1] = removedItem;
            //}

            System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed + 90);

            int randomNumber = random.Next(-5, 5);
            int numberOfItemsInSale = Mathf.Clamp(randomNumber, 0, 5);
            numberOfItemsInSale = 14;

            if (numberOfItemsInSale <= 0)
            {
                return;
            }

            List<int> list = new List<int>();
            for (int i = 0; i < __instance.buyableItemsList.Length; i++)
            {
                list.Add(i);
                __instance.itemSalesPercentages[i] = 100;
            }

            for (int j = 0; j < numberOfItemsInSale; j++)
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

            for (int i = 0; i < __instance.buyableItemsList.Length; i++)
            {
                ShopItem currentShopItem = findShopItem(__instance.buyableItemsList[i]);

                GiantExtensionLaddersV2.mls.LogInfo("index: " + i + 
                    " item: " + __instance.buyableItemsList[i].itemName + 
                    " item price: " + __instance.buyableItemsList[i].creditsWorth + 
                    " sale percentage: " + __instance.itemSalesPercentages[i]);

                if (currentShopItem != null)
                {
                    GiantExtensionLaddersV2.mls.LogInfo("shopItem: " + currentShopItem.item.itemName);
                }
            }
        }

        private static ShopItem findShopItem(Item item)
        {

            foreach (var shopItem in shopItems)
            {
                if (item.itemName == shopItem.item.itemName)
                {
                    return shopItem;
                }
            }
            return null;
        }
    }
}
