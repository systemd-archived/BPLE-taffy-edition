using UnityEngine;

public class INVersionSelector : MonoBehaviour
{
	private int m_version;

	private Canvas m_canvas;

	private INVersionButton[] m_buttons;

	private void Awake()
	{
		try
		{
			m_version = 3;
			m_canvas = base.transform.Find("Canvas")?.GetComponent<Canvas>();
			m_buttons = GetComponentsInChildren<INVersionButton>(includeInactive: true);
			base.gameObject.SetActive(false);
			EnterVersion();
		}
		catch (System.Exception e)
		{
			Debug.LogError($"[INVersionSelector] Awake failed: {e.Message}");
		}
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
				EnterVersion();
				return;
			}
		}
		if (m_version != -1 && Input.GetKeyDown(KeyCode.Return))
		{
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
	}
}
