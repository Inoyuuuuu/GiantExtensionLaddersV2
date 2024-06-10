using BepInEx.Configuration;
using BepInEx.Logging;
using CSync.Lib;
using CSync.Util;
using GiantExtensionLaddersV2.Patches;
using System;
using System.Runtime.Serialization;


namespace GiantExtensionLaddersV2.ConfigStuff;

[DataContract]
internal class MySyncedConfigs : SyncedConfig<MySyncedConfigs>
{
    private const int MIN_LADDER_PRICE = 1;
    private const int MAX_LADDER_PRICE = 999999;
    private const float MIN_EXT_TIME = 3f;
    private const float MAX_EXT_TIME = 999f;

    private const int tinyLadderBasePrice = 15;
    private const int bigLadderBasePrice = 75;
    private const int hugeLadderBasePrice = 160;
    private const int ultimateLadderBasePrice = 250;
    private const int ladderCollectorBasePrice = 75;
    private const float tinyLadderExtensionTimeBase = 10f;
    private const float bigLadderExtensionTimeBase = 25f;
    private const float hugeLadderExtensionTimeBase = 30f;
    private const float ultimateLadderExtensionTimeBase = 60f;

    private const float teleportFrequencyBase = 0.8f;

    [DataMember]
    internal SyncedEntry<bool> isTinyLadderEnabled, isBigLadderEnabled, isHugeLadderEnabled, isUltimateLadderEnabled, isLadderCollectorEnabled;
    [DataMember]
    internal SyncedEntry<float> tinyLadderExtTime, bigLadderExtTime, hugeLadderExtTime, ultimateLadderExtTime;
    [DataMember]
    internal SyncedEntry<int> tinyLadderPrice, bigLadderPrice, hugeLadderPrice, ultimateLadderPrice, ladderCollectorPrice;
    [DataMember]
    internal SyncedEntry<bool> isTinyLadderAlwaysActive, isBigLadderAlwaysActive, isHugeLadderAlwaysActive, isUltimateLadderAlwaysActive;
    [DataMember]
    internal SyncedEntry<bool> isAutoCollectLaddersEnabled;
    [DataMember]
    internal SyncedEntry<float> teleportFrequency;


    [DataMember]
    internal SyncedEntry<bool> isSalesFixEasyActive, isSalesFixTerminalActive, isDontFix;
    internal ConfigEntry<string> salesFixHeader;

