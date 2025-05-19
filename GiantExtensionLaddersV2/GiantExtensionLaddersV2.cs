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
    [BepInDependency("evaisa.lethallib", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.sigurd.csync", "5.0.0")]
    public class GiantExtensionLaddersV2 : BaseUnityPlugin
    {
        //------- configs
        internal static MySyncedConfigs mySyncedConfigs;

        //------- constants
        private const string ladderAssetbundleName = "allLadderAssets";

        private const string tinyLadderAssetbundleName = "TinyLadderAssets";
        private const string bigLadderAssetbundleName = "BigLadderAssets";
        private const string hugeLadderAssetbundleName = "HugeLadderAssets";
        private const string ultimateLadderAssetbundleName = "UltimateLadderAssets";
        private const string ladderCollectorAssetBundleName = "laddercollector";
        private const string tinyLadderItemPropertiesLocation = "Assets/extLadderTest/tinyLadder/TinyLadder.asset";
        private const string bigLadderItemPropertiesLocation = "Assets/extLadderTest/BigLadder/BigLadder.asset";
        private const string hugeLadderItemPropertiesLocation = "Assets/extLadderTest/HugeLadder/HugeLadder.asset";
        private const string ultimateLadderItemPropertiesLocation = "Assets/extLadderTest/Gigantic ladder/UltimateLadderItem.asset";
        private const string ladderCollectorItemLocation = "Assets/LethalCompany/Mods/Items/LadderCollector/LadderCollectorItem.asset";
        private const int MAX_PROPERTY_AMOUNT = 19;
        internal static int propertyCounter = 0;

        internal LadderObject tinyLadder = new LadderObject(10.3f, 75f, 0.15f, false, GiantLadderType.TINY);
        internal LadderObject bigLadder = new LadderObject(17f, 60f, 0.15f, true, GiantLadderType.BIG);
        internal LadderObject hugeLadder = new LadderObject(34.4f, 60f, 0.2f, true, GiantLadderType.HUGE);
        internal LadderObject ultimateLadder = new LadderObject(68f, 60f, 0.25f, true, GiantLadderType.ULTIMATE);

        internal static Item? tinyLadderItem;
        internal static Item? bigLadderItem;
        internal static Item? hugeLadderItem;
        internal static Item? ultimateLadderItem;
        internal static Item? ladderCollectorItem;

        internal static TerminalNode? tinyLadderNode;
        internal static TerminalNode? bigLadderNode;
        internal static TerminalNode? hugeLadderNode;
        internal static TerminalNode? ultimateLadderNode;
        internal static TerminalNode? ladderCollectorNode;

        internal static Dictionary<Item, string> originalItemNames = new Dictionary<Item, string>();

        internal static bool isBuildSuccess = true;

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
            string ladderCollectorAssetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ladderCollectorAssetBundleName);

            AssetBundle tinyLadderBundle = AssetBundle.LoadFromFile(tinyLadderAssetDir);
            AssetBundle bigLadderBundle = AssetBundle.LoadFromFile(bigLadderAssetDir);
            AssetBundle hugeLadderBundle = AssetBundle.LoadFromFile(hugeLadderAssetDir);
            AssetBundle ultimateLadderBundle = AssetBundle.LoadFromFile(ultimateLadderAssetDir);
            AssetBundle ladderCollectorAssetBundle = AssetBundle.LoadFromFile(ladderCollectorAssetDir);

            if (bigLadderBundle == null || hugeLadderBundle == null || tinyLadderBundle == null || ultimateLadderBundle == null || ladderCollectorAssetBundleName == null)
            {
                mls.LogError("failed to load assetbundles");
                return;
            }

            tinyLadderItem = tinyLadderBundle.LoadAsset<Item>(tinyLadderItemPropertiesLocation);
            bigLadderItem = bigLadderBundle.LoadAsset<Item>(bigLadderItemPropertiesLocation);
            hugeLadderItem = hugeLadderBundle.LoadAsset<Item>(hugeLadderItemPropertiesLocation);
            ultimateLadderItem = ultimateLadderBundle.LoadAsset<Item>(ultimateLadderItemPropertiesLocation);
            ladderCollectorItem = ladderCollectorAssetBundle.LoadAsset<Item>(ladderCollectorItemLocation);

            if (bigLadderItem == null || hugeLadderItem == null || tinyLadderItem == null || ultimateLadderItem == null || ladderCollectorItem == null)
            {
                mls.LogError("failed to load items from item assetbundles");
                return;
            }

            //----- build tiny ladder item
            mls.LogInfo("building tiny ladder...");
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

            tinyLadderScript.grabbable = true;
            tinyLadderScript.grabbableToEnemies = true;
            tinyLadderScript.itemProperties = tinyLadderItem;

            tinyLadderScript.maxExtension = tinyLadder.LADDER_MAX_EXTENSION;
            tinyLadderScript.minInteractableRotation = tinyLadder.LADDER_MIN_ROTATION_FOR_INTERACTION;
            tinyLadderScript.ladderRotateSpeedMultiplier = tinyLadder.LADDER_ROTATE_SPEED;
            tinyLadderScript.isClimbable = true;
            tinyLadderScript.isClimbableInShip = true;
            tinyLadderScript.giantLadderType = tinyLadder.ladderType;

            BuildLadderItemScript(tinyLadder.meshRenderers, tinyLadder.animators, tinyLadder.transforms, tinyLadder.audioClips, tinyLadder.audioSources,
                tinyLadder.interactTriggers, tinyLadder.boxColliders, tinyLadderScript);

            tinyLadderItem.canBeGrabbedBeforeGameStart = true;

            //----- build big ladder item
            mls.LogInfo("building big ladder...");
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

            bigLadderScript.grabbable = true;
            bigLadderScript.grabbableToEnemies = true;
            bigLadderScript.itemProperties = bigLadderItem;

            bigLadderScript.maxExtension = bigLadder.LADDER_MAX_EXTENSION;
            bigLadderScript.minInteractableRotation = bigLadder.LADDER_MIN_ROTATION_FOR_INTERACTION;
            bigLadderScript.ladderRotateSpeedMultiplier = bigLadder.LADDER_ROTATE_SPEED;
            bigLadderScript.isClimbable = bigLadder.LADDER_IS_CLIMBABLE;
            bigLadderScript.giantLadderType = bigLadder.ladderType;

            BuildLadderItemScript(bigLadder.meshRenderers, bigLadder.animators, bigLadder.transforms, bigLadder.audioClips, bigLadder.audioSources,
                bigLadder.interactTriggers, bigLadder.boxColliders, bigLadderScript);

            //----- build huge ladder item
            mls.LogInfo("building huge ladder...");
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

            hugeLadderScript.grabbable = true;
            hugeLadderScript.grabbableToEnemies = true;
            hugeLadderScript.itemProperties = hugeLadderItem;

            hugeLadderScript.maxExtension = hugeLadder.LADDER_MAX_EXTENSION;
            hugeLadderScript.minInteractableRotation = hugeLadder.LADDER_MIN_ROTATION_FOR_INTERACTION;
            hugeLadderScript.ladderRotateSpeedMultiplier = hugeLadder.LADDER_ROTATE_SPEED;
            hugeLadderScript.isClimbable = hugeLadder.LADDER_IS_CLIMBABLE;
            hugeLadderScript.giantLadderType = hugeLadder.ladderType;

            BuildLadderItemScript(hugeLadder.meshRenderers, hugeLadder.animators, hugeLadder.transforms, hugeLadder.audioClips, hugeLadder.audioSources,
                hugeLadder.interactTriggers, hugeLadder.boxColliders, hugeLadderScript);

            //----- build ultimate ladder item
            mls.LogInfo("building ultimate ladder...");
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

            ultimateLadderScript.grabbable = true;
            ultimateLadderScript.grabbableToEnemies = true;
            ultimateLadderScript.itemProperties = ultimateLadderItem;

            ultimateLadderScript.maxExtension = ultimateLadder.LADDER_MAX_EXTENSION;
            ultimateLadderScript.minInteractableRotation = ultimateLadder.LADDER_MIN_ROTATION_FOR_INTERACTION;
            ultimateLadderScript.ladderRotateSpeedMultiplier = ultimateLadder.LADDER_ROTATE_SPEED;
            ultimateLadderScript.isClimbable = ultimateLadder.LADDER_IS_CLIMBABLE;
            ultimateLadderScript.giantLadderType = ultimateLadder.ladderType;

            BuildLadderItemScript(ultimateLadder.meshRenderers, ultimateLadder.animators, ultimateLadder.transforms, ultimateLadder.audioClips, ultimateLadder.audioSources,
                ultimateLadder.interactTriggers, ultimateLadder.boxColliders, ultimateLadderScript);

            if (!isBuildSuccess)
            {
                mls.LogError("Ran into a problem finding all ladder components on some ladders. If you see this, please add it as an issue on this mod's GitHub page.");
                return;
            }

            //-------- build ladder collector
            LadderCollectorScript ladderCollectorScript = ladderCollectorItem.spawnPrefab.AddComponent<LadderCollectorScript>();
            if (ladderCollectorScript == null)
            {
                mls.LogError("ladderCollectorScript failed");
                return;
            }

            ladderCollectorScript.grabbable = true;
            ladderCollectorScript.grabbableToEnemies = false;
            ladderCollectorScript.itemProperties = ladderCollectorItem;

            ladderCollectorScript.lcAudioSource = ladderCollectorItem.spawnPrefab.GetComponent<AudioSource>();
            ladderCollectorScript.teleportationLight = ladderCollectorItem.spawnPrefab.GetComponentInChildren<Light>();
            List<Transform> ladderCollectorTransforms = ladderCollectorItem.spawnPrefab.GetComponentsInChildren<Transform>().ToList();
            List<AudioClip> ladderCollectorAudioClips = ladderCollectorAssetBundle.LoadAllAssets<AudioClip>().ToList();

            foreach (Transform transform in ladderCollectorTransforms)
            {
                if (transform.name.Equals("BaseNode"))
                {
                    ladderCollectorScript.baseNode = transform;
                } 
                else if (transform.name.Equals("LadderSpawnNode"))
                {
                    ladderCollectorScript.ladderSpawnNode = transform;
                }
            }

            foreach (AudioClip audio in ladderCollectorAudioClips)
            {
                if (audio.name.Equals("spawnSFX3"))
                {
                    ladderCollectorScript.spawnAudio = audio;
                }
            }

            if (ladderCollectorScript.baseNode == null  || ladderCollectorScript.ladderSpawnNode == null || ladderCollectorScript.spawnAudio == null || ladderCollectorScript.lcAudioSource == null || ladderCollectorScript.teleportationLight == null)
            {
                mls.LogError("some components for ladderCollector couldn't be found!");
                return;
            }


            //-------- register items
            NetworkPrefabs.RegisterNetworkPrefab(tinyLadderItem.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(bigLadderItem.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(hugeLadderItem.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(ultimateLadderItem.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(ladderCollectorItem.spawnPrefab);

            Utilities.FixMixerGroups(tinyLadderItem.spawnPrefab);
            Utilities.FixMixerGroups(bigLadderItem.spawnPrefab);
            Utilities.FixMixerGroups(hugeLadderItem.spawnPrefab);
            Utilities.FixMixerGroups(ultimateLadderItem.spawnPrefab);
            Utilities.FixMixerGroups(ladderCollectorItem.spawnPrefab);

            tinyLadderNode = ScriptableObject.CreateInstance<TerminalNode>();
            tinyLadderNode.clearPreviousText = true;
            tinyLadderNode.displayText = "Awwww... tiny ladder! Can be used if the person climbing is also tiny!\n\n";
            Items.RegisterShopItem(tinyLadderItem, null, null, tinyLadderNode, GiantExtensionLaddersV2.mySyncedConfigs.tinyLadderPrice);

            bigLadderNode = ScriptableObject.CreateInstance<TerminalNode>();
            bigLadderNode.clearPreviousText = true;
            bigLadderNode.displayText = "This ladder is 17m high, thats about 1,75x height of the standard ladder.\n\n";
            Items.RegisterShopItem(bigLadderItem, null, null, bigLadderNode, GiantExtensionLaddersV2.mySyncedConfigs.bigLadderPrice);

            hugeLadderNode = ScriptableObject.CreateInstance<TerminalNode>();
            hugeLadderNode.clearPreviousText = true;
            hugeLadderNode.displayText = "This ladder is 34m high, thats about 3,5x height of the standard ladder.\n\n";
            Items.RegisterShopItem(hugeLadderItem, null, null, hugeLadderNode, GiantExtensionLaddersV2.mySyncedConfigs.hugeLadderPrice);

            ultimateLadderNode = ScriptableObject.CreateInstance<TerminalNode>();
            ultimateLadderNode.clearPreviousText = true;
            ultimateLadderNode.displayText = "This ladder is 68m high, thats about 7x height of the standard ladder.\n\n";
            Items.RegisterShopItem(ultimateLadderItem, null, null, ultimateLadderNode, GiantExtensionLaddersV2.mySyncedConfigs.ultimateLadderPrice);

            ladderCollectorNode = ScriptableObject.CreateInstance<TerminalNode>();
            ladderCollectorNode.clearPreviousText = true;
            ladderCollectorNode.displayText = "This device somehow detects and attracts ladders within an huge range.\n\n";
            Items.RegisterShopItem(ladderCollectorItem, null, null, ladderCollectorNode, 75);

            originalItemNames.Add(tinyLadderItem, tinyLadderItem.itemName);
            originalItemNames.Add(bigLadderItem, bigLadderItem.itemName);
            originalItemNames.Add(hugeLadderItem, hugeLadderItem.itemName);
            originalItemNames.Add(ultimateLadderItem, ultimateLadderItem.itemName);
            originalItemNames.Add(ladderCollectorItem, ladderCollectorItem.itemName);

            Harmony.PatchAll();

            mls.LogInfo("builds completed and items should be in shop.");
            mls.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has fully loaded!");
        }

        private void BuildLadderItemScript(List<MeshRenderer> meshRenderers, List<Animator> animators, List<Transform> transforms,
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
                    //mls.LogMessage("1. component: LadderBox");
                    break;
                }
            }
            foreach (Animator animator in animators)
            {
                if (animator.name.Equals("AnimContainer"))
                {
                    propertyCounter++;
                    ladderItemScript.ladderAnimator = animator;
                    //mls.LogMessage("2.  AnimContainer");
                }
                if (animator.name.Equals("MeshContainer"))
                {
                    propertyCounter++;
                    ladderItemScript.ladderRotateAnimator = animator;
                    //mls.LogMessage("3. component: MeshContainer");
                }
            }
            foreach (Transform transform in transforms)
            {
                if (transform.name.Equals("Base"))
                {
                    propertyCounter++;
                    ladderItemScript.baseNode = transform;
                    //mls.LogMessage("4. component: Base");
                }
                if (transform.name.Equals("TopPosition"))
                {
                    propertyCounter++;
                    ladderItemScript.topNode = transform;
                    //mls.LogMessage("5.component: TopPosition");
                }
                if (transform.name.Equals("TopCollisionNode"))
                {
                    propertyCounter++;
                    ladderItemScript.topCollisionNode = transform;
                    //mls.LogMessage("19.component: TopCollisionNode");
                }
                if (transform.name.Equals("MovableNode"))
                {
                    propertyCounter++;
                    ladderItemScript.moveableNode = transform;
                    //mls.LogMessage("6. component: MovableNode");
                }
            }
            foreach (AudioClip audioClip in audioClips)
            {
                if (audioClip.name.Equals("ExtensionLadderHitWall"))
                {
                    propertyCounter++;
                    ladderItemScript.hitRoof = audioClip;
                    //mls.LogMessage("7. component: ExtensionLadderHitWall");
                }
                if (audioClip.name.Equals("ExtensionLadderHitWall2"))
                {
                    propertyCounter += 2;
                    ladderItemScript.fullExtend = audioClip;
                    ladderItemScript.hitWall = audioClip;
                    //mls.LogMessage("8. component: ExtensionLadderHitWall2");
                    //mls.LogMessage("9. component: ExtensionLadderHitWall2 (for 2nd audio clip)");
                }
                if (audioClip.name.Equals("ExtensionLadderExtend"))
                {
                    propertyCounter++;
                    ladderItemScript.ladderExtendSFX = audioClip;
                    //mls.LogMessage("10. component: ExtensionLadderExtend");
                }
                if (audioClip.name.Equals("ExtensionLadderShrink"))
                {
                    propertyCounter++;
                    ladderItemScript.ladderShrinkSFX = audioClip;
                    //mls.LogMessage("11. component: ExtensionLadderShrink");
                }
                if (audioClip.name.Equals("ExtensionLadderAlarm"))
                {
                    propertyCounter++;
                    ladderItemScript.blinkWarningSFX = audioClip;
                    //mls.LogMessage("12. component: ExtensionLadderAlarm");
                }
                if (audioClip.name.Equals("ExtensionLadderLidOpen"))
                {
                    propertyCounter++;
                    ladderItemScript.lidOpenSFX = audioClip;
                    //mls.LogMessage("13. component: ExtensionLadderLidOpen");
                }
            }
            foreach (AudioSource audioSource in audioSources)
            {
                if (audioSource.name.Equals("LadderAudio"))
                {
                    propertyCounter++;
                    ladderItemScript.ladderAudio = audioSource;
                    //mls.LogMessage("14. component: LadderAudio");
                    break;
                }
            }
            foreach (InteractTrigger interactTrigger in interactTriggers)
            {
                if (interactTrigger.name.Equals("ExtLadderTrigger"))
                {
                    ladderItemScript.ladderScript = interactTrigger;
                    propertyCounter++;
                    //mls.LogMessage("15. component: ExtLadderTrigger (interactTrigger)");
                    break;
                }
            }

            //LayerMask mask = LayerMask.NameToLayer("Player");
            //mls.LogInfo("MASK IS: " + mask.m_Mask);

            foreach (BoxCollider boxCollider in boxColliders)
            {
                //boxCollider.set_excludeLayers_Injected(ref mask);
                
                mls.LogInfo(boxCollider.name + " has mask: " + boxCollider.excludeLayers.m_Mask);

                if (boxCollider.name.Equals("ExtLadderTrigger"))
                {
                    propertyCounter++;
                    ladderItemScript.interactCollider = boxCollider;
                    //mls.LogMessage("16. component: ExtLadderTrigger (boxCollider)");
                }
                if (boxCollider.name.Equals("LadderBridgeCollider"))
                {
                    propertyCounter++;
                    ladderItemScript.bridgeCollider = boxCollider;
                    //mls.LogMessage("17. component: LadderBridgeCollider");
                }
                if (boxCollider.name.Equals("KillTrigger"))
                {
                    propertyCounter++;
                    ladderItemScript.killTrigger = boxCollider;
                    //mls.LogMessage("18. component: KillTrigger");
                }



                //removing collision
                //if (boxCollider.name.Equals("BoxPart"))
                //{
                //    mls.LogMessage("found boxpart boxC");

                //    GameObject boxPart = boxCollider.gameObject;

                //    if (boxPart != null)
                //    {
                //        mls.LogMessage("found boxpart parent");

                //        MeshCollider meshCollider = boxPart.GetComponent<MeshCollider>();

                //        if (meshCollider != null)
                //        {
                //            mls.LogMessage("found boxpart meshC");

                //            Destroy(meshCollider);

                //            mls.LogMessage("destroyed mesh collider");
                //        }
                //    }
                //}

                //if (boxCollider.name.Equals("BaseLadder"))
                //{
                //    //propertyCounter++;
                //    ladderItemScript.anotherExtendedLadderCollider = boxCollider;
                //    //ladderItemScript.anotherExtendedLadderCollider.enabled = false;

                //    //ladderItemScript.anotherExtendedLadderCollider.set_excludeLayers_Injected(ref mask);
                //}

                //if (boxCollider.name.Equals(tinyLadderItem.spawnPrefab.name) 
                //    || boxCollider.name.Equals(bigLadderItem.spawnPrefab.name) 
                //    || boxCollider.name.Equals(hugeLadderItem.spawnPrefab.name) 
                //    || boxCollider.name.Equals(ultimateLadderItem.spawnPrefab.name))
                //{
                //    boxCollider.set_excludeLayers_Injected(ref mask);
                //}

                //boxCollider.get_excludeLayers_Injected(out LayerMask maskout);
                //if (maskout.m_Mask != 3)
                //{
                //    mls.LogInfo("LM not set! was: " + maskout.m_Mask + " mask is: " + mask.m_Mask);
                //}
                
            }

            if (propertyCounter == MAX_PROPERTY_AMOUNT)
            {
                //mls.LogMessage("every component was found for item script: " + ladderItemScript.name);
            }
            else
            {
                mls.LogError($"Some Components of {ladderItemScript.name} are missing!");
                isBuildSuccess = false;
            }
        }
    }
}
