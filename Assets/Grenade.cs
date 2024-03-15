using HQFPSTemplate.Pooling;
using Unity.Netcode;
using UnityEngine;

public class Grenade : NetworkBehaviour
{
    [SerializeField] private GameObject explosion;
    
    public float explosionForce = 100f;
    public float explosionRadius = 20f;

    private void OnCollisionEnter(Collision other)
    {
        ContactPoint contact = other.contacts[0]; // Assuming we only want to consider the first contact point
        
        Vector3 rotationAxis = contact.normal;
        
        Quaternion rotation = Quaternion.LookRotation(rotationAxis, Vector3.up);

        ExplosionClientRPC(contact.point, rotation);
        
        ApplyExplosionForce(contact.point);

        Destroy(gameObject);
    }

    [ClientRpc]
    private void ExplosionClientRPC(Vector3 position, Quaternion rotation)
    {
        PoolingManager.Instance.GetObject(explosion, position, rotation);
    }

    private void ApplyExplosionForce(Vector3 explosionPosition)
    {
        Collider[] colliders = Physics.OverlapSphere(explosionPosition, explosionRadius);
        
        foreach (Collider col in colliders)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();

            if (rb != null)
            {
                Vector3 dir = col.transform.position - explosionPosition;
                
                dir.Normalize();
                
                rb.AddForceAtPosition(dir*explosionForce, explosionPosition, ForceMode.Impulse);
            }
            
            CharacterController cc = col.GetComponent<CharacterController>();

            if (cc != null)
            {
                Vector3 dir = col.transform.position - explosionPosition;
                
                dir.Normalize();

                cc.Move(dir * explosionForce * Time.deltaTime);
            }

            Hitbox hitbox = col.GetComponent<Hitbox>();

            if (hitbox != null)
            {
                Vector3 dir = col.transform.position - explosionPosition;
                
                dir.Normalize();
                
                var damageInfo = new DamageInfo(-100, DamageType.Explosion, explosionPosition, dir, 0f, dir, null, hitbox.transform);
                
                DamageSyncManager.Instance.SendDataToClient(damageInfo, null);
            }
        }
    }
}
