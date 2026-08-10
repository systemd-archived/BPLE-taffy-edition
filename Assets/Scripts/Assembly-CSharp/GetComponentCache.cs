using UnityEngine;

/// <summary>
/// GetComponent 缓存辅助，避免在 Update/热路径中重复调用 GetComponent。
/// 用法: GetComponentCache.Get(ref m_cachedRenderer, this)
/// </summary>
public static class GetComponentCache
{
	/// <summary>
	/// 懒加载缓存获取 Component，只首次调用 GetComponent。
	/// </summary>
	public static T Get<T>(ref T cache, Component owner) where T : Component
	{
		if (cache == null)
		{
			cache = owner.GetComponent<T>();
		}
		return cache;
	}

	/// <summary>
	/// 懒加载缓存获取 Component (from GameObject)。
	/// </summary>
	public static T Get<T>(ref T cache, GameObject owner) where T : Component
	{
		if (cache == null)
		{
			cache = owner.GetComponent<T>();
		}
		return cache;
	}

	/// <summary>
	/// 懒加载缓存获取子物体上的 Component。
	/// </summary>
	public static T GetInChildren<T>(ref T cache, Component owner) where T : Component
	{
		if (cache == null)
		{
			cache = owner.GetComponentInChildren<T>();
		}
		return cache;
	}
}
