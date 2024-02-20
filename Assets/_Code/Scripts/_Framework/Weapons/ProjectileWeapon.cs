
public class ProjectileWeapon : Weapon
{
    private GunSettings.ProjectileShooting _ProjectileData;
    
    public override void Initialize(EquipmentHandler eHandler)
    {
        base.Initialize(eHandler);

        _ProjectileData = (EquipmentInfo as ProjectileWeaponInfo)?.projectile;
    }
}
