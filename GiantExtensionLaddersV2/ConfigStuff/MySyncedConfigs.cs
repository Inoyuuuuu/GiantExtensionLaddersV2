using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CSync.Lib;
using CSync.Util;
using GameNetcodeStuff;
using HarmonyLib;
using LethalLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;


namespace GiantExtensionLaddersV2.ConfigStuff;

[DataContract]
public class MySyncedConfigs : SyncedConfig<MySyncedConfigs>
{

    private const int MIN_LADDER_PRICE = 1;
    private const int MAX_LADDER_PRICE = 999;
    private const float MIN_EXT_TIME = 3f;
    private const float MAX_EXT_TIME = 999f;

    private const int tinyLadderBasePrice = 15;
    private const int bigLadderBasePrice = 75;
    private const int hugeLadderBasePrice = 160;
    private const float tinyLadderExtensionTimeBase = 10f;
    private const float bigLadderExtensionTimeBase = 25f;
    private const float hugeLadderExtensionTimeBase = 30f;

    [DataMember] public SyncedEntry<bool> IS_TINY_LADDER_ENABLED { get; private set; }
    [DataMember] public SyncedEntry<bool> IS_BIG_LADDER_ENABLED { get; private set; }
    [DataMember] public SyncedEntry<bool> IS_HUGE_LADDER_ENABLED { get; private set; }

    [DataMember] public SyncedEntry<float> TINY_LADDER_EXT_TIME { get; private set; }
    [DataMember] public SyncedEntry<float> BIG_LADDER_EXT_TIME { get; private set; }
    [DataMember] public SyncedEntry<float> HUGE_LADDER_EXT_TIME { get; private set; }

    [DataMember] public SyncedEntry<int> TINY_LADDER_PRICE { get; private set; }
    [DataMember] public SyncedEntry<int> BIG_LADDER_PRICE { get; private set; }
    [DataMember] public SyncedEntry<int> HUGE_LADDER_PRICE { get; private set; }

    [NonSerialized]
    readonly ConfigFile configFile;

