using System;
using System.Collections.Generic;
using System.Globalization;
using Innovation;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class INSettings
{
	private enum SettingScope
	{
		None,
		Level,
		Sandbox,
		Global
	}

	private enum SettingTypeCode
	{
		Empty,
		Boolean,
		Int32,
		Single,
		String,
		Array
	}

	[Serializable]
	private class SettingType
	{
		private SettingTypeCode m_mainType;

		private SettingTypeCode[] m_genericArguments;

		public SettingTypeCode MainType
		{
			get
			{
				return m_mainType;
			}
			set
			{
				m_mainType = value;
			}
		}

		public SettingTypeCode[] GenericArguments
		{
			get
			{
				return m_genericArguments;
			}
			set
			{
				m_genericArguments = value;
			}
		}

		public SettingType(SettingTypeCode mainType, SettingTypeCode[] genericArguments)
		{
			m_mainType = mainType;
			m_genericArguments = genericArguments;
		}
	}

	private class SettingDeclaration
	{
		public SettingType Type { get; private set; }

		public Variant InitialValue { get; private set; }

		public SettingDeclaration(SettingType type, Variant initialValue)
		{
			Type = type;
			InitialValue = initialValue;
		}
	}

	private class SettingDeclarationContainer
	{
		private SettingDeclaration[] m_items;

		public int Count => m_items.Length;

		public SettingDeclarationContainer(int count)
		{
			m_items = new SettingDeclaration[count];
		}

		public SettingDeclaration GetDeclaration(INFeature name)
		{
			return m_items[(int)name];
		}

		public void SetDeclaration(INFeature name, SettingDeclaration data)
		{
			m_items[(int)name] = data;
		}
	}

	private class SettingData
	{
		public SettingScope Scope { get; set; }

		public Variant Value { get; set; }

		public SettingData()
		{
			Scope = SettingScope.None;
			Value = null;
		}

		public SettingData(SettingScope scope, Variant value)
		{
			Scope = scope;
			Value = value;
		}
	}

	private class SettingDataContainer
	{
		private SettingData[] m_items;

		public int Count => m_items.Length;

		public SettingDataContainer(int count)
		{
			m_items = new SettingData[count];
		}

		public SettingDataContainer(SettingDataContainer settings)
		{
			int num = settings.m_items.Length;
			m_items = new SettingData[num];
			Array.Copy(settings.m_items, m_items, num);
		}

		public SettingData GetData(INFeature name)
		{
			return m_items[(int)name];
		}

		public void SetData(INFeature name, SettingData data)
		{
			m_items[(int)name] = data;
		}
	}

	[Serializable]
	private class SerializedDeclaration
	{
		public INFeature Name { get; set; }

		public SettingType Type { get; set; }

		public object Value { get; set; }

		public SerializedDeclaration(INFeature name, SettingType type, object value)
		{
			Name = name;
			Type = type;
			Value = value;
		}
	}

	[Serializable]
	private class SerializedDeclarations
	{
		public SerializedDeclaration[] Items { get; set; }

		public SerializedDeclarations(SerializedDeclaration[] items)
		{
			Items = items;
		}

		public SettingDeclarationContainer Convert()
		{
			SettingDeclarationContainer settingDeclarationContainer = new SettingDeclarationContainer(s_count);
			SerializedDeclaration[] items = Items;
			foreach (SerializedDeclaration serializedDeclaration in items)
			{
				INFeature name = serializedDeclaration.Name;
				SettingType type = serializedDeclaration.Type;
				Variant initialValue = ToVariant(type, serializedDeclaration.Value);
				settingDeclarationContainer.SetDeclaration(name, new SettingDeclaration(type, initialValue));
			}
			return settingDeclarationContainer;
		}
	}

	[Serializable]
	private class SerializedSetting
	{
		public INFeature Name { get; set; }

		public SettingScope Scope { get; set; }

		public object Value { get; set; }

		public SerializedSetting(INFeature name, SettingScope scope, object value)
		{
			Name = name;
			Scope = scope;
			Value = value;
		}
	}

	[Serializable]
	private class SerializedSettings
	{
		public SerializedSetting[] Items { get; set; }

		public SerializedSettings(SerializedSetting[] items)
		{
			Items = items;
		}

		public SettingDataContainer Convert(SettingDeclarationContainer declarations)
		{
			int s_count = INSettings.s_count;
			SettingDataContainer settingDataContainer = new SettingDataContainer(s_count);
			for (int i = 0; i < s_count; i++)
			{
				INFeature name = (INFeature)i;
				settingDataContainer.SetData(name, new SettingData(SettingScope.Global, declarations.GetDeclaration(name).InitialValue));
			}
			SerializedSetting[] items = Items;
			foreach (SerializedSetting serializedSetting in items)
			{
				INFeature name2 = serializedSetting.Name;
				SettingScope scope = serializedSetting.Scope;
				Variant value = ToVariant(declarations.GetDeclaration(name2).Type, serializedSetting.Value);
				settingDataContainer.SetData(name2, new SettingData(scope, value));
			}
			return settingDataContainer;
		}
	}

	private class AliasSettings
	{
		private string[] m_nameList;

		private Dictionary<string, INFeature> m_nameTable;

		public static AliasSettings Create(Dictionary<INFeature, string> data)
		{
			AliasSettings aliasSettings = new AliasSettings();
			aliasSettings.m_nameList = new string[s_count];
			aliasSettings.m_nameTable = new Dictionary<string, INFeature>(data.Count);
			foreach (KeyValuePair<INFeature, string> datum in data)
			{
				INFeature key = datum.Key;
				string value = datum.Value;
				aliasSettings.m_nameList[(int)key] = value;
				aliasSettings.m_nameTable.Add(value, key);
			}
			return aliasSettings;
		}

		public string GetAlias(INFeature feature)
		{
			return m_nameList[(int)feature];
		}

		public bool TryGetValue(string alias, out INFeature feature)
		{
			return m_nameTable.TryGetValue(alias, out feature);
		}
	}

	private static readonly int s_count = Enum.GetNames(typeof(INFeature)).Length;

	private static bool s_versionSelected;

	private static int s_versionType;

	private static SettingDeclarationContainer s_declarations;

	private static SettingDataContainer s_defaultSettings;

	private static SettingDataContainer s_runtimeSettings;

	private static Action[] s_settingEditedEvents = new Action[s_count];

	private static string s_filePath;

	private static AliasSettings s_aliasSettings;

	public static bool VersionSelected => s_versionSelected;

	public static int VersionType => s_versionType;

	public static string FilePath => s_filePath;

	public static void Initialize(int version)
	{
		s_versionSelected = true;
		CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
		Load(version);
		InitializeSettings();
	}

	private static void Load(int version)
	{
		string text = version switch
		{
			2 => "A", 
			1 => "O", 
			0 => string.Empty, 
			_ => "B", 
		};
		s_versionType = version;
		s_declarations = Json.Deserialize<SerializedDeclarations>(INUnity.LoadTextAsset("INDeclarationSettings" + "Exp").text).Convert();
		if (version != 0)
		{
			s_defaultSettings = Json.Deserialize<SerializedSettings>(INUnity.LoadTextAsset(string.Concat("INSettings" + text, "Exp")).text).Convert(s_declarations);
		}
		else
		{
			s_defaultSettings = new SettingDataContainer(s_count);
			for (int i = 0; i < s_count; i++)
			{
				INFeature name = (INFeature)i;
				s_defaultSettings.SetData(name, new SettingData());
			}
		}
		s_runtimeSettings = new SettingDataContainer(s_defaultSettings);
		s_aliasSettings = AliasSettings.Create(Json.Deserialize<Dictionary<INFeature, string>>(INUnity.LoadTextAsset("INAliasSettings").text));
	}

	public static bool IsEnabled(INFeature name)
	{
		return IsEnabled(GetScope(name));
	}

	private static bool IsEnabled(SettingScope scope)
	{
		return scope switch
		{
			SettingScope.None => false, 
			SettingScope.Level => !IsSandboxMode(), 
			SettingScope.Sandbox => IsSandboxMode(), 
			SettingScope.Global => true, 
			_ => false, 
		};
	}

	public static bool GetBool(INFeature name)
	{
		return GetVariant(name).Unbox<bool>();
	}

	public static int GetInt(INFeature name)
	{
		return GetVariant(name).Unbox<int>();
	}

	public static float GetFloat(INFeature name)
	{
		return GetVariant(name).Unbox<float>();
	}

	public static string GetString(INFeature name)
	{
		return GetVariant(name).Unbox<string>();
	}

	public static T[] GetArray<T>(INFeature name)
	{
		return GetVariant(name).Unbox<T[]>();
	}

	public static T GetValue<T>(INFeature name)
	{
		return GetVariant(name).Unbox<T>();
	}

	public static bool GetInitialBool(INFeature name)
	{
		return GetInitialVariant(name).Unbox<bool>();
	}

	public static int GetInitialInt(INFeature name)
	{
		return GetInitialVariant(name).Unbox<int>();
	}

	public static float GetInitialFloat(INFeature name)
	{
		return GetInitialVariant(name).Unbox<float>();
	}

	public static string GetInitialString(INFeature name)
	{
		return GetInitialVariant(name).Unbox<string>();
	}

	public static T[] GetInitialArray<T>(INFeature name)
	{
		return GetInitialVariant(name).Unbox<T[]>();
	}

	public static T GetInitialValue<T>(INFeature name)
	{
		return GetInitialVariant(name).Unbox<T>();
	}

	private static SettingScope GetScope(INFeature name)
	{
		return s_runtimeSettings.GetData(name).Scope;
	}

	private static Variant GetVariant(INFeature name)
	{
		if (!IsEnabled(name))
		{
			return GetInitialVariant(name);
		}
		return s_runtimeSettings.GetData(name).Value;
	}

	private static Variant GetInitialVariant(INFeature name)
	{
		return s_declarations.GetDeclaration(name).InitialValue;
	}

	public static INFeature ConvertToFeature(string name)
	{
		if (s_aliasSettings.TryGetValue(name, out var feature))
		{
			return feature;
		}
		return name.ToEnum<INFeature>(ignoreCase: true);
	}

	public static void GetSettingsValue(string name, out Variant result)
	{
		INFeature name2 = ConvertToFeature(name);
		result = GetVariant(name2);
	}

	public static void SetSettingsValue(string name, object obj)
	{
		INFeature name2 = ConvertToFeature(name);
		SettingType type = s_declarations.GetDeclaration(name2).Type;
		Variant value = ((!(obj is string a) || !string.Equals(a, "default", StringComparison.OrdinalIgnoreCase)) ? ToVariant(type, obj) : s_defaultSettings.GetData(name2).Value);
		SetValue(name2, value);
	}

	public static void ResetSettings()
	{
		for (int i = 0; i < s_count; i++)
		{
			INFeature name = (INFeature)i;
			SettingData data = s_defaultSettings.GetData(name);
			SetScopeAndValue(name, data.Scope, data.Value);
		}
	}

	private static void SetScopeAndValue(INFeature name, SettingScope scope, Variant value)
	{
		Variant variant = GetVariant(name);
		s_runtimeSettings.GetData(name).Scope = scope;
		s_runtimeSettings.GetData(name).Value = value;
		Action listener = GetListener(name);
		if (listener != null && variant != GetVariant(name))
		{
			listener();
		}
	}

	public static void SetValue(INFeature name, Variant value)
	{
		Variant variant = GetVariant(name);
		s_runtimeSettings.GetData(name).Value = value;
		Action listener = GetListener(name);
		if (listener != null && variant != GetVariant(name))
		{
			listener();
		}
	}

	private static void SetScope(INFeature name, SettingScope scope)
	{
		Variant variant = GetVariant(name);
		s_runtimeSettings.GetData(name).Scope = scope;
		Action listener = GetListener(name);
		if (listener != null && variant != GetVariant(name))
		{
			listener();
		}
	}

	private static bool IsSandboxMode()
	{
		GameManager instance = Singleton<GameManager>.Instance;
		LevelManager levelManager = WPFMonoBehaviour.levelManager;
		if (instance != null && levelManager != null && instance.CurrentEpisodeType == GameManager.EpisodeType.Sandbox)
		{
			return levelManager.CurrentGameMode is BaseGameMode;
		}
		return false;
	}

	public static void AddListener(INFeature name, Action action)
	{
		ref Action reference = ref s_settingEditedEvents[(int)name];
		reference = (Action)Delegate.Combine(reference, action);
	}

	public static void RemoveListener(INFeature name, Action action)
	{
		ref Action reference = ref s_settingEditedEvents[(int)name];
		reference = (Action)Delegate.Remove(reference, action);
	}

	private static Action GetListener(INFeature name)
	{
		return s_settingEditedEvents[(int)name];
	}

	private static void InitializeSettings()
	{
		UnityEngine.Object.DontDestroyOnLoad(UnityEngine.Object.Instantiate(INUnity.LoadGameObject("EventSystem")));
		INLocalization.Create();
		INUserSettings.Load();
		INContraptionDataManager.SetContraptionData();
	}

	private static Variant ToVariant(SettingType type, object obj)
	{
		if (type.MainType == SettingTypeCode.Array)
		{
			if (obj.GetType().IsArray)
			{
				return new Variant<object>(obj);
			}
			if (obj is JArray jArray)
			{
				Type objectType = type.GenericArguments[0].ToType().MakeArrayType();
				return new Variant<object>(jArray.ToObject(objectType));
			}
			throw new InvalidCastException();
		}
		Type type2 = type.MainType.ToType();
		return Variant.Create((IConvertible)obj, type2, CultureInfo.InvariantCulture);
	}

	private static TypeCode ToTypeCode(this SettingTypeCode settingsType)
	{
		return settingsType switch
		{
			SettingTypeCode.Boolean => TypeCode.Boolean, 
			SettingTypeCode.Int32 => TypeCode.Int32, 
			SettingTypeCode.Single => TypeCode.Single, 
			SettingTypeCode.String => TypeCode.String, 
			_ => TypeCode.Empty, 
		};
	}

	private static Type ToType(this SettingTypeCode settingsType)
	{
		return settingsType switch
		{
			SettingTypeCode.Boolean => typeof(bool), 
			SettingTypeCode.Int32 => typeof(int), 
			SettingTypeCode.Single => typeof(float), 
			SettingTypeCode.String => typeof(string), 
			_ => null, 
		};
	}
}
