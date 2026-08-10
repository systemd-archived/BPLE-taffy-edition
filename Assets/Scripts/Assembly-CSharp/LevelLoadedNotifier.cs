using System;
using UnityEngine;

public class LevelLoadedNotifier : MonoBehaviour
{
	public static event Action OnLevelLoaded;

	private void Start()
	{
		OnLevelLoaded();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	static LevelLoadedNotifier()
	{
		OnLevelLoaded = delegate
		{
		};
	}
}
