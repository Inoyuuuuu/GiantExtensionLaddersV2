using GameNetcodeStuff;
using GiantExtensionLaddersV2.ConfigStuff;
using HarmonyLib;
using LethalLib.Modules;
using System.Collections.Generic;
using UnityEngine;

namespace GiantExtensionLaddersV2.Patches
{
    [HarmonyPatch()]
    public class LoadLadderConfigsPatch
    {
        private static float methodUptime = 10f; //letting this patch run for couple of times since csync takes a bit to fully sync
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
                    GiantExtensionLaddersV2.mls.LogInfo("syncing ladder prices: " + methodUptime);

                    syncLadderPrices();
                }
            }
            else if (isPatchActive && methodUptime <= 0)
            {
                GiantExtensionLaddersV2.mls.LogInfo("evaluating config patch");

                evaluatePatch();

                if (isFirstPatch && wasFirstPatchFail)
                {
                    GiantExtensionLaddersV2.mls.LogWarning("Initial config sync failed, trying again...");
                    isFirstPatch = false;
                    methodUptime = 10f;
                } else
                {
                    GiantExtensionLaddersV2.mls.LogInfo("config patch done");

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
            GiantExtensionLaddersV2.mls.LogInfo("reset sync");
        }

        private static void evaluatePatch()
        {
            bool isPatchSuccess = true;

            foreach (var shopItem in Items.shopItems)
            {

                if (shopItem.item.spawnPrefab.name.Equals(GiantExtensionLaddersV2.tinyLadderItem.spawnPrefab.name) && shopItem.item.creditsWorth != MySyncedConfigs.Instance.TINY_LADDER_PRICE.Value)
                {
                    isPatchSuccess = false;
                }
                else if (shopItem.item.spawnPrefab.name.Equals(GiantExtensionLaddersV2.bigLadderItem.spawnPrefab.name) && shopItem.item.creditsWorth != MySyncedConfigs.Instance.BIG_LADDER_PRICE.Value)
                {
                    isPatchSuccess = false;
                }
                else if (shopItem.item.spawnPrefab.name.Equals(GiantExtensionLaddersV2.hugeLadderItem.spawnPrefab.name) && shopItem.item.creditsWorth != MySyncedConfigs.Instance.HUGE_LADDER_PRICE.Value)
                {
                    isPatchSuccess = false;
                }
                else if (shopItem.item.spawnPrefab.name.Equals(GiantExtensionLaddersV2.ultimateLadderItem.spawnPrefab.name) && shopItem.item.creditsWorth != MySyncedConfigs.Instance.ULTIMATE_LADDER_PRICE.Value)
                {
                    isPatchSuccess = false;
                }
            }

            if (isPatchSuccess)
            {
                GiantExtensionLaddersV2.mls.LogInfo("Config sync success! All settings should now be synced with the host's settings.");
                GiantExtensionLaddersV2.mls.LogDebug("Tiny ladder price is: " + MySyncedConfigs.Instance.TINY_LADDER_PRICE.Value);
                GiantExtensionLaddersV2.mls.LogDebug("Big ladder price is: " + MySyncedConfigs.Instance.BIG_LADDER_PRICE.Value);
                GiantExtensionLaddersV2.mls.LogDebug("Huge ladder price is: " + MySyncedConfigs.Instance.HUGE_LADDER_PRICE.Value);
                GiantExtensionLaddersV2.mls.LogDebug("ultimate ladder price is: " + MySyncedConfigs.Instance.ULTIMATE_LADDER_PRICE.Value);
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
            GiantExtensionLaddersV2.mls.LogInfo("tiny ladder enabled" + MySyncedConfigs.Instance.IS_TINY_LADDER_ENABLED);

            if (MySyncedConfigs.Instance.IS_TINY_LADDER_ENABLED)
            {
                Items.UpdateShopItemPrice(GiantExtensionLaddersV2.tinyLadderItem, MySyncedConfigs.Instance.TINY_LADDER_PRICE.Value);
            }
            else
            {
                Items.RemoveShopItem(GiantExtensionLaddersV2.tinyLadderItem);
            }

            GiantExtensionLaddersV2.mls.LogInfo("big ladder enabled" + MySyncedConfigs.Instance.IS_BIG_LADDER_ENABLED);

            if (MySyncedConfigs.Instance.IS_BIG_LADDER_ENABLED)
            {
                Items.UpdateShopItemPrice(GiantExtensionLaddersV2.bigLadderItem, MySyncedConfigs.Instance.BIG_LADDER_PRICE.Value);
            }
            else
            {
                Items.RemoveShopItem(GiantExtensionLaddersV2.bigLadderItem);
            }

            GiantExtensionLaddersV2.mls.LogInfo("huge ladder enabled" + MySyncedConfigs.Instance.IS_HUGE_LADDER_ENABLED);

            if (MySyncedConfigs.Instance.IS_HUGE_LADDER_ENABLED)
            {
                Items.UpdateShopItemPrice(GiantExtensionLaddersV2.hugeLadderItem, MySyncedConfigs.Instance.HUGE_LADDER_PRICE.Value);
            }
            else
            {
                Items.RemoveShopItem(GiantExtensionLaddersV2.hugeLadderItem);
            }

            GiantExtensionLaddersV2.mls.LogInfo("ult ladder enabled" + MySyncedConfigs.Instance.IS_ULTIMATE_LADDER_ENABLED);

            if (MySyncedConfigs.Instance.IS_ULTIMATE_LADDER_ENABLED)
            {
                Items.UpdateShopItemPrice(GiantExtensionLaddersV2.ultimateLadderItem, MySyncedConfigs.Instance.ULTIMATE_LADDER_PRICE.Value);
            }
            else
            {
                Items.RemoveShopItem(GiantExtensionLaddersV2.ultimateLadderItem);
            }
        }
    }
}
