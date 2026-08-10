using System;

[Serializable]
public class ButtonSettings : SettingsBase
{
	private bool m_enabled;

	private float m_buttonScale;

	private float m_scrollViewHeightScale;

	private int m_maxSeparationCount;

	private bool m_displayButtonIndex;

	public bool Enabled
	{
		get
		{
			return m_enabled;
		}
		set
		{
			m_enabled = value;
		}
	}

	public float ButtonScale
	{
		get
		{
			return m_buttonScale;
		}
		set
		{
			if (float.IsFinite(value))
			{
				m_buttonScale = value;
			}
		}
	}

	public float ScrollViewHeightScale
	{
		get
		{
			return m_scrollViewHeightScale;
		}
		set
		{
			if (float.IsFinite(value))
			{
				m_scrollViewHeightScale = value;
			}
		}
	}

	public int MaxSeparationCount
	{
		get
		{
			return m_maxSeparationCount;
		}
		set
		{
			if (value >= 0)
			{
				m_maxSeparationCount = value;
			}
		}
	}

	public bool DisplayButtonIndex
	{
		get
		{
			return m_displayButtonIndex;
		}
		set
		{
			m_displayButtonIndex = value;
		}
	}

	public ButtonSettings()
	{
		Enabled = true;
		ButtonScale = 1f;
		ScrollViewHeightScale = 1f;
		MaxSeparationCount = 3;
		DisplayButtonIndex = false;
	}

	public ButtonSettings(ButtonSettings settings)
		: this()
	{
		Update(settings);
	}

	public override void Apply()
	{
	}

	public void Update(ButtonSettings settings)
	{
		if (settings != null)
		{
			Enabled = settings.Enabled;
			ButtonScale = settings.ButtonScale;
			ScrollViewHeightScale = settings.ScrollViewHeightScale;
			MaxSeparationCount = settings.MaxSeparationCount;
			DisplayButtonIndex = settings.DisplayButtonIndex;
		}
	}
}
