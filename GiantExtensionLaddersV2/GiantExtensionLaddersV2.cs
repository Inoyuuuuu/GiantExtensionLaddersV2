using BepInEx;
using BepInEx.Logging;
using GameNetcodeStuff;
using GiantExtensionLaddersV2.Behaviours;
using GiantExtensionLaddersV2.ConfigStuff;
using GiantExtensionLaddersV2.Patches;
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
    [BepInDependency("evaisa.lethallib", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("io.github.CSync", BepInDependency.DependencyFlags.SoftDependency)]
    public class GiantExtensionLaddersV2 : BaseUnityPlugin
    {
        //------- configs
        internal static MySyncedConfigs mySyncedConfigs;

        //------- constants
        private const string tinyLadderAssetbundleName = "TinyLadderAssets";
        private const string bigLadderAssetbundleName = "BigLadderAssets";
        private const string hugeLadderAssetbundleName = "HugeLadderAssets";
        private const string ultimateLadderAssetbundleName = "UltimateLadderAssets";
        private const string tinyLadderItemPropertiesLocation = "Assets/extLadderTest/tinyLadder/ExtensionLadder_0.asset";
        private const string bigLadderItemPropertiesLocation = "Assets/extLadderTest/lcLadder/ExtensionLadder_0.asset";
        private const string hugeLadderItemPropertiesLocation = "Assets/extLadderTest/newLongerLadder/ExtensionLadder_0.asset";
        private const string ultimateLadderItemPropertiesLocation = "Assets/extLadderTest/Gigantic ladder/ExtensionLadder_0.asset";
        private const int MAX_PROPERTY_AMOUNT = 19;
        internal static int propertyCounter = 0;

        private const float HEIGHT_DIVIDE_CONST = 2.43f;

        internal LadderObject tinyLadder = new LadderObject(10.3f, 75f, 2, 2, 10.3f / HEIGHT_DIVIDE_CONST, 0.15f, false, GiantLadderType.TINY);
        internal LadderObject bigLadder = new LadderObject(17f, 60f, 3, 7, 7f, 0.15f, true, GiantLadderType.BIG);
        internal LadderObject hugeLadder = new LadderObject(34.4f, 60f, 4, 8, 14.15f, 0.2f, true, GiantLadderType.HUGE);
        internal LadderObject ultimateLadder = new LadderObject(68f, 60f, 5, 9, 27.98f, 0.25f, true, GiantLadderType.ULTIMATE);

        internal static Item tinyLadderItem;
        internal static Item bigLadderItem;
        internal static Item hugeLadderItem;
        internal static Item ultimateLadderItem;

        internal static bool isPlayerOnTinyLadder = false;

        internal static GiantExtensionLaddersV2 Instance { get; private set; } = null!;
        internal static ManualLogSource mls { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }

        private void Awake()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);
            mls = base.Logger;
            Instance = this;

            mySyncedConfigs = new MySyncedConfigs(Config);

            mls.LogInfo("loading assetbundles");
            string tinyLadderAssetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), tinyLadderAssetbundleName);
            string bigLadderAssetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), bigLadderAssetbundleName);
            string hugeLadderAssetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), hugeLadderAssetbundleName);
            string ultimateLadderAssetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ultimateLadderAssetbundleName);
            
            AssetBundle tinyLadderBundle = AssetBundle.LoadFromFile(tinyLadderAssetDir);
            AssetBundle bigLadderBundle = AssetBundle.LoadFromFile(bigLadderAssetDir);
            AssetBundle hugeLadderBundle = AssetBundle.LoadFromFile(hugeLadderAssetDir);
            AssetBundle ultimateLadderBundle = AssetBundle.LoadFromFile(ultimateLadderAssetDir);

            if (bigLadderBundle != null && hugeLadderBundle != null && tinyLadderBundle != null && ultimateLadderBundle != null)
            {
                mls.LogDebug("successfully loaded all assetbundles from files");
            }
            else
            {
                mls.LogError("failed to load assetbundles");
                return;
            }

            tinyLadderItem = tinyLadderBundle.LoadAsset<Item>(tinyLadderItemPropertiesLocation);
            bigLadderItem = bigLadderBundle.LoadAsset<Item>(bigLadderItemPropertiesLocation);
            hugeLadderItem = hugeLadderBundle.LoadAsset<Item>(hugeLadderItemPropertiesLocation);
            ultimateLadderItem = ultimateLadderBundle.LoadAsset<Item>(ultimateLadderItemPropertiesLocation);

            if (bigLadderItem != null && hugeLadderItem != null && tinyLadderItem != null && ultimateLadderItem != null)
            {
                mls.LogDebug("successfully loaded all items from assetbundles");
            }
            else
            {
                mls.LogError("failed to load items from item assetbundles");
                return;
            }

            //----- build tiny ladder item
            mls.LogInfo("attempting to build the tiny ladder");
            tinyLadder.ladderPrefab = tinyLadderItem.spawnPrefab;

            tinyLadder.meshRenderers = tinyLadder.ladderPrefab.GetComponentsInChildren<MeshRenderer>().ToList();
            tinyLadder.animators = tinyLadder.ladderPrefab.GetComponentsInChildren<Animator>().ToList();
            tinyLadder.transforms = tinyLadder.ladderPrefab.GetComponentsInChildren<Transform>().ToList();
            tinyLadder.audioClips = tinyLadderBundle.LoadAllAssets<AudioClip>().ToList();
            tinyLadder.audioSources = tinyLadder.ladderPrefab.GetComponentsInChildren<AudioSource>().ToList();
            tinyLadder.interactTriggers = tinyLadder.ladderPrefab.GetComponentsInChildren<InteractTrigger>().ToList();
            tinyLadder.boxColliders = tinyLadder.ladderPrefab.GetComponentsInChildren<BoxCollider>().ToList();

            LadderItemScript tinyLadderScript = tinyLadder.ladderPrefab.AddComponent<LadderItemScript>();

            if (tinyLadder.ladderPrefab.GetComponent<LadderItemScript>() == null)
            {
                mls.LogError("tinyLadderItemPrefab failed");
                return;
            }
            else
            {
                mls.LogDebug("tinyLadderItemPrefab okay");
            }

            tinyLadderScript.grabbable = true;
            tinyLadderScript.grabbableToEnemies = true;
            tinyLadderScript.itemProperties = tinyLadderItem;

            tinyLadderScript.maxExtension = tinyLadder.LADDER_MAX_EXTENSION;
            tinyLadderScript.minInteractableRotation = tinyLadder.LADDER_MIN_ROTATION_FOR_INTERACTION;
            tinyLadderScript.linecastChecksMultiplier = tinyLadder.LADDER_LINECAST_CHECKS_MULTIPLIER;
            tinyLadderScript.linecastMinCheckHeight = tinyLadder.LADDER_LINECAST_MIN_CHECK_HEIGHT;
            tinyLadderScript.ladderHeightMultiplier = tinyLadder.LADDER_HEIGHT_MULTIPLIER;
            tinyLadderScript.ladderRotateSpeedMultiplier = tinyLadder.LADDER_ROTATE_SPEED;
            tinyLadderScript.isClimbable = true;
            tinyLadderScript.isClimbableInShip = true;
            tinyLadderScript.giantLadderType = tinyLadder.ladderType;

            buildLadderItem(tinyLadder.meshRenderers, tinyLadder.animators, tinyLadder.transforms, tinyLadder.audioClips, tinyLadder.audioSources,
                tinyLadder.interactTriggers, tinyLadder.boxColliders, tinyLadderScript);

            tinyLadderItem.canBeGrabbedBeforeGameStart = true;

            //----- build big ladder item
            mls.LogInfo("attempting to build the big ladder");
            bigLadder.ladderPrefab = bigLadderItem.spawnPrefab;

            bigLadder.meshRenderers = bigLadder.ladderPrefab.GetComponentsInChildren<MeshRenderer>().ToList();
            bigLadder.animators = bigLadder.ladderPrefab.GetComponentsInChildren<Animator>().ToList();
            bigLadder.transforms = bigLadder.ladderPrefab.GetComponentsInChildren<Transform>().ToList();
            bigLadder.audioClips = bigLadderBundle.LoadAllAssets<AudioClip>().ToList();
            bigLadder.audioSources = bigLadder.ladderPrefab.GetComponentsInChildren<AudioSource>().ToList();
            bigLadder.interactTriggers = bigLadder.ladderPrefab.GetComponentsInChildren<InteractTrigger>().ToList();
            bigLadder.boxColliders = bigLadder.ladderPrefab.GetComponentsInChildren<BoxCollider>().ToList();

            LadderItemScript bigLadderScript = bigLadder.ladderPrefab.AddComponent<LadderItemScript>();

            if (bigLadder.ladderPrefab.GetComponent<LadderItemScript>() == null)
            {
                mls.LogError("bigLadderScript failed");
                return;
            }
            else
            {
                mls.LogDebug("bigLadderScript okay");
            }

            bigLadderScript.grabbable = true;
            bigLadderScript.grabbableToEnemies = true;
            bigLadderScript.itemProperties = bigLadderItem;

            bigLadderScript.maxExtension = bigLadder.LADDER_MAX_EXTENSION;
            bigLadderScript.minInteractableRotation = bigLadder.LADDER_MIN_ROTATION_FOR_INTERACTION;
            bigLadderScript.linecastChecksMultiplier = bigLadder.LADDER_LINECAST_CHECKS_MULTIPLIER;
            bigLadderScript.linecastMinCheckHeight = bigLadder.LADDER_LINECAST_MIN_CHECK_HEIGHT;
            bigLadderScript.ladderHeightMultiplier = bigLadder.LADDER_HEIGHT_MULTIPLIER;
            bigLadderScript.ladderRotateSpeedMultiplier = bigLadder.LADDER_ROTATE_SPEED;
            bigLadderScript.isClimbable = bigLadder.LADDER_IS_CLIMBABLE;
            bigLadderScript.giantLadderType = bigLadder.ladderType;

            buildLadderItem(bigLadder.meshRenderers, bigLadder.animators, bigLadder.transforms, bigLadder.audioClips, bigLadder.audioSources,
                bigLadder.interactTriggers, bigLadder.boxColliders, bigLadderScript);

            //----- build huge ladder item
            mls.LogInfo("attempting to build the huge ladder");
            hugeLadder.ladderPrefab = hugeLadderItem.spawnPrefab;

            hugeLadder.meshRenderers = hugeLadder.ladderPrefab.GetComponentsInChildren<MeshRenderer>().ToList();
            hugeLadder.animators = hugeLadder.ladderPrefab.GetComponentsInChildren<Animator>().ToList();
            hugeLadder.transforms = hugeLadder.ladderPrefab.GetComponentsInChildren<Transform>().ToList();
            hugeLadder.audioClips = hugeLadderBundle.LoadAllAssets<AudioClip>().ToList();
            hugeLadder.audioSources = hugeLadder.ladderPrefab.GetComponentsInChildren<AudioSource>().ToList();
            hugeLadder.interactTriggers = hugeLadder.ladderPrefab.GetComponentsInChildren<InteractTrigger>().ToList();
            hugeLadder.boxColliders = hugeLadder.ladderPrefab.GetComponentsInChildren<BoxCollider>().ToList();

            LadderItemScript hugeLadderScript = hugeLadder.ladderPrefab.AddComponent<LadderItemScript>();

            if (hugeLadder.ladderPrefab.GetComponent<LadderItemScript>() == null)
            {
                mls.LogError("hugeLadderScript failed");
                return;
            }
            else
            {
                mls.LogDebug("hugeLadderScript okay");
            }

            hugeLadderScript.grabbable = true;
            hugeLadderScript.grabbableToEnemies = true;
            hugeLadderScript.itemProperties = hugeLadderItem;

            hugeLadderScript.maxExtension = hugeLadder.LADDER_MAX_EXTENSION;
            hugeLadderScript.minInteractableRotation = hugeLadder.LADDER_MIN_ROTATION_FOR_INTERACTION;
            hugeLadderScript.linecastChecksMultiplier = hugeLadder.LADDER_LINECAST_CHECKS_MULTIPLIER;
            hugeLadderScript.linecastMinCheckHeight = hugeLadder.LADDER_LINECAST_MIN_CHECK_HEIGHT;
            hugeLadderScript.ladderHeightMultiplier = hugeLadder.LADDER_HEIGHT_MULTIPLIER;
            hugeLadderScript.ladderRotateSpeedMultiplier = hugeLadder.LADDER_ROTATE_SPEED;
            hugeLadderScript.isClimbable = hugeLadder.LADDER_IS_CLIMBABLE;
            hugeLadderScript.giantLadderType = hugeLadder.ladderType;

            buildLadderItem(hugeLadder.meshRenderers, hugeLadder.animators, hugeLadder.transforms, hugeLadder.audioClips, hugeLadder.audioSources,
                hugeLadder.interactTriggers, hugeLadder.boxColliders, hugeLadderScript);

            //----- build ultimate ladder item
            mls.LogInfo("attempting to build the ultimate ladder");
            ultimateLadder.ladderPrefab = ultimateLadderItem.spawnPrefab;

            ultimateLadder.meshRenderers = ultimateLadder.ladderPrefab.GetComponentsInChildren<MeshRenderer>().ToList();
            ultimateLadder.animators = ultimateLadder.ladderPrefab.GetComponentsInChildren<Animator>().ToList();
            ultimateLadder.transforms = ultimateLadder.ladderPrefab.GetComponentsInChildren<Transform>().ToList();
            ultimateLadder.audioClips = ultimateLadderBundle.LoadAllAssets<AudioClip>().ToList();
            ultimateLadder.audioSources = ultimateLadder.ladderPrefab.GetComponentsInChildren<AudioSource>().ToList();
            ultimateLadder.interactTriggers = ultimateLadder.ladderPrefab.GetComponentsInChildren<InteractTrigger>().ToList();
            ultimateLadder.boxColliders = ultimateLadder.ladderPrefab.GetComponentsInChildren<BoxCollider>().ToList();

            LadderItemScript ultimateLadderScript = ultimateLadder.ladderPrefab.AddComponent<LadderItemScript>();
            
            if (ultimateLadder.ladderPrefab.GetComponent<LadderItemScript>() == null)
            {
                mls.LogError("ultimateLadderScript failed");
                return;
            }
            else
            {
                mls.LogDebug("ultimateLadderScript okay");
            }

            ultimateLadderScript.grabbable = true;
            ultimateLadderScript.grabbableToEnemies = true;
            ultimateLadderScript.itemProperties = ultimateLadderItem;

            ultimateLadderScript.maxExtension = ultimateLadder.LADDER_MAX_EXTENSION;
            ultimateLadderScript.minInteractableRotation = ultimateLadder.LADDER_MIN_ROTATION_FOR_INTERACTION;
            ultimateLadderScript.linecastChecksMultiplier = ultimateLadder.LADDER_LINECAST_CHECKS_MULTIPLIER;
            ultimateLadderScript.linecastMinCheckHeight = ultimateLadder.LADDER_LINECAST_MIN_CHECK_HEIGHT;
            ultimateLadderScript.ladderHeightMultiplier = ultimateLadder.LADDER_HEIGHT_MULTIPLIER;
            ultimateLadderScript.ladderRotateSpeedMultiplier = ultimateLadder.LADDER_ROTATE_SPEED;
            ultimateLadderScript.isClimbable = ultimateLadder.LADDER_IS_CLIMBABLE;
            ultimateLadderScript.giantLadderType = ultimateLadder.ladderType;

            buildLadderItem(ultimateLadder.meshRenderers, ultimateLadder.animators, ultimateLadder.transforms, ultimateLadder.audioClips, ultimateLadder.audioSources,
                ultimateLadder.interactTriggers, ultimateLadder.boxColliders, ultimateLadderScript);

            //-------- register items
            NetworkPrefabs.RegisterNetworkPrefab(tinyLadderItem.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(bigLadderItem.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(hugeLadderItem.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(ultimateLadderItem.spawnPrefab);
            mls.LogInfo("registered network prefabs");

            Utilities.FixMixerGroups(tinyLadderItem.spawnPrefab);
            Utilities.FixMixerGroups(bigLadderItem.spawnPrefab);
            Utilities.FixMixerGroups(hugeLadderItem.spawnPrefab);
            Utilities.FixMixerGroups(ultimateLadderItem.spawnPrefab);
            mls.LogInfo("fixed mixers");

            TerminalNode tinyLadderNode = ScriptableObject.CreateInstance<TerminalNode>();
            tinyLadderNode.clearPreviousText = true;
            tinyLadderNode.displayText = "Awwww... tiny ladder!\n\n";
            Items.RegisterShopItem(tinyLadderItem, null, null, tinyLadderNode, MySyncedConfigs.Instance.TINY_LADDER_PRICE);

            TerminalNode bigLadderNode = ScriptableObject.CreateInstance<TerminalNode>();
            bigLadderNode.clearPreviousText = true;
            bigLadderNode.displayText = "This ladder seems a bit higher than the normal one..\n\n";
            Items.RegisterShopItem(bigLadderItem, null, null, bigLadderNode, MySyncedConfigs.Instance.BIG_LADDER_PRICE);

            TerminalNode hugeLadderNode = ScriptableObject.CreateInstance<TerminalNode>();
            hugeLadderNode.clearPreviousText = true;
            hugeLadderNode.displayText = "This ladder seems EVEN higher than the big one..\n\n";
            Items.RegisterShopItem(hugeLadderItem, null, null, hugeLadderNode, MySyncedConfigs.Instance.HUGE_LADDER_PRICE);

            TerminalNode ultimateLadderNode = ScriptableObject.CreateInstance<TerminalNode>();
            ultimateLadderNode.clearPreviousText = true;
            ultimateLadderNode.displayText = "Not the tiny, not the big, not the huge, THIS IS THE ULTIMATE EXTENSION LADDER!\n\n";
            Items.RegisterShopItem(ultimateLadderItem, null, null, ultimateLadderNode, MySyncedConfigs.Instance.ULTIMATE_LADDER_PRICE);

            Harmony.PatchAll();

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
                    mls.LogDebug("1. component: LadderBox");
                    break;
                }
            }
            foreach (Animator animator in animators)
            {
                if (animator.name.Equals("AnimContainer"))
                {
                    propertyCounter++;
                    ladderItemScript.ladderAnimator = animator;
                    mls.LogDebug("2.  AnimContainer");
                }
                if (animator.name.Equals("MeshContainer"))
                {
                    propertyCounter++;
                    ladderItemScript.ladderRotateAnimator = animator;
                    mls.LogDebug("3. component: MeshContainer");
                }
            }
            foreach (Transform transform in transforms)
            {
                if (transform.name.Equals("Base"))
                {
                    propertyCounter++;
                    ladderItemScript.baseNode = transform;
                    mls.LogDebug("4. component: Base");
                }
                if (transform.name.Equals("TopPosition"))
                {
                    propertyCounter++;
                    ladderItemScript.topNode = transform;
                    mls.LogDebug("5.component: TopPosition");
                }
                if (transform.name.Equals("TopCollisionNode"))
                {
                    propertyCounter++;
                    ladderItemScript.topCollisionNode = transform;
                    mls.LogDebug("19.component: TopCollisionNode");
                }
                if (transform.name.Equals("MovableNode"))
                {
                    propertyCounter++;
                    ladderItemScript.moveableNode = transform;
                    mls.LogDebug("6. component: MovableNode");
                }
            }
            foreach (AudioClip audioClip in audioClips)
            {
                if (audioClip.name.Equals("ExtensionLadderHitWall"))
                {
                    propertyCounter++;
                    ladderItemScript.hitRoof = audioClip;
                    mls.LogDebug("7. component: ExtensionLadderHitWall");
                }
                if (audioClip.name.Equals("ExtensionLadderHitWall2"))
                {
                    propertyCounter += 2;
                    ladderItemScript.fullExtend = audioClip;
                    ladderItemScript.hitWall = audioClip;
                    mls.LogDebug("8. component: ExtensionLadderHitWall2");
                    mls.LogDebug("9. component: ExtensionLadderHitWall2 (for 2nd audio clip)");
                }
                if (audioClip.name.Equals("ExtensionLadderExtend"))
                {
                    propertyCounter++;
                    ladderItemScript.ladderExtendSFX = audioClip;
                    mls.LogDebug("10. component: ExtensionLadderExtend");
                }
                if (audioClip.name.Equals("ExtensionLadderShrink"))
                {
                    propertyCounter++;
                    ladderItemScript.ladderShrinkSFX = audioClip;
                    mls.LogDebug("11. component: ExtensionLadderShrink");
                }
                if (audioClip.name.Equals("ExtensionLadderAlarm"))
                {
                    propertyCounter++;
                    ladderItemScript.blinkWarningSFX = audioClip;
                    mls.LogDebug("12. component: ExtensionLadderAlarm");
                }
                if (audioClip.name.Equals("ExtensionLadderLidOpen"))
                {
                    propertyCounter++;
                    ladderItemScript.lidOpenSFX = audioClip;
                    mls.LogDebug("13. component: ExtensionLadderLidOpen");
                }
            }
            foreach (AudioSource audioSource in audioSources)
            {
                if (audioSource.name.Equals("LadderAudio"))
                {
                    propertyCounter++;
                    ladderItemScript.ladderAudio = audioSource;
                    mls.LogDebug("14. component: LadderAudio");
                    break;
                }
            }
            foreach (InteractTrigger interactTrigger in interactTriggers)
            {
                if (interactTrigger.name.Equals("ExtLadderTrigger"))
                {
                    ladderItemScript.ladderScript = interactTrigger;
                    propertyCounter++;
                    mls.LogDebug("15. component: ExtLadderTrigger (interactTrigger)");
                    break;
                }
            }
            foreach (BoxCollider boxCollider in boxColliders)
            {
                if (boxCollider.name.Equals("ExtLadderTrigger"))
                {
                    propertyCounter++;
                    ladderItemScript.interactCollider = boxCollider;
                    mls.LogDebug("16. component: ExtLadderTrigger (boxCollider)");
                }
                if (boxCollider.name.Equals("LadderBridgeCollider"))
                {
                    propertyCounter++;
                    ladderItemScript.bridgeCollider = boxCollider;
                    mls.LogDebug("17. component: LadderBridgeCollider");
                }
                if (boxCollider.name.Equals("KillTrigger"))
                {
                    propertyCounter++;
                    ladderItemScript.killTrigger = boxCollider;
                    mls.LogDebug("18. component: KillTrigger");
                }
            }

            if (propertyCounter == MAX_PROPERTY_AMOUNT)
            {
                mls.LogDebug("every component was found for item script: " + ladderItemScript.name);
            }
            else
            {
                mls.LogError($"Some Components of {ladderItemScript.name} are missing!");
            }
        }
    }
}
