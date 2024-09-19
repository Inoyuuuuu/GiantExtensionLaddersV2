using System;
using System.Collections;
using UnityEngine;

namespace GiantExtensionLaddersV2.Behaviours
{
    internal class LadderItemScript : GrabbableObject
    {
        private bool ladderActivated;

        internal bool ladderAnimationBegun;

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
        public bool isAlwaysExtended = false;
        public Transform topCollisionNode;

        private const float RAYCAST_DISTANCE_CORRECTION = 4f;
        private bool isOnAnotherLadder = false;
        private bool hasFallenOnALadder = false;
        private bool isLeaningAgainstALadder = false;
        private bool hasHitRoof = false;

        private Vector3 linecastStart = Vector3.zero;
        private Vector3 linecastEnd = Vector3.zero;

        //rotation collision detection
        private const int checkpointsPerTenMeters = 10;
        private const int minAmountOfChecksPerCheckpoints = 50;
        private const float amountOfChecksMulitplier = 1.2f;
        private const float minDegrees = 9f;
        private const int startingCheckPointNumber = 2;

        //public override void Start()
        //{
        //    base.Start();
        //    for (int i = 0; i < this.propColliders.Length; i++)
        //    {
        //        this.propColliders[i].excludeLayers = 0;
        //    }
        //}

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

