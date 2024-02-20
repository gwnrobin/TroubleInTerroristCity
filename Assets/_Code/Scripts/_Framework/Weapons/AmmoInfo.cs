public struct AmmoInfo
{
    public int CurrentInMagazine;
    public int CurrentInStorage;

    public override string ToString()
    {
        return string.Format("Ammo In Mag: {0}. Total Ammo: {1}", CurrentInMagazine, CurrentInStorage);
    }
}