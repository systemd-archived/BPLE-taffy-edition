using System;
using System.Collections.Generic;
using System.Reflection;
using Innovation;
using UnityEngine;
using UnityEngine.UI;

public class INSettingsInterface : MonoBehaviour
{
	private delegate TOutput Converter<TInput, TOutput>(TInput input);

	private delegate bool TryConverter<TInput, TOutput>(TInput input, out TOutput output);

	private class SettingsGroup
	{
		public SettingsBase Source { get; private set; }

		public GameObject GameObject { get; private set; }

		public Text GroupName { get; private set; }

		public UITextLocale GroupNameLocale { get; private set; }

		public List<SettingsElement> Elements { get; private set; }

		public SettingsGroup(SettingsBase source, GameObject gameObject)
			: this(source, gameObject, new List<SettingsElement>())
		{
		}

		public SettingsGroup(SettingsBase source, GameObject gameObject, List<SettingsElement> elements)
		{
			Source = source;
			GameObject = gameObject;
			GroupName = gameObject.transform.Find("GroupName").GetComponent<Text>();
			GroupNameLocale = GroupName.GetComponent<UITextLocale>();
			Elements = elements;
		}

		public void UpdateValues()
		{
			foreach (SettingsElement element in Elements)
			{
				element.UpdateValue();
			}
		}
	}

	private abstract class SettingsElement
	{
		public SettingsGroup Group { get; private set; }

		public GameObject GameObject { get; private set; }

		public Text Name { get; private set; }

		public UITextLocale NameLocale { get; private set; }

		public SettingsElement(SettingsGroup group, GameObject gameObject)
		{
			Group = group;
			GameObject = gameObject;
			Name = gameObject.transform.Find("Name").GetComponent<Text>();
			NameLocale = Name.GetComponent<UITextLocale>();
		}

		public abstract object GetBoxedValue();

		public abstract void SetBoxedValue(object value);

		public abstract void UpdateValue();
	}

	private abstract class SettingsElement<T> : SettingsElement
	{
		public Func<T> Getter { get; private set; }

		public Action<T> Setter { get; private set; }

		public SettingsElement(SettingsGroup group, GameObject gameObject, Func<T> getter, Action<T> setter)
			: base(group, gameObject)
		{
			Getter = getter;
			Setter = setter;
		}

		public abstract T GetValue();

		public abstract void SetValue(T value);
	}

	private class SettingsElementToggle : SettingsElement<bool>
	{
		public Toggle Toggle { get; private set; }

		public SettingsElementToggle(SettingsGroup group, GameObject gameObject, Func<bool> getter, Action<bool> setter)
			: base(group, gameObject, getter, setter)
		{
			Toggle = gameObject.transform.Find("Toggle").GetComponent<Toggle>();
			Toggle.gameObject.SetActive(value: true);
			Toggle.isOn = getter();
			if (base.Setter != null)
			{
				Toggle.onValueChanged.AddListener(OnValueChanged);
			}
		}

		public override object GetBoxedValue()
		{
			return Toggle.isOn;
		}

		public override void SetBoxedValue(object value)
		{
			if (value is bool value2)
			{
				SetValue(value2);
				return;
			}
			if (value is string value3 && bool.TryParse(value3, out var result))
			{
				SetValue(result);
				return;
			}
			throw new InvalidCastException();
		}

		public override void UpdateValue()
		{
			Toggle.isOn = base.Getter();
		}

		public override bool GetValue()
		{
			return Toggle.isOn;
		}

		public override void SetValue(bool value)
		{
			bool num = base.Getter();
			base.Setter(value);
			if (num != base.Getter())
			{
				Instance.IsChanged = true;
				base.Group.Source.Apply();
			}
			UpdateValue();
		}

		private void OnValueChanged(bool value)
		{
			try
			{
				SetValue(value);
			}
			catch
			{
			}
			UpdateValue();
		}
	}

	private class SettingsElementInputField<T> : SettingsElement<T>
	{
		public InputField InputField { get; private set; }

		public TryConverter<string, T> Converter { get; private set; }

		public SettingsElementInputField(SettingsGroup group, GameObject gameObject, Func<T> getter, Action<T> setter, TryConverter<string, T> converter)
			: base(group, gameObject, getter, setter)
		{
			InputField = gameObject.transform.Find("InputField").GetComponent<InputField>();
			InputField.gameObject.SetActive(value: true);
			InputField.text = getter().ToString();
			InputField.onEndEdit.AddListener(OnEndEdit);
			Converter = converter;
		}

		public override object GetBoxedValue()
		{
			return base.Getter();
		}

