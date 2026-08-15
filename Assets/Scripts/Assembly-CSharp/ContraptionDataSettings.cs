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
			if (m_enabled != value)
			{
				m_enabled = value;
				OnPropertyChanged("Enabled");
			}
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
			if (m_slotCount != value && value >= 3 && value <= 8)
			{
				m_slotCount = value;
				OnPropertyChanged("SlotCount");
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
			if (m_loadFormat != value && Enum.IsDefined(typeof(SerializationFormat), value))
			{
				m_loadFormat = value;
				OnPropertyChanged("LoadFormat");
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
			if (m_saveFormat != value && Enum.IsDefined(typeof(SerializationFormat), value))
			{
				m_saveFormat = value;
				OnPropertyChanged("SaveFormat");
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
			if (m_backupData != value)
			{
				m_backupData = value;
				OnPropertyChanged("BackupData");
			}
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
			if (m_backupOriginalData != value)
			{
				m_backupOriginalData = value;
				OnPropertyChanged("BackupOriginalData");
			}
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
			if (m_saveAsOriginalData != value)
			{
				m_saveAsOriginalData = value;
				OnPropertyChanged("SaveAsOriginalData");
			}
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
