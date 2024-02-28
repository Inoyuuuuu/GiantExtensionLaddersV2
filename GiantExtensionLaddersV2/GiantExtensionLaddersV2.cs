using BepInEx;
using BepInEx.Logging;
using GiantExtensionLaddersV2.Behaviours;
using GiantExtensionLaddersV2.ConfigStuff;
using HarmonyLib;
using LethalLib.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GiantExtensionLaddersV2
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("io.github.CSync", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("evaisa.lethallib", BepInDependency.DependencyFlags.HardDependency)]
    public class GiantExtensionLaddersV2 : BaseUnityPlugin
    {
        //------- configs
        public static new MySyncedConfigs Config { get; private set; }

        //------- constants
        private const string tinyLadderAssetbundleName = "TinyLadderAssets";
        private const string bigLadderAssetbundleName = "BigLadderAssets";
        private const string hugeLadderAssetbundleName = "HugeLadderAssets";
        private const string tinyLadderItemPropertiesLocation = "Assets/extLadderTest/tinyLadder/ExtensionLadder_0.asset";
        private const string bigLadderItemPropertiesLocation = "Assets/extLadderTest/lcLadder/ExtensionLadder_0.asset";
        private const string hugeLadderItemPropertiesLocation = "Assets/extLadderTest/newLongerLadder/ExtensionLadder_0.asset";
        private const int MAX_PROPERTY_AMOUNT = 18;

        internal static int propertyCounter = 0;

        //------- configs TinyLadder
        private const int TINY_LADDER_ALARM_DURATION = 4;
        private const float TINY_LADDER_MAX_EXTENSION = 10.3f;
        private const float TINY_LADDER_MIN_ROTATION_COLLISION = 60f;
        private const int TINY_LADDER_LINECAST_CHECKS_MULTIPLIER = 2;
        private const int TINY_LADDER_LINECAST_MIN_CHECK_HEIGHT = 2;
        private const float TINY_LADDER_HEIGHT_MULTIPLIER = TINY_LADDER_MAX_EXTENSION / 2.43f; //this is for line 272 in BigLadderScript, where 2.43 * x = ladder height
        private const float TINY_LADDER_ROTATE_SPEED = 2f;
        private const bool TINY_LADDER_IS_CLIMBABLE = false;

        //------- configs BigLadder
        private const int BIG_LADDER_ALARM_DURATION = 5;
        private const float BIG_LADDER_MAX_EXTENSION = 17f;
        private const float BIG_LADDER_MIN_ROTATION_COLLISION = 60f;
        private const int BIG_LADDER_LINECAST_CHECKS_MULTIPLIER = 3;
        private const int BIG_LADDER_LINECAST_MIN_CHECK_HEIGHT = 8;
        private const float BIG_LADDER_HEIGHT_MULTIPLIER = 7f;
        private const float BIG_LADDER_ROTATE_SPEED = 2f;
        private const bool BIG_LADDER_IS_CLIMBABLE = true;

        //------- configs HugeLadder
        private const int HUGE_LADDER_ALARM_DURATION = 5;
        private const float HUGE_LADDER_MAX_EXTENSION = 34.4f;
        private const float HUGE_LADDER_MIN_ROTATION_COLLISION = 60f;
        private const int HUGE_LADDER_LINECAST_CHECKS_MULTIPLIER = 4;
        private const int HUGE_LADDER_LINECAST_MIN_CHECK_HEIGHT = 8;
        private const float HUGE_LADDER_HEIGHT_MULTIPLIER = 14.15f;
        private const float HUGE_LADDER_ROTATE_SPEED = 2f;
        private const bool HUGE_LADDER_IS_CLIMBABLE = true;

        //------- lists for TinyLadderScript properties
        internal static List<MeshRenderer> tinyLadderMeshRenderers = new List<MeshRenderer>();
        internal static List<Animator> tinyLadderAnimators = new List<Animator>();
        internal static List<Transform> tinyLadderTransforms = new List<Transform>();
        internal static List<AudioClip> tinyLadderAudioClips = new List<AudioClip>();
        internal static List<AudioSource> tinyLadderAudioSources = new List<AudioSource>();
        internal static List<InteractTrigger> tinyLadderInteractTriggers = new List<InteractTrigger>();
        internal static List<BoxCollider> tinyLadderBoxColliders = new List<BoxCollider>();

        //------- lists for BigLadderScript properties
        internal static List<MeshRenderer> bigLadderMeshRenderers = new List<MeshRenderer>();
        internal static List<Animator> bigLadderAnimators = new List<Animator>();
        internal static List<Transform> bigLadderTransforms = new List<Transform>();
        internal static List<AudioClip> bigLadderAudioClips = new List<AudioClip>();
        internal static List<AudioSource> bigLadderAudioSources = new List<AudioSource>();
        internal static List<InteractTrigger> bigLadderInteractTriggers = new List<InteractTrigger>();
        internal static List<BoxCollider> bigLadderBoxColliders = new List<BoxCollider>();

        //------- lists for HugeLadderScript properties
        internal static List<MeshRenderer> hugeLadderMeshRenderers = new List<MeshRenderer>();
        internal static List<Animator> hugeLadderAnimators = new List<Animator>();
        internal static List<Transform> hugeLadderTransforms = new List<Transform>();
        internal static List<AudioClip> hugeLadderAudioClips = new List<AudioClip>();
        internal static List<AudioSource> hugeLadderAudioSources = new List<AudioSource>();
        internal static List<InteractTrigger> hugeLadderInteractTriggers = new List<InteractTrigger>();
        internal static List<BoxCollider> hugeLadderBoxColliders = new List<BoxCollider>();

        public static GiantExtensionLaddersV2 Instance { get; private set; } = null!;
        internal new static ManualLogSource mls { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }

        private void Awake()
        {
            mls = base.Logger;
            Instance = this;

            Config = new MySyncedConfigs(base.Config);

            mls.LogInfo("loading assetbundles");
            string tinyLadderAssetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), tinyLadderAssetbundleName);
            string bigLadderAssetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), bigLadderAssetbundleName);
            string hugeLadderAssetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), hugeLadderAssetbundleName);
            AssetBundle tinyLadderBundle = AssetBundle.LoadFromFile(tinyLadderAssetDir);
            AssetBundle bigLadderBundle = AssetBundle.LoadFromFile(bigLadderAssetDir);
            AssetBundle hugeLadderBundle = AssetBundle.LoadFromFile(hugeLadderAssetDir);

            if (bigLadderBundle != null && hugeLadderBundle != null && tinyLadderBundle != null)
            {
                mls.LogInfo("successfully loaded two assetbundles from files");
            }
            else
            {
                mls.LogInfo("failed to load assetbundles");
            }

            mls.LogInfo("loading items from assetbundles");
            Item tinyLadderItem = tinyLadderBundle.LoadAsset<Item>(tinyLadderItemPropertiesLocation);
            Item bigLadderItem = bigLadderBundle.LoadAsset<Item>(bigLadderItemPropertiesLocation);
            Item hugeLadderItem = hugeLadderBundle.LoadAsset<Item>(hugeLadderItemPropertiesLocation);

            if (bigLadderItem != null && hugeLadderItem != null && tinyLadderItem != null)
            {
                mls.LogInfo("successfully loaded items from assetbundles");
            }
            else
            {
                mls.LogInfo("failed to load items from item assetbundles");
            }

            GameObject tinyLadderItemPrefab = tinyLadderItem.spawnPrefab;
            GameObject bigLadderItemPrefab = bigLadderItem.spawnPrefab;
            GameObject hugeLadderItemPrefab = hugeLadderItem.spawnPrefab;

            //----- build tiny ladder item
            mls.LogInfo("attempting to build the tiny ladder");
            tinyLadderMeshRenderers = tinyLadderItemPrefab.GetComponentsInChildren<MeshRenderer>().ToList();
            tinyLadderAnimators = tinyLadderItemPrefab.GetComponentsInChildren<Animator>().ToList();
            tinyLadderTransforms = tinyLadderItemPrefab.GetComponentsInChildren<Transform>().ToList();
            tinyLadderAudioClips = tinyLadderBundle.LoadAllAssets<AudioClip>().ToList();
            tinyLadderAudioSources = tinyLadderItemPrefab.GetComponentsInChildren<AudioSource>().ToList();
            tinyLadderInteractTriggers = tinyLadderItemPrefab.GetComponentsInChildren<InteractTrigger>().ToList();
            tinyLadderBoxColliders = tinyLadderItemPrefab.GetComponentsInChildren<BoxCollider>().ToList();

            LadderItemScript tinyLadderScript = tinyLadderItemPrefab.AddComponent<LadderItemScript>();

            if (tinyLadderItemPrefab.GetComponent<LadderItemScript>() == null)
            {
                mls.LogInfo("tinyLadderItemPrefab failed");
            }
            else
            {
                mls.LogInfo("tinyLadderItemPrefab okay");
            }

            tinyLadderScript.grabbable = true;
            tinyLadderScript.grabbableToEnemies = true;
            tinyLadderScript.itemProperties = tinyLadderItem;

            //------- custom properties
            tinyLadderScript.ladderAlarmTime = ConfigStuff.MySyncedConfigs.Instance.TINY_LADDER_EXT_TIME - TINY_LADDER_ALARM_DURATION;
            tinyLadderScript.ladderExtensionTime = ConfigStuff.MySyncedConfigs.Instance.TINY_LADDER_EXT_TIME;
            tinyLadderScript.maxExtension = TINY_LADDER_MAX_EXTENSION;
            tinyLadderScript.minRotationCollisionCheck = TINY_LADDER_MIN_ROTATION_COLLISION;
            tinyLadderScript.linecastChecksMultiplier = TINY_LADDER_LINECAST_CHECKS_MULTIPLIER;
            tinyLadderScript.linecastMinCheckHeight = TINY_LADDER_LINECAST_MIN_CHECK_HEIGHT;
            tinyLadderScript.ladderHeightMultiplier = TINY_LADDER_HEIGHT_MULTIPLIER;
            tinyLadderScript.ladderRotateSpeedMultiplier = TINY_LADDER_ROTATE_SPEED;
            tinyLadderScript.isClimbable = TINY_LADDER_IS_CLIMBABLE;

            buildLadderItem(tinyLadderMeshRenderers, tinyLadderAnimators, tinyLadderTransforms, tinyLadderAudioClips, tinyLadderAudioSources,
                tinyLadderInteractTriggers, tinyLadderBoxColliders, tinyLadderScript);

            //----- build big ladder item
            mls.LogInfo("attempting to build the big ladder");
            bigLadderMeshRenderers = bigLadderItemPrefab.GetComponentsInChildren<MeshRenderer>().ToList();
            bigLadderAnimators = bigLadderItemPrefab.GetComponentsInChildren<Animator>().ToList();
            bigLadderTransforms = bigLadderItemPrefab.GetComponentsInChildren<Transform>().ToList();
            bigLadderAudioClips = bigLadderBundle.LoadAllAssets<AudioClip>().ToList();
            bigLadderAudioSources = bigLadderItemPrefab.GetComponentsInChildren<AudioSource>().ToList();
            bigLadderInteractTriggers = bigLadderItemPrefab.GetComponentsInChildren<InteractTrigger>().ToList();
            bigLadderBoxColliders = bigLadderItemPrefab.GetComponentsInChildren<BoxCollider>().ToList();

            LadderItemScript bigLadderScript = bigLadderItemPrefab.AddComponent<LadderItemScript>();
            if (bigLadderItemPrefab.GetComponent<LadderItemScript>() == null)
            {
                mls.LogInfo("bigLadderScript failed");
            }
            else
            {
                mls.LogInfo("bigLadderScript okay");
            }

            bigLadderScript.grabbable = true;
            bigLadderScript.grabbableToEnemies = true;
            bigLadderScript.itemProperties = bigLadderItem;

            //------- custom properties
            bigLadderScript.ladderAlarmTime = ConfigStuff.MySyncedConfigs.Instance.BIG_LADDER_EXT_TIME - BIG_LADDER_ALARM_DURATION;
            bigLadderScript.ladderExtensionTime = ConfigStuff.MySyncedConfigs.Instance.BIG_LADDER_EXT_TIME;
            bigLadderScript.maxExtension = BIG_LADDER_MAX_EXTENSION;
            bigLadderScript.minRotationCollisionCheck = BIG_LADDER_MIN_ROTATION_COLLISION;
            bigLadderScript.linecastChecksMultiplier = BIG_LADDER_LINECAST_CHECKS_MULTIPLIER;
            bigLadderScript.linecastMinCheckHeight = BIG_LADDER_LINECAST_MIN_CHECK_HEIGHT;
            bigLadderScript.ladderHeightMultiplier = BIG_LADDER_HEIGHT_MULTIPLIER;
            bigLadderScript.ladderRotateSpeedMultiplier = BIG_LADDER_ROTATE_SPEED;
            bigLadderScript.isClimbable = BIG_LADDER_IS_CLIMBABLE;

            buildLadderItem(bigLadderMeshRenderers, bigLadderAnimators, bigLadderTransforms, bigLadderAudioClips, bigLadderAudioSources,
                bigLadderInteractTriggers, bigLadderBoxColliders, bigLadderScript);

            //----- build huge ladder item
            mls.LogInfo("attempting to build the huge ladder");
            hugeLadderMeshRenderers = hugeLadderItemPrefab.GetComponentsInChildren<MeshRenderer>().ToList();
            hugeLadderAnimators = hugeLadderItemPrefab.GetComponentsInChildren<Animator>().ToList();
            hugeLadderTransforms = hugeLadderItemPrefab.GetComponentsInChildren<Transform>().ToList();
            hugeLadderAudioClips = hugeLadderBundle.LoadAllAssets<AudioClip>().ToList();
            hugeLadderAudioSources = hugeLadderItemPrefab.GetComponentsInChildren<AudioSource>().ToList();
            hugeLadderInteractTriggers = hugeLadderItemPrefab.GetComponentsInChildren<InteractTrigger>().ToList();
            hugeLadderBoxColliders = hugeLadderItemPrefab.GetComponentsInChildren<BoxCollider>().ToList();

            LadderItemScript hugeLadderScript = hugeLadderItemPrefab.AddComponent<LadderItemScript>();
            if (hugeLadderItemPrefab.GetComponent<LadderItemScript>() == null)
            {
                mls.LogInfo("hugeLadderScript failed");
            }
            else
            {
                mls.LogInfo("hugeLadderScript okay");
            }

            hugeLadderScript.grabbable = true;
            hugeLadderScript.grabbableToEnemies = true;
            hugeLadderScript.itemProperties = hugeLadderItem;

            //------- custom properties
            hugeLadderScript.ladderAlarmTime = ConfigStuff.MySyncedConfigs.Instance.HUGE_LADDER_EXT_TIME - HUGE_LADDER_ALARM_DURATION;
            hugeLadderScript.ladderExtensionTime = ConfigStuff.MySyncedConfigs.Instance.HUGE_LADDER_EXT_TIME;
            hugeLadderScript.maxExtension = HUGE_LADDER_MAX_EXTENSION;
            hugeLadderScript.minRotationCollisionCheck = HUGE_LADDER_MIN_ROTATION_COLLISION;
            hugeLadderScript.linecastChecksMultiplier = HUGE_LADDER_LINECAST_CHECKS_MULTIPLIER;
            hugeLadderScript.linecastMinCheckHeight = HUGE_LADDER_LINECAST_MIN_CHECK_HEIGHT;
            hugeLadderScript.ladderHeightMultiplier = HUGE_LADDER_HEIGHT_MULTIPLIER;
            hugeLadderScript.ladderRotateSpeedMultiplier = HUGE_LADDER_ROTATE_SPEED;
            hugeLadderScript.isClimbable = HUGE_LADDER_IS_CLIMBABLE;

            buildLadderItem(hugeLadderMeshRenderers, hugeLadderAnimators, hugeLadderTransforms, hugeLadderAudioClips, hugeLadderAudioSources,
                hugeLadderInteractTriggers, hugeLadderBoxColliders, hugeLadderScript);

            //-------- register items
            NetworkPrefabs.RegisterNetworkPrefab(tinyLadderItem.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(bigLadderItem.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(hugeLadderItem.spawnPrefab);
            mls.LogInfo("registered network prefabs");

            Utilities.FixMixerGroups(tinyLadderItem.spawnPrefab);
            Utilities.FixMixerGroups(bigLadderItem.spawnPrefab);
            Utilities.FixMixerGroups(hugeLadderItem.spawnPrefab);
            mls.LogInfo("fixed mixers");

            if (ConfigStuff.MySyncedConfigs.Instance.IS_TINY_LADDER_ENABLED)
            {
                TerminalNode tinyLadderNode = ScriptableObject.CreateInstance<TerminalNode>();
                tinyLadderNode.clearPreviousText = true;
                tinyLadderNode.displayText = "Awwww... tiny ladder!\n\n";
                Items.RegisterShopItem(tinyLadderItem, null, null, tinyLadderNode, ConfigStuff.MySyncedConfigs.Instance.TINY_LADDER_PRICE);
            }

            if (ConfigStuff.MySyncedConfigs.Instance.IS_BIG_LADDER_ENABLED)
            {
                TerminalNode bigLadderNode = ScriptableObject.CreateInstance<TerminalNode>();
                bigLadderNode.clearPreviousText = true;
                bigLadderNode.displayText = "This ladder seems longer than the normal one..\n\n";
                Items.RegisterShopItem(bigLadderItem, null, null, bigLadderNode, ConfigStuff.MySyncedConfigs.Instance.BIG_LADDER_PRICE);
            }

            if (ConfigStuff.MySyncedConfigs.Instance.IS_HUGE_LADDER_ENABLED)
            {
                TerminalNode hugeLadderNode = ScriptableObject.CreateInstance<TerminalNode>();
                hugeLadderNode.clearPreviousText = true;
                hugeLadderNode.displayText = "This ladder seems EVEN longer than the big one..\n\n";
                Items.RegisterShopItem(hugeLadderItem, null, null, hugeLadderNode, ConfigStuff.MySyncedConfigs.Instance.HUGE_LADDER_PRICE);
            }

            mls.LogInfo("items should be in shop.");
            mls.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        private void buildLadderItem(List<MeshRenderer> meshRenderers, List<Animator> animators, List<Transform> transforms,
            List<AudioClip> audioClips, List<AudioSource> audioSources, List<InteractTrigger> interactTriggers, List<BoxCollider> boxColliders,
            LadderItemScript ladderItemScript)
        {
            propertyCounter = 0;

            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                if (meshRenderer.name.Equals("LadderBox"))
                {
                    propertyCounter++;
                    ladderItemScript.mainObjectRenderer = meshRenderer;
                    mls.LogInfo("1");
                    break;
                }
            }
            foreach (Animator animator in animators)
            {
                if (animator.name.Equals("AnimContainer"))
                {
                    propertyCounter++;
                    ladderItemScript.ladderAnimator = animator;
                    mls.LogInfo("2");
                }
                if (animator.name.Equals("MeshContainer"))
                {
                    propertyCounter++;
                    ladderItemScript.ladderRotateAnimator = animator;
                    mls.LogInfo("3");
                }
            }
            foreach (Transform transform in transforms)
            {
                if (transform.name.Equals("Base"))
                {
                    propertyCounter++;
                    ladderItemScript.baseNode = transform;
                    mls.LogInfo("4");
                }
                if (transform.name.Equals("TopPosition"))
                {
                    propertyCounter++;
                    ladderItemScript.topNode = transform;
                    mls.LogInfo("5");
                }
                if (transform.name.Equals("MovableNode"))
                {
                    propertyCounter++;
                    ladderItemScript.moveableNode = transform;
                    mls.LogInfo("6");
                }
            }
            foreach (AudioClip audioClip in audioClips)
            {
                if (audioClip.name.Equals("ExtensionLadderHitWall"))
                {
                    propertyCounter++;
                    ladderItemScript.hitRoof = audioClip;
                    mls.LogInfo("7");
                }
                if (audioClip.name.Equals("ExtensionLadderHitWall2"))
                {
                    propertyCounter += 2;
                    ladderItemScript.fullExtend = audioClip;
                    ladderItemScript.hitWall = audioClip;
                    mls.LogInfo("8 + 9");
                }
                if (audioClip.name.Equals("ExtensionLadderExtend"))
                {
                    propertyCounter++;
                    ladderItemScript.ladderExtendSFX = audioClip;
                    mls.LogInfo("10");
                }
                if (audioClip.name.Equals("ExtensionLadderShrink"))
                {
                    propertyCounter++;
                    ladderItemScript.ladderShrinkSFX = audioClip;
                    mls.LogInfo("11");
                }
                if (audioClip.name.Equals("ExtensionLadderAlarm"))
                {
                    propertyCounter++;
                    ladderItemScript.blinkWarningSFX = audioClip;
                    mls.LogInfo("12");
                }
                if (audioClip.name.Equals("ExtensionLadderLidOpen"))
                {
                    propertyCounter++;
                    ladderItemScript.lidOpenSFX = audioClip;
                    mls.LogInfo("13");
                }
            }
            foreach (AudioSource audioSource in audioSources)
            {
                if (audioSource.name.Equals("LadderAudio"))
                {
                    propertyCounter++;
                    ladderItemScript.ladderAudio = audioSource;
                    mls.LogInfo("14");
                    break;
                }
            }
            foreach (InteractTrigger interactTrigger in interactTriggers)
            {
                if (interactTrigger.name.Equals("ExtLadderTrigger"))
                {
                    ladderItemScript.ladderScript = interactTrigger;
                    propertyCounter++;
                    mls.LogInfo("15");
                    break;
                }
            }
            foreach (BoxCollider boxCollider in boxColliders)
            {
                if (boxCollider.name.Equals("ExtLadderTrigger"))
                {
                    propertyCounter++;
                    ladderItemScript.interactCollider = boxCollider;
                    mls.LogInfo("16");
                }
                if (boxCollider.name.Equals("LadderBridgeCollider"))
                {
                    propertyCounter++;
                    ladderItemScript.bridgeCollider = boxCollider;
                    mls.LogInfo("17");
                }
                if (boxCollider.name.Equals("KillTrigger"))
                {
                    propertyCounter++;
                    ladderItemScript.killTrigger = boxCollider;
                    mls.LogInfo("18");
                }
            }

            if (propertyCounter == MAX_PROPERTY_AMOUNT)
            {
                mls.LogInfo("all properties found for script: " + ladderItemScript.name);
            }
            else
            {
                mls.LogInfo("NOT all properties found. Counter: " + propertyCounter);
            }
        }

        internal static void Patch()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

            mls.LogDebug("Patching...");

            Harmony.PatchAll();

            mls.LogDebug("Finished patching!");
        }

        internal static void Unpatch()
        {
            mls.LogDebug("Unpatching...");

            Harmony?.UnpatchSelf();

            mls.LogDebug("Finished unpatching!");
        }
    }
}
