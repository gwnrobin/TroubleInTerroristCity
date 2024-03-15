using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LevelResetter : Singleton<LevelResetter>
{
    private List<GameObject> ToDelete = new();

    public void ResetLevel()
    {
        foreach (var toDeleteObject in ToDelete)
        {
            toDeleteObject.GetComponent<NetworkObject>()?.Despawn();
            Destroy(toDeleteObject);
        }
        ToDelete.Clear();
    }

    public void AddToDelete(GameObject toDelete)
    {
        ToDelete.Add(toDelete);
    }
}
