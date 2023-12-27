using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VendingMachine : InteractiveObject
{
    [SerializeField] private Transform rotator;
    [SerializeField] private GameObject light;
    
    private ItemInfo _itemInfo;

    private GameObject currentGameobject;
    
    public void SetWeapon(string itemName)
    {
        RemoveCurrentItem();
        light.SetActive(true);
        
        _itemInfo = ItemDatabase.GetItemByName(itemName);

         currentGameobject = Instantiate(_itemInfo.Pickup, rotator);
    }
    
    public override void OnInteractionStart(Humanoid humanoid)
    {
        if(humanoid.Inventory.AddItem(new Item(_itemInfo), ItemContainerFlags.Holster))
            RemoveCurrentItem();
    }

    private void RemoveCurrentItem()
    {
        if (currentGameobject == null)
            return;
        
        Destroy(currentGameobject);

        light.SetActive(false);
    }
}
