using UnityEngine.Events;

public class PlayerSpawnDispatcher : NetworkSingleton<PlayerSpawnDispatcher>
{
    public UnityEvent<Player> PlayerSpawn;
    public void OnPlayerSpawn(Player player)
    {
        PlayerSpawn.Invoke(player);
    }
}
