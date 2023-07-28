// Designed by Kinemation, 2023

using Kinemation.FPSFramework.Runtime.Core.Components;
using Kinemation.FPSFramework.Runtime.Core.Types;
using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.Layers
{
    public class WeaponCollision : AnimLayer
    {
        [SerializeField] protected LayerMask layerMask;
        
        protected Vector3 start;
        protected Vector3 end;
        protected LocRot smoothPose;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(start, end);
        }

        public override void OnAnimUpdate()
        {
            var blockData = GetGunAsset() != null ? GetGunAsset().blockData : GetGunData().blockData;
            
            float traceLength = blockData.weaponLength;
            float startOffset = blockData.startOffset;
            float threshold = blockData.threshold;
            LocRot restPose = blockData.restPose;
            
            start = GetMasterPivot().position - GetMasterPivot().forward * startOffset;
            end = start + GetMasterPivot().forward * traceLength;
            
            LocRot offsetPose = new LocRot(Vector3.zero, Quaternion.identity);
            if (Physics.Raycast(start, GetMasterPivot().forward, out RaycastHit hit, traceLength, layerMask))
            {
                float distance = (end - start).magnitude - (hit.point - start).magnitude;
                if (distance > threshold)
                {
                    offsetPose = restPose;
                }
                else
                {
                    offsetPose.position = new Vector3(0f, 0f, -distance);
                    offsetPose.rotation = Quaternion.Euler(0f, 0f, 15f * (distance / threshold));
                }
            }

            smoothPose = CoreToolkitLib.Glerp(smoothPose, offsetPose, 10f);
            
            GetMasterIK().Move(smoothPose.position, smoothLayerAlpha);
            GetMasterIK().Rotate(smoothPose.rotation, smoothLayerAlpha);
        }
    }
}