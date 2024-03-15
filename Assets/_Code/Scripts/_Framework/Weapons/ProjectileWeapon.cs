using UnityEngine;

public class ProjectileWeapon : Weapon
{
    private GunSettings.ProjectileShooting _projectileData;
    
    public override void Initialize(EquipmentHandler eHandler)
    {
        base.Initialize(eHandler);

        _projectileData = (EquipmentInfo as ProjectileWeaponInfo)?.projectile;
    }
    
    public override void Shoot(Ray[] itemUseRays)
    {
        base.Shoot(itemUseRays);
        if (!Player.IsOwner)
            return;

        SpawnManager.Instance.RequestNetworkSpawn(_projectileData.projectile, generalInfo.weaponTransformData.barrel.position, itemUseRays[0].direction * 1000);
    }
}
