using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HQFPSTemplate;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "HQ FPS Template/Item Database")]
	public class ItemDatabase : AssetSingleton<ItemDatabase>
	{
		public static bool AssetExists { get => Instance != null; }

		public ItemCategory[] Categories { get { return m_Categories; } }

		[SerializeField]
		private ItemCategory[] m_Categories;

		[SerializeField]
		//[Reorderable]
		private ItemPropertyDefinitionList m_ItemProperties;

		private List<ItemInfo> m_Items = new List<ItemInfo>();
		private Dictionary<int, ItemInfo> m_ItemsById = new Dictionary<int, ItemInfo>();
		private Dictionary<string, ItemInfo> m_ItemsByName = new Dictionary<string, ItemInfo>();


		public static ItemInfo GetItemAtIndex(int index)
		{
			List<ItemInfo> items = Instance.m_Items;

			if(items != null && items.Count > 0)
				return items[Mathf.Clamp(index, 0, items.Count - 1)];
			else
				return null;
		}

		public static int IndexOfItem(int itemId)
		{
			List<ItemInfo> items = Instance.m_Items;

			for(int i = 0;i < items.Count;i++)
			{
				if(items[i].Id == itemId)
					return i;
			}

			return -1;
		}

		public static bool TryGetItemByName(string name, out ItemInfo itemInfo)
		{
			itemInfo = GetItemByName(name);

			return itemInfo != null;
		}

		public static bool TryGetItemById(int id, out ItemInfo itemInfo)
		{
			itemInfo = GetItemById(id);

			return itemInfo != null;
		}

		public static ItemInfo GetItemByName(string name)
		{
			if(Instance == null)
			{
				Debug.LogError("No item database asset found in the Resources folder!");
				return null;
			}

			if(Instance.m_ItemsByName.TryGetValue(name, out ItemInfo itemInfo))
				return itemInfo;
			else
				return null;
		}

		public static ItemInfo GetItemById(int id)
		{
			if(Instance == null)
			{
				Debug.LogError("No item database asset found in the Resources folder!");
				return null;
			}

			if(Instance.m_ItemsById.TryGetValue(id, out ItemInfo itemInfo))
				return itemInfo;
			else
				return null;
		}

		public static List<string> GetItemNames()
		{
			List<string> names = new List<string>();

			for(int i = 0;i < Instance.m_Categories.Length;i++)
			{
				var category = Instance.m_Categories[i];

				for(int j = 0;j < category.Items.Length;j++)
					names.Add(category.Items[j].Name);
			}

			return names;
		}

		public static List<string> GetItemNamesFull()
		{
			List<string> names = new List<string>();

			for(int i = 0;i < Instance.m_Categories.Length;i++)
			{
				var category = Instance.m_Categories[i];

				for(int j = 0;j < category.Items.Length;j++)
					names.Add(Instance.m_Categories[i].Name + "/" + category.Items[j].Name);
			}

			return names;
		}

		public static List<string> GetCategoryNames()
		{
			List<string> names = new List<string>();

			for(int i = 0;i < Instance.m_Categories.Length;i++)
				names.Add(Instance.m_Categories[i].Name);

			return names;
		}

		public static string[] GetPropertyNames()
		{
			string[] names = new string[Instance.m_ItemProperties.Length];

			for(int i = 0;i < Instance.m_ItemProperties.Length;i++)
				names[i] = Instance.m_ItemProperties[i].Name;

			return names;
		}

		public static ItemPropertyDefinition[] GetProperties()
		{
			return Instance.m_ItemProperties.ToArray();
		}

		public static ItemPropertyDefinition GetPropertyByName(string name)
		{
			foreach(var property in Instance.m_ItemProperties)
			{
				if(property.Name == name)
					return property;
			}

			return null;
		}

		public static ItemPropertyDefinition GetPropertyAtIndex(int index)
		{
			if(index >= Instance.m_ItemProperties.Length)
				return null;
			else
				return Instance.m_ItemProperties[index];
		}

		public static ItemCategory GetCategoryByName(string name)
		{
			for(int i = 0;i < Instance.m_Categories.Length;i ++)
				if(Instance.m_Categories[i].Name == name)
					return Instance.m_Categories[i];

			return null;
		}

		public static ItemCategory GetRandomCategory()
		{
			return Instance.m_Categories[Random.Range(0, Instance.m_Categories.Length)];
		}

		public static ItemInfo GetRandomItemFromCategory(string categoryName)
		{
			ItemCategory category = GetCategoryByName(categoryName);

			if(category != null && category.Items.Length > 0)
				return category.Items[Random.Range(0, category.Items.Length)];

			return null;
		}

		public static int GetItemCount()
		{
			int count = 0;

			for(int c = 0;c < Instance.m_Categories.Length;c ++)
				count += Instance.m_Categories[c].Items.Length;

			return count;
		}

		private void OnEnable()
		{
			GenerateDictionaries();
			RefreshItemIDs();
		}

		private void OnValidate()
		{
			int currentID = 0;

			foreach(var category in m_Categories)
			{
				for(int j = 0;j < category.Items.Length;j++)
				{
					category.Items[j].Category = category.Name;

					currentID++;
				}
			}

			GenerateDictionaries();
			RefreshItemIDs();
		}

		private void GenerateDictionaries()
		{
			m_Items = new List<ItemInfo>();
			m_ItemsByName = new Dictionary<string, ItemInfo>();
			m_ItemsById = new Dictionary<int, ItemInfo>();

			for(int c = 0;c < m_Categories.Length;c ++)
			{
				var category = m_Categories[c];

				for(int i = 0;i < category.Items.Length;i ++)
				{
					ItemInfo item = category.Items[i];

					m_Items.Add(item);

					if(!m_ItemsByName.ContainsKey(item.Name))
						m_ItemsByName.Add(item.Name, item);

					if(!m_ItemsById.ContainsKey(item.Id))
						m_ItemsById.Add(item.Id, item);
				}
			}
		}

		private void RefreshItemIDs()
		{
			int maxAssignmentTries = 50;

			List<int> idList = new List<int>();
			int i = 0;

			foreach(var category in m_Categories)
			{
				foreach(var item in category.Items)
				{
					idList.Add(item.Id);
				}
			}

			foreach(var category in m_Categories)
			{
				foreach(var item in category.Items)
				{
					int assignmentTries = 0;
					int assignedId = idList[i];
 
					while((assignedId == 0 || idList.Contains(assignedId) && (idList.IndexOf(assignedId) != i)) && assignmentTries < maxAssignmentTries)
					{
						assignedId = IdGenerator.GenerateIntegerId();
						assignmentTries++;
					}

					if(assignmentTries == maxAssignmentTries)
					{
						Debug.LogError("Couldn't generate an unique id for item: " + item.Name);
						return;
					}
					else
					{
						idList[i] = assignedId;
						AssignIdToItem(item, assignedId);
					}

					i++;
				}
			}
		}

		private int AssignIdToItem(ItemInfo itemInfo, int id)
		{
			Type itemInfoType = typeof(ItemInfo);
			FieldInfo idField = itemInfoType.GetField("m_Id", BindingFlags.NonPublic | BindingFlags.Instance);

			idField.SetValue(itemInfo, id);

			return id;
		}
	}
	
