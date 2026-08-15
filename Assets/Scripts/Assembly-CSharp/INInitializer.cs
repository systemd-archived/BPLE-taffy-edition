using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

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
	private float AnimationInTime = 0f;
	private float AnimationOutTime = 0.5f;

	public bool Initialized => m_initialized;
	
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern System.IntPtr GetActiveWindow();

    [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(System.IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_OLD = 19;

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
#endif

	private void Awake()
	{
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        bool systemIsDark;

        try
        {
            object value = Microsoft.Win32.Registry.GetValue(
                @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                "AppsUseLightTheme", 1);
            systemIsDark = value is int i && i == 0;
        }
        catch
        {
            systemIsDark = false;
        }

        if (systemIsDark)
		{
			System.IntPtr hwnd = GetActiveWindow();
			int useDark = 1;

			if (DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDark, sizeof(int)) != 0)
			{
	            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE_OLD, ref useDark, sizeof(int));
	        }
		}
#endif

		m_useAlphaAnimation = true;
		m_time = 15f;
		Initialize().Forget();
	}

	private async UniTask Initialize()
	{
		for (int i = 0; i < m_splashes.Count; i++)
		{
			GameObject splash = Object.Instantiate(m_splashes[i], Vector3.zero, Quaternion.identity);
			await PlayAnimation(splash);
			Object.Destroy(splash);
		}
		INUnity.Initialize(m_resourceData);
		foreach (GameObject prefab in m_prefabs)
		{
			Object.Instantiate(prefab);
		}
		await UniTask.WaitUntil(() => INSettings.VersionSelected);
		m_initialized = true;
		await LoadMainMenu();
	}

	private async UniTask LoadMainMenu()
	{
		await UniTask.WaitUntil(() => SingletonSpawner.SpawnDone);
		await UniTask.WaitUntil(() => Bundle.initialized && !Bundle.checkingBundles && Singleton<GameConfigurationManager>.Instance.HasData).TimeoutWithoutException(System.TimeSpan.FromSeconds(15f));
		PostInitialize();
		Singleton<GameManager>.Instance.LoadMainMenu(showLoadingScreen: true);
	}

	private void PostInitialize()
	{
		if (INSettings.GetBool(INFeature.RuntimeGameData))
		{
			Object.Instantiate(INUnity.LoadGameObject("INPartFactoryManager"));
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

	private async UniTask PlayAnimation(GameObject gameObject)
	{
		if (!m_useAlphaAnimation)
		{
			await UniTask.Delay((int)(m_time * 1000f), ignoreTimeScale: true);
			return;
		}
		CanvasRenderer canvasRenderer = gameObject.GetComponentInChildren<CanvasRenderer>();
		if (canvasRenderer != null)
		{
			await canvasRenderer.PlayFadeInAnimation(AnimationInTime, ignoreTimeScale: true);
			await UniTask.Delay((int)(m_time / 3f * 1000f), ignoreTimeScale: true);
			await canvasRenderer.PlayFadeOutAnimation(AnimationOutTime, ignoreTimeScale: true);
		}
	}
}