    private static ManualLogSource mlsConfig = Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID + ".Config");

    internal MySyncedConfigs(ConfigFile cfg) : base(MyPluginInfo.PLUGIN_NAME)
    {
        ConfigManager.Register(this);

        //itemsEnabled
        isTinyLadderEnabled = cfg.BindSyncedEntry("DeactivateLadders", "isTinyLadderEnabled", true, "Tiny ladder doesn't appear in the shop if this is set to false.");
        isBigLadderEnabled = cfg.BindSyncedEntry("DeactivateLadders", "isBigLadderEnabled", true, "Big ladder doesn't appear in the shop if this is set to false.");
        isHugeLadderEnabled = cfg.BindSyncedEntry("DeactivateLadders", "isHugeLadderEnabled", true, "Huge ladder doesn't appear in the shop if this is set to false.");
        isUltimateLadderEnabled = cfg.BindSyncedEntry("DeactivateLadders", "isUltimateLadderEnabled", true, "Ultimate ladder doesn't appear in the shop if this is set to false.");
        isLadderCollectorEnabled = cfg.BindSyncedEntry("DeactivateLadders", "isLadderCollectorEnabled", true, "Ladder Collector doesn't appear in the shop if this is set to false.");

        //itemPrices
        tinyLadderPrice = cfg.BindSyncedEntry("LadderPrices", "tinyLadderPrice", tinyLadderBasePrice, "Sets the price of the tiny ladder.");
        bigLadderPrice = cfg.BindSyncedEntry("LadderPrices", "bigLadderPrice", bigLadderBasePrice, "Sets the price of the big ladder.");
        hugeLadderPrice = cfg.BindSyncedEntry("LadderPrices", "hugeLadderPrice", hugeLadderBasePrice, "Sets the price of the huge ladder.");
        ultimateLadderPrice = cfg.BindSyncedEntry("LadderPrices", "ultimateLadderPrice", ultimateLadderBasePrice, "Sets the price of the ultimate ladder.");
        ladderCollectorPrice = cfg.BindSyncedEntry("LadderPrices", "ladderCollectorItemPrice", ladderCollectorBasePrice, "Sets the price of the ladder collector item.");

        //ladderExtTime always active
        isTinyLadderAlwaysActive = cfg.BindSyncedEntry("LadderAlwaysActive", "isTinyLadderAlwaysActive", false, "Sets the tiny ladder to always being extended.");
        isBigLadderAlwaysActive = cfg.BindSyncedEntry("LadderAlwaysActive", "isBigLadderAlwaysActive", false, "Sets the big ladder to always being extended.");
        isHugeLadderAlwaysActive = cfg.BindSyncedEntry("LadderAlwaysActive", "isHugeLadderAlwaysActive", false, "Sets the huge ladder to always being extended.");
        isUltimateLadderAlwaysActive = cfg.BindSyncedEntry("LadderAlwaysActive", "isUltimateLadderAlwaysActive", false, "Sets the ultimate ladder to always being extended.");

        //ladderExtTime
        tinyLadderExtTime = cfg.BindSyncedEntry("LadderExtensionTime", "tinyLadderExtensionTime", tinyLadderExtensionTimeBase, "Sets the amount of seconds the tiny ladder stays extended.");
        bigLadderExtTime = cfg.BindSyncedEntry("LadderExtensionTime", "bigLadderExtensionTime", bigLadderExtensionTimeBase, "Sets the amount of seconds the big ladder stays extended.");
        hugeLadderExtTime = cfg.BindSyncedEntry("LadderExtensionTime", "hugeLadderExtensionTime", hugeLadderExtensionTimeBase, "Sets the amount of seconds the huge ladder stays extended");
        ultimateLadderExtTime = cfg.BindSyncedEntry("LadderExtensionTime", "ultimateLadderExtensionTime", ultimateLadderExtensionTimeBase, "Sets the amount of seconds the ultimate ladder stays extended.");
        
        //SalesFixes
        salesFixHeader = cfg.Bind("SalesBugfixMethod", "WhatIsThisConfigSection?", "(„• ᴗ •„) I am a happy placeholder!", "The sales bug is a relatively small bug that shifts sales by one slot occasionally. " + Environment.NewLine +
            "To fix this I have two solutions: the safe one, which will most likely always work and the experimental one, which looks way nicer in the terminal but could cause some issues.");
        isSalesFixEasyActive = cfg.BindSyncedEntry("SalesBugfixMethod", "safeSalesFix", true, "This will fix sales in a very safe way, but disabled ladders will appear as an item named " + '"' + LoadLadderConfigsPatch.DISABLED_ITEM_NAME + '"' + " in the store.");
        isSalesFixTerminalActive = cfg.BindSyncedEntry("SalesBugfixMethod", "experimentalSalesFix", false, "This will fix sales and fully remove disabled ladders from the store without " + '"' + LoadLadderConfigsPatch.DISABLED_ITEM_NAME + '"' + " being displayed." + Environment.NewLine +
            "WARNING: This might cause store related bugs or have influence on the compatibility with other mods!");
        isDontFix = cfg.BindSyncedEntry("SalesBugfixMethod", "dontFixSales", false, "This will not fix the sales, resulting in sales sometimes being displayed on an item which is not on sale (or the opposite of that).");

        //ladder collector specific
        isAutoCollectLaddersEnabled = cfg.BindSyncedEntry("LadderCollector", "enableAutoCollectLadders", false, "This will try to teleport all remaining ladders on the map to the ship when pulling the lever to leave the planet");
        teleportFrequency = cfg.BindSyncedEntry("LadderCollector", "ladderTeleportFrequency", teleportFrequencyBase, "Time between ladder-teleports (in seconds)");

        fixConfigs();
    }

    private void fixConfigs()
    {
        //prices
        if (tinyLadderPrice.Value < MIN_LADDER_PRICE)
        {
            tinyLadderPrice.Value = tinyLadderBasePrice;
            mlsConfig.LogWarning("big ladder price was too low, was set to basic value: " + tinyLadderBasePrice);
        }
        else if (tinyLadderPrice.Value > MAX_LADDER_PRICE)
        {
            tinyLadderPrice.Value = MAX_LADDER_PRICE;
            mlsConfig.LogWarning("big ladder price was too high, was set to max value: " + MAX_LADDER_PRICE);
        }

        if (bigLadderPrice.Value < MIN_LADDER_PRICE)
        {
            bigLadderPrice.Value = bigLadderBasePrice;
            mlsConfig.LogWarning("big ladder price was too low, was set to basic value: " + bigLadderBasePrice);
        }
        else if (bigLadderPrice.Value > MAX_LADDER_PRICE)
        {
            bigLadderPrice.Value = MAX_LADDER_PRICE;
            mlsConfig.LogWarning("big ladder price was too high, was set to max value: " + MAX_LADDER_PRICE);
        }

        if (hugeLadderPrice.Value < MIN_LADDER_PRICE)
        {
            hugeLadderPrice.Value = hugeLadderBasePrice;
            mlsConfig.LogWarning("huge ladder price was too low, was set to basic value: " + hugeLadderBasePrice);
        }
        else if (hugeLadderPrice.Value > MAX_LADDER_PRICE)
        {
            hugeLadderPrice.Value = MAX_LADDER_PRICE;
            mlsConfig.LogWarning("huge ladder price was too high, was set to max value: " + MAX_LADDER_PRICE);
        }

        if (ultimateLadderPrice.Value < MIN_LADDER_PRICE)
        {
            ultimateLadderPrice.Value = ultimateLadderBasePrice;
            mlsConfig.LogWarning("ultimate ladder price was too low, was set to basic value: " + ultimateLadderBasePrice);
        }
        else if (ultimateLadderPrice.Value > MAX_LADDER_PRICE)
        {
            ultimateLadderPrice.Value = MAX_LADDER_PRICE;
            mlsConfig.LogWarning("ultimate ladder price was too high, was set to max value: " + MAX_LADDER_PRICE);
        }

        if (ladderCollectorPrice.Value < MIN_LADDER_PRICE)
        {
            ladderCollectorPrice.Value = ladderCollectorBasePrice;
            mlsConfig.LogWarning("ladder collector price was too low, was set to basic value: " + ladderCollectorBasePrice);
        }
        else if (ladderCollectorPrice.Value > MAX_LADDER_PRICE)
        {
            ladderCollectorPrice.Value = MAX_LADDER_PRICE;
            mlsConfig.LogWarning("ladder collector price was too high, was set to max value: " + MAX_LADDER_PRICE);
        }

        //ext-time
        if (tinyLadderExtTime < MIN_EXT_TIME)
        {
            tinyLadderExtTime.Value = tinyLadderExtensionTimeBase;
            mlsConfig.LogWarning("big ladder extension time was too low, was set to basic value: " + tinyLadderExtensionTimeBase);
        }
        else if (tinyLadderExtTime.Value > MAX_EXT_TIME)
        {
            tinyLadderExtTime.Value = MAX_EXT_TIME;
            mlsConfig.LogWarning("big ladder extension time was too high, was set to max value: " + MAX_EXT_TIME);
            mlsConfig.LogWarning("Values over 660 already last longer than a day");
        }

        if (bigLadderExtTime.Value < MIN_EXT_TIME)
        {
            bigLadderExtTime.Value = bigLadderExtensionTimeBase;
            mlsConfig.LogWarning("big ladder extension time was too low, was set to basic value: " + bigLadderExtensionTimeBase);
        }
        else if (bigLadderExtTime.Value > MAX_EXT_TIME)
        {
            bigLadderExtTime.Value = MAX_EXT_TIME;
            mlsConfig.LogWarning("big ladder extension time was too high, was set to max value: " + MAX_EXT_TIME);
            mlsConfig.LogWarning("Values over 660 already last longer than a day");
        }

        if (hugeLadderExtTime.Value < MIN_EXT_TIME)
        {
            hugeLadderExtTime.Value = hugeLadderExtensionTimeBase;
            mlsConfig.LogWarning("huge ladder extension time was too low, was set to basic value: " + hugeLadderExtensionTimeBase);
        }
        else if (hugeLadderExtTime.Value > MAX_EXT_TIME)
        {
            hugeLadderExtTime.Value = MAX_EXT_TIME;
            mlsConfig.LogWarning("huge ladder extension time was too high, was set to max value: " + MAX_EXT_TIME);
            mlsConfig.LogWarning("Values over 660 already last longer than a day");
        }

        if (ultimateLadderExtTime.Value < MIN_EXT_TIME)
        {
            ultimateLadderExtTime.Value = ultimateLadderExtensionTimeBase;
            mlsConfig.LogWarning("huge ladder extension time was too low, was set to basic value: " + ultimateLadderExtensionTimeBase);
        }
        else if (ultimateLadderExtTime.Value > MAX_EXT_TIME)
        {
            ultimateLadderExtTime.Value = MAX_EXT_TIME;
            mlsConfig.LogWarning("huge ladder extension time was too high, was set to max value: " + MAX_EXT_TIME);
            mlsConfig.LogWarning("Values over 660 already last longer than a day");
        }

        //sales fix
        if (isDontFix)
        {
            isSalesFixEasyActive.Value = false;
            isSalesFixTerminalActive.Value = false;
        }

        if (isSalesFixEasyActive && isSalesFixTerminalActive)
        {
            isSalesFixTerminalActive.Value = false;
        }

        //ladder collector
        if (teleportFrequency <= 0)
        {
            mlsConfig.LogWarning("teleport frequency time was too low, was set to base value: " + teleportFrequencyBase);
            teleportFrequency.Value = teleportFrequencyBase;
        }
    }
}
