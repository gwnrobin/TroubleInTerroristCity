// Designed by Kinemation, 2023

using System;
using System.Collections.Generic;
using Kinemation.FPSFramework.Runtime.FPSAnimator;
using UnityEngine;

namespace Demo.Scripts.Runtime.Base
{
    public enum OverlayType
    {
        Default,
        Pistol,
        Rifle
    }
    
    public class Weapon : FPSAnimWeapon
    {
        public AnimSequence reloadClip;
        public AnimSequence grenadeClip;
        public OverlayType overlayType;
        
        [SerializeField] private List<Transform> scopes;
        private Animator _animator;
        private int _scopeIndex;

        protected void Start()
        {
            _animator = GetComponentInChildren<Animator>();
        }

        public override Transform GetAimPoint()
        {
            _scopeIndex++;
            _scopeIndex = _scopeIndex > scopes.Count - 1 ? 0 : _scopeIndex;
            return scopes[_scopeIndex];
        }
        
        public void OnFire()
        {
            if (_animator == null)
            {
                return;
            }

            _animator.Play("Fire", 0, 0f);
        }

        public void Reload()
        {
            if (_animator == null)
            {
                return;
            }
            
            _animator.Play("Reload", 0, 0f);
        }
    }
}