// Designed by Kinemation, 2023

using UnityEngine;

namespace Demo.Scripts.Runtime.Base
{
    public class AnimEventReceiver : MonoBehaviour
    {
        [SerializeField] private PlayerAnimController controller;

        private void Start()
        {
            if (controller == null)
            {
                controller = GetComponentInParent<PlayerAnimController>();
            }
        }
        
        public void SetActionActive(int isActive)
        {
            controller.SetActionActive(isActive);
        }
        /*
        public void ChangeWeapon()
        {
            controller.EquipWeapon();
        }*/
    }
}
