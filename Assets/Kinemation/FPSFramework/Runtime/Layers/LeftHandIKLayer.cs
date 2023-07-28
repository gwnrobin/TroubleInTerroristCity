// Designed by Kinemation, 2023

using System.Collections.Generic;
using Kinemation.FPSFramework.Runtime.Core.Components;
using Kinemation.FPSFramework.Runtime.Core.Types;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;

namespace Kinemation.FPSFramework.Runtime.Layers
{
    public struct BoneTransform
    {
        public Transform bone;
        public Quaternion rotation;

        public BoneTransform(Transform boneRef)
        {
            bone = boneRef;
            rotation = Quaternion.identity;
        }

        public void CopyBone()
        {
            rotation = bone.localRotation;
        }
    }
    
    public class LeftHandIKLayer : AnimLayer
    {
        [Header("Layer Blending")]
        [AnimCurveName] public string maskCurveName;
        public Transform leftHandTarget;
        public AvatarMask leftHandMask;

        private LocRot _cache = LocRot.identity;
        private LocRot _final = LocRot.identity;
        
        private LocRot defaultLeftHand = LocRot.identity;
        private List<BoneTransform> leftHandChain = new List<BoneTransform>();

        public override void OnAnimStart()
        {
            if (leftHandMask == null)
            {
                Debug.LogWarning("LeftHandIKLayer: no mask for the left hand assigned!");
                return;
            }
            
            leftHandChain.Clear();
            for (int i = 1; i < leftHandMask.transformCount; i++)
            {
                if (leftHandMask.GetTransformActive(i))
                {
                    var t = transform.Find(leftHandMask.GetTransformPath(i));
                    leftHandChain.Add(new BoneTransform(t));
                }
            }
        }

        public override void OnPoseSampled()
        {
            _cache = _final;

            if (leftHandMask == null) return;

            for (int i = 0; i < leftHandChain.Count; i++)
            {
                var bone = leftHandChain[i];
                bone.CopyBone();
                leftHandChain[i] = bone;
            }

            var rotOffset = GetGunAsset() != null ? GetGunAsset().rotationOffset : GetGunData().rotationOffset;
            
            GetPivotPoint().rotation *= rotOffset;
            defaultLeftHand.position = GetPivotPoint().InverseTransformPoint(GetLeftHandIK().target.position);
            defaultLeftHand.rotation = Quaternion.Inverse(GetPivotPoint().rotation) * GetLeftHandIK().target.rotation;
            GetPivotPoint().rotation *= Quaternion.Inverse(rotOffset);
        }

        private void OverrideLeftHand(float weight)
        {
            weight = Mathf.Clamp01(weight);
            foreach (var bone in leftHandChain)
            {
                bone.bone.localRotation = Quaternion.Slerp(bone.bone.localRotation, bone.rotation, weight);
            }
        }

        public override void OnAnimUpdate()
        {
            var basePos = GetMasterPivot().InverseTransformPoint(GetLeftHand().position) + GetPivotOffset();
            var baseRot = 
                Quaternion.Inverse(Quaternion.Inverse(GetMasterPivot().rotation) * GetLeftHand().rotation);

            LocRot handTransform;
            if (GetTransforms().leftHandTarget == null)
            {
                handTransform = defaultLeftHand;
            }
            else
            {
                var target = GetTransforms().leftHandTarget;
                handTransform = new LocRot(target.localPosition, target.localRotation);
            }
            
            float alpha = (1f - GetCurveValue(maskCurveName)) * (1f - smoothLayerAlpha) * layerAlpha;
            float progress = core.animGraph.GetPoseProgress();
            
            handTransform.position -= basePos;
            handTransform.rotation *= baseRot;

            _final = CoreToolkitLib.Lerp(_cache, handTransform, progress);

            OverrideLeftHand(alpha);
            GetLeftHandIK().Move(GetMasterPivot(), _final.position, alpha);
            GetLeftHandIK().Rotate(GetMasterPivot().rotation, 
                _final.rotation, alpha);
        }
    }
}