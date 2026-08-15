using Cysharp.Threading.Tasks;
using UnityEngine;

public class INVersionSelector : MonoBehaviour
{
	private int m_version;

	private Canvas m_canvas;

	private INVersionButton[] m_buttons;

	private void Awake()
	{
		m_version = -1;
		m_canvas = base.transform.Find("Canvas").GetComponent<Canvas>();
		m_buttons = GetComponentsInChildren<INVersionButton>(includeInactive: true);
		m_canvas.planeDistance = 9f;
		m_canvas.worldCamera = Object.FindObjectOfType<Camera>();
		base.transform.Find("Canvas").GetComponent<CanvasGroup>().PlayFadeInAnimation(0.5f, ignoreTimeScale: true)
			.Forget();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
			return;
		}
		for (int i = 0; i < 4; i++)
		{
			if (Input.GetKeyDown((KeyCode)(49 + i)))
			{
				SelectVersion(i);
				if (i < m_buttons.Length)
				{
					m_buttons[i].Select();
				}
				EnterVersion();
				return;
			}
		}
		if (Input.GetKeyDown(KeyCode.Return))
		{
			if (m_version == -1)
			{
				SelectVersion(0);
				if (m_buttons.Length > 0)
				{
					m_buttons[0].Select();
				}
			}
			EnterVersion();
			return;
		}
		bool flag = false;
		INVersionButton[] buttons = m_buttons;
		foreach (INVersionButton iNVersionButton in buttons)
		{
			flag |= iNVersionButton.IsEnabled;
		}
		buttons = m_buttons;
		foreach (INVersionButton iNVersionButton2 in buttons)
		{
			if (iNVersionButton2.Type == -1 && (flag ^ iNVersionButton2.gameObject.activeSelf))
			{
				iNVersionButton2.gameObject.SetActive(flag);
			}
		}
	}

	public void SelectVersion(int version)
	{
		m_version = version;
	}

	public void EnterVersion()
	{
		INSettings.Initialize(m_version);
		foreach (INVersionButton button in m_buttons)
		{
			button.interactable = false;
		}
	}
}
