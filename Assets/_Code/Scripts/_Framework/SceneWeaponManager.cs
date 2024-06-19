using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SceneWeaponManager : NetworkSingleton<SceneWeaponManager>
{
    public List<Transform> spawnPoints = new();

    [SerializeField] 
    private ItemCollection weaponItemCollection;
    [SerializeField] 
    private ItemCollection traitorItemCollection;
    
    [SerializeField]
    private List<NetworkVendingMachine> vendingMachines = new();
    
    private List<GameObject> _existingItems = new();
    
    public void SpawnWeapons()
    {
        if (!IsServer)
            return;

        foreach (var machine in vendingMachines)
        {
            machine.SetWeapon(traitorItemCollection.items[Random.Range(0, traitorItemCollection.items.Count)]);
        }
        
        foreach (var spawnTransform in spawnPoints)
        {
            int index = Random.Range(0, weaponItemCollection.items.Count);

            ItemInfo item = ItemDatabase.GetItemByName(weaponItemCollection.items[index]);
            int ammoName = 0;
            foreach (var prop in item.Properties)
            {
                if (prop.Name == "AmmoType")
                {
                    ammoName = prop.GetAsInteger();
                }
            }
            ItemInfo ammo = ItemDatabase.GetItemById(ammoName);
            
            GameObject weapon = Instantiate(item.Pickup, spawnTransform.position, Quaternion.identity);
            GameObject weaponAmmo = Instantiate(ammo.Pickup, spawnTransform.position, Quaternion.identity);
            weapon.GetComponent<NetworkObject>().Spawn();
            weaponAmmo.GetComponent<NetworkObject>().Spawn();
            
            _existingItems.Add(weapon);
        }
    }

    public void ClearWeapons()
    {
        for (int i = 0; i < _existingItems.Count; i++)
        {
            if (_existingItems[i] == null)
                return;
                
            Destroy(_existingItems[i]);
        }
        _existingItems.Clear(); 
    }
}
