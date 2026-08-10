public class AppInterfaceSettings : SettingsBase
{
	private int m_enterButtonPositionX;

	private int m_enterButtonPositionY;

	private float m_enterButtonAlpha;

	public int EnterButtonPositionX
	{
		get
		{
			return m_enterButtonPositionX;
		}
		set
		{
			if (value >= -1920 && value <= 0)
			{
				m_enterButtonPositionX = value;
			}
		}
	}

	public int EnterButtonPositionY
	{
		get
		{
			return m_enterButtonPositionY;
		}
		set
		{
			if (value >= -1080 && value <= 0)
			{
				m_enterButtonPositionY = value;
			}
		}
	}

	public float EnterButtonAlpha
	{
		get
		{
			return m_enterButtonAlpha;
		}
		set
		{
			if (float.IsFinite(value) && value >= 0f && value <= 1f)
			{
				m_enterButtonAlpha = value;
			}
		}
	}

	public AppInterfaceSettings()
	{
		EnterButtonPositionX = -60;
		EnterButtonPositionY = -60;
		EnterButtonAlpha = 1f;
	}

	public override void Apply()
	{
		INAppInterface instance = INAppInterface.Instance;
		instance?.SetEnterButtonPosition(EnterButtonPositionX, EnterButtonPositionY);
		instance?.SetEnterButtonAlpha(EnterButtonAlpha);
	}

	public void Update(AppInterfaceSettings settings)
	{
		EnterButtonPositionX = settings.EnterButtonPositionX;
		EnterButtonPositionY = settings.EnterButtonPositionY;
		EnterButtonAlpha = settings.EnterButtonAlpha;
	}
}