[Serializable]
public class ItemCategory
{
	public string Name { get { return m_Name; } }

	public ItemInfo[] Items { get { return m_Items; } }

	[SerializeField]
	private string m_Name;

	[SerializeField]
	private ItemInfo[] m_Items;
}

[Serializable]
public class ItemInfo
{
	public int Id { get => m_Id; }

	public string Name { get { return m_Name; } }

	public string Category { get { return m_Category; } set { m_Category = value; } }

	public Sprite Icon { get { return m_Icon; } }

	public string Description { get { return m_Description; } }

	public GameObject Pickup { get { return m_Pickup; } }

	public int StackSize { get { return m_StackSize; } }

	public ItemPropertyInfoList Properties { get { return m_Properties; } }

	[SerializeField]
	private string m_Name;

	[Space]

	[SerializeField]
	[ReadOnly]
	private int m_Id;

	[SerializeField]
	[ReadOnly]
	private string m_Category;

	[Space]

	[SerializeField]
	//[PreviewSprite]
	private Sprite m_Icon;

	[Space]

	[SerializeField]
	//[MultilineCustom(5)]
	private string m_Description;

	[SerializeField]
	private GameObject m_Pickup;

	[SerializeField]
	//[Clamp(1, 1000)]
	private int m_StackSize = 1;

