using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shader.Find 缓存封装。
/// Shader.Find 涉及全局 I/O 查找，在运行时循环中调用会造成严重性能损失。
/// 用法: var shader = ShaderCache.Get("e2d/Curve");
/// </summary>
public static class ShaderCache
{
	private static Dictionary<string, Shader> s_cache = new Dictionary<string, Shader>();

	public static Shader Get(string name)
	{
		if (!s_cache.TryGetValue(name, out var shader) || shader == null)
		{
			shader = Shader.Find(name);
			if (shader != null)
			{
				s_cache[name] = shader;
			}
		}
		return shader;
	}

	/// <summary>预热常用 Shader（在启动时调用）</summary>
	public static void Prewarm(params string[] names)
	{
		foreach (string name in names)
		{
			Get(name);
		}
	}

	public static void Clear()
	{
		s_cache.Clear();
	}
}
