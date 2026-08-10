using UnityEngine;

public class ProgressResetButton : MonoBehaviour
{
#if UNITY_EDITOR
	private void OnGUI()
	{
		if (GUI.Button(new Rect(0f, 80f, 120f, 100f), "Open Cheats"))
		{
			Singleton<GameManager>.Instance.LoadCheatsPanel();
		}
	}
#endif
}
