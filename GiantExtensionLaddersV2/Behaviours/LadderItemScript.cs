using System;
using System.Collections;
using UnityEngine;

namespace GiantExtensionLaddersV2.Behaviours
{
    internal class LadderItemScript : GrabbableObject
    {
        private bool ladderActivated;

        private bool ladderAnimationBegun;

        private Coroutine ladderAnimationCoroutine;

        private RaycastHit hit;

        private int layerMask = 268437761;

        private float rotateAmount;

        private float extendAmount;

        private float ladderTimer;

        private bool ladderBlinkWarning;

        private bool ladderShrunkAutomatically;

        private AudioClip ladderFallSFX;

        public Animator ladderAnimator;

        public Animator ladderRotateAnimator;

        public Transform baseNode;

        public Transform topNode;

        public Transform moveableNode;

        public AudioClip hitRoof;

        public AudioClip fullExtend;

        public AudioClip hitWall;

        public AudioClip ladderExtendSFX;

        public AudioClip ladderShrinkSFX;

        public AudioClip blinkWarningSFX;

        public AudioClip lidOpenSFX;

        public AudioSource ladderAudio;

        public InteractTrigger ladderScript;

        public Collider interactCollider;

        public Collider bridgeCollider;

        public Collider killTrigger;

        //------- custom properties
        public GiantLadderType giantLadderType;
        public float ladderAlarmTime;
        public float ladderExtensionTime;
        public float maxExtension;
        public float minInteractableRotation;
        public float ladderRotateSpeedMultiplier;
        public bool isClimbable = true;
        public bool isClimbableInShip = false;
        public Transform topCollisionNode;

        private const float RAYCAST_DISTANCE_CORRECTION = 4f;
        private bool isOnAnotherLadder = false;

        //rotation collision detection
        private const int checkpointsPerTenMeters = 5;
        private const int minAmountOfChecksPerCheckpoints = 50;
        private const float amountOfChecksMulitplier = 1.2f;
        private const float minDegrees = 9f;
        private const int checkUntilCheckpointNumber = 2;

        public override void Update()
        {
            base.Update();
            calculateExtensionTimes();

            if (!isClimbableInShip)
            {
                if (isInShipRoom)
                {
                    isClimbable = false;
                }
                else
                {
                    isClimbable = true;
                }
            }

            if (playerHeldBy == null && !isHeld && !isHeldByEnemy && reachedFloorTarget && ladderActivated)
            {

                if (Physics.Raycast(base.transform.position, Vector3.down, out var hitInfo, 80f, 268437760, QueryTriggerInteraction.Ignore))
                {
                    if (hitInfo.collider.GetComponentInParent<LadderItemScript>() != null)
                    {
                        isOnAnotherLadder = true;
                    }
                    else
                    {
                        if (isOnAnotherLadder)
                        {
                            FallToGround();
                        }
                        isOnAnotherLadder = false;
                    }
                }

                if (!ladderAnimationBegun)
                {
                    ladderTimer = 0f;
                    StartLadderAnimation();
                }
                else if (ladderAnimationBegun)
                {
                    ladderTimer += Time.deltaTime;
                    if (!ladderBlinkWarning && ladderTimer > ladderAlarmTime)
                    {
                        ladderBlinkWarning = true;
                        ladderAnimator.SetBool("blinkWarning", value: true);
                        ladderAudio.clip = blinkWarningSFX;
                        ladderAudio.Play();
                    }
                    else if (ladderTimer >= ladderExtensionTime)
                    {
                        ladderActivated = false;
                        ladderBlinkWarning = false;
                        ladderAudio.Stop();
                        ladderAnimator.SetBool("blinkWarning", value: false);
                    }
                }
                return;

            }

            if (ladderAnimationBegun)
            {
                ladderAnimationBegun = false;
                ladderAudio.Stop();
                killTrigger.enabled = false;
                bridgeCollider.enabled = false;
                interactCollider.enabled = false;
                if (ladderAnimationCoroutine != null)
                {
                    StopCoroutine(ladderAnimationCoroutine);
                }
                ladderAnimator.SetBool("blinkWarning", value: false);
                ladderAudio.transform.position = base.transform.position;
                ladderAudio.PlayOneShot(ladderShrinkSFX);
                ladderActivated = false;
            }
            if (killTrigger != null)
            {
                killTrigger.enabled = false;
            }
            ladderScript.interactable = false;

            if (GameNetworkManager.Instance.localPlayerController != null && GameNetworkManager.Instance.localPlayerController.currentTriggerInAnimationWith == ladderScript)
            {
                ladderScript.CancelAnimationExternally();
            }
            if (rotateAmount > 0f)
            {
                rotateAmount = Mathf.Max(rotateAmount - Time.deltaTime * 2f, 0f);
                ladderRotateAnimator.SetFloat("rotationAmount", rotateAmount);
            }
            else
            {
                ladderRotateAnimator.SetFloat("rotationAmount", 0f);
            }
            if (extendAmount > 0f)
            {
                extendAmount = Mathf.Max(extendAmount - Time.deltaTime * 2f, 0f);
                ladderAnimator.SetFloat("extensionAmount", extendAmount);
            }
            else
            {
                ladderAnimator.SetBool("openLid", value: false);
                ladderAnimator.SetBool("extend", value: false);
                ladderAnimator.SetFloat("extensionAmount", 0f);
            }
        }

