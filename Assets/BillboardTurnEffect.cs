using UnityEngine;

public class BillboardTurnEffect : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main == null)
            return;
        
        transform.forward = Camera.main.transform.forward;
    }
}
