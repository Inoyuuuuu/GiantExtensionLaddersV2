using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        //private bool ladderShrunkAutomatically;

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
        public float minRotationCollisionCheck;
        public int linecastChecksMultiplier;
        public int linecastMinCheckHeight;
        public float ladderHeightMultiplier; //this is for line 272, where 2.43 * x = ladder height
        public float ladderRotateSpeedMultiplier;
        public bool isClimbable = true;

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
                default:
                    ladderExtensionTime = 25;
                    ladderAlarmTime = 20;
                    break;
            }
        }

        public override void Update()
        {
            calculateExtensionTimes();
            base.Update();

            if (playerHeldBy == null && !isHeld && !isHeldByEnemy && reachedFloorTarget && ladderActivated)
            {

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

            killTrigger.enabled = false;
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
            float ladderExtendAmountNormalized = GetLadderExtensionDistance() / maxExtension;
            float ladderRotateAmountNormalized = Mathf.Clamp(GetLadderRotationDegrees(ladderExtendAmountNormalized) / -90f, 0f, 0.99f);
            ladderAudio.clip = ladderExtendSFX;
            ladderAudio.Play();
            float currentNormalizedTime2 = 0f;
            float speedMultiplier2 = 0.1f;

            while (currentNormalizedTime2 < ladderExtendAmountNormalized)
            {
                speedMultiplier2 += Time.deltaTime * 2f;
                currentNormalizedTime2 = Mathf.Min(currentNormalizedTime2 + Time.deltaTime * speedMultiplier2, ladderExtendAmountNormalized);
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
            speedMultiplier2 = 0.15f;
            currentNormalizedTime2 = 0f;

            while (currentNormalizedTime2 < ladderRotateAmountNormalized)
            {
                speedMultiplier2 += Time.deltaTime * ladderRotateSpeedMultiplier;
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

            if (isClimbable && ladderRotateAmountNormalized * 90f < minRotationCollisionCheck)
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
                return hit.distance;
            }
            return maxExtension;
        }

        //------- Gets the intersection point between objects and ladder
        private float GetLadderRotationDegrees(float topOfLadder)
        {
            float num = 90f;
            for (float num2 = ladderHeightMultiplier * linecastChecksMultiplier; num2 >= linecastMinCheckHeight; num2--)
            {
                float y = (2.43f / linecastChecksMultiplier) * (float)num2;
                moveableNode.transform.localPosition = new Vector3(0f, y, 0f);
                baseNode.localEulerAngles = Vector3.zero;
                for (int i = 1; i < 40; i++)
                {
                    Vector3 position = moveableNode.transform.position;
                    baseNode.localEulerAngles = new Vector3((float)(-i / 2) * 4.5f, 0f, 0f);
                    if (Physics.Linecast(position, moveableNode.transform.position, layerMask, QueryTriggerInteraction.Ignore))
                    {
                        float num3 = (float)((i / 2) - 1) * 4.5f;
                        if (num3 < num)
                        {
                            num = num3;
                        }
                        break;
                    }
                }
                if (num <= 8f)
                {
                    break;
                }
            }
            return 0f - num;
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
