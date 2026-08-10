using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Innovation;
using NLayer;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

public class INAddonManager : MonoBehaviour
{
	public class AddonPackageManager
	{
		private bool m_needsUpdate;

		private AddonPackageList m_packageInfo;

		private Dictionary<string, AddonPackageInfo> m_packageMap;

		public IReadOnlyCollection<AddonPackageInfo> Packages => m_packageMap.Values;

		public AddonPackageManager()
		{
			m_packageMap = new Dictionary<string, AddonPackageInfo>();
		}

		public void Initialize()
		{
			LoadPackageInfo(ResolvePath("package-list.json"));
			LoadPackages();
		}

		public AddonPackage FindPackage(string name)
		{
			return m_packageMap[name].Package;
		}

		private void LoadPackageInfo(string path)
		{
			AddonPackageList addonPackageList = null;
			if (File.Exists(path))
			{
				try
				{
					using StreamReader reader = new StreamReader(path);
					addonPackageList = Json.Deserialize<AddonPackageList>(reader);
				}
				catch
				{
				}
			}
			m_packageInfo = ((addonPackageList != null && addonPackageList.Packages != null) ? addonPackageList : new AddonPackageList());
		}

		private void SavePackageInfo(string path)
		{
			try
			{
				using StreamWriter writer = new StreamWriter(path);
				m_packageInfo.Packages.Clear();
				m_packageInfo.Packages.AddRange(m_packageMap.Values);
				Json.Serialize(writer, m_packageInfo);
			}
			catch
			{
			}
		}

		public void LoadPackages()
		{
			if (m_packageInfo == null || m_packageInfo.Packages == null)
			{
				return;
			}
			foreach (AddonPackageInfo package in m_packageInfo.Packages)
			{
				m_packageMap.Add(package.ID, package);
				LoadPackage(package.ID);
			}
		}

		public void LoadPackage(string id)
		{
			AddonPackageInfo addonPackageInfo = m_packageMap[id];
			string path = ResolvePath(addonPackageInfo.Path);
			try
			{
				if (addonPackageInfo.Kind == AddonPackageKind.ZipArchive)
				{
					AddonPackage package = LoadPackageFromZipArchive(id, path);
					using (FileStream stream = File.OpenRead(path))
					{
						addonPackageInfo.MD5 = ComputeMD5(stream);
					}
					if (addonPackageInfo.State == AddonPackageState.Failed)
					{
						addonPackageInfo.State = AddonPackageState.Enabled;
					}
					addonPackageInfo.Package = package;
					OnPackageLoaded(addonPackageInfo.ID);
					if (addonPackageInfo.State == AddonPackageState.Enabled)
					{
						SetPackageEnabled(addonPackageInfo.ID, enabled: true, force: true);
					}
					INAddonInterface.Instance.ApplyPackage(addonPackageInfo, null);
					return;
				}
				throw new AddonException("Unsupported addon package kind.");
			}
			catch (Exception exception)
			{
				addonPackageInfo.State = AddonPackageState.Failed;
				addonPackageInfo.Package = null;
				INAddonInterface.Instance.ApplyPackage(addonPackageInfo, exception);
			}
		}

		private AddonPackage LoadPackageFromZipArchive(string id, string path)
		{
			using FileStream stream = File.OpenRead(path);
			return LoadPackageFromZipArchive(id, stream);
		}

