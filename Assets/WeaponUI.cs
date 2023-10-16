using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WeaponUI : MonoBehaviour
{
	[Header("Item Container")]

	[SerializeField]
	private bool m_IsPlayerContainer = true;

	[SerializeField]
	private List<string> m_ContainerName = new();

	private ItemContainer m_ItemContainer = null;

	protected List<ItemSlot> m_ItemSlots = new();
	[SerializeField]
	private List<UIItemSlot> text = new();

	public void InitWeaponUI(Player player)
	{
		for(int i = 0; i < m_ContainerName.Count; i++)
		{
			ItemContainer itemContainer = player.Inventory.GetContainerWithName(m_ContainerName[i]);

			UIItemSlot textComponent = text[i];

			if (itemContainer != null)
				itemContainer.Changed.AddListener(textComponent.ChangeText);
		}
	}
}
