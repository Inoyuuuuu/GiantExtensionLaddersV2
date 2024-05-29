using GameNetcodeStuff;
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
    public class LoadLadderConfigsPatch
    {
        internal const string DISABLED_LADDER_PREFIX = "- - - ";
        internal const string DISABLED_LADDER_NAME = DISABLED_LADDER_PREFIX + "(removed item: {0})";
        internal const int DISABLED_LADDER_PRICE = 99999;
        private static List<Item> removedItemsTerminalFix = new List<Item>();
        private static List<Item> removedItemsSafeFix = new List<Item>();

        private static float methodUptime = 10f;     //letting this patch run for couple of times since csync takes a bit to fully sync
        private static float updateConfigStart = 4f; //start sync after 4 seconds
        private static bool isPatchActive = true;

        private static bool isFirstPatch = true;
        private static bool wasFirstPatchFail = false;


        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPriority(Priority.Last)]
        public static void PatchLaddersConfigs(PlayerControllerB __instance)
        {
            if (isPatchActive && methodUptime > 0)
            {
                methodUptime -= Time.deltaTime;

                if (methodUptime < updateConfigStart)
                {
                    syncLadderPrices();
                }
            }
            else if (isPatchActive && methodUptime <= 0)
            {
                evaluateConfigSync();

                if (isFirstPatch && wasFirstPatchFail)
                {
                    GiantExtensionLaddersV2.mls.LogWarning("Initial config sync failed, trying again...");
                    isFirstPatch = false;
                    methodUptime = 10f;
                    removedItemsTerminalFix = new List<Item>();
                } else
                {
                    isPatchActive = false;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), "StartDisconnect")]
        public static void PlayerLeave()
        {
            removedItemsTerminalFix = new List<Item>();
            removedItemsSafeFix = new List<Item>();
            methodUptime = 10f;
            updateConfigStart = 4f;
            isPatchActive = true;
            isFirstPatch = true;
            wasFirstPatchFail = false;
        }

        private static void evaluateConfigSync()
        {
            bool isConfigSyncSuccess = true;

            foreach (var shopItem in Items.shopItems)
            {

                if (!shopItem.item.itemName.StartsWith(DISABLED_LADDER_PREFIX) && shopItem.item.itemName.Equals(GiantExtensionLaddersV2.tinyLadderItem.itemName) && shopItem.item.creditsWorth != MySyncedConfigs.Instance.tinyLadderPrice.Value)
                {
                    isConfigSyncSuccess = false;
                }
                else if (!shopItem.item.itemName.StartsWith(DISABLED_LADDER_PREFIX) && shopItem.item.itemName.Equals(GiantExtensionLaddersV2.bigLadderItem.itemName) && shopItem.item.creditsWorth != MySyncedConfigs.Instance.bigLadderPrice.Value)
                {
                    isConfigSyncSuccess = false;
                }
                else if (!shopItem.item.itemName.StartsWith(DISABLED_LADDER_PREFIX) && shopItem.item.itemName.Equals(GiantExtensionLaddersV2.hugeLadderItem.itemName) && shopItem.item.creditsWorth != MySyncedConfigs.Instance.hugeLadderPrice.Value)
                {
                    isConfigSyncSuccess = false;
                }
                else if (!shopItem.item.itemName.StartsWith(DISABLED_LADDER_PREFIX) && shopItem.item.itemName.Equals(GiantExtensionLaddersV2.ultimateLadderItem.itemName) && shopItem.item.creditsWorth != MySyncedConfigs.Instance.ultimateLadderPrice.Value)
                {
                    isConfigSyncSuccess = false;
                }
            }

            if (isConfigSyncSuccess)
            {
                GiantExtensionLaddersV2.mls.LogInfo("Config sync success! All settings should now be synced with the host's settings.");
                GiantExtensionLaddersV2.mls.LogInfo("Updating sales...");

                UnityEngine.Object.FindObjectOfType<Terminal>().SetItemSales();
            }
            else if (isFirstPatch)
            {
                wasFirstPatchFail = true;
            }
            else
            {
                GiantExtensionLaddersV2.mls.LogError("Config sync failed! \n " +
                    "Some of your settings are not synchronized with the host settings! This can lead to players having differently priced ladders.");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        public static void UpdateSales(PlayerControllerB __instance)
        {
            if (__instance.isJumping)
            {
                UnityEngine.Object.FindObjectOfType<Terminal>().SetItemSales();
            }
        }

        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch(typeof(Terminal), nameof(Terminal.SetItemSales))]
        public static void PatchSales(Terminal __instance)
        {
            if (__instance.itemSalesPercentages == null || __instance.itemSalesPercentages.Length == 0)
            {
                __instance.InitializeItemSalesPercentages();
            }
            System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed + 90);

            int numberOfItemsOnSale = __instance.buyableItemsList.Length;
            if (numberOfItemsOnSale <= 0)
            {
                return;
            }
            List<int> list = new List<int>();
            for (int i = 0; i < __instance.buyableItemsList.Length; i++)
            {
                list.Add(i);
                __instance.itemSalesPercentages[i] = 100;
            }
            for (int j = 0; j < numberOfItemsOnSale; j++)
            {
                if (list.Count <= 0)
                {
                    break;
                }
                int num2 = random.Next(0, list.Count);
                int maxValue = Mathf.Clamp(__instance.buyableItemsList[num2].highestSalePercentage, 0, 90);
                int salePercentage = 100 - random.Next(0, maxValue);
                salePercentage = __instance.RoundToNearestTen(salePercentage);
                __instance.itemSalesPercentages[num2] = salePercentage;
                list.RemoveAt(num2);
            }
        }

        private static void syncLadderPrices()
        {
            Terminal terminal = Object.FindObjectOfType<Terminal>();
            int amountOfRemovedItems = 0;

            if (MySyncedConfigs.Instance.isTinyLadderEnabled)
            {
                Items.UpdateShopItemPrice(GiantExtensionLaddersV2.tinyLadderItem, MySyncedConfigs.Instance.tinyLadderPrice.Value);
            }
            else
            {
                amountOfRemovedItems++;
                RemoveItem(GiantExtensionLaddersV2.tinyLadderItem, terminal, amountOfRemovedItems);
            }

            if (MySyncedConfigs.Instance.isBigLadderEnabled)
            {
                Items.UpdateShopItemPrice(GiantExtensionLaddersV2.bigLadderItem, MySyncedConfigs.Instance.bigLadderPrice.Value);
            }
            else
            {
                amountOfRemovedItems++;
                RemoveItem(GiantExtensionLaddersV2.bigLadderItem, terminal, amountOfRemovedItems);
            }

            if (MySyncedConfigs.Instance.isHugeLadderEnabled)
            {
                Items.UpdateShopItemPrice(GiantExtensionLaddersV2.hugeLadderItem, MySyncedConfigs.Instance.hugeLadderPrice.Value);
            }
            else
            {
                amountOfRemovedItems++;
                RemoveItem(GiantExtensionLaddersV2.hugeLadderItem, terminal, amountOfRemovedItems);
            }

            if (MySyncedConfigs.Instance.isUltimateLadderEnabled)
            {
                Items.UpdateShopItemPrice(GiantExtensionLaddersV2.ultimateLadderItem, MySyncedConfigs.Instance.ultimateLadderPrice.Value);
            }
            else
            {
                amountOfRemovedItems++;
                RemoveItem(GiantExtensionLaddersV2.ultimateLadderItem, terminal, amountOfRemovedItems);
            }
        }

        private static void RemoveItem(Item targetItem, Terminal terminal, int amountOfRemovedItems)
        {
            if (MySyncedConfigs.Instance.isSalesFixEasyActive)
            {
                Item buyableItem = FindBuyableItem(targetItem, terminal);
                if (buyableItem != null)
                {
                    if (!removedItemsSafeFix.Contains(buyableItem))
                    {
                        removedItemsSafeFix.Add(buyableItem);

                        buyableItem.itemName = string.Format(DISABLED_LADDER_NAME, removedItemsSafeFix.Count);
                        buyableItem.creditsWorth = DISABLED_LADDER_PRICE;
                        buyableItem.highestSalePercentage = 0;
                    }
                }
                else
                {
                    GiantExtensionLaddersV2.mls.LogWarning("Item to be removed could not be found. Will remove the item the old way, this can lead to sales being displayed falsely!");
                    Items.RemoveShopItem(targetItem);
                }
            } 
            else if (MySyncedConfigs.Instance.isSalesFixTerminalActive)
            {
                int removedItemIndex = 0;

                for (int i = 0; i < terminal.buyableItemsList.Length; i++)
                {
                    if (terminal.buyableItemsList[i].itemName == targetItem.itemName)
                    {
                        removedItemIndex = i;
                        break;
                    }
                }

                Item removedItem = terminal.buyableItemsList[removedItemIndex];
                Item itemLastOnList = terminal.buyableItemsList[terminal.buyableItemsList.Length - amountOfRemovedItems];

                UpdateBuyItemIndex(removedItem, terminal.buyableItemsList.Length - amountOfRemovedItems);
                UpdateBuyItemIndex(itemLastOnList, removedItemIndex);
                terminal.buyableItemsList[removedItemIndex] = itemLastOnList;
                terminal.buyableItemsList[terminal.buyableItemsList.Length - amountOfRemovedItems] = removedItem;

                if (!removedItemsTerminalFix.Contains(targetItem))
                {
                    removedItemsTerminalFix.Add(targetItem);
                    Items.RemoveShopItem(targetItem);
                }
            }
            else
            {
                Items.RemoveShopItem(targetItem);
            }
        }

        private static Item? FindBuyableItem(Item item, Terminal terminal)
        {
            foreach (var buyableItem in terminal.buyableItemsList)
            {
                if (buyableItem.itemName.Equals(item.itemName))
                {
                    return buyableItem;
                }
            }
            return null;
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
