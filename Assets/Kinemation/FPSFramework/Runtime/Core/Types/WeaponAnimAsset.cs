// Designed by Kinemation, 2023

using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.Core.Types
{
    public class WeaponAnimAsset : ScriptableObject
    {
        [Header("Weapon Transform"), Tooltip("Adjusts weapon model rotation")]
        public Quaternion rotationOffset = Quaternion.identity;
        
        [Header("AdsLayer")]
        public AdsData adsData;
        
        [Tooltip("Offsets the arms pose")]
        public LocRot viewOffset = LocRot.identity;
        
        [Header("SwayLayer")]
        
        [Tooltip("Aiming sway")]
        public LocRotSpringData springData;
        public FreeAimData freeAimData;
        public MoveSwayData moveSwayData;
        
        [Header("WeaponCollision")] 
        public GunBlockData blockData;
    }
}