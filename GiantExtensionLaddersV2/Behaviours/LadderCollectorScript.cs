using GiantExtensionLaddersV2.ConfigStuff;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace GiantExtensionLaddersV2.Behaviours
{
    internal class LadderCollectorScript : GrabbableObject
    {
        public Transform? baseNode;
        public Transform? ladderSpawnNode;
        public AudioSource? lcAudioSource;
        public AudioClip? spawnAudio;
        public Light? teleportationLight;
        private bool wasPlayerInShipRoom = false;
        private float teleportTimer = 0;
        private bool isTimerActive = false;
        private float waitBetweenTeleport = 0.8f;

        public override void Update()
        {
            base.Update();

            if (isTimerActive && teleportTimer > 0)
            {
                teleportTimer -= Time.deltaTime;
            } else if (isTimerActive)
            {
                isTimerActive = false;
            }
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            waitBetweenTeleport = GiantExtensionLaddersV2.mySyncedConfigs.teleportFrequency.Value;

            if (!StartOfRound.Instance.inShipPhase && !isTimerActive)
            {
                if (base.IsOwner)
                {
                    playerHeldBy.DiscardHeldObject();
                }

                wasPlayerInShipRoom = false;
                if (StartOfRound.Instance.localPlayerController.isInHangarShipRoom)
                {
                    wasPlayerInShipRoom = true;
                }

                CollectLadders();
            }
        }

        private void CollectLadders()
        {
            List<GrabbableObject> grabableObjectsInScene = [.. FindObjectsOfType<LadderItemScript>()];
            grabableObjectsInScene.AddRange([.. FindObjectsOfType<ExtensionLadderItem>()]);

            StartCoroutine(SpawnLaddersAnim(grabableObjectsInScene));
        }

        private IEnumerator SpawnLaddersAnim(List<GrabbableObject> ladders)
        {
            List<GrabbableObject> laddersToTeleport = new List<GrabbableObject>();

            // Filter ladders to teleport
            foreach (var ladder in ladders)
            {
                bool areLadderTeleportConditionsMet = (ladder.isInFactory == StartOfRound.Instance.localPlayerController.isInsideFactory) && (!ladder.isInShipRoom || GiantExtensionLaddersV2.mySyncedConfigs.isTeleportFromShipRoomEnabled);
                if (areLadderTeleportConditionsMet)
                {
                    laddersToTeleport.Add(ladder);
                }
            }

            teleportTimer = laddersToTeleport.Count * waitBetweenTeleport + 1;
            isTimerActive = true;

            // teleport all ladders
            for (int i = 0; i < laddersToTeleport.Count; i++)
            {
                LadderItemScript? ladder = laddersToTeleport[i] as LadderItemScript;
                ExtensionLadderItem? normalLadder = laddersToTeleport[i] as ExtensionLadderItem;
                float baseNodeRotationAmount = 360f / laddersToTeleport.Count;

                baseNode.transform.rotation = Quaternion.Euler(0, i * baseNodeRotationAmount, 0);

                if (ladder == null && normalLadder == null)
                {
                    yield return null;
                    continue;
                }

                yield return TeleportLadderItem(ladder, normalLadder, ladderSpawnNode.position, baseNode.transform.rotation);
            }
        }

        private IEnumerator TeleportLadderItem(LadderItemScript? ladder, ExtensionLadderItem? normalLadder, Vector3 position, Quaternion rotation)
        {
            GrabbableObject? item = (GrabbableObject?)ladder ?? normalLadder;

            if (item == null)
            {
                yield return null;
                yield break;
            }

            bool isPocketed = ladder?.isPocketed ?? normalLadder?.isPocketed ?? false;
            bool isHeld = ladder?.isHeld ?? normalLadder?.isHeld ?? false;
            bool isHeldByEnemy = ladder?.isHeldByEnemy ?? normalLadder?.isHeldByEnemy ?? false;
            bool isBeingUsed = ladder?.isBeingUsed ?? normalLadder?.isBeingUsed ?? false;
            bool ladderAnimationBegun = ladder?.ladderAnimationBegun ?? normalLadder?.ladderAnimationBegun ?? false;

            if (!isPocketed && !isHeld && !isHeldByEnemy && !isBeingUsed && (!ladderAnimationBegun || GiantExtensionLaddersV2.mySyncedConfigs.isCollectExtendedLaddersEnabled))
            {
                teleportationLight.intensity = 0;
                StartCoroutine(TeleportLightAnim());

                lcAudioSource.pitch = Random.Range(0.94f, 1.06f);
                lcAudioSource.PlayOneShot(spawnAudio);

                item.transform.position = position;
                item.transform.localEulerAngles = Vector3.zero;
                item.transform.eulerAngles = Vector3.zero;
                item.transform.localRotation = Quaternion.Euler(0, 0, 0);
                item.transform.rotation = rotation;

                item.FallToGround();
                StartOfRound.Instance.localPlayerController.SetItemInElevator(wasPlayerInShipRoom, false, item);

                yield return new WaitForSeconds(waitBetweenTeleport);
            }
        }

        private IEnumerator TeleportLightAnim()
        {
            float numberOfSteps = 100 * waitBetweenTeleport / 2;
            float maxIntensity = 800;

            float intensityPerStep = maxIntensity / numberOfSteps;

            for (int i = 0; i < numberOfSteps; i++)
            {
                teleportationLight.intensity += intensityPerStep;
                yield return new WaitForSeconds(0.01f);
            }
            for (int i = 0; i < numberOfSteps; i++)
            {
                teleportationLight.intensity -= intensityPerStep;
                yield return new WaitForSeconds(0.01f);
            }

            teleportationLight.intensity = 0;
        }

        public override void __initializeVariables()
        {
            base.__initializeVariables();
        }

        public override string __getTypeName()
        {
            return "LadderCollector";
        }
    }
}
