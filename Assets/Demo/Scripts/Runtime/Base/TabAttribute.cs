// Designed by Kinemation, 2023

namespace Demo.Scripts.Runtime.Base
{
    using UnityEngine;

    public class TabAttribute : PropertyAttribute
    {
        public readonly string tabName;

        public TabAttribute(string tabName)
        {
            this.tabName = tabName;
        }
    }
}