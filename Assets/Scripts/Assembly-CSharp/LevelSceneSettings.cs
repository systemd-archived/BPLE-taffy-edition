using System;
using Innovation;

[Serializable]
public class LevelSceneSettings : SettingsBase
{
	private float m_terrainScale;

	private float m_minCameraScale;

	private float m_maxCameraScale;

	private bool m_hideRuntimeButtons;

	private bool m_hideStarAndDessertCounter;

	private bool m_enablePropertyPanel;

	private bool m_enableEnhancedPropertyPanel;

	private bool m_enableCustomBackgroundColor;

	private HexColor m_customBackgroundColor;

	public float TerrainScale
	{
		get
		{
			return m_terrainScale;
		}
		set
		{
			if (float.IsFinite(value) && value >= 0f)
			{
				m_terrainScale = value;
			}
		}
	}

	public float MinCameraScale
	{
		get
		{
			return m_minCameraScale;
		}
		set
		{
			if (float.IsFinite(value) && value >= 0f)
			{
				m_minCameraScale = value;
			}
		}
	}

	public float MaxCameraScale
	{
		get
		{
			return m_maxCameraScale;
		}
		set
		{
			if (float.IsFinite(value) && value >= 0f)
			{
				m_maxCameraScale = value;
			}
		}
	}

	public bool HideRuntimeButtons
	{
		get
		{
			return m_hideRuntimeButtons;
		}
		set
		{
			m_hideRuntimeButtons = value;
		}
	}

	public bool HideStarAndDessertCounter
	{
		get
		{
			return m_hideStarAndDessertCounter;
		}
		set
		{
			m_hideStarAndDessertCounter = value;
		}
	}

	public bool EnablePropertyPanel
	{
		get
		{
			return m_enablePropertyPanel;
		}
		set
		{
			m_enablePropertyPanel = value;
		}
	}

	public bool EnableEnhancedPropertyPanel
	{
		get
		{
			return m_enableEnhancedPropertyPanel;
		}
		set
		{
			m_enableEnhancedPropertyPanel = value;
		}
	}

	public bool EnableCustomBackgroundColor
	{
		get
		{
			return m_enableCustomBackgroundColor;
		}
		set
		{
			m_enableCustomBackgroundColor = value;
		}
	}

	public HexColor CustomBackgroundColor
	{
		get
		{
			return m_customBackgroundColor;
		}
		set
		{
			m_customBackgroundColor = value;
		}
	}

	public LevelSceneSettings()
	{
		TerrainScale = 1f;
		MinCameraScale = 1f;
		MaxCameraScale = 1f;
		HideRuntimeButtons = false;
		HideStarAndDessertCounter = false;
		EnablePropertyPanel = true;
		EnableEnhancedPropertyPanel = false;
		EnableCustomBackgroundColor = false;
		CustomBackgroundColor = HexColor.Black;
	}

	public LevelSceneSettings(LevelSceneSettings settings)
		: this()
	{
		Update(settings);
	}

	public override void Apply()
	{
		INSettings.SetValue(INFeature.EnhancedPropertyPanel, new Variant<bool>(EnableEnhancedPropertyPanel));
	}

	public void Update(LevelSceneSettings settings)
	{
		if (settings != null)
		{
			TerrainScale = settings.TerrainScale;
			MinCameraScale = settings.MinCameraScale;
			MaxCameraScale = settings.MaxCameraScale;
			HideRuntimeButtons = settings.HideRuntimeButtons;
			HideStarAndDessertCounter = settings.HideStarAndDessertCounter;
			EnablePropertyPanel = settings.EnablePropertyPanel;
			EnableEnhancedPropertyPanel = settings.EnableEnhancedPropertyPanel;
			EnableCustomBackgroundColor = settings.EnableCustomBackgroundColor;
			CustomBackgroundColor = settings.CustomBackgroundColor;
		}
	}
}
