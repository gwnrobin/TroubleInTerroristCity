using TMPro;
using UnityEngine;

public class UIItemSlot : MonoBehaviour
{
    private TextMeshProUGUI textComponent;
    private void Start()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    public void ChangeText(ItemSlot item)
    {

        if(item.Item != null)
            textComponent.text = item.Item.Name;
        else
            textComponent.text = "Empty";
    }
}
