using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Resources.Load 缓存封装，避免重复的磁盘 IO 和字符串操作。
/// 用法: var prefab = ResourcesCache.Get<GameObject>("Prefabs/PointLight");
/// </summary>
public static class ResourcesCache
{
	private static Dictionary<string, Object> s_cache = new Dictionary<string, Object>();

	/// <summary>
	/// 从缓存获取资源，若未缓存则通过 Resources.Load 加载并存入缓存。
	/// </summary>
	public static T Get<T>(string path) where T : Object
	{
		if (!s_cache.TryGetValue(path, out var obj) || obj == null)
		{
			obj = Resources.Load<T>(path);
			if (obj != null)
			{
				s_cache[path] = obj;
			}
		}
		return obj as T;
	}

	/// <summary>
	/// 预热缓存（在启动时调用）。
	/// </summary>
	public static void Prewarm<T>(string path) where T : Object
	{
		Get<T>(path);
	}

	/// <summary>
	/// 清理所有缓存（场景切换或内存压力时调用）。
	/// </summary>
	public static void Clear()
	{
		s_cache.Clear();
		Resources.UnloadUnusedAssets();
	}

	/// <summary>
	/// 移除指定路径的缓存。
	/// </summary>
	public static void Remove(string path)
	{
		s_cache.Remove(path);
	}
}
