using System;

[Serializable]
public class PartSettings : SettingsBase
{
	private bool m_noDrag;

	private bool m_disableAlienPartParticles;

	private bool m_enableWaterSystem;

	private float m_waterLevel;

	private float m_buoyancyCoefficient;

	private float m_coloredFrameSaturation1;

	private float m_coloredFrameSaturation2;

	private float m_coloredFrameBrightness1;

	private float m_coloredFrameBrightness2;

	private float m_coloredFrameBrightness3;

	private float m_jetEngineDefaultForce;

	private float m_jetEngineForceStep;

	public bool NoDrag
	{
		get
		{
			return m_noDrag;
		}
		set
		{
			m_noDrag = value;
		}
	}

	public bool DisableAlienPartParticles
	{
		get
		{
			return m_disableAlienPartParticles;
		}
		set
		{
			m_disableAlienPartParticles = value;
		}
	}

	public bool EnableWaterSystem
	{
		get
		{
			return m_enableWaterSystem;
		}
		set
		{
			m_enableWaterSystem = value;
		}
	}

	public float WaterLevel
	{
		get
		{
			return m_waterLevel;
		}
		set
		{
			if (float.IsFinite(value))
			{
				m_waterLevel = value;
			}
		}
	}

	public float BuoyancyCoefficient
	{
		get
		{
			return m_buoyancyCoefficient;
		}
		set
		{
			if (float.IsFinite(value))
			{
				m_buoyancyCoefficient = value;
			}
		}
	}

	public float ColoredFrameSaturation1
	{
		get
		{
			return m_coloredFrameSaturation1;
		}
		set
		{
			if (float.IsFinite(value) && value >= 0f && value <= 1f)
			{
				m_coloredFrameSaturation1 = value;
			}
		}
	}

	public float ColoredFrameSaturation2
	{
		get
		{
			return m_coloredFrameSaturation2;
		}
		set
		{
			if (float.IsFinite(value) && value >= 0f && value <= 1f)
			{
				m_coloredFrameSaturation2 = value;
			}
		}
	}

	public float ColoredFrameBrightness1
	{
		get
		{
			return m_coloredFrameBrightness1;
		}
		set
		{
			if (float.IsFinite(value) && value >= 0f && value <= 1f)
			{
				m_coloredFrameBrightness1 = value;
			}
		}
	}

	public float ColoredFrameBrightness2
	{
		get
		{
			return m_coloredFrameBrightness2;
		}
		set
		{
			if (float.IsFinite(value) && value >= 0f && value <= 1f)
			{
				m_coloredFrameBrightness2 = value;
			}
		}
	}

	public float ColoredFrameBrightness3
	{
		get
		{
			return m_coloredFrameBrightness3;
		}
		set
		{
			if (float.IsFinite(value) && value >= 0f && value <= 1f)
			{
				m_coloredFrameBrightness3 = value;
			}
		}
	}

	public float JetEngineDefaultForce
	{
		get
		{
			return m_jetEngineDefaultForce;
		}
		set
		{
			if (float.IsFinite(value) && value >= 0f && value <= 3000f)
			{
				m_jetEngineDefaultForce = value;
			}
		}
	}

	public float JetEngineForceStep
	{
		get
		{
			return m_jetEngineForceStep;
		}
		set
		{
			if (float.IsFinite(value) && value >= 0f)
			{
				m_jetEngineForceStep = value;
			}
		}
	}

	public PartSettings()
	{
		NoDrag = false;
		DisableAlienPartParticles = false;
		EnableWaterSystem = false;
		WaterLevel = 0f;
		BuoyancyCoefficient = 1f;
		ColoredFrameSaturation1 = 0.7f;
		ColoredFrameSaturation2 = 0.4f;
		ColoredFrameBrightness1 = 0.9f;
		ColoredFrameBrightness2 = 0.6f;
		ColoredFrameBrightness3 = 0.3f;
		JetEngineDefaultForce = 1500f;
		JetEngineForceStep = 100f;
	}

	public PartSettings(PartSettings settings)
		: this()
	{
		Update(settings);
	}

	public override void Apply()
	{
		INSettings.SetValue(INFeature.NoDrag, new Variant<bool>(NoDrag));
		INSettings.SetValue(INFeature.DisableAlienPartParticles, new Variant<bool>(DisableAlienPartParticles));
		if (INSettings.GetBool(INFeature.ColoredFrame) && Singleton<GameManager>.Instance != null && WPFMonoBehaviour.gameData != null)
		{
			for (int i = 12; i <= 129; i++)
			{
				((ColoredFrame)WPFMonoBehaviour.gameData.GetCustomPart(BasePart.PartType.MetalFrame, i)).InitializeColor();
			}
		}
	}

	public void Update(PartSettings settings)
	{
		if (settings != null)
		{
			NoDrag = settings.NoDrag;
			DisableAlienPartParticles = settings.DisableAlienPartParticles;
			EnableWaterSystem = settings.EnableWaterSystem;
			WaterLevel = settings.WaterLevel;
			BuoyancyCoefficient = settings.BuoyancyCoefficient;
			ColoredFrameSaturation1 = settings.ColoredFrameSaturation1;
			ColoredFrameSaturation2 = settings.ColoredFrameSaturation2;
			ColoredFrameBrightness1 = settings.ColoredFrameBrightness1;
			ColoredFrameBrightness2 = settings.ColoredFrameBrightness2;
			ColoredFrameBrightness3 = settings.ColoredFrameBrightness3;
			JetEngineDefaultForce = settings.JetEngineDefaultForce;
			JetEngineForceStep = settings.JetEngineForceStep;
		}
	}
}
