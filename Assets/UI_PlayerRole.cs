using AYellowpaper.SerializedCollections;
using Michsky.UI.Heat;
using UnityEngine;

public class UI_PlayerRole : MonoBehaviour
{
    [SerializeField] private RoleNetworkHandler _roleNetworkHandler;
    
    [SerializedDictionary("roleName", "questItem")]
    public SerializedDictionary<Roles, QuestItem> _quests = new();

    public void PopRoleQuest()
    {
        if(_quests.TryGetValue(_roleNetworkHandler.role, out QuestItem questItem))
        {
            questItem.AnimateQuest();
        }
    }
}
