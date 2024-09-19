using BepInEx.Configuration;
using BepInEx.Logging;
using CSync.Extensions;
using CSync.Lib;
using GiantExtensionLaddersV2.Patches;
using System;
using System.Runtime.Serialization;


namespace GiantExtensionLaddersV2.ConfigStuff;

[DataContract]
internal class MySyncedConfigs : SyncedConfig2<MySyncedConfigs>
{
    private const int MIN_LADDER_PRICE = 1;
    private const int MAX_LADDER_PRICE = 9999;
    private const float MIN_EXT_TIME = 3f;
    private const float MAX_EXT_TIME = 999f;
    private const float MIN_TP_FREQ = 1;
    private const float MAX_TP_FREQ = 60;

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

    [SyncedEntryField]
    internal SyncedEntry<bool> isTinyLadderEnabled, isBigLadderEnabled, isHugeLadderEnabled, isUltimateLadderEnabled, isLadderCollectorEnabled;
    [SyncedEntryField]
    internal SyncedEntry<float> tinyLadderExtTime, bigLadderExtTime, hugeLadderExtTime, ultimateLadderExtTime;
    [SyncedEntryField]
    internal SyncedEntry<int> tinyLadderPrice, bigLadderPrice, hugeLadderPrice, ultimateLadderPrice, ladderCollectorPrice;
    [SyncedEntryField]
    internal SyncedEntry<bool> isTinyLadderAlwaysActive, isBigLadderAlwaysActive, isHugeLadderAlwaysActive, isUltimateLadderAlwaysActive;
    [SyncedEntryField]
    internal SyncedEntry<bool> isAutoCollectLaddersEnabled, isTeleportFromShipRoomEnabled, isCollectExtendedLaddersEnabled;
    [SyncedEntryField]
    internal SyncedEntry<float> teleportFrequency;


    [SyncedEntryField]
    internal SyncedEntry<bool> isSalesFixEasyActive, isSalesFixTerminalActive, isDontFix;
    internal ConfigEntry<string> salesFixHeader;

