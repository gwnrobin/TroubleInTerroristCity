using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSpawn : MonoBehaviour
{
    public GameObject spawn;
    void Start()
    {
        Instantiate(spawn);
    }

}