            if (this.playerHeldBy == null && !this.isHeld && !this.isHeldByEnemy && this.reachedFloorTarget && this.ladderActivated)
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
                    StartLadderAnimation(false, -1);
                }
                else if (ladderAnimationBegun)
                {

                    if (hasFallenOnALadder)
                    {
                        if (!Physics.Linecast(linecastStart, linecastEnd, out var ladderCheckLinecast, layerMask, QueryTriggerInteraction.Ignore))
                        {
                            StartLadderAnimation(true, rotateAmount);
                            hasFallenOnALadder = false;
                        }
                    }

                    if (!isAlwaysExtended)
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
                    isAlwaysExtended = GiantExtensionLaddersV2.mySyncedConfigs.isTinyLadderAlwaysActive;
                    ladderAlarmTime = GiantExtensionLaddersV2.mySyncedConfigs.tinyLadderExtTime - 4;
                    ladderExtensionTime = GiantExtensionLaddersV2.mySyncedConfigs.tinyLadderExtTime;
                    break;
                case GiantLadderType.BIG:
                    isAlwaysExtended = GiantExtensionLaddersV2.mySyncedConfigs.isBigLadderAlwaysActive;
                    ladderAlarmTime = GiantExtensionLaddersV2.mySyncedConfigs.bigLadderExtTime - 5;
                    ladderExtensionTime = GiantExtensionLaddersV2.mySyncedConfigs.bigLadderExtTime;
                    break;
                case GiantLadderType.HUGE:
                    isAlwaysExtended = GiantExtensionLaddersV2.mySyncedConfigs.isHugeLadderAlwaysActive;
                    ladderAlarmTime = GiantExtensionLaddersV2.mySyncedConfigs.hugeLadderExtTime - 5;
                    ladderExtensionTime = GiantExtensionLaddersV2.mySyncedConfigs.hugeLadderExtTime;
                    break;
                case GiantLadderType.ULTIMATE:
                    isAlwaysExtended = GiantExtensionLaddersV2.mySyncedConfigs.isUltimateLadderAlwaysActive;
                    ladderAlarmTime = GiantExtensionLaddersV2.mySyncedConfigs.ultimateLadderExtTime - 5;
                    ladderExtensionTime = GiantExtensionLaddersV2.mySyncedConfigs.ultimateLadderExtTime;
                    break;
                default:
                    isAlwaysExtended = false;
                    ladderExtensionTime = 25;
                    ladderAlarmTime = 20;
                    break;
            }
        }

        private void StartLadderAnimation(bool isSkipExtension, float externalRotNormalTime)
        {
            ladderAnimationBegun = true;
            ladderScript.interactable = false;
            if (ladderAnimationCoroutine != null)
            {
                StopCoroutine(ladderAnimationCoroutine);
            }
            ladderAnimationCoroutine = StartCoroutine(LadderAnimation(isSkipExtension, externalRotNormalTime));
        }

        private IEnumerator LadderAnimation(bool isSkipExtension, float externalRotNormalTime)
        {
            float ladderMaxExtension = GetLadderExtensionDistance();
            float ladderExtendAmountNormalized = ladderMaxExtension / maxExtension;
            float ladderRotateAmountNormalized = 1; //calculated later on

            float currentNormalizedTime = 0f;
            float extensionSpeedMultiplier2 = 0.1f;

            ladderAudio.volume = 1f;
            ladderScript.interactable = false;
            interactCollider.enabled = false;
            bridgeCollider.enabled = false;
            killTrigger.enabled = false;

            if (!isSkipExtension)
            {

                ladderAnimator.SetBool("openLid", value: false);
                ladderAnimator.SetBool("extend", value: false);
                yield return null;

                ladderAnimator.SetBool("openLid", value: true);
                ladderAudio.transform.position = base.transform.position;
                ladderAudio.PlayOneShot(lidOpenSFX, 1f);
                RoundManager.Instance.PlayAudibleNoise(ladderAudio.transform.position, 18f, 0.8f, 0, isInShipRoom);
                yield return new WaitForSeconds(1f);

                ladderAnimator.SetBool("extend", value: true);


                ladderAudio.clip = ladderExtendSFX;
                ladderAudio.Play();

                ladderMaxExtension += baseNode.transform.position.y + RAYCAST_DISTANCE_CORRECTION;

                while (currentNormalizedTime < 2 && topCollisionNode.position.y < ladderMaxExtension)
                {
                    extensionSpeedMultiplier2 += Time.deltaTime * 2f;
                    currentNormalizedTime = Mathf.Min(currentNormalizedTime + Time.deltaTime * extensionSpeedMultiplier2, 2);
                    ladderAnimator.SetFloat("extensionAmount", currentNormalizedTime);
                    yield return null;
                }

                if (topCollisionNode.position.y >= ladderMaxExtension)
                {
                    hasHitRoof = true;
                } else
                {
                    hasHitRoof = false;
                }

                extendAmount = currentNormalizedTime;

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
            }

            ladderAudio.clip = ladderFallSFX;
            ladderAudio.Play();
            ladderAudio.volume = 0f;

            extensionSpeedMultiplier2 = ladderRotateSpeedMultiplier;
            currentNormalizedTime = 0f;

            ladderRotateAmountNormalized = Mathf.Clamp(GetLadderRotationDegrees(90f) / -90f, 0f, 0.99f);

            if (externalRotNormalTime > 0.01f)
            {
                //float ladderDegreeCheckStart = 90f * externalRotNormalTime;
                //float ladderRotDegrees = GetLadderRotationDegrees(ladderDegreeCheckStart);

                //GiantExtensionLaddersV2.mls.LogInfo("started anim with ext skip");
                //GiantExtensionLaddersV2.mls.LogInfo("externalRotNormalTime was: " + externalRotNormalTime);
                //GiantExtensionLaddersV2.mls.LogInfo("0. ladderRotDegrees: " + ladderRotDegrees);

                //ladderRotDegrees /= -ladderDegreeCheckStart;   
                //GiantExtensionLaddersV2.mls.LogInfo("0.5 ladderRotDegrees div: " + ladderRotDegrees);

                //ladderRotateAmountNormalized = Mathf.Clamp(ladderRotDegrees, 0f, 0.99f);
                //GiantExtensionLaddersV2.mls.LogInfo("1. ladderRotateAmountNormalized: " + ladderRotateAmountNormalized);

                ladderRotateAmountNormalized = 0.99f;
            }

            if (externalRotNormalTime > 0)
            {
                currentNormalizedTime = externalRotNormalTime;
            }

            while (currentNormalizedTime < ladderRotateAmountNormalized)
            {

                extensionSpeedMultiplier2 += Time.deltaTime * 2f;
                currentNormalizedTime = Mathf.Min(currentNormalizedTime + Time.deltaTime * extensionSpeedMultiplier2, ladderRotateAmountNormalized);
                if (currentNormalizedTime > 0.3f && ladderExtendAmountNormalized > 0.6f)
                {
                    killTrigger.enabled = true;
                }
                ladderAudio.volume = Mathf.Min(ladderAudio.volume + Time.deltaTime * 1.75f, 1f);
                ladderRotateAnimator.SetFloat("rotationAmount", currentNormalizedTime);

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

            if (hasHitRoof)
            {
                float newColliderScale = (((100 - maxExtension) / 100) * 5.16f) / 6;
                interactCollider.transform.localScale = new Vector3(interactCollider.transform.localScale.x, newColliderScale, interactCollider.transform.localScale.z);
                bridgeCollider.transform.localScale = new Vector3(bridgeCollider.transform.localScale.x, newColliderScale, bridgeCollider.transform.localScale.z);
                
                interactCollider.transform.localPosition = new Vector3(interactCollider.transform.localPosition.x, 1, interactCollider.transform.localPosition.z);
                bridgeCollider.transform.localPosition = new Vector3(bridgeCollider.transform.localPosition.x, 1, bridgeCollider.transform.localPosition.z);
            } else
            {
                interactCollider.transform.localScale = new Vector3(interactCollider.transform.localScale.x, 5.15892f, interactCollider.transform.localScale.z);
                bridgeCollider.transform.localScale = new Vector3(bridgeCollider.transform.localScale.x, 5.15892f, bridgeCollider.transform.localScale.z);

                interactCollider.transform.localPosition = new Vector3(interactCollider.transform.localPosition.x, 3.8f, interactCollider.transform.localPosition.z);
                bridgeCollider.transform.localPosition = new Vector3(bridgeCollider.transform.localPosition.x, 3.29f, bridgeCollider.transform.localPosition.z);

            }
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

        private float GetLadderRotationDegrees(float startAt)
        {
            int amountOfLadderCheckpoints = (int) Math.Ceiling((maxExtension / 10) * checkpointsPerTenMeters);
            float ladderSectionsLength = maxExtension / amountOfLadderCheckpoints;
            int amountOfChecksPerCheckpoint;
            float currentLowestDegree = 90f;                                       //lowest degree where collision occured
            float rotationAmountBetweenChecks;


            //big for-loop is for checks on each checkpoint, starting from bot to top
            for (int currentCheckPointNumber = startingCheckPointNumber; currentCheckPointNumber < amountOfLadderCheckpoints; currentCheckPointNumber++)
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
                    if (Physics.Linecast(checkpointPosition, checkpointPositionAfterOneRotationStep, out var hitInfo, layerMask, QueryTriggerInteraction.Ignore))
                    {
                        float previousTotalRotation = (float)(i - 1.8f) * rotationAmountBetweenChecks;
                        if (previousTotalRotation < currentLowestDegree)
                        {

                            LadderItemScript ladderItemScript = hitInfo.collider.GetComponentInParent<LadderItemScript>();
                            if (ladderItemScript != null && ladderItemScript.GetInstanceID() != this.GetInstanceID())
                            {
                                linecastStart = checkpointPosition;
                                linecastEnd = checkpointPositionAfterOneRotationStep;
                                hasFallenOnALadder = true;
                            }
                            else
                            {
                                hasFallenOnALadder = false;
                            }

                            currentLowestDegree = previousTotalRotation;
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
            if (ladderAnimationCoroutine != null)
            {
                StopCoroutine(ladderAnimationCoroutine);
            }

            base.EquipItem();
        }

        public override void DiscardItemFromEnemy()
        {
            base.DiscardItemFromEnemy();
            this.ladderActivated = true;
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            isOnAnotherLadder = false;
            hasFallenOnALadder = false;
            isLeaningAgainstALadder = false;
            hasHitRoof = false;
            rotateAmount = -1;
            linecastStart = Vector3.zero;
            linecastEnd = Vector3.zero;
            if (ladderAnimationCoroutine != null)
            {
                StopCoroutine(ladderAnimationCoroutine);
            }

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
