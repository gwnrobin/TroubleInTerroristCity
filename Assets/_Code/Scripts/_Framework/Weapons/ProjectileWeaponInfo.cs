using UnityEngine;

[CreateAssetMenu(fileName = "Projectile Weapon Info", menuName = "Equipment/ProjecitleWeapon")]
public class ProjectileWeaponInfo : WeaponInfo
{
    [Group("7: ")] public GunSettings.ProjectileShooting projectile;
}

