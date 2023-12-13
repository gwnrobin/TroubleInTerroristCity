using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuyMenuCollection", menuName = "CustomScriptableObject")]
public class BuyMenuItemCollection : ScriptableObject
{
    [DatabaseItem]
    public List<string> items = new List<string>();
}
