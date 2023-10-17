﻿using Unity.Netcode;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	public static T Instance
	{
		get
		{
			if (m_Instance == null)
				m_Instance = FindObjectOfType<T>();

			return m_Instance;
		}
	}

	private static T m_Instance;
}

public class NetworkSingleton<T> : NetworkBehaviour where T : NetworkBehaviour
{
	public static T Instance
	{
		get
		{
			if (m_Instance == null)
				m_Instance = FindObjectOfType<T>();

			return m_Instance;
		}
	}

	private static T m_Instance;
}