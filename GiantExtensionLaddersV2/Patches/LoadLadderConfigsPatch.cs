using BepInEx.Logging;
using GameNetcodeStuff;
using GiantExtensionLaddersV2.ConfigStuff;
using HarmonyLib;
using LethalLib.Modules;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace GiantExtensionLaddersV2.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    public class LoadLadderConfigsPatch
    {
        private static float methodUptime = 10f; //this shit lmao. letting this patch run for couple of times since csync takes a bit to fully sync
        private static float updateConfigStart = 5f; //start at 5sec
        private static bool patchActive = true;

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        [HarmonyPriority(Priority.Last)]
        public static void PatchLaddersConfigs(PlayerControllerB __instance)
        {
            if (patchActive && methodUptime > 0)
            {
                methodUptime -= Time.deltaTime;

                if (methodUptime < updateConfigStart)
                {
                    if (ConfigStuff.MySyncedConfigs.Instance.IS_TINY_LADDER_ENABLED)
                    {
                        Items.UpdateShopItemPrice(GiantExtensionLaddersV2.tinyLadderItem, ConfigStuff.MySyncedConfigs.Instance.TINY_LADDER_PRICE.Value);
                    }
                    else
                    {
                        Items.RemoveShopItem(GiantExtensionLaddersV2.tinyLadderItem);
                    }

                    if (ConfigStuff.MySyncedConfigs.Instance.IS_BIG_LADDER_ENABLED)
                    {
                        Items.UpdateShopItemPrice(GiantExtensionLaddersV2.bigLadderItem, ConfigStuff.MySyncedConfigs.Instance.BIG_LADDER_PRICE.Value);
                    }
                    else
                    {
                        Items.RemoveShopItem(GiantExtensionLaddersV2.bigLadderItem);
                    }

                    if (ConfigStuff.MySyncedConfigs.Instance.IS_HUGE_LADDER_ENABLED)
                    {
                        Items.UpdateShopItemPrice(GiantExtensionLaddersV2.hugeLadderItem, ConfigStuff.MySyncedConfigs.Instance.HUGE_LADDER_PRICE.Value);
                    }
                    else
                    {
                        Items.RemoveShopItem(GiantExtensionLaddersV2.hugeLadderItem);
                    }
                }
            }
            else if (patchActive && methodUptime <= 0)
            {
                GiantExtensionLaddersV2.mls.LogInfo("config sync finished");
                GiantExtensionLaddersV2.mls.LogInfo("Tiny ladder price is now: " + MySyncedConfigs.Instance.TINY_LADDER_PRICE.Value);
                GiantExtensionLaddersV2.mls.LogInfo("Big ladder price is now: " + MySyncedConfigs.Instance.BIG_LADDER_PRICE.Value);
                GiantExtensionLaddersV2.mls.LogInfo("Huge ladder price is now: " + MySyncedConfigs.Instance.HUGE_LADDER_PRICE.Value);
                patchActive = false;
            }
        }
    }
}
