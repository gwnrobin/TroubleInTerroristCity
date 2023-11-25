using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyMenu : MonoBehaviour
{
    [SerializeField] private GameObject MenuPanel;
    
    [SerializeField] private BuyMenuItemCollection activeCollection;
    [SerializeField] private Button buyButton;

    [SerializeField] private Transform itemTransform;

    [SerializeField] private GameObject itemVisualPrefab;

    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;

    private ItemInfo selectedItem = null;

    private Humanoid player;

    public void SetPlayer(Humanoid player)
    {
        this.player = player;
    }

    private void Start()
    {
        foreach (var item in activeCollection.items)
        {
            GameObject i = Instantiate(itemVisualPrefab, itemTransform);
            BuyMenuItem buyMenuItem = i.GetComponent<BuyMenuItem>();
            buyMenuItem.SelectItem(item);
            buyMenuItem.ButtonPress.AddListener(ItemSelected);
        }
        
        buyButton.onClick.AddListener(BuyItem);
    }

    public void ToggleBuyMenu()
    {
        MenuPanel.SetActive(!MenuPanel.activeSelf);
        
        Cursor.visible = !Cursor.visible;
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void ItemSelected(ItemInfo itemInfo)
    {
        title.text = itemInfo.Name;
        description.text = itemInfo.Description;

        selectedItem = itemInfo;
    }

    private void BuyItem()
    {
        if (selectedItem == null)
            return;

        print("Test");
        
        player.Inventory.AddItem(new Item(selectedItem), ItemContainerFlags.Holster);
    }
}
