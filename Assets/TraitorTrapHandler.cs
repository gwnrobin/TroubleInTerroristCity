using System;
using System.Collections.Generic;
using UnityEngine;

public class TraitorTrapHandler : MonoBehaviour
{
    [SerializeField] private List<GameObject> traps = new();

    private void Start()
    {
        HideTraps();
    }

    public void HideTraps()
    {
        foreach (GameObject trap in traps)
        {
            trap.SetActive(false);
        }
    }

    public void ShowTraps()
    {
        foreach (GameObject trap in traps)
        {
            trap.SetActive(true);
        }
    }
}
