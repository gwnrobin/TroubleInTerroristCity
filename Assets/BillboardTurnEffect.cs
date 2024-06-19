using UnityEngine;

public class BillboardTurnEffect : MonoBehaviour
{
    void LateUpdate()
    {
        transform.forward = Camera.main.transform.forward;
    }
}
