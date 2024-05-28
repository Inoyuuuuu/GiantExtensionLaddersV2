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
        internal const string DISABLED_LADDER_NAME = "                          ";
        internal const int DISABLED_LADDER_PRICE = 99999;

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

                if (shopItem.item.spawnPrefab.name.Equals(GiantExtensionLaddersV2.tinyLadderItem.spawnPrefab.name) && shopItem.item.creditsWorth != MySyncedConfigs.Instance.tinyLadderPrice.Value)
                {
                    isConfigSyncSuccess = false;
                }
                else if (shopItem.item.spawnPrefab.name.Equals(GiantExtensionLaddersV2.bigLadderItem.spawnPrefab.name) && shopItem.item.creditsWorth != MySyncedConfigs.Instance.bigLadderPrice.Value)
                {
                    isConfigSyncSuccess = false;
                }
                else if (shopItem.item.spawnPrefab.name.Equals(GiantExtensionLaddersV2.hugeLadderItem.spawnPrefab.name) && shopItem.item.creditsWorth != MySyncedConfigs.Instance.hugeLadderPrice.Value)
                {
                    isConfigSyncSuccess = false;
                }
                else if (shopItem.item.spawnPrefab.name.Equals(GiantExtensionLaddersV2.ultimateLadderItem.spawnPrefab.name) && shopItem.item.creditsWorth != MySyncedConfigs.Instance.ultimateLadderPrice.Value)
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

        private static void syncLadderPrices()
        {
            Terminal terminal = Object.FindObjectOfType<Terminal>();

            if (MySyncedConfigs.Instance.isTinyLadderEnabled)
            {
                Items.UpdateShopItemPrice(GiantExtensionLaddersV2.tinyLadderItem, MySyncedConfigs.Instance.tinyLadderPrice.Value);
            }
            else if (MySyncedConfigs.Instance.isSalesFixEasyActive) 
            {
                Item item = FindBuyableItem(GiantExtensionLaddersV2.tinyLadderItem, terminal);
                item.itemName = DISABLED_LADDER_NAME;
                item.creditsWorth = DISABLED_LADDER_PRICE;
            }
            else
            {
                Items.RemoveShopItem(GiantExtensionLaddersV2.tinyLadderItem);
            }

            if (MySyncedConfigs.Instance.isBigLadderEnabled)
            {
                Items.UpdateShopItemPrice(GiantExtensionLaddersV2.bigLadderItem, MySyncedConfigs.Instance.bigLadderPrice.Value);
            }
            else if (MySyncedConfigs.Instance.isSalesFixEasyActive)
            {
                Item item = FindBuyableItem(GiantExtensionLaddersV2.bigLadderItem, terminal);
                item.itemName = DISABLED_LADDER_NAME;
                item.creditsWorth = DISABLED_LADDER_PRICE;
            }
            else
            {
                Items.RemoveShopItem(GiantExtensionLaddersV2.bigLadderItem);
            }

            if (MySyncedConfigs.Instance.isHugeLadderEnabled)
            {
                Items.UpdateShopItemPrice(GiantExtensionLaddersV2.hugeLadderItem, MySyncedConfigs.Instance.hugeLadderPrice.Value);
            }
            else if (MySyncedConfigs.Instance.isSalesFixEasyActive)
            {
                Item item = FindBuyableItem(GiantExtensionLaddersV2.hugeLadderItem, terminal);
                item.itemName = DISABLED_LADDER_NAME;
                item.creditsWorth = DISABLED_LADDER_PRICE;
            }
            else
            {
                Items.RemoveShopItem(GiantExtensionLaddersV2.hugeLadderItem);
            }

            if (MySyncedConfigs.Instance.isUltimateLadderEnabled)
            {
                Items.UpdateShopItemPrice(GiantExtensionLaddersV2.ultimateLadderItem, MySyncedConfigs.Instance.ultimateLadderPrice.Value);
            }
            else if (MySyncedConfigs.Instance.isSalesFixEasyActive)
            {
                Item item = FindBuyableItem(GiantExtensionLaddersV2.ultimateLadderItem, terminal);
                item.itemName = DISABLED_LADDER_NAME;
                item.creditsWorth = DISABLED_LADDER_PRICE;
            }
            else
            {
                Items.RemoveShopItem(GiantExtensionLaddersV2.ultimateLadderItem);
            }
        }

        private static Item FindBuyableItem(Item item, Terminal terminal)
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
    }
}