		private AddonPackage LoadPackageFromZipArchive(string id, Stream stream)
		{
			using ZipArchive zipArchive = new ZipArchive(stream, ZipArchiveMode.Read);
			SerializedAddonPackage serializedAddonPackage;
			using (StreamReader reader = new StreamReader(zipArchive.GetEntry("package.json").Open()))
			{
				serializedAddonPackage = Json.Deserialize<SerializedAddonPackage>(reader);
			}
			if (string.IsNullOrEmpty(serializedAddonPackage.ID) || serializedAddonPackage.ID != id)
			{
				throw new AddonException("Invalid addon package ID.");
			}
			if (string.IsNullOrEmpty(serializedAddonPackage.Name))
			{
				throw new AddonException("Invalid addon package name.");
			}
			AddonPackage addonPackage = new AddonPackage(serializedAddonPackage.ID, serializedAddonPackage.Name, serializedAddonPackage.Developer, serializedAddonPackage.Version, serializedAddonPackage.Kind, serializedAddonPackage.EntryPoint);
			foreach (SerializedAddonPackage.ResourceData resource in serializedAddonPackage.Resources)
			{
				using Stream stream2 = zipArchive.GetEntry(resource.Path).Open();
				using StreamReader streamReader = new StreamReader(stream2);
				switch (resource.Kind)
				{
				case SerializedAddonPackage.ResourceKind.Script:
				{
					string value4 = streamReader.ReadToEnd();
					addonPackage.Scripts[resource.Name] = value4;
					break;
				}
				case SerializedAddonPackage.ResourceKind.BinaryAsset:
				{
					MemoryStream memoryStream = new MemoryStream();
					stream2.CopyTo(memoryStream);
					addonPackage.BinaryAssets[resource.Name] = memoryStream.ToArray();
					memoryStream.Dispose();
					break;
				}
				case SerializedAddonPackage.ResourceKind.TextAsset:
				{
					string value3 = streamReader.ReadToEnd();
					addonPackage.TextAssets[resource.Name] = value3;
					break;
				}
				case SerializedAddonPackage.ResourceKind.Texture:
				{
					Texture2D value2 = LoadTexture(stream2, resource.Name);
					addonPackage.Textures[resource.Name] = value2;
					break;
				}
				case SerializedAddonPackage.ResourceKind.AudioClip:
				{
					MemoryStream memoryStream = new MemoryStream();
					stream2.CopyTo(memoryStream);
					AudioClip value = LoadAudioWithNLayer(memoryStream, resource.Name);
					addonPackage.AudioClips[resource.Name] = value;
					break;
				}
				}
			}
			return addonPackage;
		}

		public void ImportExternalPackage(string path)
		{
			if (!File.Exists(path))
			{
				throw new FileNotFoundException("Could not find file '" + path + "'", path);
			}
			if (Path.GetDirectoryName(path).Replace('\\', '/').StartsWith(DataPath))
			{
				throw new FileLoadException("Could not load file '" + path + "'", path);
			}
			using FileStream stream = File.OpenRead(path);
			ImportExternalPackage(stream);
		}

		public void ImportExternalPackage(Stream stream)
		{
			using ZipArchive zipArchive = new ZipArchive(stream, ZipArchiveMode.Read);
			SerializedAddonPackage serializedAddonPackage;
			using (StreamReader reader = new StreamReader(zipArchive.GetEntry("package.json").Open()))
			{
				serializedAddonPackage = Json.Deserialize<SerializedAddonPackage>(reader);
			}
			if (string.IsNullOrEmpty(serializedAddonPackage.ID))
			{
				throw new AddonException("Invalid addon package ID.");
			}
			if (string.IsNullOrEmpty(serializedAddonPackage.Name))
			{
				throw new AddonException("Invalid addon package name.");
			}
			if (serializedAddonPackage.Kind == AddonPackageKind.ZipArchive)
			{
				string path = serializedAddonPackage.ID + ".zip";
				using (FileStream destination = File.Open(ResolvePath(path), FileMode.Create))
				{
					stream.Seek(0L, SeekOrigin.Begin);
					stream.CopyTo(destination);
				}
				stream.Seek(0L, SeekOrigin.Begin);
				string md = ComputeMD5(stream);
				AddonPackageInfo addonPackageInfo = new AddonPackageInfo(serializedAddonPackage.ID, path, serializedAddonPackage.Kind, AddonPackageState.Enabled, md);
				m_needsUpdate = true;
				m_packageMap[addonPackageInfo.ID] = addonPackageInfo;
				LoadPackage(serializedAddonPackage.ID);
				return;
			}
			throw new AddonException("Unsupported addon package kind.");
		}

		public void OnPackageLoaded(string id)
		{
			SetPackageEnabled(id, m_packageMap[id].State == AddonPackageState.Enabled, force: true);
			AddonPackage package = m_packageMap[id].Package;
			Singleton<INCommandManager>.Instance?.Execute(package.LoadScript(package.EntryPoint), printCode: false, Singleton<INCommandManager>.Instance.CreateEngine());
			try
			{
				package.Runner?.OnLoaded();
			}
			catch
			{
			}
		}

