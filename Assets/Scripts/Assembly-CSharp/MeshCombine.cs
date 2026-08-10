using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MeshCombine : Singleton<MeshCombine>
{
	private const int VERTEX_LIMIT = 65534;

	private List<int> groundDepths;

	// GC优化：复用临时列表
	private List<GameObject> m_childCache = new List<GameObject>();
	private List<GameObject> m_groundCache = new List<GameObject>();
	private List<List<GameObject>> m_groundGroupCache = new List<List<GameObject>>();
	private List<int> m_depthCache = new List<int>();
	private List<string> m_groundNameCache = new List<string>();
	private List<List<GameObject>> m_propsGroupCache = new List<List<GameObject>>();
	private List<bool> m_propsVisitedCache = new List<bool>();
	private List<MeshFilter> m_meshFilterCache = new List<MeshFilter>();
	private List<MeshFilter> m_meshFilterBatch = new List<MeshFilter>();
	private Dictionary<string, List<GameObject>> m_materialSortCache = new Dictionary<string, List<GameObject>>();

	private void Awake()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void Start()
	{
		SetAsPersistant();
		CombineScene();
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
	{
		CombineScene();
	}

	private void CombineScene()
	{
		m_childCache.Clear();
		m_groundCache.Clear();
		m_groundGroupCache.Clear();
		m_depthCache.Clear();
		m_groundNameCache.Clear();
		m_propsGroupCache.Clear();
		m_propsVisitedCache.Clear();
		m_meshFilterCache.Clear();
		m_meshFilterBatch.Clear();
		m_materialSortCache.Clear();

		FindGrounds();
		for (int i = 0; i < m_groundGroupCache.Count; i++)
		{
			m_childCache.Clear();
			for (int j = 0; j < m_groundGroupCache[i].Count; j++)
			{
				FindChilds(m_groundGroupCache[i][j]);
			}
			SortGameObjectsByMaterial(m_childCache);
			for (int k = 0; k < m_materialSortGroups.Count; k++)
			{
				CombineInternal(m_materialSortGroups[k]);
			}
		}
		FindProps();
		for (int l = 0; l < m_propsGroupCache.Count; l++)
		{
			SortGameObjectsByMaterial(m_propsGroupCache[l]);
			for (int m = 0; m < m_materialSortGroups.Count; m++)
			{
				CombineInternal(m_materialSortGroups[m]);
			}
		}
	}

	private void FindChilds(GameObject parent)
	{
		for (int i = 0; i < parent.transform.childCount; i++)
		{
			GameObject gameObject = parent.transform.GetChild(i).gameObject;
			if (gameObject.CompareTag("Static") && gameObject.GetComponent<Renderer>() != null)
			{
				m_childCache.Add(gameObject);
			}
			FindChilds(gameObject);
		}
	}

	private void CombineInternal(List<GameObject> objects)
	{
		if (objects.Count == 0) return;
		m_meshFilterCache.Clear();
		float num = 0f;
		for (int i = 0; i < objects.Count; i++)
		{
			num = ((i != 0) ? ((num + objects[i].transform.position.z) * 0.5f) : objects[i].transform.position.z);
			MeshFilter component = objects[i].GetComponent<MeshFilter>();
			if (component != null)
			{
				m_meshFilterCache.Add(component);
			}
		}
		m_meshFilterCache.Sort((MeshFilter a, MeshFilter b) => b.sharedMesh.vertexCount.CompareTo(a.sharedMesh.vertexCount));
		int num2 = 0;
		m_meshFilterBatch.Clear();
		for (int j = 0; j < m_meshFilterCache.Count; j++)
		{
			MeshFilter meshFilter = m_meshFilterCache[j];
			int vertexCount = meshFilter.sharedMesh.vertexCount;
			if (num2 + vertexCount > VERTEX_LIMIT)
			{
				CombineBatch(m_meshFilterBatch, num);
				m_meshFilterBatch.Clear();
				num2 = 0;
			}
			m_meshFilterBatch.Add(meshFilter);
			num2 += vertexCount;
		}
		if (m_meshFilterBatch.Count > 0)
		{
			CombineBatch(m_meshFilterBatch, num);
		}
	}

	private void CombineBatch(List<MeshFilter> meshFilters, float depth)
	{
		if (meshFilters.Count == 0)
		{
			return;
		}
		CombineInstance[] array = new CombineInstance[meshFilters.Count];
		for (int i = 0; i < array.Length; i++)
		{
			meshFilters[i].transform.position -= Vector3.forward * depth;
			array[i].mesh = meshFilters[i].sharedMesh;
			array[i].transform = meshFilters[i].transform.localToWorldMatrix;
			meshFilters[i].GetComponent<Renderer>().enabled = false;
		}
		GameObject obj = new GameObject("CombinedMesh_" + meshFilters[0].GetComponent<Renderer>().sharedMaterial.name);
		obj.transform.position += Vector3.forward * depth;
		MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
		obj.AddComponent<MeshRenderer>();
		obj.GetComponent<Renderer>().sharedMaterial = meshFilters[0].GetComponent<Renderer>().sharedMaterial;
		meshFilter.sharedMesh = new Mesh();
		meshFilter.sharedMesh.name = "CombinedMesh";
		meshFilter.sharedMesh.CombineMeshes(array);
		for (int j = 0; j < meshFilters.Count; j++)
		{
			if (meshFilters[j].gameObject.GetComponent<PointLightSource>() == null)
			{
				UnityEngine.Object.Destroy(meshFilters[j].gameObject);
			}
		}
	}

	private List<List<GameObject>> m_materialSortGroups = new List<List<GameObject>>();

	private void SortGameObjectsByMaterial(List<GameObject> gameObjects)
	{
		m_materialSortCache.Clear();
		m_materialSortGroups.Clear();
		for (int i = 0; i < gameObjects.Count; i++)
		{
			Renderer component = gameObjects[i].GetComponent<Renderer>();
			if (component == null)
			{
				continue;
			}
			string text = ((component.sharedMaterial.mainTexture != null) ? (component.sharedMaterial.name + "_" + component.sharedMaterial.mainTexture.name) : component.sharedMaterial.name);
			if (!m_materialSortCache.TryGetValue(text, out var value))
			{
				value = new List<GameObject>();
				m_materialSortCache[text] = value;
			}
			if (!value.Contains(gameObjects[i]))
			{
				value.Add(gameObjects[i]);
			}
		}
		foreach (List<GameObject> value2 in m_materialSortCache.Values)
		{
			m_materialSortGroups.Add(value2);
		}
	}

	private void FindGrounds()
	{
		m_groundCache.Clear();
		m_groundCache.AddRange(GameObject.FindGameObjectsWithTag("Ground"));
		m_groundGroupCache.Clear();
		m_depthCache.Clear();
		groundDepths = new List<int>();
		groundDepths.Add(0);

		if (m_groundCache.Count == 0)
		{
			return;
		}

		m_groundNameCache.Clear();
		for (int i = 0; i < m_groundCache.Count; i++)
		{
			int num = (int)(m_groundCache[i].transform.position.z * 100f);
			bool flag = false;
			for (int j = 0; j < groundDepths.Count; j++)
			{
				if (groundDepths[j] == num)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				int num2 = -1;
				for (int k = 0; k < groundDepths.Count; k++)
				{
					if (groundDepths[k] > num)
					{
						num2 = k;
						break;
					}
				}
				if (num2 >= 0)
				{
					groundDepths.Insert(num2, num);
				}
				else
				{
					groundDepths.Add(num);
				}
			}
			m_depthCache.Add(num);
			string groundName = GenerateGroundName(m_groundCache[i], num);
			if (!m_groundNameCache.Contains(groundName))
			{
				m_groundNameCache.Add(groundName);
			}
		}
		for (int l = 0; l < m_groundNameCache.Count; l++)
		{
			m_groundGroupCache.Add(new List<GameObject>());
		}
		for (int m = 0; m < m_groundCache.Count; m++)
		{
			int index = m_groundNameCache.IndexOf(GenerateGroundName(m_groundCache[m], m_depthCache[m]));
			m_groundGroupCache[index].Add(m_groundCache[m]);
		}
	}

	private string GenerateGroundName(GameObject go, int depth)
	{
		if (depth >= 0)
		{
			return $"{go.name}_{depth}";
		}
		return go.name;
	}

	private void FindProps()
	{
		m_propsGroupCache.Clear();
		List<GameObject> props = new List<GameObject>(GameObject.FindGameObjectsWithTag("Prop"));
		if (props.Count == 0)
		{
			return;
		}
		m_propsVisitedCache.Clear();
		for (int i = 0; i < props.Count; i++)
		{
			m_propsVisitedCache.Add(false);
		}
		for (int j = 0; j < groundDepths.Count; j++)
		{
			List<GameObject> list = new List<GameObject>();
			for (int k = 0; k < props.Count; k++)
			{
				int num = (int)(props[k].transform.position.z * 100f);
				if (m_propsVisitedCache[k] || num >= groundDepths[j])
				{
					continue;
				}
				int num2 = -1;
				for (int l = 0; l < list.Count; l++)
				{
					if ((int)(list[l].transform.position.z * 100f) < num)
					{
						num2 = l;
						break;
					}
				}
				if (num2 >= 0)
				{
					list.Insert(num2, props[k]);
				}
				else
				{
					list.Add(props[k]);
				}
				m_propsVisitedCache[k] = true;
			}
			if (list.Count > 0)
			{
				m_propsGroupCache.Add(list);
			}
		}
	}

}
