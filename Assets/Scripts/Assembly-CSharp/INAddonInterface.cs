using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Innovation;
using ShellFileDialogs;
using UnityEngine;
using UnityEngine.UI;

public class INAddonInterface : MonoBehaviour
{
	public class AddonPackageItem
	{
		private GameObject m_gameObject;

		private Text m_primaryText;

		private Text m_secondaryText;

		private Text m_statusText;

		private Toggle m_enabledToggle;

		private UnityEngine.UI.Button m_reloadButton;

		private UnityEngine.UI.Button m_unloadButton;

		private AddonPackageInfo m_info;

		public GameObject GameObject => m_gameObject;

		public AddonPackageItem(GameObject gameObject)
		{
			m_gameObject = gameObject;
			m_primaryText = gameObject.transform.Find("TextGroup").Find("Primary").GetComponent<Text>();
			m_secondaryText = gameObject.transform.Find("TextGroup").Find("Secondary").GetComponent<Text>();
			m_statusText = gameObject.transform.Find("Status").GetComponent<Text>();
			m_enabledToggle = gameObject.transform.Find("EnabledToggle").GetComponent<Toggle>();
			m_reloadButton = gameObject.transform.Find("ReloadButton").GetComponent<UnityEngine.UI.Button>();
			m_unloadButton = gameObject.transform.Find("UnloadButton").GetComponent<UnityEngine.UI.Button>();
			m_enabledToggle.onValueChanged.AddListener(SetEnabled);
			m_reloadButton.onClick.AddListener(Reload);
			m_unloadButton.onClick.AddListener(Unload);
		}

		public void ApplyPackage(AddonPackageInfo info, Exception exception)
		{
			m_info = info;
			if (info.State == AddonPackageState.Failed)
			{
				m_primaryText.text = info.ID;
				m_secondaryText.text = string.Empty;
				m_statusText.text = "<color=#BF6060>状态异常 - " + exception.GetType().ToString() + ": " + exception.Message + "</color>";
				m_enabledToggle.interactable = false;
				return;
			}
			AddonPackage package = info.Package;
			m_primaryText.text = package.Name;
			m_secondaryText.text = "  |  " + package.ID + " " + package.Version?.ToString() + "  |  " + package.Developer;
			if (info.State == AddonPackageState.Enabled)
			{
				m_enabledToggle.SetIsOnWithoutNotify(value: true);
				m_statusText.text = "MD5 - " + info.MD5 + "  |  <color=#60BF60>状态正常 - 启用中</color>";
			}
			else
			{
				m_enabledToggle.SetIsOnWithoutNotify(value: false);
				m_statusText.text = "MD5 - " + info.MD5 + "  |  <color=#808080>状态正常 - 禁用中</color>";
			}
		}

		public void SetEnabled(bool enabled)
		{
			if (m_info.State != AddonPackageState.Failed)
			{
				INAddonManager.Instance.PackageManager.SetPackageEnabled(m_info.ID, enabled);
				ApplyPackage(m_info, null);
			}
		}

		public void Reload()
		{
			UnityEngine.Object.Destroy(m_gameObject);
			Instance.m_packageItemMap.Remove(m_info.ID);
			INAddonManager.Instance.PackageManager.ReloadPackage(m_info.ID);
		}

		public void Unload()
		{
			INAddonManager.Instance.PackageManager.UnloadPackage(m_info.ID);
			UnityEngine.Object.Destroy(m_gameObject);
			Instance.m_packageItemMap.Remove(m_info.ID);
		}
	}

	public class FileReceiver : AndroidJavaProxy
	{
		public event Action<bool, string> Received;

		public FileReceiver()
			: base("com.innovation.filedialog.FileReceiver")
		{
		}

		public void onActivityResult(bool success, string result)
		{
			try
			{
				Received?.Invoke(success, result);
			}
			catch
			{
			}
		}
	}

	public class PermissionHandler : AndroidJavaProxy
	{
		public event Action<int, string[], int[]> Handler;

		public PermissionHandler()
			: base("com.innovation.filedialog.PermissionHandler")
		{
		}

		public void onRequestPermissionsResult(int requestCode, string[] permissions, int[] grantResults)
		{
			try
			{
				Handler?.Invoke(requestCode, permissions, grantResults);
			}
			catch
			{
			}
		}
	}

	[SerializeField]
	private UnityEngine.UI.Button m_importButton;

	[SerializeField]
	private GameObject m_content;

	[SerializeField]
	private GameObject m_packageItemTemplate;

	private Dictionary<string, AddonPackageItem> m_packageItemMap;

	private ConcurrentQueue<string> m_pathQueue;

	public static INAddonInterface Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
		m_importButton.onClick.AddListener(ImportPackage);
		m_packageItemMap = new Dictionary<string, AddonPackageItem>();
		m_pathQueue = new ConcurrentQueue<string>();
	}

	private void Start()
	{
	}

	private void ImportPackage()
	{
		OpenFileDialog(delegate(bool success, string path)
		{
			if (success)
			{
				m_pathQueue.Enqueue(path);
			}
		});
	}

	private void OpenFileDialog(Action<bool, string> handler)
	{
		Filter[] filters = new Filter[1]
		{
			new Filter("All files", "*")
		};
		string arg = FileOpenDialog.ShowSingleSelectDialog(IntPtr.Zero, string.Empty, Application.dataPath, string.Empty, filters, 0);
		handler(arg1: true, arg);
	}

	public void ApplyPackage(AddonPackageInfo info, Exception exception)
	{
		GameObject obj = UnityEngine.Object.Instantiate(m_packageItemTemplate);
		obj.SetActive(value: true);
		obj.name = "PackageItem";
		obj.transform.SetParent(m_content.transform, worldPositionStays: false);
		if (m_packageItemMap.TryGetValue(info.ID, out var value))
		{
			UnityEngine.Object.Destroy(value.GameObject);
		}
		AddonPackageItem addonPackageItem = new AddonPackageItem(obj);
		addonPackageItem.ApplyPackage(info, exception);
		m_packageItemMap[info.ID] = addonPackageItem;
	}

	private void Update()
	{
		string result;
		while (m_pathQueue.TryDequeue(out result))
		{
			try
			{
				INAddonManager.Instance.PackageManager.ImportExternalPackage(result);
			}
			catch
			{
			}
		}
	}
}