		private string ComputeMD5(Stream stream)
		{
			using MD5 mD = MD5.Create();
			return string.Concat(from value in mD.ComputeHash(stream)
				select value.ToString("X2"));
		}

		public void UnloadPackage(string id)
		{
			m_needsUpdate = true;
			if (m_packageMap[id].State != AddonPackageState.Failed)
			{
				SetPackageEnabled(id, enabled: false);
				OnPackageUnloaded(id);
			}
			m_packageMap.Remove(id);
		}

		public void OnPackageUnloaded(string id)
		{
			AddonPackageInfo addonPackageInfo = m_packageMap[id];
			try
			{
				addonPackageInfo.Package.Runner?.OnUnloaded();
			}
			catch
			{
			}
		}

		public void ReloadPackage(string id)
		{
			m_needsUpdate = true;
			if (m_packageMap[id].State != AddonPackageState.Failed)
			{
				try
				{
					m_packageMap[id].Package.Runner?.OnDisabled();
				}
				catch
				{
				}
				OnPackageUnloaded(id);
			}
			LoadPackage(id);
		}

		public void SetPackageEnabled(string id, bool enabled)
		{
			SetPackageEnabled(id, enabled, force: false);
		}

		public void SetPackageEnabled(string id, bool enabled, bool force)
		{
			AddonPackageInfo addonPackageInfo = m_packageMap[id];
			AddonPackageState addonPackageState = ((!enabled) ? AddonPackageState.Disabled : AddonPackageState.Enabled);
			if (!force && addonPackageInfo.State == addonPackageState)
			{
				return;
			}
			if (enabled)
			{
				addonPackageInfo.State = AddonPackageState.Enabled;
				try
				{
					m_packageMap[id].Package.Runner?.OnEnabled();
				}
				catch
				{
				}
			}
			else
			{
				addonPackageInfo.State = AddonPackageState.Disabled;
				try
				{
					m_packageMap[id].Package.Runner?.OnDisabled();
				}
				catch
				{
				}
			}
			m_needsUpdate = true;
		}

		public void Update()
		{
			if (m_needsUpdate)
			{
				m_needsUpdate = false;
				SavePackageInfo(ResolvePath("package-list.json"));
			}
		}
	}

	private Dictionary<(Shader, Color), Material> m_texturePackMaterials;

	private List<AddonComponent> m_addonComponents;

	private AddonPackageManager m_packageManager;

	public AddonPackageManager PackageManager => m_packageManager;

	public static INAddonManager Instance { get; private set; }

	public static string DataPath { get; private set; }

