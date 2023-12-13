using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BuyMenuItem : MonoBehaviour
{
    public UnityEvent<ItemInfo> ButtonPress = new();
    
    [SerializeField] private Image image;
    [SerializeField] private Button button;
    [SerializeField] private Color color;
    
    private ItemInfo _itemInfo;
    
    private void Start()
    {
        GetComponent<Graphic>().color = color;
        button.onClick.AddListener(() => ButtonPress.Invoke(_itemInfo));
    }

    public void SelectItem(string itemName)
    {
        _itemInfo = ItemDatabase.GetItemByName(itemName);
        image.sprite = _itemInfo.Icon;
    }
}
