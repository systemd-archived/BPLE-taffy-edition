using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
	public static List<Vector3> enabledLightPositions;

	private static LightManager instance;

	[SerializeField]
	private Material maskNormalMaterial;

	[SerializeField]
	private Material maskNVMaterial;

	[SerializeField]
	private Material lightBorderNormalMaterial;

	[SerializeField]
	private Material lightBorderNVMaterial;

	private bool disableNv;

	private bool isInit;

	private bool nvOn;

	private PointLightContainer container;

	private GameObject pointLightPrefab;

	private static GameObject s_maskQuadNightVisionPrefab;

	private static GameObject s_maskQuadPrefab;

	private static GameObject s_pointLightContainerPrefab;

	private LevelManager levelManager;

	private PointLightSource startPls;

	private GameObject mask;

	private GameObject nightVisionMask;

	public bool NightVisionOn => nvOn;

	public static LightManager Instance => instance;

	private void Awake()
	{
		instance = this;
		if (s_maskQuadNightVisionPrefab == null)
		{
			s_maskQuadNightVisionPrefab = ResourcesCache.Get<GameObject>("Prefabs/Lights/MaskQuadNightVision");
			s_maskQuadPrefab = ResourcesCache.Get<GameObject>("Prefabs/Lights/MaskQuad");
			s_pointLightContainerPrefab = ResourcesCache.Get<GameObject>("Prefabs/Lights/PointLightContainer");
		}
		nightVisionMask = Object.Instantiate(s_maskQuadNightVisionPrefab);
		nightVisionMask.transform.parent = WPFMonoBehaviour.ingameCamera.transform;
		nightVisionMask.transform.localPosition = Vector3.forward * 0.5f;
		nightVisionMask.SetActive(value: false);
		EventManager.Connect<GameStateChanged>(OnGameStateChanged);
	}

	private void OnDestroy()
	{
		EventManager.Disconnect<GameStateChanged>(OnGameStateChanged);
	}

	public void Init(LevelManager _levelManager)
	{
		levelManager = _levelManager;
		mask = Object.Instantiate(s_maskQuadPrefab);
		mask.transform.parent = WPFMonoBehaviour.ingameCamera.transform;
		mask.transform.localPosition = Vector3.forward * 2.5f;
		if (INSettings.GetBool(INFeature.HideDarkMask))
		{
			mask.gameObject.SetActive(value: false);
		}
		if (pointLightPrefab == null)
		{
			pointLightPrefab = ResourcesCache.Get<GameObject>("Prefabs/Lights/PointLight");
		}
		GameObject gameObject = Object.Instantiate(s_pointLightContainerPrefab);
		if (gameObject != null)
		{
			gameObject.transform.position = WPFMonoBehaviour.ingameCamera.transform.position + Vector3.forward;
			container = gameObject.GetComponent<PointLightContainer>();
			if (container != null)
			{
				GameObject gameObject2 = Object.Instantiate(pointLightPrefab);
				startPls = gameObject2.GetComponent<PointLightSource>();
				if (startPls != null)
				{
					startPls.size = 1f + 0.5f * (float)Mathf.Max(levelManager.GridWidth, levelManager.GridHeight);
					startPls.usesCurves = false;
				}
				if (gameObject2 != null)
				{
					gameObject2.name = "StartPointLight";
					Vector3 startingPosition = levelManager.StartingPosition;
					startingPosition += 0.5f * Vector3.up * levelManager.GridHeight;
					startingPosition -= Vector3.up * 0.5f;
					if (levelManager.GridWidth % 2 == 0)
					{
						startingPosition += Vector3.right * 0.5f;
					}
					gameObject2.transform.position = startingPosition;
				}
			}
		}
		isInit = true;
	}

	[ContextMenu("Toggle Nightvision")]
	public void ToggleNightVision()
	{
		if (isInit)
		{
			nvOn = !nvOn;
			nightVisionMask.SetActive(nvOn);
			mask.GetComponent<Renderer>().sharedMaterial = ((!nvOn) ? maskNormalMaterial : maskNVMaterial);
			container.borderMaterial = ((!nvOn) ? lightBorderNormalMaterial : lightBorderNVMaterial);
			UpdateLights();
			if (Singleton<SocialGameManager>.IsInstantiated())
			{
				Singleton<SocialGameManager>.Instance.ReportAchievementProgress("grp.LIGHT_UP_DARKNESS", 100.0);
			}
		}
	}

	private void OnGameStateChanged(GameStateChanged newState)
	{
		if (!isInit)
		{
			return;
		}
		if (newState.state == LevelManager.GameState.Building)
		{
			if ((bool)startPls)
			{
				startPls.isEnabled = true;
			}
			if (disableNv && nvOn)
			{
				ToggleNightVision();
				disableNv = false;
			}
		}
		else 		if (newState.state == LevelManager.GameState.Running)
		{
			if ((bool)startPls)
			{
				startPls.isEnabled = false;
			}
			GameObject gameObject = Object.Instantiate(pointLightPrefab);
			PointLightSource component = gameObject.GetComponent<PointLightSource>();
			if (component != null)
			{
				component.size = 2f;
				component.canCollide = true;
				component.canLitObjects = true;
				component.usesCurves = false;
			}
			if (nvOn)
			{
				disableNv = true;
			}
			if (gameObject != null)
			{
				gameObject.transform.parent = levelManager.ContraptionRunning.FindPig().transform;
				gameObject.transform.localPosition = Vector3.zero;
			}
			BasePart basePart = levelManager.ContraptionRunning.FindPart(BasePart.PartType.GoldenPig);
			if ((bool)basePart)
			{
				Transform transform = basePart.transform;
				GameObject obj = Object.Instantiate(pointLightPrefab);
				PointLightSource component2 = obj.GetComponent<PointLightSource>();
				if (component2 != null)
				{
					component2.size = 2f;
					component2.canCollide = true;
					component2.canLitObjects = true;
					component2.usesCurves = false;
				}
				obj.transform.parent = transform.Find("Graphics");
				obj.transform.localPosition = Vector3.zero;
			}

			// 优化：遍历 Contraption 的 Parts 来附加光源，而非多次 FindObjectsOfType
			List<BasePart> parts = levelManager.ContraptionRunning.Parts;
			for (int i = 0; i < parts.Count; i++)
			{
				BasePart p = parts[i];
				if (p == null) continue;

				if (p is TNT || p is TNTBox || p is DynamicTNTBox)
				{
					GameObject obj2 = Object.Instantiate(pointLightPrefab);
					PointLightSource pls = obj2.GetComponent<PointLightSource>();
					pls.size = 4f;
					pls.canCollide = true;
					pls.canLitObjects = true;
					pls.isEnabled = false;
					obj2.transform.parent = p.transform;
					obj2.transform.localPosition = Vector3.zero;
				}
				else if (p is Rocket)
				{
					GameObject obj3 = Object.Instantiate(pointLightPrefab);
					PointLightSource pls2 = obj3.GetComponent<PointLightSource>();
					pls2.size = 2f;
					pls2.canCollide = true;
					pls2.canLitObjects = true;
					pls2.isEnabled = false;
					obj3.transform.parent = p.transform;
					obj3.transform.localPosition = Vector3.zero;
				}
			}

			UpdateLights();
		}
		if (newState.state == LevelManager.GameState.Building && newState.prevState == LevelManager.GameState.Running)
		{
			UpdateLights();
		}
	}

	public void UpdateLights(bool waitOneFrame = true)
	{
		if (waitOneFrame)
		{
			StartCoroutine(WaitAndUpdate());
		}
		else
		{
			container.UpdateMeshes();
		}
	}

	private IEnumerator WaitAndUpdate()
	{
		yield return new WaitForEndOfFrame();
		UpdateLights(waitOneFrame: false);
	}
}
