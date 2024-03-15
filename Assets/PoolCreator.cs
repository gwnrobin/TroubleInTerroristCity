using System.Collections.Generic;
using HQFPSTemplate.Pooling;
using HQFPSTemplate.Surfaces;
using UnityEngine;

public class PoolCreator : MonoBehaviour
{
    [SerializeField] private List<GameObject> pools = new();
    
    void Awake()
    {
        foreach (var gameObject in pools)
        {
            PoolingManager.Instance.CreatePool(gameObject, 10, 40, true, gameObject.GetInstanceID().ToString(), 3f);
        }
    }
}