    private static ManualLogSource mlsConfig = Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID + ".Config");

    public MySyncedConfigs(ConfigFile cfg) : base(MyPluginInfo.PLUGIN_GUID)
    {
        ConfigManager.Register(this);

        configFile = cfg;

        //laddersActive
        IS_TINY_LADDER_ENABLED = cfg.BindSyncedEntry("DeactivateLadders", "isTinyLadderEnabled", true, "Tiny ladder doesn't appear in the shop if instance is set to false.");
        IS_BIG_LADDER_ENABLED = cfg.BindSyncedEntry("DeactivateLadders", "isBigLadderEnabled", true, "Big ladder doesn't appear in the shop if instance is set to false.");
        IS_HUGE_LADDER_ENABLED = cfg.BindSyncedEntry("DeactivateLadders", "isHugeLadderEnabled", true, "Huge ladder doesn't appear in the shop if instance is set to false.");

        //ladderPrices
        TINY_LADDER_PRICE = cfg.BindSyncedEntry("LadderPrices", "tinyLadderPrice", tinyLadderBasePrice, "Sets the price of the tiny ladder");
        BIG_LADDER_PRICE = cfg.BindSyncedEntry("LadderPrices", "bigLadderPrice", bigLadderBasePrice, "Sets the price of the big ladder");
        HUGE_LADDER_PRICE = cfg.BindSyncedEntry("LadderPrices", "hugeLadderPrice", hugeLadderBasePrice, "Sets the price of the huge ladder");

        //ladderExtTime
        TINY_LADDER_EXT_TIME = cfg.BindSyncedEntry("LadderExtensionTime", "tinyLadderExtensionTime", tinyLadderExtensionTimeBase, "RULES THAT APPLY FOR ALL LADDER TIME CONFIGS:" + Environment.NewLine
        + "Values above 660s last longer than a whole day." + Environment.NewLine
        + "Values below 8 seconds have audio issues!" + Environment.NewLine + Environment.NewLine
        + "Sets the amount of seconds the tiny ladder stays extended.");
        BIG_LADDER_EXT_TIME = cfg.BindSyncedEntry("LadderExtensionTime", "bigLadderExtensionTime", bigLadderExtensionTimeBase, "Sets the amount of seconds the big ladder stays extended.");
        HUGE_LADDER_EXT_TIME = cfg.BindSyncedEntry("LadderExtensionTime", "hugeLadderExtensionTime", hugeLadderExtensionTimeBase, "Sets the amount of seconds the huge ladder stays extended");

        fixConfigs();
    }

    private void fixConfigs()
    {
        //prices
        if (TINY_LADDER_PRICE.Value < MIN_LADDER_PRICE)
        {
            TINY_LADDER_PRICE.Value = tinyLadderBasePrice;
            mlsConfig.LogWarning("big ladder price was too low, was set to basic value: " + tinyLadderBasePrice);
        }
        if (TINY_LADDER_PRICE.Value > MAX_LADDER_PRICE)
        {
            TINY_LADDER_PRICE.Value = MAX_LADDER_PRICE;
            mlsConfig.LogWarning("big ladder price was too high, was set to max value: " + MAX_EXT_TIME);
        }

        if (BIG_LADDER_PRICE.Value < MIN_LADDER_PRICE)
        {
            BIG_LADDER_PRICE.Value = bigLadderBasePrice;
            mlsConfig.LogWarning("big ladder price was too low, was set to basic value: " + bigLadderBasePrice);
        }
        if (BIG_LADDER_PRICE.Value > MAX_LADDER_PRICE)
        {
            BIG_LADDER_PRICE.Value = MAX_LADDER_PRICE;
            mlsConfig.LogWarning("big ladder price was too high, was set to max value: " + MAX_EXT_TIME);
        }

        if (HUGE_LADDER_PRICE.Value < MIN_LADDER_PRICE)
        {
            HUGE_LADDER_PRICE.Value = hugeLadderBasePrice;
            mlsConfig.LogWarning("huge ladder price was too low, was set to basic value: " + hugeLadderBasePrice);
        }
        if (HUGE_LADDER_PRICE.Value > MAX_LADDER_PRICE)
        {
            HUGE_LADDER_PRICE.Value = MAX_LADDER_PRICE;
            mlsConfig.LogWarning("huge ladder price was too high, was set to max value: " + MAX_LADDER_PRICE);
        }

        //ext-time
        if (TINY_LADDER_EXT_TIME < MIN_EXT_TIME)
        {
            TINY_LADDER_EXT_TIME.Value = tinyLadderExtensionTimeBase;
            mlsConfig.LogWarning("big ladder extension time was too low, was set to basic value: " + tinyLadderExtensionTimeBase);
        }
        if (TINY_LADDER_EXT_TIME.Value > MAX_EXT_TIME)
        {
            TINY_LADDER_EXT_TIME.Value = MAX_EXT_TIME;
            mlsConfig.LogWarning("big ladder extension time was too high, was set to max value: " + MAX_EXT_TIME);
            mlsConfig.LogWarning("Values over 660 already last longer than a day");
        }

        if (BIG_LADDER_EXT_TIME.Value < MIN_EXT_TIME)
        {
            BIG_LADDER_EXT_TIME.Value = bigLadderExtensionTimeBase;
            mlsConfig.LogWarning("big ladder extension time was too low, was set to basic value: " + bigLadderExtensionTimeBase);
        }
        if (BIG_LADDER_EXT_TIME.Value > MAX_EXT_TIME)
        {
            BIG_LADDER_EXT_TIME.Value = MAX_EXT_TIME;
            mlsConfig.LogWarning("big ladder extension time was too high, was set to max value: " + MAX_EXT_TIME);
            mlsConfig.LogWarning("Values over 660 already last longer than a day");
        }

        if (HUGE_LADDER_EXT_TIME.Value < MIN_EXT_TIME)
        {
            HUGE_LADDER_EXT_TIME.Value = hugeLadderExtensionTimeBase;
            mlsConfig.LogWarning("huge ladder extension time was too low, was set to basic value: " + hugeLadderExtensionTimeBase);
        }
        if (HUGE_LADDER_EXT_TIME.Value > MAX_EXT_TIME)
        {
            HUGE_LADDER_EXT_TIME.Value = MAX_EXT_TIME;
            mlsConfig.LogWarning("huge ladder extension time was too high, was set to max value: " + MAX_EXT_TIME);
            mlsConfig.LogWarning("Values over 660 already last longer than a day");
        }
    }
}
