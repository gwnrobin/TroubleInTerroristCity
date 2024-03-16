using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class LevelResetter : Singleton<LevelResetter>
{
    public UnityEvent Reset;
    
    private List<GameObject> ToDelete = new();

    public void ResetLevel()
    {
        foreach (var toDeleteObject in ToDelete)
        {
            toDeleteObject.GetComponent<NetworkObject>()?.Despawn();
            Destroy(toDeleteObject);
        }
        ToDelete.Clear();
        
        Reset.Invoke();
    }

    public void AddToDelete(GameObject toDelete)
    {
        ToDelete.Add(toDelete);
    }
}
