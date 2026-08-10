using System;

[Serializable]
public class ContraptionDataSettings : SettingsBase
{
	public enum SerializationFormat
	{
		CSV = 0,
		JSON = 1,
		ALL = -1
	}

	private bool m_enabled;

	private int m_slotCount;

	private SerializationFormat m_loadFormat;

	private SerializationFormat m_saveFormat;

	private bool m_backupData;

	private bool m_backupOriginalData;

	private bool m_saveAsOriginalData;

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

	public int SlotCount
	{
		get
		{
			return m_slotCount;
		}
		set
		{
			if (value >= 3 && value <= 8)
			{
				m_slotCount = value;
			}
		}
	}

	public SerializationFormat LoadFormat
	{
		get
		{
			return m_loadFormat;
		}
		set
		{
			if (Enum.IsDefined(typeof(SerializationFormat), value))
			{
				m_loadFormat = value;
			}
		}
	}

	public SerializationFormat SaveFormat
	{
		get
		{
			return m_saveFormat;
		}
		set
		{
			if (Enum.IsDefined(typeof(SerializationFormat), value))
			{
				m_saveFormat = value;
			}
		}
	}

	public bool BackupData
	{
		get
		{
			return m_backupData;
		}
		set
		{
			m_backupData = value;
		}
	}

	public bool BackupOriginalData
	{
		get
		{
			return m_backupOriginalData;
		}
		set
		{
			m_backupOriginalData = value;
		}
	}

	public bool SaveAsOriginalData
	{
		get
		{
			return m_saveAsOriginalData;
		}
		set
		{
			m_saveAsOriginalData = value;
		}
	}

	public ContraptionDataSettings()
	{
		Enabled = true;
		SlotCount = 3;
		LoadFormat = SerializationFormat.ALL;
		SaveFormat = SerializationFormat.CSV;
		BackupData = true;
		BackupOriginalData = true;
		SaveAsOriginalData = false;
	}

	public ContraptionDataSettings(ContraptionDataSettings settings)
		: this()
	{
		Update(settings);
	}

	public override void Apply()
	{
	}

	public void Update(ContraptionDataSettings settings)
	{
		if (settings != null)
		{
			Enabled = settings.Enabled;
			SlotCount = settings.SlotCount;
			LoadFormat = settings.LoadFormat;
			SaveFormat = settings.SaveFormat;
			BackupData = settings.BackupData;
			BackupOriginalData = settings.BackupOriginalData;
			SaveAsOriginalData = settings.SaveAsOriginalData;
		}
	}
}
