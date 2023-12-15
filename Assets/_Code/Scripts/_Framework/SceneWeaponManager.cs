using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SceneWeaponManager : NetworkSingleton<SceneWeaponManager>
{
    public List<Transform> spawnPoints = new();
    public List<GameObject> items = new();

    [SerializeField]
    private List<GameObject> _existingItems = new();
    
    public void SpawnWeapons()
    {
        if (!IsServer)
            return;
        
        foreach (var spawnTransform in spawnPoints)
        {
            GameObject weapon = Instantiate(items[Random.Range(0, items.Count)], spawnTransform.position, Quaternion.identity);
            weapon.GetComponent<NetworkObject>().Spawn();
            
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
