using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(Respawn(other));
    }

    private IEnumerator Respawn(Collider other)
    {
        yield return new WaitForEndOfFrame();

        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
        
            player.DisabledMovement.ForceStart();
            other.transform.position = GameManager.Instance.GetRandomSpawnPoint().position;
            player.DisabledMovement.ForceStop();
        }
    }
}