		public override void SetBoxedValue(object value)
		{
			if (value is T value2)
			{
				SetValue(value2);
				return;
			}
			if (value is string input && Converter(input, out var output))
			{
				SetValue(output);
				return;
			}
			throw new InvalidCastException();
		}

		public override void UpdateValue()
		{
			InputField.text = base.Getter().ToString();
		}

		public override T GetValue()
		{
			return base.Getter();
		}

		public override void SetValue(T value)
		{
			T val = base.Getter();
			base.Setter(value);
			if (!val.Equals(base.Getter()))
			{
				Instance.IsChanged = true;
				base.Group.Source.Apply();
			}
			UpdateValue();
		}

		private void OnEndEdit(string text)
		{
			if (Converter(text, out var output))
			{
				try
				{
					SetValue(output);
				}
				catch
				{
				}
			}
			UpdateValue();
		}
	}

	[SerializeField]
	private GameObject m_content;

	[SerializeField]
	private GameObject m_settingsElementTemplate;

	[SerializeField]
	private GameObject m_settingsGroupTemplate;

	[SerializeField]
	private UnityEngine.UI.Button m_saveButton;

	[SerializeField]
	private UnityEngine.UI.Button m_resetButton;

	private bool m_changed;

	private List<SettingsGroup> m_settingsGroups;

	private Dictionary<string, SettingsElement> m_settingsMap;

	public bool IsChanged
	{
		get
		{
			return m_changed;
		}
		private set
		{
			m_changed = value;
		}
	}

	public static INSettingsInterface Instance { get; private set; }

	public object GetValue(string name)
	{
		return m_settingsMap[name].GetBoxedValue();
	}

	public void SetValue(string name, object value)
	{
		m_settingsMap[name].SetBoxedValue(value);
	}

	public void Save()
	{
		INUserSettings.Save();
		m_changed = false;
		UpdateValues();
	}

	public void Reset()
	{
		INUserSettings.Reset();
		m_changed = true;
		UpdateValues();
	}

	private void Awake()
	{
		Instance = this;
		m_saveButton.onClick.AddListener(Save);
		m_resetButton.onClick.AddListener(Reset);
		m_settingsGroups = GenerateGroupList(INUserSettings.Instance);
		m_settingsMap = new Dictionary<string, SettingsElement>(StringComparer.OrdinalIgnoreCase);
		foreach (SettingsGroup settingsGroup in m_settingsGroups)
		{
			foreach (SettingsElement element in settingsGroup.Elements)
			{
				m_settingsMap.Add(element.NameLocale.ID, element);
			}
		}
		SetLayout();
	}

	private List<SettingsGroup> GenerateGroupList(INUserSettings userSettings)
	{
		int num = 0;
		List<SettingsGroup> list = new List<SettingsGroup>();
		foreach (SettingsBase settings in userSettings.SettingsList)
		{
			SettingsGroup item = GenerateGroup(num, settings.GetType().Name + "_Name", settings);
			list.Add(item);
			num++;
		}
		return list;
	}