        private void calculateExtensionTimes()
        {
            switch (giantLadderType)
            {
                case GiantLadderType.TINY:
                    ladderAlarmTime = ConfigStuff.MySyncedConfigs.Instance.TINY_LADDER_EXT_TIME - 4;
                    ladderExtensionTime = ConfigStuff.MySyncedConfigs.Instance.TINY_LADDER_EXT_TIME;
                    break;
                case GiantLadderType.BIG:
                    ladderAlarmTime = ConfigStuff.MySyncedConfigs.Instance.BIG_LADDER_EXT_TIME - 5;
                    ladderExtensionTime = ConfigStuff.MySyncedConfigs.Instance.BIG_LADDER_EXT_TIME;
                    break;
                case GiantLadderType.HUGE:
                    ladderAlarmTime = ConfigStuff.MySyncedConfigs.Instance.HUGE_LADDER_EXT_TIME - 5;
                    ladderExtensionTime = ConfigStuff.MySyncedConfigs.Instance.HUGE_LADDER_EXT_TIME;
                    break;
                case GiantLadderType.ULTIMATE:
                    ladderAlarmTime = ConfigStuff.MySyncedConfigs.Instance.ULTIMATE_LADDER_EXT_TIME - 5;
                    ladderExtensionTime = ConfigStuff.MySyncedConfigs.Instance.ULTIMATE_LADDER_EXT_TIME;
                    break;
                default:
                    ladderExtensionTime = 25;
                    ladderAlarmTime = 20;
                    break;
            }
        }

        private void StartLadderAnimation()
        {
            ladderAnimationBegun = true;
            ladderScript.interactable = false;
            if (ladderAnimationCoroutine != null)
            {
                StopCoroutine(ladderAnimationCoroutine);
            }
            ladderAnimationCoroutine = StartCoroutine(LadderAnimation());
        }

