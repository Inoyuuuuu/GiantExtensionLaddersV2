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
        private const bool debugLogsActive = false;
        internal const string DISABLED_LADDER_PREFIX = "- - - ";
        internal const string DISABLED_LADDER_NAME = DISABLED_LADDER_PREFIX + "(removed item: {0})";
        internal const int DISABLED_LADDER_PRICE = 99999;
        private static List<Item> removedItemsTerminalFix = new List<Item>();
        private static List<Item> removedItemsSafeFix = new List<Item>();

        private static float methodUptime = 10f;     //letting this patch run in loop for couple of seconds since csync takes a bit to fully sync
        private static float updateConfigStart = 3.5f; //start sync after this (in seconds)
        private static bool isPatchActive = true;

        private static bool isFirstPatch = true;
        private static bool wasFirstPatchFail = false;

        private static int amountOfRemovedItems = 0;


        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPriority(Priority.Last)]
        public static void PatchLaddersConfigs()
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

                GiantExtensionLaddersV2.mls.LogInfo("done!");

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

        private static void syncLadderPrices()
        {
            Terminal terminal = Object.FindObjectOfType<Terminal>();
            if (terminal == null)
            {
                GiantExtensionLaddersV2.mls.LogWarning("Terminal was not found in scene!");
                return;
            }

            amountOfRemovedItems = 0;

            UpdatePriceOrRemove(GiantExtensionLaddersV2.tinyLadderItem, MySyncedConfigs.Instance.isTinyLadderEnabled, MySyncedConfigs.Instance.tinyLadderPrice);
            UpdatePriceOrRemove(GiantExtensionLaddersV2.bigLadderItem, MySyncedConfigs.Instance.isBigLadderEnabled, MySyncedConfigs.Instance.bigLadderPrice);
            UpdatePriceOrRemove(GiantExtensionLaddersV2.hugeLadderItem, MySyncedConfigs.Instance.isHugeLadderEnabled, MySyncedConfigs.Instance.hugeLadderPrice);
            UpdatePriceOrRemove(GiantExtensionLaddersV2.ultimateLadderItem, MySyncedConfigs.Instance.isUltimateLadderEnabled, MySyncedConfigs.Instance.ultimateLadderPrice);

        }

        private static void UpdatePriceOrRemove(Item ladderItem, bool isLadderEnabled, int updatedPrice)
        {
            if (ladderItem != null)
            {
                if (isLadderEnabled)
                {
                    Items.UpdateShopItemPrice(ladderItem, updatedPrice);
                }
                else
                {
                    amountOfRemovedItems++;
                    RemoveItem(ladderItem, terminal, amountOfRemovedItems);
                }
            } else
            {
                GiantExtensionLaddersV2.mls.LogDebug("ladder was null");
            }
        }

        private static void RemoveItem(Item targetItem, Terminal terminal, int amountOfRemovedItems)
        {
            if (MySyncedConfigs.Instance.isSalesFixEasyActive)
            {
                Item? buyableItem = FindBuyableItem(targetItem, terminal);
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
                if (debugLogsActive) GiantExtensionLaddersV2.mls.LogDebug("early return: StartOfRound was null? Dafuq how did that happen??");

                return;
            }

            TerminalKeyword terminalKeyword = terminal.terminalNodes.allKeywords.First((TerminalKeyword keyword) => keyword.word == "buy");
            TerminalNode itemTerminalNode = terminalKeyword.compatibleNouns[0].result.terminalOptions[1].result;
            List<CompatibleNoun> source = terminalKeyword.compatibleNouns.ToList();

            if (!buyableItemAssetInfos.Any((BuyableItemAssetInfo x) => x.itemAsset == item))
            {
                if (debugLogsActive) GiantExtensionLaddersV2.mls.LogDebug("early return: buyableItemAssetInfos not found");

                return;
            }

            BuyableItemAssetInfo asset = buyableItemAssetInfos.First((BuyableItemAssetInfo x) => x.itemAsset == item);

            if (!source.Any((CompatibleNoun noun) => noun.noun == asset.keyword))
            {
                if (debugLogsActive) GiantExtensionLaddersV2.mls.LogDebug("early return: compatible noun not found");

                return;
            }


            TerminalNode result = source.First((CompatibleNoun noun) => noun.noun == asset.keyword).result;

            if (debugLogsActive) GiantExtensionLaddersV2.mls.LogDebug("SUCCESS old buyItemIndex: " + result.buyItemIndex + " new buyItemIndex: " + newIndex);

            result.buyItemIndex = newIndex;

            if (result.terminalOptions.Length == 0)
            {
                if (debugLogsActive) GiantExtensionLaddersV2.mls.LogDebug("return: terminal options == 0");
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
