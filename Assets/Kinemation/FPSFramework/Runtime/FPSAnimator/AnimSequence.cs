// Designed by Kinemation, 2023

using System.Collections.Generic;
using Kinemation.FPSFramework.Runtime.Core.Types;
using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.FPSAnimator
{
    [System.Serializable]
    public class AnimSequence : ScriptableObject
    {
        public AnimationClip clip = null;
        public AvatarMask mask = null;
        public BlendTime blendTime;
        public List<AnimCurve> curves;
    }
}