	[Space]

	[SerializeField]
	//[Reorderable]
	private ItemPropertyInfoList m_Properties;
}

[Serializable]
public class ItemPropertyInfoList : ReorderableArray<ItemPropertyInfo> { }

[Serializable]
public class ItemPropertyDefinitionList : ReorderableArray<ItemPropertyDefinition> { }

[Serializable]
public class ItemPropertyInfo
{
	public string Name { get => m_Name; }
	public ItemPropertyType Type { get => m_Type; }

	[SerializeField]
	private string m_Name;

	[SerializeField]
	private ItemPropertyType m_Type;

	[SerializeField]
	private float m_FixedValue;

	[SerializeField]
	private bool m_UseRandomValue;

	[SerializeField]
	private Vector2 m_RandomValueRange;


	/// <summary>
	/// Retrieves the internal value as a boolean.
	/// </summary>
	public bool GetAsBoolean()
	{
		return GetAsInteger() > 0;
	}

	/// <summary>
	/// Retrieves the internal value as an integer.
	/// </summary>
	public int GetAsInteger()
	{
		return (int)GetInternalValue();
	}

	/// <summary>
	/// Retrieves the internal value as a float.
	/// </summary>
	public float GetAsFloat()
	{
		return GetInternalValue();
	}

	private float GetInternalValue()
	{
		if(m_Type == ItemPropertyType.Boolean || m_Type == ItemPropertyType.ItemId)
			return m_FixedValue;
		else
		{
			float value = 0f;

			if(m_Type == ItemPropertyType.Float)
				value = m_UseRandomValue ? Random.Range(m_RandomValueRange.x, m_RandomValueRange.y) : m_FixedValue;
			else if(m_Type == ItemPropertyType.Integer)
				value = m_UseRandomValue ? Random.Range((int)m_RandomValueRange.x, (int)m_RandomValueRange.y) : m_FixedValue;

			return value;
		}
	}
}

[Serializable]
public class ItemPropertyDefinition
{
	public string Name;
	public ItemPropertyType Type;
}

