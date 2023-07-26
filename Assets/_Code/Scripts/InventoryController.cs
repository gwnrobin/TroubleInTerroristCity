using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class InventoryController: HumanoidComponent
{
	[SerializeField]
	private LayerMask m_WallsLayer = new LayerMask();

	[SerializeField]
	private bool m_DropItemsOnDeath = true;

	[Space]

	[SerializeField]
	private Vector3 m_DropOffset = new Vector3(0f, 0f, 0.8f);

	[SerializeField]
	[Range(0.01f, 1f)]
	private float m_CrouchHeightDropMod = 0.5f;

	[SerializeField]
	private float m_DropAngularFactor = 150f;

	[SerializeField]
	private float m_DropSpeed = 8f;

	[Space]

	[SerializeField]
	[Group]
	private SoundPlayer m_DropSounds = null;

	private Inventory m_Inventory;


	public override void OnEntityStart()
	{
		m_Inventory = GetComponent<Inventory>();
		Humanoid.DropItem.SetTryer(TryDropItem);
		Humanoid.DropItem.AddListener(OnPlayerDropItem);
		Entity.Death.AddListener(OnEntityDeath);
	}

	public bool TryDropItem(Item item)
	{
		bool canBeDropped = item != null &&
			item.Pickup != null &&
			Humanoid.DropItem.LastExecutionTime + 0.5f < Time.time &&
			Humanoid.EquipItem.LastExecutionTime + 0.5f < Time.time &&
			m_Inventory.RemoveItem(item);

		if (canBeDropped)
		{
			float heightDropMultiplier = 1f;

			if (Humanoid.Crouch.Active)
				heightDropMultiplier = m_CrouchHeightDropMod;

			StartCoroutine(C_Drop(item, heightDropMultiplier));

			return true;
		}

		return false;
	}

	private void OnPlayerDropItem(Item droppedItem)
	{
		Humanoid.EquipItem.Try(Humanoid.Inventory.GetContainerWithName("Pistol").Slots[0].Item, true);
	}

	private IEnumerator C_Drop(Item item, float heightDropMultiplier)
	{
		if (item == null)
			yield return null;

		bool nearWall = false;

		Vector3 dropPosition;
		Quaternion dropRotation;

		if (Physics.Raycast(transform.position, transform.InverseTransformDirection(Vector3.forward) * 1.5f, m_DropOffset.z, m_WallsLayer))
		{
			dropPosition = transform.position + transform.TransformVector(new Vector3(0f, m_DropOffset.y * heightDropMultiplier, -0.2f));
			dropRotation = Quaternion.LookRotation(Entity.LookDirection.Get());
			nearWall = true;
		}
		else
		{
			dropPosition = transform.position + transform.TransformVector(new Vector3(m_DropOffset.x, m_DropOffset.y * heightDropMultiplier, m_DropOffset.z));
			dropRotation = Random.rotationUniform;
		}

		GameObject droppedItem = GetEquipmentPickup(item);

		droppedItem.transform.parent = null;
		droppedItem.SetActive(true);
		droppedItem.transform.position = dropPosition;
		droppedItem.transform.rotation = dropRotation;

		var rigidbody = droppedItem.GetComponent<Rigidbody>();
		var collider = droppedItem.GetComponent<Collider>();

		if (rigidbody != null)
		{
			Physics.IgnoreCollision(Entity.GetComponent<Collider>(), collider);

			rigidbody.isKinematic = false;

			if (rigidbody != null && !nearWall)
			{
				rigidbody.AddTorque(Random.rotation.eulerAngles * m_DropAngularFactor);
				rigidbody.AddForce(Entity.LookDirection.Get() * m_DropSpeed, ForceMode.VelocityChange);
			}
		}

		m_DropSounds.Play2D(ItemSelection.Method.RandomExcludeLast);

		var pickup = droppedItem.GetComponent<ItemPickup>();

		if (pickup != null)
			pickup.SetItem(item);
	}

	private GameObject GetEquipmentPickup(Item item)
    {
		Item[] items = gameObject.transform.GetComponentsInChildren<Item>(true);

		foreach(Item i in items)
        {
			print(i);
			if(i.Id == item.Id)
            {
				return i.gameObject;
			}
        }			

		return null;
    }

	private void OnEntityDeath()
	{
		if (m_DropItemsOnDeath)
		{
			for (int i = 0; i < m_Inventory.Containers.Count; i++)
			{
				for (int j = 0; j < m_Inventory.Containers[i].Slots.Length; j++)
				{
					var slot = m_Inventory.Containers[i].Slots[j];

					if (slot.Item)
					{
						TryDropItem(slot.Item);
						slot.SetItem(null);
					}
				}
			}
		}
	}
}
