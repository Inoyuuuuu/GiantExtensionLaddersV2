using GiantExtensionLaddersV2.ConfigStuff;
using HarmonyLib;
using LethalLib.Modules;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static LethalLib.Modules.Items;

namespace GiantExtensionLaddersV2.Patches
{
    [HarmonyPatch()]
    internal class TerminalSetSalesPatch
    {

        private static bool isItemListSorted = true;

        [HarmonyPatch(typeof(Terminal), nameof(Terminal.SetItemSales))]
        [HarmonyPriority(Priority.Low)]
        [HarmonyPostfix]
        static void patchRemovedItemSales(Terminal __instance)
        {
            if (MySyncedConfigs.Instance.isSalesFixTerminalActive)
            {

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

                if (__instance.itemSalesPercentages == null || __instance.itemSalesPercentages.Length == 0)
                {
                    __instance.itemSalesPercentages = new int[__instance.buyableItemsList.Length];
                    for (int i = 0; i < __instance.itemSalesPercentages.Length; i++)
                    {
                        __instance.itemSalesPercentages[i] = 100;
                    }
                }

                for (int i = 0; i < removedItemsIndexes.Count; i++)
                {
                    if (removedItemsIndexes[i] != __instance.buyableItemsList.Length - removedItemsIndexes.Count - 1)
                    {
                        isItemListSorted = false;
                    }
                }

                if (!isItemListSorted)
                {

                    for (int i = 0; i < removedItemsIndexes.Count; i++)
                    {
                        Item removedItem = __instance.buyableItemsList[removedItemsIndexes[i]];
                        Item itemLastOnList = __instance.buyableItemsList[__instance.buyableItemsList.Length - i - 1];

                        int removedItemPrice = removedItem.creditsWorth;
                        int itemLastOnListPrice = itemLastOnList.creditsWorth;

                        UpdateBuyItemIndex(removedItem, __instance.buyableItemsList.Length - i - 1);
                        UpdateBuyItemIndex(itemLastOnList, removedItemsIndexes[i]);

                        __instance.buyableItemsList[removedItemsIndexes[i]] = itemLastOnList;
                        __instance.buyableItemsList[removedItemsIndexes[i]].creditsWorth = itemLastOnListPrice;

                        __instance.buyableItemsList[__instance.buyableItemsList.Length - i - 1] = removedItem;
                        __instance.buyableItemsList[__instance.buyableItemsList.Length - i - 1].creditsWorth = removedItemPrice;

                        }
                }

                System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed + 90);

                int randomNumber = random.Next(-5, 5);
                int numberOfItemsInSale = Mathf.Clamp(randomNumber, 0, 5);
                //numberOfItemsInSale = 17;

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
                    //indexOfItemList = IndexPositionAfterRemovedItems(removedItemsIndexes, indexOfItemList);

                    if (!removedItemsIndexes.Contains(indexOfItemList) && indexOfItemList < __instance.buyableItemsList.Length - 1)
                    {
                        int maxValue = Mathf.Clamp(__instance.buyableItemsList[indexOfItemList].highestSalePercentage, 0, 90);
                        int salePercentage = 100 - random.Next(0, maxValue);
                        salePercentage = __instance.RoundToNearestTen(salePercentage);
                        __instance.itemSalesPercentages[indexOfItemList] = salePercentage;
                    }
                }
            }
        }

        private static int IndexPositionAfterRemovedItems(List<int> removedItemsIndexes, int currentIndex)
        {
            for (int i = 0; i < removedItemsIndexes.Count; i++)
            {
                if (currentIndex >= removedItemsIndexes[i])
                {
                    currentIndex++;
                }
            }

            return currentIndex;
        }

        private static void UpdateBuyItemIndex(Item item, int newIndex)
        {
            if (!(StartOfRound.Instance != null))
            {
                GiantExtensionLaddersV2.mls.LogInfo("early return: SoR == null");

                return;
            }

            TerminalKeyword terminalKeyword = terminal.terminalNodes.allKeywords.First((TerminalKeyword keyword) => keyword.word == "buy");
            TerminalNode itemTerminalNode = terminalKeyword.compatibleNouns[0].result.terminalOptions[1].result;
            List<CompatibleNoun> source = terminalKeyword.compatibleNouns.ToList();

            if (!buyableItemAssetInfos.Any((BuyableItemAssetInfo x) => x.itemAsset == item))
            {
                GiantExtensionLaddersV2.mls.LogInfo("early return: buyableItemAssetInfos not found?");

                return;
            }

            BuyableItemAssetInfo asset = buyableItemAssetInfos.First((BuyableItemAssetInfo x) => x.itemAsset == item);

            if (!source.Any((CompatibleNoun noun) => noun.noun == asset.keyword))
            {
                GiantExtensionLaddersV2.mls.LogInfo("early return: compatible noun not found?");

                return;
            }


            TerminalNode result = source.First((CompatibleNoun noun) => noun.noun == asset.keyword).result;

            GiantExtensionLaddersV2.mls.LogInfo("SUCCESS old buyItemIndex: " + result.buyItemIndex + " new buyItemIndex: " + newIndex);

            result.buyItemIndex = newIndex;

            if (result.terminalOptions.Length == 0)
            {
                GiantExtensionLaddersV2.mls.LogInfo("return: terminal options == 0");
                return;
            }

            CompatibleNoun[] terminalOptions = result.terminalOptions;
            foreach (CompatibleNoun compatibleNoun in terminalOptions)
            {
                if (compatibleNoun.result != null && compatibleNoun.result.buyItemIndex != -1)
                {
                    compatibleNoun.result.buyItemIndex = newIndex;
                }
            }
        }
    }
}
