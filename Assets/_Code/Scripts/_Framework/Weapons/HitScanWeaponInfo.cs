using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "HitScan Weapon Info", menuName = "Equipment/HitScanWeapon")]
public class HitScanWeaponInfo : WeaponInfo
{
    [Group("7: ")] public GunSettings.RayShooting projectile;
}

[CreateAssetMenu(fileName = "Projectile Weapon Info", menuName = "Equipment/ProjectileWeapon")]
public class ProjectileWeaponInfo : WeaponInfo
{
    [Group("7: ")] public GunSettings.ProjectileShooting projectile;
}