        private IEnumerator LadderAnimation()
        {
            ladderAudio.volume = 1f;
            ladderScript.interactable = false;
            interactCollider.enabled = false;
            bridgeCollider.enabled = false;
            killTrigger.enabled = false;

            ladderAnimator.SetBool("openLid", value: false);
            ladderAnimator.SetBool("extend", value: false);
            yield return null;

            ladderAnimator.SetBool("openLid", value: true);
            ladderAudio.transform.position = base.transform.position;
            ladderAudio.PlayOneShot(lidOpenSFX, 1f);
            RoundManager.Instance.PlayAudibleNoise(ladderAudio.transform.position, 18f, 0.8f, 0, isInShipRoom);
            yield return new WaitForSeconds(1f);

            ladderAnimator.SetBool("extend", value: true);
            float ladderMaxExtension = GetLadderExtensionDistance();
            float ladderExtendAmountNormalized = ladderMaxExtension / maxExtension;
            float ladderRotateAmountNormalized = Mathf.Clamp(GetLadderRotationDegrees(ladderExtendAmountNormalized) / -90f, 0f, 0.99f);
            ladderAudio.clip = ladderExtendSFX;
            ladderAudio.Play();
            float currentNormalizedTime2 = 0f;
            float speedMultiplier2 = 0.1f;

            ladderMaxExtension += baseNode.transform.position.y + RAYCAST_DISTANCE_CORRECTION;

            while (currentNormalizedTime2 < 2 && topCollisionNode.position.y < ladderMaxExtension)
            {
                //GiantExtensionLaddersV2.mls.LogDebug("currentNormalizedTime2: " + currentNormalizedTime2 + " topCollisionNode.position.y: " + topCollisionNode.position.y + " --- ladderMaxExtension pos y: " + ladderMaxExtension);

                speedMultiplier2 += Time.deltaTime * 2f;
                currentNormalizedTime2 = Mathf.Min(currentNormalizedTime2 + Time.deltaTime * speedMultiplier2, 2);
                ladderAnimator.SetFloat("extensionAmount", currentNormalizedTime2);
                yield return null;
            }

            extendAmount = currentNormalizedTime2;
            interactCollider.enabled = true;
            bridgeCollider.enabled = false;
            killTrigger.enabled = false;
            ladderAudio.Stop();

            if (ladderExtendAmountNormalized == 1f)
            {
                ladderAudio.transform.position = baseNode.transform.position + baseNode.transform.up * maxExtension;
                ladderAudio.PlayOneShot(fullExtend, 0.7f);
                WalkieTalkie.TransmitOneShotAudio(ladderAudio, fullExtend, 0.7f);
                RoundManager.Instance.PlayAudibleNoise(ladderAudio.transform.position, 8f, 0.5f, 0, isInShipRoom);
            }
            else
            {
                ladderAudio.transform.position = baseNode.transform.position + baseNode.transform.up * (ladderExtendAmountNormalized * maxExtension);
                ladderAudio.PlayOneShot(hitRoof);
                WalkieTalkie.TransmitOneShotAudio(ladderAudio, hitRoof);
                RoundManager.Instance.PlayAudibleNoise(ladderAudio.transform.position, 17f, 0.8f, 0, isInShipRoom);
            }
            yield return new WaitForSeconds(0.4f);

            ladderAudio.clip = ladderFallSFX;
            ladderAudio.Play();
            ladderAudio.volume = 0f;
            speedMultiplier2 = ladderRotateSpeedMultiplier;
            currentNormalizedTime2 = 0f;

            while (currentNormalizedTime2 < ladderRotateAmountNormalized)
            {
                speedMultiplier2 += Time.deltaTime * 2f;
                currentNormalizedTime2 = Mathf.Min(currentNormalizedTime2 + Time.deltaTime * speedMultiplier2, ladderRotateAmountNormalized);
                if (ladderExtendAmountNormalized > 0.6f && currentNormalizedTime2 > 0.5f)
                {
                    killTrigger.enabled = true;
                }
                ladderAudio.volume = Mathf.Min(ladderAudio.volume + Time.deltaTime * 1.75f, 1f);
                ladderRotateAnimator.SetFloat("rotationAmount", currentNormalizedTime2);
                yield return null;
            }

            rotateAmount = ladderRotateAmountNormalized;
            ladderAudio.volume = 1f;
            ladderAudio.Stop();
            ladderAudio.transform.position = moveableNode.transform.position;
            ladderAudio.PlayOneShot(hitWall, Mathf.Min(ladderRotateAmountNormalized + 0.3f, 1f));
            RoundManager.Instance.PlayAudibleNoise(ladderAudio.transform.position, 18f, 0.7f, 0, isInShipRoom);

            if (isClimbable && ladderRotateAmountNormalized * 90f < minInteractableRotation)
            {
                ladderScript.interactable = true;
                interactCollider.enabled = true;
            }
            else
            {
                bridgeCollider.enabled = true;
            }
            killTrigger.enabled = false;
        }

