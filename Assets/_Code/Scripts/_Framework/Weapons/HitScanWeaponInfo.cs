using UnityEngine;

[CreateAssetMenu(fileName = "HitScan Weapon Info", menuName = "Equipment/HitScanWeapon")]
public class HitScanWeaponInfo : WeaponInfo
{
    [Group("7: ")] public GunSettings.RayShooting projectile;
}

