using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GiantExtensionLaddersV2
{
    internal class LadderObject
    {
        internal List<MeshRenderer> meshRenderers { get; set; }
        internal List<Animator> animators { get; set; }
        internal List<Transform> transforms { get; set; }
        internal List<AudioClip> audioClips { get; set; }
        internal List<AudioSource> audioSources { get; set; }
        internal List<InteractTrigger> interactTriggers { get; set; }
        internal List<BoxCollider> boxColliders { get; set; }
        internal GameObject ladderPrefab { get; set; }
        internal GiantLadderType ladderType { get; private set; }

        internal float LADDER_MAX_EXTENSION { get; private set; }
        internal float LADDER_MIN_ROTATION_COLLISION { get; private set; }
        internal int LADDER_LINECAST_CHECKS_MULTIPLIER { get; private set; }
        internal int LADDER_LINECAST_MIN_CHECK_HEIGHT { get; private set; }
        internal float LADDER_HEIGHT_MULTIPLIER { get; private set; }
        internal float LADDER_ROTATE_SPEED { get; private set; }
        internal bool LADDER_IS_CLIMBABLE { get; private set; }

        public LadderObject(float ladderMaxExtension, float ladderMinRotationCollision, int ladderLinecastChecksMultiplier,
            int ladderLinecastMinCheckHeight, float ladderHeightMultiplier, float ladderRotateSpeed, bool ladderIsClimbable, 
            GiantLadderType type)
        {
            LADDER_MAX_EXTENSION = ladderMaxExtension;
            LADDER_MIN_ROTATION_COLLISION = ladderMinRotationCollision;
            LADDER_LINECAST_CHECKS_MULTIPLIER = ladderLinecastChecksMultiplier;
            LADDER_LINECAST_MIN_CHECK_HEIGHT = ladderLinecastMinCheckHeight;
            LADDER_HEIGHT_MULTIPLIER = ladderHeightMultiplier;
            LADDER_ROTATE_SPEED = ladderRotateSpeed;
            LADDER_IS_CLIMBABLE = ladderIsClimbable;
            this.ladderType = type;
        }
    }
}
