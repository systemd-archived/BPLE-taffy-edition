using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class INInitializer : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> m_splashes;

	[SerializeField]
	private List<GameObject> m_prefabs;

	[SerializeField]
	private ResourceData m_resourceData;

	private bool m_initialized;

	private bool m_useAlphaAnimation;

	private float m_time;

	public bool Initialized => m_initialized;

	private void Awake()
	{
		m_useAlphaAnimation = true;
		m_time = 3f;
		StartCoroutine(Initialize());
	}

	private IEnumerator Initialize()
	{
		for (int i = 0; i < m_splashes.Count; i++)
		{
			GameObject splash = Object.Instantiate(m_splashes[i], Vector3.zero, Quaternion.identity);
			yield return PlayAnimation(splash);
			Object.Destroy(splash);
		}
		INUnity.Initialize(m_resourceData);
		foreach (GameObject prefab in m_prefabs)
		{
			Object.Instantiate(prefab);
		}
		float versionTimeout = 15f;
		while (!INSettings.VersionSelected && versionTimeout > 0f)
		{
			versionTimeout -= Time.unscaledDeltaTime;
			yield return null;
		}
		if (!INSettings.VersionSelected)
		{
			Debug.LogWarning("[INInitializer] INSettings.VersionSelected timed out, forcing version 3 (B)");
			INSettings.Initialize(3);
		}
		m_initialized = true;
		yield return LoadMainMenu();
	}

	private IEnumerator LoadMainMenu()
	{
		float spawnTimeout = 30f;
		while (!SingletonSpawner.SpawnDone && spawnTimeout > 0f)
		{
			spawnTimeout -= Time.unscaledDeltaTime;
			yield return null;
		}
		if (!SingletonSpawner.SpawnDone)
		{
			Debug.LogWarning("[INInitializer] SingletonSpawner.SpawnDone timed out after 30s, continuing");
		}
		float bundleCfgTimeout = 30f;
		while ((!Bundle.initialized || Bundle.checkingBundles || !Singleton<GameConfigurationManager>.Instance.HasData) && bundleCfgTimeout > 0f)
		{
			bundleCfgTimeout -= Time.unscaledDeltaTime;
			yield return null;
		}
		if (!Singleton<GameConfigurationManager>.Instance.HasData)
		{
			Debug.LogWarning("[INInitializer] GameConfigurationManager.HasData still false, continuing anyway");
		}
		if (!Bundle.initialized)
		{
			Debug.LogWarning("[INInitializer] Bundle.initialized still false, continuing anyway");
		}
		PostInitialize();
		Singleton<GameManager>.Instance.LoadMainMenu(showLoadingScreen: true);
	}

	private void PostInitialize()
	{
		if (INSettings.GetBool(INFeature.RuntimeGameData))
		{
			Object.Instantiate(INUnity.LoadGameObject("INRuntimeGameData"));
		}
		if (INSettings.GetBool(INFeature.ApplicationInterface))
		{
			Object.Instantiate(INUnity.LoadGameObject("INApplicationInterface"));
		}
		if (INSettings.GetBool(INFeature.CommandSystem))
		{
			new GameObject("INAddonManager").AddComponent<INAddonManager>();
		}
	}

	private IEnumerator PlayAnimation(GameObject gameObject)
	{
		VideoPlayer vp = gameObject.GetComponent<VideoPlayer>();
		if (vp != null && vp.clip != null)
		{
			vp.Play();
			float startTimeout = 2f;
			while (!vp.isPlaying && startTimeout > 0f)
			{
				startTimeout -= Time.unscaledDeltaTime;
				yield return null;
			}
			while (vp.isPlaying)
			{
				yield return null;
			}
			yield break;
		}
		if (!m_useAlphaAnimation)
		{
			yield return new WaitForSeconds(m_time);
			yield break;
		}
		CanvasRenderer cr = gameObject.GetComponentInChildren<CanvasRenderer>();
		if (cr != null)
		{
			yield return cr.PlayFadeInAnimation(m_time / 3f);
			yield return new WaitForSeconds(m_time / 3f);
			yield return cr.PlayFadeOutAnimation(m_time / 3f);
		}
	}
}