    private static ManualLogSource mlsConfig = Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID + ".Config");

    internal MySyncedConfigs(ConfigFile cfg) : base(MyPluginInfo.PLUGIN_NAME)
    {

        //itemsEnabled
        isTinyLadderEnabled = cfg.BindSyncedEntry("DeactivateLadders", "isTinyLadderEnabled", true, 
            "Tiny ladder doesn't appear in the shop if this is set to false.");
        isBigLadderEnabled = cfg.BindSyncedEntry("DeactivateLadders", "isBigLadderEnabled", true, 
            "Big ladder doesn't appear in the shop if this is set to false.");
        isHugeLadderEnabled = cfg.BindSyncedEntry("DeactivateLadders", "isHugeLadderEnabled", true, 
            "Huge ladder doesn't appear in the shop if this is set to false.");
        isUltimateLadderEnabled = cfg.BindSyncedEntry("DeactivateLadders", "isUltimateLadderEnabled", true, 
            "Ultimate ladder doesn't appear in the shop if this is set to false.");
        isLadderCollectorEnabled = cfg.BindSyncedEntry("DeactivateLadders", "isLadderCollectorEnabled", true, 
            "Ladder Collector doesn't appear in the shop if this is set to false.");

        //itemPrices
        AcceptableValueRange<int> acceptablePriceRange = new AcceptableValueRange<int>(MIN_LADDER_PRICE, MAX_LADDER_PRICE);
        
        tinyLadderPrice = cfg.BindSyncedEntry("LadderPrices", "tinyLadderPrice", tinyLadderBasePrice, 
            new ConfigDescription("Sets the price of the tiny ladder.", acceptablePriceRange));
        bigLadderPrice = cfg.BindSyncedEntry("LadderPrices", "bigLadderPrice", bigLadderBasePrice, 
            new ConfigDescription("Sets the price of the big ladder.", acceptablePriceRange));
        hugeLadderPrice = cfg.BindSyncedEntry("LadderPrices", "hugeLadderPrice", hugeLadderBasePrice, 
            new ConfigDescription("Sets the price of the huge ladder.", acceptablePriceRange));
        ultimateLadderPrice = cfg.BindSyncedEntry("LadderPrices", "ultimateLadderPrice", ultimateLadderBasePrice, 
            new ConfigDescription("Sets the price of the ultimate ladder.", acceptablePriceRange));
        ladderCollectorPrice = cfg.BindSyncedEntry("LadderPrices", "ladderCollectorItemPrice", ladderCollectorBasePrice, 
            new ConfigDescription("Sets the price of ladder collector.", acceptablePriceRange));

        //ladderExtTime always active
        isTinyLadderAlwaysActive = cfg.BindSyncedEntry("LadderAlwaysActive", "isTinyLadderAlwaysActive", false, 
            "Sets the tiny ladder to always being extended.");
        isBigLadderAlwaysActive = cfg.BindSyncedEntry("LadderAlwaysActive", "isBigLadderAlwaysActive", false, 
            "Sets the big ladder to always being extended.");
        isHugeLadderAlwaysActive = cfg.BindSyncedEntry("LadderAlwaysActive", "isHugeLadderAlwaysActive", false, 
            "Sets the huge ladder to always being extended.");
        isUltimateLadderAlwaysActive = cfg.BindSyncedEntry("LadderAlwaysActive", "isUltimateLadderAlwaysActive", false, 
            "Sets the ultimate ladder to always being extended.");

        //ladderExtTime
        AcceptableValueRange<float> acceptableExtensionTimes = new AcceptableValueRange<float>(MIN_EXT_TIME, MAX_EXT_TIME);

        tinyLadderExtTime = cfg.BindSyncedEntry("LadderExtensionTime", "tinyLadderExtensionTime", tinyLadderExtensionTimeBase,
            new ConfigDescription("Amount of seconds the tiny ladder stays extended.", acceptableExtensionTimes));
        bigLadderExtTime = cfg.BindSyncedEntry("LadderExtensionTime", "bigLadderExtensionTime", bigLadderExtensionTimeBase,
            new ConfigDescription("Amount of seconds the big ladder stays extended.", acceptableExtensionTimes));
        hugeLadderExtTime = cfg.BindSyncedEntry("LadderExtensionTime", "hugeLadderExtensionTime", hugeLadderExtensionTimeBase,
            new ConfigDescription("Amount of seconds the huge ladder stays extended.", acceptableExtensionTimes));
        ultimateLadderExtTime = cfg.BindSyncedEntry("LadderExtensionTime", "ultimateLadderExtensionTime", ultimateLadderExtensionTimeBase,
            new ConfigDescription("Amount of seconds the ultimate ladder stays extended.", acceptableExtensionTimes));

        //SalesFixes
        salesFixHeader = cfg.Bind("SalesBugfixMethod", "WhatIsThisConfigSection?", "(„• ᴗ •„) I am a happy placeholder!", "The sales bug is a relatively small bug that shifts sales by one slot occasionally. " + Environment.NewLine +
            "To fix this I have two solutions: the safe one, which will most likely always work and the experimental one, which looks way nicer in the terminal but could cause some issues.");
        isSalesFixEasyActive = cfg.BindSyncedEntry("SalesBugfixMethod", "safeSalesFix", true, 
            "This will fix sales in a very safe way, but disabled ladders will appear as an item named " + '"' + LoadLadderConfigsPatch.DISABLED_ITEM_NAME + '"' + " in the store.");
        isSalesFixTerminalActive = cfg.BindSyncedEntry("SalesBugfixMethod", "experimentalSalesFix", false, "This will fix sales and fully remove disabled ladders from the store without " + '"' + LoadLadderConfigsPatch.DISABLED_ITEM_NAME + '"' + " being displayed." + Environment.NewLine +
            "WARNING: This might cause store related bugs or have influence on the compatibility with other mods!");
        isDontFix = cfg.BindSyncedEntry("SalesBugfixMethod", "dontFixSales", false, 
            "This will not fix the sales, resulting in sales sometimes being displayed on an item which is not on sale (or the opposite of that).");

        //ladder collector specific
        isAutoCollectLaddersEnabled = cfg.BindSyncedEntry("LadderCollector", "enableAutoCollectLadders", false, 
            "This will try to teleport all remaining ladders on the map to the ship when pulling the lever to leave the planet.");
        isTeleportFromShipRoomEnabled = cfg.BindSyncedEntry("LadderCollector", "allowTeleportFromShipRoom", false, 
            "This allows the ladder collector to teleport ladders that are in the ship room (or very close to it).");
        teleportFrequency = cfg.BindSyncedEntry("LadderCollector", "ladderTeleportFrequency", teleportFrequencyBase, 
            new ConfigDescription("Time between ladder-teleports (in seconds)", new AcceptableValueRange<float>(MIN_TP_FREQ, MAX_TP_FREQ)));
        isCollectExtendedLaddersEnabled = cfg.BindSyncedEntry("LadderCollector", "collectExtendedLadders", true, 
            "This allows the ladder collector to teleport ladders that are extended.");

        ConfigManager.Register(this);
    }
}