[CustomPropertyDrawer(typeof(ItemPropertyInfo))]
	public class ItemPropertyInfoDrawer : PropertyDrawer
	{
		private static ItemPropertyDefinition[] m_Properties;
		private static string[] m_PropertyNames;

		private static string[] m_AllItemsFull;

		private static bool m_Initialized;


		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if(!m_Initialized)
			{
				EditorGUICustom.OneSecondPassed += GetDataFromDatabase;
				GetDataFromDatabase();
				m_Initialized = true;
			}

			position.height = EditorGUIUtility.singleLineHeight;

			SerializedProperty nameProp = property.FindPropertyRelative("m_Name");
			SerializedProperty typeProp = property.FindPropertyRelative("m_Type");

			if(m_Properties != null && m_Properties.Length > 0)
			{
				Rect popupRect = new Rect(position.x, position.y, position.width * 0.8f, position.height);

				nameProp.stringValue = EditorGUICustom.StringAtIndex(EditorGUI.Popup(popupRect, EditorGUICustom.IndexOfString(nameProp.stringValue, m_PropertyNames), m_PropertyNames), m_PropertyNames);

				int selectedPropertyIdx = EditorGUICustom.IndexOfString(nameProp.stringValue, m_PropertyNames);
				typeProp.enumValueIndex = (int)m_Properties[selectedPropertyIdx].Type;

				ItemPropertyType propType = m_Properties[selectedPropertyIdx].Type;

				Rect descriptionRect = new Rect(position.xMax - position.width * 0.2f + EditorGUIUtility.standardVerticalSpacing, position.y, position.width * 0.2f - EditorGUIUtility.standardVerticalSpacing, position.height);
				EditorGUI.LabelField(descriptionRect, "Type: " + propType.ToString().DoUnityLikeNameFormat(), EditorStyles.miniLabel);

				SerializedProperty valueProp = property.FindPropertyRelative("m_FixedValue");
				SerializedProperty isRandomProp = property.FindPropertyRelative("m_UseRandomValue");
				SerializedProperty randomValueRangeProp = property.FindPropertyRelative("m_RandomValueRange");

				position.y = position.yMax + EditorGUIUtility.standardVerticalSpacing;

				if(propType == ItemPropertyType.Boolean)
				{
					bool boolean = Mathf.Approximately(valueProp.floatValue, 0f) ? false : true;

					EditorGUI.LabelField(position, "True/False");
					boolean = EditorGUI.Toggle(new Rect(position.x + 86f, position.y, 16f, position.height), boolean);

					valueProp.floatValue = boolean ? 1f : 0f;
				}
				else if(propType == ItemPropertyType.Float || propType == ItemPropertyType.Integer)
				{
					Rect selectModeRect = new Rect(position.x, position.y, position.width * 0.35f, position.height);

					int selectedMode = GUI.Toolbar(selectModeRect, isRandomProp.boolValue == true ? 1 : 0, new string[] { "Fixed", "Random" });
					isRandomProp.boolValue = selectedMode == 1 ? true : false;

					Rect valueRect = new Rect(selectModeRect.xMax + EditorGUIUtility.singleLineHeight, position.y, position.width - selectModeRect.width - EditorGUIUtility.singleLineHeight, position.height);

					if(selectedMode == 0)
					{
						if(propType == ItemPropertyType.Float)
							valueProp.floatValue = EditorGUI.FloatField(valueRect, valueProp.floatValue);
						else
							valueProp.floatValue = Mathf.Clamp(EditorGUI.IntField(valueRect, Mathf.RoundToInt(valueProp.floatValue)), -9999999, 9999999);
					}
					else
					{
						float[] randomRange = new float[] { randomValueRangeProp.vector2Value.x, randomValueRangeProp.vector2Value.y };

						if(propType == ItemPropertyType.Float)
							randomValueRangeProp.vector2Value = EditorGUI.Vector2Field(valueRect, GUIContent.none, randomValueRangeProp.vector2Value);
						else
							randomValueRangeProp.vector2Value = EditorGUI.Vector2IntField(valueRect, GUIContent.none, new Vector2Int(Mathf.Clamp(Mathf.RoundToInt(randomRange[0]), -9999999, 9999999), Mathf.Clamp(Mathf.RoundToInt(randomRange[1]), -9999999, 9999999)));
					}
				}
				else if(propType == ItemPropertyType.ItemId)
				{
					EditorGUI.LabelField(position, "Target Item");

					Rect itemPopupRect = EditorGUI.IndentedRect(position);
					itemPopupRect = new Rect(itemPopupRect.x + 80f, itemPopupRect.y, itemPopupRect.width * 0.8f - 80f, itemPopupRect.height);

					int itemId = Mathf.RoundToInt(valueProp.floatValue);

					if(itemId == 0)
						itemId = ItemDatabase.GetItemAtIndex(0).Id;

					int selectedItem = ItemDatabase.IndexOfItem(itemId);
					selectedItem = EditorGUI.Popup(itemPopupRect, selectedItem, m_AllItemsFull);
					
					valueProp.floatValue = System.Convert.ToSingle(ItemDatabase.GetItemAtIndex(selectedItem).Id);
				}
			}
		}

		public override bool CanCacheInspectorGUI(SerializedProperty property)
		{
			return false;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2;
		}

		private void GetDataFromDatabase()
		{
			m_Properties = ItemDatabase.GetProperties();
			m_PropertyNames = ItemDatabase.GetPropertyNames();

			m_AllItemsFull = ItemDatabase.GetItemNamesFull().ToArray();
		}
	}