        private float GetLadderExtensionDistance()
        {
            if (Physics.Raycast(baseNode.transform.position, Vector3.up, out hit, maxExtension, layerMask, QueryTriggerInteraction.Ignore))
            {
                if (this.isInShipRoom || this.isInFactory)
                {
                    return hit.distance;
                }
            }
            return maxExtension;
        }

        private float GetLadderRotationDegrees(float topOfLadder)
        {
            int amountOfLadderCheckpoints = (int) Math.Ceiling((maxExtension / 10) * checkpointsPerTenMeters);
            float ladderSectionsLength = maxExtension / amountOfLadderCheckpoints;

            int amountOfChecksPerCheckpoint = minAmountOfChecksPerCheckpoints;
            float rotationAmountBetweenChecks = 90f / amountOfChecksPerCheckpoint;

            float currentLowestDegree = 90f; //lowest degree where collision occured

            //big for-loop is for checks on each checkpoint, starting from top to bottom
            for (int currentCheckPointNumber = amountOfLadderCheckpoints; currentCheckPointNumber > checkUntilCheckpointNumber; currentCheckPointNumber--)
            {
                amountOfChecksPerCheckpoint = minAmountOfChecksPerCheckpoints;
                amountOfChecksPerCheckpoint += (int) (amountOfChecksMulitplier * currentCheckPointNumber);
                rotationAmountBetweenChecks = 90f / amountOfChecksPerCheckpoint;

                //sets current checkpoint position and resets the rotation of base node.
                //MovableNode is a position along the ladder frame, base node is a parent of MovableNode located at the ladderbox
                float yPositionOnTheLadder = ladderSectionsLength * (float)currentCheckPointNumber;
                moveableNode.transform.localPosition = new Vector3(0f, yPositionOnTheLadder, 0f);
                baseNode.localEulerAngles = Vector3.zero;

                //for-loop: all checks for collision on a single checkpoint
                for (int i = 2; i < amountOfChecksPerCheckpoint; i++)
                {
                    //position before and after rotating one step
                    Vector3 checkpointPosition = moveableNode.transform.position;
                    baseNode.localEulerAngles = new Vector3((float)(-i) * rotationAmountBetweenChecks, 0f, 0f);
                    Vector3 checkpointPositionAfterOneRotationStep = moveableNode.transform.position;

                    //if collision between those points is detected, store previous rotation amount and go to next checkpoint
                    if (Physics.Linecast(checkpointPosition, checkpointPositionAfterOneRotationStep, layerMask, QueryTriggerInteraction.Ignore))
                    {
                        float previousRotationAmount = (float)(i - 2) * rotationAmountBetweenChecks;
                        if (previousRotationAmount < currentLowestDegree)
                        {
                            currentLowestDegree = previousRotationAmount;
                        }
                        break;
                    }
                }

                if (currentLowestDegree < minDegrees)
                {
                    break;
                }
            }
            return -currentLowestDegree;
        }

        public override void DiscardItem()
        {
            base.DiscardItem();
        }

        public override void EquipItem()
        {
            base.EquipItem();
        }

        public override void DiscardItemFromEnemy()
        {
            base.DiscardItemFromEnemy();
            ladderActivated = true;
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            ladderActivated = true;
            if (base.IsOwner)
            {
                playerHeldBy.DiscardHeldObject();
            }
        }

        public override void __initializeVariables()
        {
            base.__initializeVariables();
        }

        public override string __getTypeName()
        {
            return "BigLadderItem";
        }
    }

}
