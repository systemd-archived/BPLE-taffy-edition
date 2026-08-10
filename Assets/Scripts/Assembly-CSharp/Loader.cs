using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loader : Singleton<Loader>
{
	public static bool isLoadingLevel;

	private Vector3 originalPosition = Vector3.zero;

	private string m_lastLoadedLevel = string.Empty;

	private TextMesh m_loadingText;

	private Transform m_spinner;

	public TextMesh LoadingText
	{
		get
		{
			if (m_loadingText == null)
			{
				var child = transform.Find("LoadingText");
				if (child != null) m_loadingText = child.GetComponent<TextMesh>();
			}
			return m_loadingText;
		}
	}

	public Transform Spinner
	{
		get
		{
			if (m_spinner == null)
			{
				m_spinner = transform.Find("LoadingIndicator");
			}
			return m_spinner;
		}
	}

	public string LastLoadedString => m_lastLoadedLevel;

	public void LoadLevel(string levelName, GameManager.GameState nextState, bool showLoadingScreen, bool enableGUIAfterLoad = true)
	{
		isLoadingLevel = true;
		m_lastLoadedLevel = levelName;
		if (showLoadingScreen)
		{
			Show();
		}
		else
		{
			base.gameObject.SetActive(value: true);
		}
		GameProgress.Save();
		CoroutineRunner.Instance.StartCoroutine(LoadLevelAsync(levelName, nextState, enableGUIAfterLoad));
	}

	private IEnumerator LoadLevelAsync(string levelName, GameManager.GameState nextState, bool enableGUIAfterLoad = true)
	{
		Singleton<GuiManager>.Instance.IsEnabled = false;
		yield return null;
		string bundleId = null;
		if (!levelName.Equals("DailyChallenge") && !levelName.Equals("CakeRaceIntro") && (levelName.Equals("LevelStub") || nextState == GameManager.GameState.Cutscene || nextState == GameManager.GameState.StarLevelCutscene))
		{
			LevelLoader levelLoader = Singleton<GameManager>.instance.CurrentLevelLoader();
			string id = ((!(levelLoader != null)) ? string.Empty : levelLoader.AssetBundleName);
			if (!string.IsNullOrEmpty(id) && Bundle.HasBundle(id))
			{
				Bundle.LoadBundleAsync(id);
				bundleId = id;
			}
		}
		CoroutineRunner.Instance.StartCoroutine(DelayLoadLevelEvent(Singleton<GameManager>.Instance.GetGameState(), nextState, levelName));
		Singleton<GameManager>.Instance.SetLoadingLevelGameState(nextState);
		AsyncOperation sceneAsync = SceneManager.LoadSceneAsync(levelName);
		if (bundleId != null)
		{
			sceneAsync.allowSceneActivation = false;
			while (!Bundle.IsBundleLoaded(bundleId))
			{
				yield return null;
			}
			sceneAsync.allowSceneActivation = true;
		}
		yield return sceneAsync;
		if (Singleton<GameManager>.Instance.GetGameState() == GameManager.GameState.LevelSelection || Singleton<GameManager>.Instance.GetGameState() == GameManager.GameState.SandboxLevelSelection || Singleton<GameManager>.Instance.GetGameState() == GameManager.GameState.EpisodeSelection)
		{
			GameTime.Pause(pause: false);
		}
		Singleton<GuiManager>.Instance.IsEnabled = enableGUIAfterLoad;
		isLoadingLevel = false;
	}

	private void Awake()
	{
		SetAsPersistant();
		originalPosition = base.transform.position;
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void Start()
	{
		Hide();
	}

	private Coroutine m_blinkRoutine;

	private void Show()
	{
		RepositionToNearplane();
		base.gameObject.SetActive(value: true);
		if (Spinner != null)
		{
			Spinner.gameObject.SetActive(false);
		}
		if (LoadingText != null)
		{
			LoadingText.gameObject.SetActive(true);
			LoadingText.text = "LOADING...";
			m_blinkRoutine = StartCoroutine(BlinkLoadingText());
		}
	}

	private void Hide()
	{
		if (m_blinkRoutine != null)
		{
			StopCoroutine(m_blinkRoutine);
			m_blinkRoutine = null;
		}
		if (LoadingText != null)
		{
			LoadingText.gameObject.SetActive(false);
		}
		base.gameObject.SetActive(value: false);
	}

	private IEnumerator BlinkLoadingText()
	{
		float blinkInterval = 0.6f;
		while (true)
		{
			LoadingText.text = "LOADING...";
			yield return new WaitForSeconds(blinkInterval);
			LoadingText.text = "";
			yield return new WaitForSeconds(blinkInterval);
		}
	}

	private void RepositionToNearplane()
	{
		Camera hudCamera = WPFMonoBehaviour.hudCamera;
		if ((bool)hudCamera)
		{
			float z = hudCamera.transform.position.z + hudCamera.nearClipPlane * 2f;
			base.transform.position = new Vector3(originalPosition.x, originalPosition.y - hudCamera.transform.InverseTransformPoint(0f, 0f, 0f).y, z);
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
	{
		Singleton<GuiManager>.Instance.IsEnabled = true;
		Hide();
		RepositionToNearplane();
		DisableScreenSleep(Singleton<GameManager>.Instance.IsInGame());
		EventManager.SendOnNextUpdate(CoroutineRunner.Instance, new LevelLoadedEvent(Singleton<GameManager>.Instance.GetGameState()));
	}

	private IEnumerator DelayLoadLevelEvent(GameManager.GameState currentState, GameManager.GameState nextState, string levelName)
	{
		yield return new WaitForEndOfFrame();
		EventManager.Send(new LoadLevelEvent(currentState, nextState, levelName));
	}

	private void DisableScreenSleep(bool disable)
	{
		Screen.sleepTimeout = ((!disable) ? (-2) : (-1));
	}
}
