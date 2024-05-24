using BepInEx.Configuration;
using BepInEx.Logging;
using CSync.Lib;
using CSync.Util;
using System;
using System.Runtime.Serialization;


namespace GiantExtensionLaddersV2.ConfigStuff;

[DataContract]
internal class MySyncedConfigs : SyncedConfig<MySyncedConfigs>
{

    private const int MIN_LADDER_PRICE = 1;
    private const int MAX_LADDER_PRICE = 99999;
    private const float MIN_EXT_TIME = 3f;
    private const float MAX_EXT_TIME = 999f;

    private const int tinyLadderBasePrice = 15;
    private const int bigLadderBasePrice = 75;
    private const int hugeLadderBasePrice = 160;
    private const int ultimateLadderBasePrice = 250;
    private const float tinyLadderExtensionTimeBase = 10f;
    private const float bigLadderExtensionTimeBase = 25f;
    private const float hugeLadderExtensionTimeBase = 30f;
    private const float ultimateLadderExtensionTimeBase = 60f;

    internal static bool waspreSyncTinyLadderEnabled;
    internal static bool waspreSyncBigLadderEnabled;
    internal static bool waspreSyncHugeLadderEnabled;
    internal static bool waspreSyncUltimateLadderEnabled;
    internal static int preSyncTinyLadderPrice;
    internal static int preSyncBigLadderPrice;
    internal static int preSyncHugeLadderPrice;
    internal static int preSyncUltimateLadderPrice;
    internal static float preSyncTinyLadderExtTime;
    internal static float preSyncdBigLadderExtTime;
    internal static float preSyncHugeLadderExtTime;
    internal static float preSyncUltimateLadderExtTime;

    [DataMember]
    internal SyncedEntry<bool> IS_TINY_LADDER_ENABLED, IS_BIG_LADDER_ENABLED, IS_HUGE_LADDER_ENABLED, IS_ULTIMATE_LADDER_ENABLED;
    [DataMember]
    internal SyncedEntry<float> TINY_LADDER_EXT_TIME, BIG_LADDER_EXT_TIME, HUGE_LADDER_EXT_TIME, ULTIMATE_LADDER_EXT_TIME;
    [DataMember]
    internal SyncedEntry<int> TINY_LADDER_PRICE, BIG_LADDER_PRICE, HUGE_LADDER_PRICE, ULTIMATE_LADDER_PRICE;

