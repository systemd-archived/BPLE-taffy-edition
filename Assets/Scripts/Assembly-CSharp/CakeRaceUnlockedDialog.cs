using UnityEngine;

public class CakeRaceUnlockedDialog : TextDialog
{
	private bool m_try;

	private MainMenu m_mainMenu;

	protected override void Awake()
	{
		base.Awake();
		base.onClose += HandleClosed;
		m_try = false;
		m_mainMenu = WPFMonoBehaviour.FindSceneObjectOfType<MainMenu>();
		ResourceBar.Instance.ShowItem(ResourceBar.Item.PlayerProgress, showItem: true, enableItem: false);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		EventManager.Send(new UIEvent(UIEvent.Type.OpenedCakeRaceUnlockedPopup));
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		EventManager.Send(new UIEvent(UIEvent.Type.ClosedCakeRaceUnlockedPopup));
	}

	private void OnDestroy()
	{
		base.onClose -= HandleClosed;
	}

	public void TryNow()
	{
		m_try = true;
		Close();
	}

	public new void Close()
	{
		if (m_try)
		{
			ForceCakeRace();
		}
		else
		{
			UnlockCakeRace();
		}
		base.Close();
	}

	private void ForceCakeRace()
	{
		if (m_mainMenu == null)
			m_mainMenu = WPFMonoBehaviour.FindSceneObjectOfType<MainMenu>();
		if (Singleton<GameManager>.Instance.GetGameState() == GameManager.GameState.MainMenu && m_mainMenu != null)
		{
			m_mainMenu.ForceCakeRaceButton();
		}
		else
		{
			Singleton<GameManager>.Instance.LoadMainMenu(showLoadingScreen: true);
		}
	}

	private void UnlockCakeRace()
	{
		if (m_mainMenu == null)
			m_mainMenu = WPFMonoBehaviour.FindSceneObjectOfType<MainMenu>();
		if (Singleton<GameManager>.Instance.GetGameState() == GameManager.GameState.MainMenu && m_mainMenu != null)
		{
			m_mainMenu.UnlockCakeRaceButton();
		}
	}

	private void HandleClosed()
	{
		Object.Destroy(base.gameObject);
	}
}
