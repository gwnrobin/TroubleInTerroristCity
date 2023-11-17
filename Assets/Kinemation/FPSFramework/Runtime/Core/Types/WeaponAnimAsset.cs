// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Runtime.Recoil;
using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.Core.Types
{
    [System.Serializable, CreateAssetMenu(fileName = "NewWeaponAsset", menuName = "FPS Animator/WeaponAnimAsset")]
    public class WeaponAnimAsset : ScriptableObject
    {
        [Header("General"), Tooltip("Adjusts weapon model rotation")]
        public Quaternion rotationOffset = Quaternion.identity;
        public AimOffsetTable aimOffsetTable;
        public RecoilAnimData recoilData;
        public LocRot weaponBone = LocRot.identity;
        [Header("Animations"), Tooltip("Adjusts weapon model rotation")]
        public AnimSequence overlayPose;
        public AnimSequence reloadClip;
        public AnimSequence grenadeClip;
        
        [Header("AdsLayer")]
        public AdsData adsData;
        
        [Tooltip("Offsets the arms pose")]
        public LocRot viewOffset = LocRot.identity;
        
        [Header("SwayLayer")]
        [Tooltip("Aiming sway")] public LocRotSpringData springData;
        public FreeAimData freeAimData;
        public MoveSwayData moveSwayData;
        
        [Header("WeaponCollision")] 
        public GunBlockData blockData;
    }
}