    private static ManualLogSource mlsConfig = Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID + ".Config");

    internal MySyncedConfigs(ConfigFile cfg) : base(MyPluginInfo.PLUGIN_NAME)
    {
        ConfigManager.Register(this);

        //laddersActive
        IS_TINY_LADDER_ENABLED = cfg.BindSyncedEntry("DeactivateLadders", "isTinyLadderEnabled", true, "Tiny ladder doesn't appear in the shop if this is set to false.");
        IS_BIG_LADDER_ENABLED = cfg.BindSyncedEntry("DeactivateLadders", "isBigLadderEnabled", true, "Big ladder doesn't appear in the shop if this is set to false.");
        IS_HUGE_LADDER_ENABLED = cfg.BindSyncedEntry("DeactivateLadders", "isHugeLadderEnabled", true, "Huge ladder doesn't appear in the shop if this is set to false.");
        IS_ULTIMATE_LADDER_ENABLED = cfg.BindSyncedEntry("DeactivateLadders", "isUltimateLadderEnabled", true, "Ultimate ladder doesn't appear in the shop if this is set to false.");


        //ladderPrices
        TINY_LADDER_PRICE = cfg.BindSyncedEntry("LadderPrices", "tinyLadderPrice", tinyLadderBasePrice, "Sets the price of the tiny ladder");
        BIG_LADDER_PRICE = cfg.BindSyncedEntry("LadderPrices", "bigLadderPrice", bigLadderBasePrice, "Sets the price of the big ladder");
        HUGE_LADDER_PRICE = cfg.BindSyncedEntry("LadderPrices", "hugeLadderPrice", hugeLadderBasePrice, "Sets the price of the huge ladder");
        ULTIMATE_LADDER_PRICE = cfg.BindSyncedEntry("LadderPrices", "ultimateLadderPrice", ultimateLadderBasePrice, "Sets the price of the ultimate ladder");

        //ladderExtTime
        TINY_LADDER_EXT_TIME = cfg.BindSyncedEntry("LadderExtensionTime", "tinyLadderExtensionTime", tinyLadderExtensionTimeBase, "RULES THAT APPLY FOR ALL LADDER TIME CONFIGS:" + Environment.NewLine
        + "Values above 660s last longer than a whole day." + Environment.NewLine
        + "Values below 8 seconds have audio issues!" + Environment.NewLine + Environment.NewLine
        + "Sets the amount of seconds the tiny ladder stays extended.");
        BIG_LADDER_EXT_TIME = cfg.BindSyncedEntry("LadderExtensionTime", "bigLadderExtensionTime", bigLadderExtensionTimeBase, "Sets the amount of seconds the big ladder stays extended.");
        HUGE_LADDER_EXT_TIME = cfg.BindSyncedEntry("LadderExtensionTime", "hugeLadderExtensionTime", hugeLadderExtensionTimeBase, "Sets the amount of seconds the huge ladder stays extended");
        ULTIMATE_LADDER_EXT_TIME = cfg.BindSyncedEntry("LadderExtensionTime", "ultimateLadderExtensionTime", ultimateLadderExtensionTimeBase, "Sets the amount of seconds the ultimate ladder stays extended");

        fixConfigs();
    }

    private void fixConfigs()
    {
        //prices
        if (TINY_LADDER_PRICE.Value < MIN_LADDER_PRICE)
        {
            TINY_LADDER_PRICE.Value = tinyLadderBasePrice;
            mlsConfig.LogWarning("big ladder price was too low, was set to basic value: " + tinyLadderBasePrice);
        } else if (TINY_LADDER_PRICE.Value > MAX_LADDER_PRICE)
        {
            TINY_LADDER_PRICE.Value = MAX_LADDER_PRICE;
            mlsConfig.LogWarning("big ladder price was too high, was set to max value: " + MAX_EXT_TIME);
        }

        if (BIG_LADDER_PRICE.Value < MIN_LADDER_PRICE)
        {
            BIG_LADDER_PRICE.Value = bigLadderBasePrice;
            mlsConfig.LogWarning("big ladder price was too low, was set to basic value: " + bigLadderBasePrice);
        } else if (BIG_LADDER_PRICE.Value > MAX_LADDER_PRICE)
        {
            BIG_LADDER_PRICE.Value = MAX_LADDER_PRICE;
            mlsConfig.LogWarning("big ladder price was too high, was set to max value: " + MAX_EXT_TIME);
        }

        if (HUGE_LADDER_PRICE.Value < MIN_LADDER_PRICE)
        {
            HUGE_LADDER_PRICE.Value = hugeLadderBasePrice;
            mlsConfig.LogWarning("huge ladder price was too low, was set to basic value: " + hugeLadderBasePrice);
        } else if (HUGE_LADDER_PRICE.Value > MAX_LADDER_PRICE)
        {
            HUGE_LADDER_PRICE.Value = MAX_LADDER_PRICE;
            mlsConfig.LogWarning("huge ladder price was too high, was set to max value: " + MAX_LADDER_PRICE);
        }

        if (ULTIMATE_LADDER_PRICE.Value < MIN_LADDER_PRICE)
        {
            ULTIMATE_LADDER_PRICE.Value = ultimateLadderBasePrice;
            mlsConfig.LogWarning("ultimate ladder price was too low, was set to basic value: " + ultimateLadderBasePrice);
        }
        else if (ULTIMATE_LADDER_PRICE.Value > MAX_LADDER_PRICE)
        {
            ULTIMATE_LADDER_PRICE.Value = MAX_LADDER_PRICE;
            mlsConfig.LogWarning("ultimate ladder price was too high, was set to max value: " + MAX_LADDER_PRICE);
        }

        //ext-time
        if (TINY_LADDER_EXT_TIME < MIN_EXT_TIME)
        {
            TINY_LADDER_EXT_TIME.Value = tinyLadderExtensionTimeBase;
            mlsConfig.LogWarning("big ladder extension time was too low, was set to basic value: " + tinyLadderExtensionTimeBase);
        } else if (TINY_LADDER_EXT_TIME.Value > MAX_EXT_TIME)
        {
            TINY_LADDER_EXT_TIME.Value = MAX_EXT_TIME;
            mlsConfig.LogWarning("big ladder extension time was too high, was set to max value: " + MAX_EXT_TIME);
            mlsConfig.LogWarning("Values over 660 already last longer than a day");
        }

        if (BIG_LADDER_EXT_TIME.Value < MIN_EXT_TIME)
        {
            BIG_LADDER_EXT_TIME.Value = bigLadderExtensionTimeBase;
            mlsConfig.LogWarning("big ladder extension time was too low, was set to basic value: " + bigLadderExtensionTimeBase);
        } else if (BIG_LADDER_EXT_TIME.Value > MAX_EXT_TIME)
        {
            BIG_LADDER_EXT_TIME.Value = MAX_EXT_TIME;
            mlsConfig.LogWarning("big ladder extension time was too high, was set to max value: " + MAX_EXT_TIME);
            mlsConfig.LogWarning("Values over 660 already last longer than a day");
        }

        if (HUGE_LADDER_EXT_TIME.Value < MIN_EXT_TIME)
        {
            HUGE_LADDER_EXT_TIME.Value = hugeLadderExtensionTimeBase;
            mlsConfig.LogWarning("huge ladder extension time was too low, was set to basic value: " + hugeLadderExtensionTimeBase);
        } else if (HUGE_LADDER_EXT_TIME.Value > MAX_EXT_TIME)
        {
            HUGE_LADDER_EXT_TIME.Value = MAX_EXT_TIME;
            mlsConfig.LogWarning("huge ladder extension time was too high, was set to max value: " + MAX_EXT_TIME);
            mlsConfig.LogWarning("Values over 660 already last longer than a day");
        }

        if (ULTIMATE_LADDER_EXT_TIME.Value < MIN_EXT_TIME)
        {
            ULTIMATE_LADDER_EXT_TIME.Value = ultimateLadderExtensionTimeBase;
            mlsConfig.LogWarning("huge ladder extension time was too low, was set to basic value: " + ultimateLadderExtensionTimeBase);
        }
        else if (ULTIMATE_LADDER_EXT_TIME.Value > MAX_EXT_TIME)
        {
            ULTIMATE_LADDER_EXT_TIME.Value = MAX_EXT_TIME;
            mlsConfig.LogWarning("huge ladder extension time was too high, was set to max value: " + MAX_EXT_TIME);
            mlsConfig.LogWarning("Values over 660 already last longer than a day");
        }
    }
}