	static INAddonManager()
	{
		DataPath = INUnity.DataPath + "/Addons";
	}

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(this);
		Instance = this;
		Directory.CreateDirectory(DataPath);
		m_addonComponents = new List<AddonComponent>();
		m_texturePackMaterials = new Dictionary<(Shader, Color), Material>();
		m_packageManager = new AddonPackageManager();
		m_packageManager.Initialize();
	}

	private void Update()
	{
		m_packageManager.Update();
	}

	public static string ResolvePath(string path)
	{
		if (Path.IsPathRooted(path))
		{
			return path;
		}
		return DataPath + "/" + path;
	}

	public static string LoadFileAsString(string path)
	{
		return File.ReadAllText(ResolvePath(path));
	}

	public static byte[] LoadFileAsBytes(string path)
	{
		return File.ReadAllBytes(ResolvePath(path));
	}

	public static void RunScript(string code, bool printCode)
	{
		Singleton<INCommandManager>.Instance?.Execute(code, printCode);
	}

	public static void RunScriptFromFile(string path, bool printCode)
	{
		RunScript(File.ReadAllText(ResolvePath(path)), printCode);
	}

	public static Texture2D LoadTexture(string path, string name)
	{
		return LoadTexture(File.ReadAllBytes(ResolvePath(path)), name);
	}

	public static Texture2D LoadTexture(byte[] data, string name)
	{
		Texture2D texture2D = new Texture2D(2, 2);
		texture2D.LoadImage(data);
		if (!string.IsNullOrEmpty(name))
		{
			texture2D.name = name;
		}
		return texture2D;
	}

	public static Texture2D LoadTexture(Stream stream, string name = null)
	{
		if (stream.CanSeek)
		{
			int num = 0;
			int num2 = (int)stream.Length;
			byte[] array = new byte[num2];
			while (num2 > 0)
			{
				int num3 = stream.Read(array, num, num2);
				if (num3 == 0)
				{
					throw new EndOfStreamException();
				}
				num += num3;
				num2 -= num3;
			}
			return LoadTexture(array, name);
		}
		using MemoryStream memoryStream = new MemoryStream();
		stream.CopyTo(memoryStream);
		return LoadTexture(memoryStream.ToArray(), name);
	}

	public static AudioClip LoadAudioWithNLayer(string path, string name)
	{
		using FileStream stream = File.OpenRead(path);
		return LoadAudioWithNLayer(stream, name);
	}

	public static AudioClip LoadAudioWithNLayer(byte[] data, string name)
	{
		using MemoryStream stream = new MemoryStream(data);
		return LoadAudioWithNLayer(stream, name);
	}

	public static AudioClip LoadAudioWithNLayer(Stream stream, string name)
	{
		MpegFile mpegFile = new MpegFile(stream);
		return AudioClip.Create(name, (int)(mpegFile.Length / 4 / mpegFile.Channels), mpegFile.Channels, mpegFile.SampleRate, stream: true, delegate(float[] data)
		{
			mpegFile.ReadSamples(data, 0, data.Length);
		});
	}

	public static void LoadAudioWithWebRequest(string path, string name, Action<AudioClip> resolve, Action<Exception> reject)
	{
		UnityWebRequestMultimedia.GetAudioClip(new Uri(ResolvePath(path)), AudioType.UNKNOWN).SendWebRequest().completed += delegate(AsyncOperation operation)
		{
			UnityWebRequest webRequest = ((UnityWebRequestAsyncOperation)operation).webRequest;
			if (webRequest.result != UnityWebRequest.Result.Success)
			{
				UnityException obj = new UnityException(webRequest.error);
				reject?.Invoke(obj);
				webRequest.Dispose();
			}
			else
			{
				AudioClip content = DownloadHandlerAudioClip.GetContent(webRequest);
				content.name = name;
				resolve?.Invoke(content);
				webRequest.Dispose();
			}
		};
	}

	public static async Task<AudioClip> LoadAudioWithWebRequest(string path, string name)
	{
		Uri uri = new Uri(ResolvePath(path));
		UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.UNKNOWN);
		await request.SendWebRequest();
		if (request.result != UnityWebRequest.Result.Success)
		{
			UnityException ex = new UnityException(request.error);
			request.Dispose();
			throw ex;
		}
		AudioClip content = DownloadHandlerAudioClip.GetContent(request);
		content.name = name;
		request.Dispose();
		return content;
	}

	public static AudioSource FindAudioSource(string audioName)
	{
		CommonAudio commonAudioCollection = WPFMonoBehaviour.gameData.commonAudioCollection;
		StringComparison comparisonType = StringComparison.OrdinalIgnoreCase;
		if (string.Equals(audioName, "MusicTheme", comparisonType))
		{
			return commonAudioCollection.MusicTheme.GetComponent<AudioSource>();
		}
		if (string.Equals(audioName, "LevelSelectionMusic", comparisonType))
		{
			return commonAudioCollection.LevelSelectionMusic.GetComponent<AudioSource>();
		}
		if (string.Equals(audioName, "InFlightMusic", comparisonType))
		{
			return commonAudioCollection.InFlightMusic.GetComponent<AudioSource>();
		}
		if (string.Equals(audioName, "BuildMusic", comparisonType))
		{
			return commonAudioCollection.BuildMusic.GetComponent<AudioSource>();
		}
		if (string.Equals(audioName, "FeedingMusic", comparisonType))
		{
			return commonAudioCollection.FeedingMusic.GetComponent<AudioSource>();
		}
		if (string.Equals(audioName, "CakeRaceTheme", comparisonType))
		{
			return commonAudioCollection.CakeRaceTheme.GetComponent<AudioSource>();
		}
		return null;
	}

	public void SetTexturePack(Texture2D texture)
	{
		if (texture == null)
		{
			throw new ArgumentNullException("texture");
		}
		Dictionary<(Shader, Color), Material> texturePackMaterials = m_texturePackMaterials;
		texturePackMaterials.Clear();
		Transform transform = Singleton<INRuntimeGameData>.Instance.PartContainer.transform;
		Transform transform2 = Singleton<INRuntimeGameData>.Instance.PartIconContainer.transform;
		int childCount = transform.childCount;
		int childCount2 = transform2.childCount;
		for (int i = 0; i < childCount + childCount2; i++)
		{
			Renderer[] componentsInChildren = ((i < childCount) ? transform.GetChild(i) : transform2.GetChild(i - childCount)).GetComponentsInChildren<Renderer>(includeInactive: true);
			foreach (Renderer renderer in componentsInChildren)
			{
				Material material = renderer.material;
				if (material != null && material.mainTexture != null && string.Equals(material.mainTexture.name, texture.name, StringComparison.OrdinalIgnoreCase))
				{
					(Shader, Color) key = (material.shader, material.color);
					if (!texturePackMaterials.TryGetValue(key, out var value))
					{
						value = new Material(material);
						value.mainTexture = texture;
						texturePackMaterials[key] = value;
					}
					renderer.material = value;
				}
			}
		}
	}

	public void SetAudioPack(AudioClip audioClip, string name)
	{
		MusicManager.Instance.SetAudioPack(audioClip, name ?? audioClip.name);
	}

	public AddonComponent FindAddonComponent(string name)
	{
		foreach (AddonComponent addonComponent in m_addonComponents)
		{
			if (addonComponent.name == name)
			{
				return addonComponent;
			}
		}
		return null;
	}

	public List<AddonComponent> FindAddonComponents(string name)
	{
		List<AddonComponent> list = new List<AddonComponent>();
		foreach (AddonComponent addonComponent in m_addonComponents)
		{
			if (addonComponent.name == name)
			{
				list.Add(addonComponent);
			}
		}
		return list;
	}

	public AddonBackground CreateBackground(Texture2D texture = null, Color? color = null, LocationMode locationMode = LocationMode.CameraAndScreen)
	{
		GameObject prefab = INUnity.LoadGameObject("AddonBackground");
		AddonBackground addonBackground = CreateAddonComponent<AddonBackground>(prefab);
		addonBackground.renderer.material.mainTexture = texture;
		if (color.HasValue)
		{
			addonBackground.renderer.material.color = color.Value;
		}
		addonBackground.LocationMode = locationMode;
		return addonBackground;
	}

	public AddonMusicPlayer CreateMusicPlayer(AudioClip audioClip, LocationMode locationMode)
	{
		GameObject prefab = INUnity.LoadGameObject("AddonMusicPlayer");
		AddonMusicPlayer addonMusicPlayer = CreateAddonComponent<AddonMusicPlayer>(prefab);
		addonMusicPlayer.GetComponent<AudioSource>().clip = audioClip;
		return addonMusicPlayer;
	}

	public AddonVideoPlayer CreateVideoPlayer(string path, LocationMode locationMode = LocationMode.CameraAndScreen)
	{
		string url = ResolvePath(path);
		GameObject prefab = INUnity.LoadGameObject("AddonVideoPlayer");
		AddonVideoPlayer addonVideoPlayer = CreateAddonComponent<AddonVideoPlayer>(prefab);
		addonVideoPlayer.LocationMode = locationMode;
		VideoPlayer component = addonVideoPlayer.GetComponent<VideoPlayer>();
		component.url = url;
		component.Play();
		return addonVideoPlayer;
	}

	public void SetFilter(float h, float s, float v)
	{
		GameObject obj = new GameObject("Filter");
		obj.name = base.name;
		obj.AddComponent<MeshRenderer>();
		obj.AddComponent<MeshFilter>().mesh = INUnity.QuadMesh;
		UnityEngine.Object.DontDestroyOnLoad(obj);
		Material material = new Material(INUnity.LoadShader("Filter"));
		material.SetFloat("_Hue", h);
		material.SetFloat("_Saturation", s);
		material.SetFloat("_Value", v);
		obj.transform.position = Vector3.zero;
		obj.GetComponent<MeshRenderer>().material = material;
	}

	private T CreateAddonComponent<T>(GameObject prefab) where T : AddonComponent
	{
		T component = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity).GetComponent<T>();
		component.transform.parent = base.transform;
		m_addonComponents.Add(component);
		return component;
	}
}
