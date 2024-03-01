using BepInEx.Logging;
using GameNetcodeStuff;
using GiantExtensionLaddersV2.ConfigStuff;
using HarmonyLib;
using LethalLib.Modules;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace GiantExtensionLaddersV2.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    public class LoadLadderConfigsPatch
    {
        private static int methodCallCount = 0; //letting this patch run for couple of times since csync takes a bit to fully sync

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        [HarmonyPriority(Priority.Last)]
        public static void PatchLaddersConfigs(PlayerControllerB __instance)
        {
            if (methodCallCount < 30)
            {
                methodCallCount++;

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

                GiantExtensionLaddersV2.mls.LogInfo(ConfigStuff.MySyncedConfigs.Instance.TINY_LADDER_PRICE.Value.ToString());
                GiantExtensionLaddersV2.mls.LogInfo(ConfigStuff.MySyncedConfigs.Instance.BIG_LADDER_PRICE.Value.ToString());
                GiantExtensionLaddersV2.mls.LogInfo(ConfigStuff.MySyncedConfigs.Instance.HUGE_LADDER_PRICE.Value.ToString());
            }
        }
    }
}
