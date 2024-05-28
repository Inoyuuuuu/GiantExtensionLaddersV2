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
    internal SyncedEntry<bool> isTinyLadderEnabled, isBigLadderEnabled, isHugeLadderEnabled, isUltimateLadderEnabled;
    [DataMember]
    internal SyncedEntry<float> tinyLadderExtTime, bigLadderExtTime, hugeLadderExtTime, ultimateLadderExtTime;
    [DataMember]
    internal SyncedEntry<int> tinyLadderPrice, bigLadderPrice, hugeLadderPrice, ultimateLadderPrice;
    [DataMember]
    internal SyncedEntry<bool> isTinyLadderAlwaysActive, isBigLadderAlwaysActive, isHugeLadderAlwaysActive, isUltimateLadderAlwaysActive;

    [DataMember]
    internal SyncedEntry<bool> isSalesFixEasyActive, isSalesFixTerminalActive, isDontFix;
    internal ConfigEntry<string> salesFixHeader;

    private static ManualLogSource mlsConfig = Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID + ".Config");

    internal MySyncedConfigs(ConfigFile cfg) : base(MyPluginInfo.PLUGIN_NAME)
    {
        ConfigManager.Register(this);

        //laddersActive
        isTinyLadderEnabled = cfg.BindSyncedEntry("DeactivateLadders", "isTinyLadderEnabled", true, "Tiny ladder doesn't appear in the shop if this is set to false.");
        isBigLadderEnabled = cfg.BindSyncedEntry("DeactivateLadders", "isBigLadderEnabled", true, "Big ladder doesn't appear in the shop if this is set to false.");
        isHugeLadderEnabled = cfg.BindSyncedEntry("DeactivateLadders", "isHugeLadderEnabled", true, "Huge ladder doesn't appear in the shop if this is set to false.");
        isUltimateLadderEnabled = cfg.BindSyncedEntry("DeactivateLadders", "isUltimateLadderEnabled", true, "Ultimate ladder doesn't appear in the shop if this is set to false.");

        //ladderPrices
        tinyLadderPrice = cfg.BindSyncedEntry("LadderPrices", "tinyLadderPrice", tinyLadderBasePrice, "Sets the price of the tiny ladder");
        bigLadderPrice = cfg.BindSyncedEntry("LadderPrices", "bigLadderPrice", bigLadderBasePrice, "Sets the price of the big ladder");
        hugeLadderPrice = cfg.BindSyncedEntry("LadderPrices", "hugeLadderPrice", hugeLadderBasePrice, "Sets the price of the huge ladder");
        ultimateLadderPrice = cfg.BindSyncedEntry("LadderPrices", "ultimateLadderPrice", ultimateLadderBasePrice, "Sets the price of the ultimate ladder");

        //ladderExtTime
        tinyLadderExtTime = cfg.BindSyncedEntry("LadderExtensionTime", "tinyLadderExtensionTime", tinyLadderExtensionTimeBase, "Sets the amount of seconds the tiny ladder stays extended.");
        bigLadderExtTime = cfg.BindSyncedEntry("LadderExtensionTime", "bigLadderExtensionTime", bigLadderExtensionTimeBase, "Sets the amount of seconds the big ladder stays extended.");
        hugeLadderExtTime = cfg.BindSyncedEntry("LadderExtensionTime", "hugeLadderExtensionTime", hugeLadderExtensionTimeBase, "Sets the amount of seconds the huge ladder stays extended");
        ultimateLadderExtTime = cfg.BindSyncedEntry("LadderExtensionTime", "ultimateLadderExtensionTime", ultimateLadderExtensionTimeBase, "Sets the amount of seconds the ultimate ladder stays extended");

        //ladderExtTime always active
        isTinyLadderAlwaysActive = cfg.BindSyncedEntry("LadderExtensionTime", "isTinyLadderAlwaysActive", false, "Sets the tiny ladder to always being extended.");
        isBigLadderAlwaysActive = cfg.BindSyncedEntry("LadderExtensionTime", "isBigLadderAlwaysActive", false, "Sets the big ladder to always being extended.");
        isHugeLadderAlwaysActive = cfg.BindSyncedEntry("LadderExtensionTime", "isHugeLadderAlwaysActive", false, "Sets the huge ladder to always being extended.");
        isUltimateLadderAlwaysActive = cfg.BindSyncedEntry("LadderExtensionTime", "isUltimateLadderAlwaysActive", false, "Sets the ultimate ladder to always being extended.");

        //SalesFixes
        salesFixHeader = cfg.Bind("SalesBugfixMethod", "SalesBugfixMethod", "(„• ᴗ •„) I'm a happy placeholder!", "THE FOLLOWING CONFIGS ARE NOT INITIALLY SYNCED! So make sure you all have the same settings (or installed the mods through the same modmanager-code), or restart the game after all joining the same lobby once. " + Environment.NewLine +
            "The three settings determine the method used for fixing the sales of the in-game store.");
        isSalesFixEasyActive = cfg.BindSyncedEntry("SalesBugfixMethod", "use_safe_sales_fix", true, "This will fix sales, but disabled ladders will appear as an item named " + '"' + LoadLadderConfigsPatch.DISABLED_LADDER_NAME + '"' + " in the shop.");
        isSalesFixTerminalActive = cfg.BindSyncedEntry("SalesBugfixMethod", "use_experimental_sales_fix", false, "This will fix sales and fully remove disabled ladders. This might cause some store related bugs and could cause other mods to malfunction!");
        isDontFix = cfg.BindSyncedEntry("SalesBugfixMethod", "dont_fix_sales", false, "This will not fix the sales, resulting in sales sometimes being displayed on a item which is not on sale.");

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
            mlsConfig.LogWarning("big ladder price was too high, was set to max value: " + MAX_EXT_TIME);
        }

        if (bigLadderPrice.Value < MIN_LADDER_PRICE)
        {
            bigLadderPrice.Value = bigLadderBasePrice;
            mlsConfig.LogWarning("big ladder price was too low, was set to basic value: " + bigLadderBasePrice);
        }
        else if (bigLadderPrice.Value > MAX_LADDER_PRICE)
        {
            bigLadderPrice.Value = MAX_LADDER_PRICE;
            mlsConfig.LogWarning("big ladder price was too high, was set to max value: " + MAX_EXT_TIME);
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
    }
}
