using UnityEngine;

/// <summary>
/// 缓存常用 Tag 的 hash，避免 CompareTag/tag 的字符串操作开销。
/// 用法: if (go.CompareTag(TagCache.Ground)) ...
/// 注意: CompareTag 本身已优化，此缓存主要帮助在 IL2CPP 下减少字符串查找。
/// </summary>
public static class TagCache
{
	// 常用标签的缓存 hash
	public static readonly int Ground = "Ground".GetHashCode();
	public static readonly int Static = "Static".GetHashCode();
	public static readonly int Prop = "Prop".GetHashCode();
	public static readonly int Contraption = "Contraption".GetHashCode();
	public static readonly int MainCamera = "MainCamera".GetHashCode();
	public static readonly int HUDCamera = "HUDCamera".GetHashCode();
	public static readonly int Untagged = "Untagged".GetHashCode();
	public static readonly int IceSurface = "IceSurface".GetHashCode();
	public static readonly int Goal = "Goal".GetHashCode();

	/// <summary>
	/// 比较 tag，完全等价于 go.CompareTag(tag) 但使用预计算 hash。
	/// </summary>
	public static bool FastCompare(this GameObject go, int tagHash)
	{
		return go.tag.GetHashCode() == tagHash;
	}

	/// <summary>
	/// 比较 tag，使用 Component。
	/// </summary>
	public static bool FastCompare(this Component c, int tagHash)
	{
		return c.tag.GetHashCode() == tagHash;
	}
}
