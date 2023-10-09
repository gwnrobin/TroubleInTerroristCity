using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Entity))]
public class EntityDamageDealer : EntityComponent
{
    private void Start()
    {
        Entity.DealDamage.SetTryer(DealDamage);

        //Entity.DealDamage.AddListener((DamageInfo info, IDamageable damageable) => PlayerShootGunServerRpc(info.Delta));
    }

    protected virtual bool DealDamage(DamageInfo damageInfo, IDamageable damageable = null)
    {
        if (damageable != null)
        {
            DealDamage(damageable, damageInfo);

            return true;
        }
        else if (damageInfo.HitObject.TryGetComponent(out IDamageable dmgObject))
        {
            DealDamage(dmgObject, damageInfo);

            return true;
        }
        else return false;
    }

    protected virtual void DealDamage(IDamageable damageable, DamageInfo damageInfo)
    {
        damageable.TakeDamage(damageInfo);
        //PlayerShootGunServerRpc(damageInfo.Delta);
    }
    /*
    [ServerRpc(RequireOwnership = false)]
    public void PlayerShootGunServerRpc(float damage)
    {
        print(damage);
    }*/

}