	private SettingsGroup GenerateGroup(int index, string groupName, SettingsBase source)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(m_settingsGroupTemplate);
		gameObject.SetActive(value: true);
		gameObject.name = "SettingsGroup_" + index;
		gameObject.transform.SetParent(m_content.transform, worldPositionStays: false);
		SettingsGroup settingsGroup = new SettingsGroup(source, gameObject);
		settingsGroup.GroupNameLocale.ID = groupName;
		settingsGroup.GroupNameLocale.UpdateText();
		Type type = source.GetType();
		PropertyInfo[] properties = type.GetProperties();
		for (int i = 0; i < properties.Length; i++)
		{
			PropertyInfo propertyInfo = properties[i];
			Type propertyType = propertyInfo.PropertyType;
			string text = propertyInfo.Name;
			string text2 = type.Name + "_" + text;
			Type type2 = typeof(Func<>).MakeGenericType(propertyType);
			Type type3 = typeof(Action<>).MakeGenericType(propertyType);
			Delegate obj = Delegate.CreateDelegate(type2, source, propertyInfo.GetGetMethod());
			Delegate obj2 = Delegate.CreateDelegate(type3, source, propertyInfo.GetSetMethod());
			if (propertyType == typeof(bool))
			{
				GenerateElementToggle(settingsGroup, i, text2, (Func<bool>)obj, (Action<bool>)obj2);
			}
			else if (propertyType == typeof(int))
			{
				TryConverter<string, int> converter = int.TryParse;
				GenerateElementInputField(settingsGroup, i, text2, (Func<int>)obj, (Action<int>)obj2, converter);
			}
			else if (propertyType == typeof(float))
			{
				TryConverter<string, float> converter2 = float.TryParse;
				GenerateElementInputField(settingsGroup, i, text2, (Func<float>)obj, (Action<float>)obj2, converter2);
			}
			else if (propertyType == typeof(string))
			{
				TryConverter<string, string> converter3 = TryParseString;
				GenerateElementInputField(settingsGroup, i, text2, (Func<string>)obj, (Action<string>)obj2, converter3);
			}
			else if (propertyType == typeof(HexColor))
			{
				TryConverter<string, HexColor> converter4 = HexColor.TryParse;
				GenerateElementInputField(settingsGroup, i, text2, (Func<HexColor>)obj, (Action<HexColor>)obj2, converter4);
			}
			else if (propertyType == typeof(ContraptionDataSettings.SerializationFormat))
			{
				TryConverter<string, ContraptionDataSettings.SerializationFormat> converter5 = TryParseEnum<ContraptionDataSettings.SerializationFormat>;
				GenerateElementInputField(settingsGroup, i, text2, (Func<ContraptionDataSettings.SerializationFormat>)obj, (Action<ContraptionDataSettings.SerializationFormat>)obj2, converter5);
			}
		}
		return settingsGroup;
		static bool TryParseEnum<T>(string input, out T output) where T : struct
		{
			return Enum.TryParse<T>(input, ignoreCase: true, out output);
		}
		static bool TryParseString(string input, out string output)
		{
			output = input;
			return true;
		}
	}

	private GameObject GenerateElementGameObject(SettingsGroup dataGroup, int index)
	{
		GameObject obj = UnityEngine.Object.Instantiate(m_settingsElementTemplate);
		obj.SetActive(value: true);
		obj.name = "SettingsElement_" + index;
		obj.transform.SetParent(dataGroup.GameObject.transform, worldPositionStays: false);
		return obj;
	}

	private SettingsElementToggle GenerateElementToggle(SettingsGroup group, int index, string name, Func<bool> getter, Action<bool> setter)
	{
		GameObject gameObject = GenerateElementGameObject(group, index);
		SettingsElementToggle settingsElementToggle = new SettingsElementToggle(group, gameObject, getter, setter);
		settingsElementToggle.NameLocale.ID = name;
		settingsElementToggle.NameLocale.UpdateText();
		group.Elements.Add(settingsElementToggle);
		return settingsElementToggle;
	}

	private SettingsElementInputField<T> GenerateElementInputField<T>(SettingsGroup group, int index, string name, Func<T> getter, Action<T> setter, TryConverter<string, T> converter)
	{
		GameObject gameObject = GenerateElementGameObject(group, index);
		SettingsElementInputField<T> settingsElementInputField = new SettingsElementInputField<T>(group, gameObject, getter, setter, converter);
		settingsElementInputField.NameLocale.ID = name;
		settingsElementInputField.NameLocale.UpdateText();
		group.Elements.Add(settingsElementInputField);
		return settingsElementInputField;
	}

	private void SetLayout()
	{
		float num = 100f;
		foreach (SettingsGroup settingsGroup in m_settingsGroups)
		{
			float num2 = 0f;
			RectTransform obj = (RectTransform)settingsGroup.GameObject.transform;
			obj.anchoredPosition = new Vector2(obj.anchoredPosition.x, 0f - num);
			num += 100f;
			num2 += 100f;
			foreach (SettingsElement element in settingsGroup.Elements)
			{
				RectTransform obj2 = (RectTransform)element.GameObject.transform;
				obj2.anchoredPosition = new Vector2(obj2.anchoredPosition.x, 0f - num2);
				num += 100f;
				num2 += 100f;
			}
		}
		RectTransform obj3 = (RectTransform)m_saveButton.transform;
		obj3.anchoredPosition = new Vector2(obj3.anchoredPosition.x, 0f - num);
		RectTransform obj4 = (RectTransform)m_resetButton.transform;
		obj4.anchoredPosition = new Vector2(obj4.anchoredPosition.x, 0f - num);
		num += 100f;
		RectTransform rectTransform = (RectTransform)m_content.transform;
		Vector2 sizeDelta = new Vector2(rectTransform.sizeDelta.x, num);
		rectTransform.sizeDelta = sizeDelta;
	}

	private void Update()
	{
		Text component = m_saveButton.transform.Find("Text").GetComponent<Text>();
		string text = component.GetComponent<UITextLocale>().Text;
		component.text = (m_changed ? (text + "*") : text);
	}

	private void UpdateValues()
	{
		foreach (SettingsGroup settingsGroup in m_settingsGroups)
		{
			settingsGroup.UpdateValues();
		}
	}
}
