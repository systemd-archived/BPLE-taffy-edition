using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用GameObject对象池，减少Instantiate/Destroy产生的GC压力。
/// 使用方式：GameObjectPool.GetOrCreate(key, prefab, parent).Rent() 和 .Return(go)
/// 支持 IPoolable 接口自动回调。
/// </summary>
public sealed class GameObjectPool
{
	private static Dictionary<string, GameObjectPool> s_pools = new Dictionary<string, GameObjectPool>();
	private static List<IPoolable> s_tempPoolables = new List<IPoolable>(16);

	private GameObject m_prefab;
	private Transform m_parent;
	private Stack<GameObject> m_freeObjects = new Stack<GameObject>();
	private int m_maxSize;
	private int m_activeCount;
	private Dictionary<int, IPoolable[]> m_poolableCache = new Dictionary<int, IPoolable[]>();

	/// <summary>当前活跃（已借出）的对象数量</summary>
	public int ActiveCount => m_activeCount;

	/// <summary>池中空闲可用的对象数量</summary>
	public int FreeCount => m_freeObjects.Count;

	/// <summary>
	/// 获取或创建一个对象池。
	/// </summary>
	/// <param name="key">池的唯一标识（建议使用prefab名称）</param>
	/// <param name="prefab">池化的预制体</param>
	/// <param name="parent">池对象的父Transform</param>
	/// <param name="initialSize">初始预热数量</param>
	/// <param name="maxSize">最大池大小（0=无限制）</param>
	public static GameObjectPool GetOrCreate(string key, GameObject prefab, Transform parent, int initialSize = 4, int maxSize = 0)
	{
		if (!s_pools.TryGetValue(key, out var pool))
		{
			pool = new GameObjectPool(prefab, parent, initialSize, maxSize);
			s_pools[key] = pool;
		}
		return pool;
	}

	/// <summary>
	/// 获取已有对象池，若不存在则返回null。
	/// </summary>
	public static GameObjectPool Get(string key)
	{
		s_pools.TryGetValue(key, out var pool);
		return pool;
	}

	/// <summary>
	/// 获取已有对象池，若不存在则创建。
	/// </summary>
	public static GameObjectPool Ensure(string key, GameObject prefab, Transform parent)
	{
		return GetOrCreate(key, prefab, parent, 0, 0);
	}

	/// <summary>
	/// 清理所有对象池并销毁池中所有对象。
	/// </summary>
	public static void ClearAll()
	{
		foreach (var pool in s_pools.Values)
		{
			pool.Clear();
		}
		s_pools.Clear();
	}

	private GameObjectPool(GameObject prefab, Transform parent, int initialSize, int maxSize)
	{
		m_prefab = prefab;
		m_parent = parent;
		m_maxSize = maxSize;
		Warmup(initialSize);
	}

	private GameObject CreateNew()
	{
		GameObject go = Object.Instantiate(m_prefab, m_parent);
		go.name = m_prefab.name;
		return go;
	}

	/// <summary>
	/// 预热指定数量的对象到池中。
	/// </summary>
	public void Warmup(int count)
	{
		for (int i = 0; i < count; i++)
		{
			GameObject go = CreateNew();
			go.SetActive(false);
			m_freeObjects.Push(go);
		}
	}

	/// <summary>
	/// 从池中租用一个对象（自动激活并调用 IPoolable.OnRent）。
	/// </summary>
	public GameObject Rent()
	{
		GameObject go;
		if (m_freeObjects.Count > 0)
		{
			go = m_freeObjects.Pop();
		}
		else
		{
			go = CreateNew();
			if (m_maxSize > 0 && m_freeObjects.Count + m_activeCount >= m_maxSize)
			{
				go.SetActive(true);
				m_activeCount++;
				NotifyRent(go);
				return go;
			}
		}
		go.SetActive(true);
		m_activeCount++;
		NotifyRent(go);
		return go;
	}

	/// <summary>
	/// 从池中租用一个对象并设置其位置/旋转。
	/// </summary>
	public GameObject Rent(Vector3 position, Quaternion rotation, Transform parent = null)
	{
		GameObject go = Rent();
		go.transform.position = position;
		go.transform.rotation = rotation;
		if (parent != null)
		{
			go.transform.SetParent(parent, true);
		}
		return go;
	}

	/// <summary>
	/// 将对象归还到池中（自动禁用并调用 IPoolable.OnReturn）。
	/// </summary>
	public void Return(GameObject go)
	{
		if (go == null)
		{
			return;
		}
		m_activeCount--;
		NotifyReturn(go);
		go.SetActive(false);
		go.transform.SetParent(m_parent, false);
		m_freeObjects.Push(go);
	}

	/// <summary>
	/// 清空池中所有对象并销毁它们。
	/// </summary>
	public void Clear()
	{
		m_activeCount = 0;
		m_poolableCache.Clear();
		while (m_freeObjects.Count > 0)
		{
			GameObject go = m_freeObjects.Pop();
			if (go != null)
			{
				Object.Destroy(go);
			}
		}
	}

	private IPoolable[] GetPoolables(GameObject go)
	{
		int id = go.GetInstanceID();
		if (!m_poolableCache.TryGetValue(id, out var poolables))
		{
			go.GetComponentsInChildren(includeInactive: true, s_tempPoolables);
			poolables = s_tempPoolables.ToArray();
			m_poolableCache[id] = poolables;
			s_tempPoolables.Clear();
		}
		return poolables;
	}

	private void NotifyRent(GameObject go)
	{
		IPoolable[] poolables = GetPoolables(go);
		for (int i = 0; i < poolables.Length; i++)
		{
			poolables[i].OnRent();
		}
	}

	private void NotifyReturn(GameObject go)
	{
		IPoolable[] poolables = GetPoolables(go);
		for (int i = poolables.Length - 1; i >= 0; i--)
		{
			poolables[i].OnReturn();
		}
	}
}
