using System;
using System.Collections;
using UnityEngine;

public class SecureJsonManager
{
	protected string fileName;

	public SecureJsonManager(string newFileName)
	{
		fileName = newFileName;
	}

	public void Initialize(Action<string> onDataLoaded)
	{
		try
		{
			TextAsset rawConfig = Resources.Load<TextAsset>("rawAppConfig");
			if (rawConfig == null)
			{
				Debug.LogError($"[SecureJsonManager] Resources.Load<TextAsset>(\"rawAppConfig\") returned null!");
				return;
			}
			if (string.IsNullOrEmpty(rawConfig.text))
			{
				Debug.LogError($"[SecureJsonManager] rawAppConfig.text is null or empty!");
				return;
			}
			Hashtable hashtable = MiniJSON.jsonDecode(rawConfig.text) as Hashtable;
			if (hashtable == null)
			{
				Debug.LogError("[SecureJsonManager] jsonDecode returned null!");
				return;
			}
			string key = fileName;
			if (hashtable.ContainsKey(key))
			{
				onDataLoaded?.Invoke(MiniJSON.jsonEncode(hashtable[key]));
			}
			else
			{
				Debug.LogError($"[SecureJsonManager] key \"{key}\" not found in rawAppConfig!");
			}
		}
		catch (System.Exception e)
		{
			Debug.LogError($"[SecureJsonManager] Exception in Initialize: {e.Message}\n{e.StackTrace}");
		}
	}
}
